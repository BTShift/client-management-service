using Microsoft.AspNetCore.Server.Kestrel.Core;
using ClientManagement.Application.Services;
using ClientManagement.Infrastructure.Services;
using ClientManagement.Api.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Configure Kestrel for Railway deployment with dual ports
// Enable HTTP/2 cleartext support for gRPC
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);

// Railway configuration - fail fast if required variables are missing
// PORT is set by Railway for public HTTP/1.1 traffic (health checks)
// We use GRPC_PORT for private HTTP/2 gRPC traffic
var httpPort = int.Parse(Environment.GetEnvironmentVariable("PORT")
    ?? Environment.GetEnvironmentVariable("HTTP_PORT")
    ?? "8080");  // Default for local development
var grpcPort = int.Parse(Environment.GetEnvironmentVariable("GRPC_PORT")
    ?? "5000");  // Default for local development

// Configure Kestrel with support for both HTTP/1.1 and HTTP/2
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Health + optional REST on HTTP/1.1 (Railway's public port)
    serverOptions.ListenAnyIP(httpPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    // gRPC with JSON Transcoding on HTTP/1.1 and HTTP/2 (private port)
    // This allows both gRPC (HTTP/2) and REST (HTTP/1.1) requests
    serverOptions.ListenAnyIP(grpcPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
}).AddJsonTranscoding();

// Add gRPC reflection for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready", "live" });

// Add Application and Infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Authorization
builder.Services.AddAuthorization();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Client Management API", Version = "v1" });
});

// Add CORS for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors();
    app.MapGrpcReflectionService();
}

// Railway expects certain headers in production
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        await next();
    });
}

app.UseAuthorization();

// Map health check endpoints - Railway requires these
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString()
            })
        }));
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

// Map gRPC services (to be implemented)
app.MapGrpcService<ClientService>();

// Map a simple root endpoint for verification
app.MapGet("/", () => new
{
    service = "Client Management Service",
    version = "1.0.0",
    status = "running",
    httpPort = httpPort,
    grpcPort = grpcPort,
    timestamp = DateTime.UtcNow,
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
    features = new
    {
        grpc = "enabled on port " + grpcPort,
        jsonTranscoding = "enabled - HTTP/JSON requests supported",
        grpcReflection = app.Environment.IsDevelopment() ? "enabled" : "disabled (production)",
        swagger = app.Environment.IsDevelopment() ? "enabled at /swagger" : "disabled (production)"
    }
});

// Log the service configuration
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Client Management Service configuration:");
startupLogger.LogInformation("  HTTP/1.1 port (health checks, REST via JSON Transcoding): {HttpPort}", httpPort);
startupLogger.LogInformation("  HTTP/1.1 + HTTP/2 port (gRPC + JSON Transcoding): {GrpcPort}", grpcPort);
startupLogger.LogInformation("Starting application...");

app.Run();

// Make the implicit Program class public so it can be referenced by tests
public partial class Program { }

// WeatherForecast record for the sample endpoint
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}