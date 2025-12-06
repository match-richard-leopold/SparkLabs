# Kindling

> "Can't start a fire without a spark"

A fictional web-scale dating app backend for technical interview scenarios.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Aspire Dashboard                           │
│                (Traces, Logs, Metrics - OTEL)                   │
│                   http://localhost:18888                        │
└─────────────────────────────────────────────────────────────────┘
                                │
                ┌───────────────┴───────────────┐
                ▼                               ▼
        ┌──────────────┐               ┌──────────────┐
        │  Profile API │               │  Photo API   │
        │  :5001       │               │  :5002       │
        └──────────────┘               └──────────────┘
                │                             │
                ▼                             ▼
        ┌──────────────┐               ┌────────────┐
        │  PostgreSQL  │               │ LocalStack │
        │  :5432       │               │   :4566    │
        │  (profiles)  │               │  (S3, etc) │
        └──────────────┘               └────────────┘
```

## Quick Start

### Prerequisites

- .NET 8 SDK
- Docker & Docker Compose

### Start Infrastructure

```bash
cd deploy
docker-compose up -d
```

This starts:
- **PostgreSQL** on `localhost:5432` (user: `kindling`, password: `kindling`)
- **LocalStack** on `localhost:4566` (S3 bucket: `kindling-photos`)
- **Aspire Dashboard** on `http://localhost:18888`

### Run the APIs

```bash
# Terminal 1 - Profile API
cd src/Kindling.ProfileApi
dotnet run

# Terminal 2 - Photo API
cd src/Kindling.PhotoApi
dotnet run
```

### Verify Setup

- Aspire Dashboard: http://localhost:18888
- Profile API Swagger: http://localhost:5001/swagger
- Photo API Swagger: http://localhost:5002/swagger

## Project Structure

```
Kindling/
├── lib/
│   └── Kindling.Common/           # Shared DTOs, contracts
├── src/
│   ├── Kindling.ProfileApi/       # User profiles (PostgreSQL)
│   └── Kindling.PhotoApi/         # Photo management (S3)
├── test/
│   ├── Kindling.ProfileApi.Tests/
│   └── Kindling.PhotoApi.Tests/
├── bin/                           # Utility scripts
├── deploy/
│   ├── docker-compose.yml
│   └── localstack-init/           # S3/DynamoDB bootstrap
└── Kindling.sln
```

## Services

### Profile API

Manages user profiles with PostgreSQL storage.

| Endpoint | Description |
|----------|-------------|
| `GET /profiles/{id}` | Get user profile |
| `POST /profiles` | Create profile |
| `PUT /profiles/{id}` | Update profile |
| `DELETE /profiles/{id}` | Delete profile |

### Photo API

Manages photo uploads with S3 storage.

| Endpoint | Description |
|----------|-------------|
| `POST /photos/upload-url` | Get pre-signed upload URL |
| `POST /photos/complete` | Mark upload complete |
| `GET /photos/{userId}` | Get user's photos |
| `DELETE /photos/{id}` | Delete photo |

## LocalStack Resources

Pre-created by `localstack-init/init-aws.sh`:

- **S3 Bucket**: `kindling-photos`
- **DynamoDB Table**: `PhotoMetadata` (UserId/PhotoId keys)

Access via AWS CLI:
```bash
aws --endpoint-url=http://localhost:4566 s3 ls s3://kindling-photos
aws --endpoint-url=http://localhost:4566 dynamodb scan --table-name PhotoMetadata
```

## Telemetry

All services export OTLP telemetry to the Aspire Dashboard:

- **Traces**: Distributed tracing across API calls
- **Logs**: Structured logging from all services
- **Metrics**: Request counts, latencies, etc.

View at: http://localhost:18888
