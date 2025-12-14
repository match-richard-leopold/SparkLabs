# SparkLabs

> "Can't start a fire without a spark"

A fictional web-scale dating app backend for technical interview scenarios. SparkLabs is the parent company with multiple brands: **Kindling** (astrology-based), **Spark** (hobby-based), and **Flame** (lifestyle-based).

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Aspire Dashboard                           │
│                (Traces, Logs, Metrics - OTEL)                   │
│                   http://localhost:18888                        │
└─────────────────────────────────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
┌──────────────┐        ┌──────────────┐        ┌──────────────┐
│  Profile API │───────▶│  Photo API   │        │    Worker    │
│    :5001     │        │    :5002     │        │   (Kafka)    │
└──────────────┘        └──────────────┘        └──────────────┘
        │                       │                       │
        │       ┌───────────────┤                       │
        ▼       ▼               ▼                       ▼
┌──────────────┐        ┌────────────┐          ┌────────────┐
│  PostgreSQL  │        │ LocalStack │          │   Kafka    │
│    :5432     │        │   :4566    │          │   :9092    │
│  (profiles)  │        │ (S3, Dyn.) │          └────────────┘
└──────────────┘        └────────────┘

        ┌ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐
          External Third-Party Service
        │                                     │
          ┌─────────────────────────────┐
        │ │     Moderation Service      │     │
          │          :5003              │
        │ └─────────────────────────────┘     │
        └ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┘
```

## Quick Start

### Prerequisites

- .NET 9 SDK
- Docker & Docker Compose

### Start Infrastructure

```bash
./bin/start-infra.sh
```

This starts:
- **PostgreSQL** on `localhost:5432` (user: `sparklabs`, password: `sparklabs`)
- **LocalStack** on `localhost:4566` (S3 + DynamoDB)
- **Kafka** on `localhost:9092` (topics: `message-processing`, `notifications`)
- **Aspire Dashboard** on `http://localhost:18888`
- **Moderation Service** on `localhost:5003` (external service simulation)

### Stop Infrastructure

```bash
./bin/stop-infra.sh
```

### Start All Services

```bash
./bin/start-services.sh
```

This starts ProfileApi, PhotoApi, and Worker with PIDs displayed for debugger attachment.

Or run individually:
```bash
dotnet run --project src/SparkLabs.ProfileApi
dotnet run --project src/SparkLabs.PhotoApi
dotnet run --project src/SparkLabs.Worker
```

### Verify Setup

- Aspire Dashboard: http://localhost:18888
- Profile API Swagger: http://localhost:5001/swagger
- Photo API Swagger: http://localhost:5002/swagger

## Project Structure

```
SparkLabs/
├── lib/
│   └── SparkLabs.Common/            # Shared code, clients, services
├── src/
│   ├── SparkLabs.ProfileApi/        # User profiles, photos, interactions
│   ├── SparkLabs.PhotoApi/          # Photo storage (S3 + DynamoDB)
│   └── SparkLabs.Worker/            # Kafka message consumer
├── test/
│   ├── SparkLabs.ProfileApi.Tests/
│   └── SparkLabs.PhotoApi.Tests/
├── bin/                             # Utility scripts
│   ├── start-infra.sh
│   ├── stop-infra.sh
│   ├── start-services.sh
│   └── test-batch-upload.sh
├── deploy/
│   ├── docker-compose.yml
│   ├── localstack-init/             # S3/DynamoDB bootstrap
│   ├── moderation-service/          # External service simulation
│   └── postgres/                    # Schema and seed data
└── SparkLabs.sln
```

## Services

### Profile API (:5001)

Manages user profiles, photo uploads, and interactions. Routes are brand-specific.

| Endpoint | Description |
|----------|-------------|
| `GET /{brandId}/profiles/me` | Get current user's profile |
| `GET /{brandId}/profiles/{id}` | Get profile by ID |
| `GET /{brandId}/profiles` | List profiles (discovery) |
| `POST /{brandId}/profiles` | Create profile |
| `PUT /{brandId}/profiles/me` | Update current user's profile |
| `DELETE /{brandId}/profiles/me` | Delete current user's profile |
| `POST /{brandId}/photos` | Upload single photo |
| `POST /{brandId}/photos/batch` | Upload multiple photos |
| `GET /{brandId}/photos` | Get current user's photos |
| `DELETE /{brandId}/photos/{photoId}` | Delete photo |
| `POST /interactions/like/{targetUserId}` | Like a user |
| `POST /interactions/pass/{targetUserId}` | Pass on a user |
| `GET /interactions/matches` | Get matches |
| `GET /interactions/history` | Get interaction history |

Brands: `kindling`, `spark`, `flame`

### Photo API (:5002)

Internal service for photo storage. Uses S3 for blobs, DynamoDB for metadata.

| Endpoint | Description |
|----------|-------------|
| `PUT /{brandId}/users/{userId}/photos` | Upload photo (requires moderation signature) |
| `GET /{brandId}/users/{userId}/photos` | List user's photos |
| `GET /{brandId}/photos/{photoId}` | Get photo bytes |
| `DELETE /{brandId}/photos/{photoId}` | Soft delete photo |

### Worker

Background service consuming Kafka messages for async processing.

### Moderation Service (:5003)

**This is an external third-party service and should be treated as a complete black box.** It simulates a rate-limited content moderation API that all photos must pass through before upload.

- ~400ms latency per request
- Rate limited (returns 429 if overloaded)
- Returns a signature required by PhotoApi

## LocalStack Resources

Pre-created by `localstack-init/init-aws.sh`:

**S3:**
- `sparklabs-photos` - Photo blob storage

**DynamoDB Tables:**
- `PhotoMetadata` - Photo metadata (pk: `{brandId}#{userId}`, sk: `{photoId}`)
- `KindlingExtensions` - Kindling brand profile extensions
- `SparkExtensions` - Spark brand profile extensions
- `FlameExtensions` - Flame brand profile extensions

Access via AWS CLI:
```bash
aws --endpoint-url=http://localhost:4566 s3 ls s3://sparklabs-photos
aws --endpoint-url=http://localhost:4566 dynamodb scan --table-name PhotoMetadata
```

## Testing

### Batch Photo Upload

```bash
./bin/test-batch-upload.sh [count]   # Default: 15 photos
```

### Run Unit Tests

```bash
dotnet test
```

## Telemetry

All services export OTLP telemetry to the Aspire Dashboard:

- **Traces**: Distributed tracing across API calls
- **Logs**: Structured logging from all services
- **Metrics**: Request counts, latencies, etc.

View at: http://localhost:18888

## Development

### User Impersonation

In development, use the `X-Impersonate-User` header to act as a specific user. This is a substitute for a proper authentication flow (OAuth, JWT, etc.) to simplify local development and testing.

```bash
curl -H "X-Impersonate-User: 11111111-1111-1111-1111-111111111111" \
  http://localhost:5001/kindling/profiles/me
```
