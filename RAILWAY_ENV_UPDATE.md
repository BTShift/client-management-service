# Railway Environment Variable Update Required

## Migration from Direct MassTransit to Shift.Messaging.Infrastructure

After this PR is merged, the following environment variable changes need to be made in Railway for the `client-management-service`:

### Variables to REMOVE:
```
RABBITMQ_HOST
RABBITMQ_PORT
RABBITMQ_USERNAME
RABBITMQ_PASSWORD
RABBITMQ_VIRTUAL_HOST
```

### Variables to ADD (or UPDATE if they exist):
```
RabbitMq__Host=${{RabbitMQ.RAILWAY_PRIVATE_DOMAIN}}
RabbitMq__Port=5672
RabbitMq__Username=${{RabbitMQ.RABBITMQ_DEFAULT_USER}}
RabbitMq__Password=${{RabbitMQ.RABBITMQ_DEFAULT_PASS}}
RabbitMq__VirtualHost=/
```

### Notes:
- The new format uses double underscore (`__`) which .NET Configuration system interprets as nested configuration
- These variables should reference the RabbitMQ service variables using Railway's variable reference syntax `${{ServiceName.VARIABLE_NAME}}`
- This brings client-management-service in line with other services like identity-service and tenant-management-service