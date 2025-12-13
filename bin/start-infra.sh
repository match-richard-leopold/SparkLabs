#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_ROOT/deploy"

# Handle --clean flag
if [ "$1" = "--clean" ]; then
    echo "🧹 Cleaning up volumes (fresh start)..."
    docker-compose down -v 2>/dev/null || true
    echo ""
fi

echo "🔥 Starting SparkLabs infrastructure..."
echo ""

docker-compose up -d

echo ""
echo "⏳ Waiting for services to be healthy..."

# Wait for healthy status (max 90 seconds - Kafka takes longer)
TIMEOUT=90
ELAPSED=0
while [ $ELAPSED -lt $TIMEOUT ]; do
    HEALTHY=$(docker ps --filter "name=sparklabs" --filter "status=running" --format "{{.Status}}" | grep -c "healthy" || true)
    if [ "$HEALTHY" -eq 4 ]; then
        break
    fi
    sleep 2
    ELAPSED=$((ELAPSED + 2))
    echo "  ...waiting ($ELAPSED/${TIMEOUT}s) - $HEALTHY/4 healthy"
done

if [ "$HEALTHY" -ne 4 ]; then
    echo "❌ Timeout waiting for services to be healthy"
    docker ps --filter "name=sparklabs" --format "table {{.Names}}\t{{.Status}}"
    exit 1
fi

echo ""
echo "✅ Infrastructure ready!"
echo ""
echo "┌──────────────────────────────────────────────────────────────┐"
echo "│  SparkLabs - \"Can't start a fire without a spark\"          │"
echo "├──────────────────────────────────────────────────────────────┤"
echo "│  OTEL Dashboard     │  http://localhost:18888               │"
echo "│  PostgreSQL         │  localhost:5432 (sparklabs/sparklabs) │"
echo "│  Kafka              │  localhost:9092                       │"
echo "│    - Topic          │  message-processing                   │"
echo "│  LocalStack         │  http://localhost:4566                │"
echo "│    - S3 Bucket      │  sparklabs-photos                     │"
echo "│    - DynamoDB Table │  PhotoMetadata                        │"
echo "│  OTLP Endpoint      │  http://localhost:4317                │"
echo "└──────────────────────────────────────────────────────────────┘"
echo ""
