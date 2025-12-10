# SparkLabs

> "Can't start a fire without a spark"

A fictional web-scale dating app backend for technical interview scenarios. SparkLabs is the parent company with multiple brands: Kindling, Spark, Flame.

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
                                              │
                        ┌─────────────────────┘
                        ▼
                 ┌────────────┐
                 │   Kafka    │
                 │   :9092    │
                 └────────────┘
```

## Quick Start

### Prerequisites

- .NET 9 SDK
- Docker & Docker Compose

### Start Infrastructure

```bash
./bin/start-infra.sh
```

Or manually:
```bash
cd deploy
docker-compose up -d
```

This starts:
- **PostgreSQL** on `localhost:5432` (user: `sparklabs`, password: `sparklabs`)
- **LocalStack** on `localhost:4566` (S3 bucket: `sparklabs-photos`)
- **Kafka** on `localhost:9092` (topic: `message-processing`)
- **Aspire Dashboard** on `http://localhost:18888`

### Stop Infrastructure

```bash
./bin/stop-infra.sh
```

### Run the APIs

```bash
# Terminal 1 - Profile API
cd src/SparkLabs.ProfileApi
dotnet run

# Terminal 2 - Photo API
cd src/SparkLabs.PhotoApi
dotnet run
```

### Verify Setup

- Aspire Dashboard: http://localhost:18888
- Profile API Swagger: http://localhost:5001/swagger
- Photo API Swagger: http://localhost:5002/swagger

## Project Structure

```
SparkLabs/
├── lib/
│   └── SparkLabs.Common/            # Shared DTOs, contracts, telemetry
├── src/
│   ├── SparkLabs.ProfileApi/        # User profiles (PostgreSQL)
│   └── SparkLabs.PhotoApi/          # Photo management (S3)
├── test/
│   ├── SparkLabs.ProfileApi.Tests/
│   └── SparkLabs.PhotoApi.Tests/
├── bin/                             # Utility scripts
├── deploy/
│   ├── docker-compose.yml
│   └── localstack-init/             # S3/DynamoDB bootstrap
└── SparkLabs.sln
```

## Services

### Profile API

Manages user profiles with PostgreSQL storage (using Dapper).

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

- **S3 Bucket**: `sparklabs-photos`
- **DynamoDB Table**: `PhotoMetadata` (UserId/PhotoId keys)

Access via AWS CLI:
```bash
aws --endpoint-url=http://localhost:4566 s3 ls s3://sparklabs-photos
aws --endpoint-url=http://localhost:4566 dynamodb scan --table-name PhotoMetadata
```

## Telemetry

All services export OTLP telemetry to the Aspire Dashboard:

- **Traces**: Distributed tracing across API calls
- **Logs**: Structured logging from all services
- **Metrics**: Request counts, latencies, etc.

View at: http://localhost:18888
