#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "🛑 Stopping Kindling infrastructure..."
echo ""

cd "$PROJECT_ROOT/deploy"
docker-compose down

echo ""
echo "✅ Infrastructure stopped."
