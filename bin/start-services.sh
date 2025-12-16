#!/bin/bash
# Start all .NET services
# Usage: ./bin/start-services.sh
#
# For debugging: use Rider's Run/Debug configurations instead
# Press Ctrl+C to stop all services

set -e

echo "=== Starting SparkLabs Services ==="
echo ""

# Trap Ctrl+C to kill all background processes
cleanup() {
    echo ""
    echo "Stopping services..."
    jobs -p | xargs -r kill 2>/dev/null || true
    echo "All services stopped."
}
trap cleanup EXIT INT TERM

echo "Starting PhotoApi on port 5002..."
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="http://localhost:5002" dotnet run --project src/SparkLabs.PhotoApi --no-launch-profile &

echo "Starting ProfileApi on port 5001..."
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="http://localhost:5001" dotnet run --project src/SparkLabs.ProfileApi --no-launch-profile &

echo "Starting Worker..."
DOTNET_ENVIRONMENT=Development dotnet run --project src/SparkLabs.Worker --no-launch-profile &

sleep 3

echo ""
echo "=== All Services Started ==="
echo ""
echo "Process IDs (for debugger attachment):"
echo "  PhotoApi:   $(pgrep -f 'bin/Debug.*SparkLabs.PhotoApi$' | head -1)"
echo "  ProfileApi: $(pgrep -f 'bin/Debug.*SparkLabs.ProfileApi$' | head -1)"
echo "  Worker:     $(pgrep -f 'bin/Debug.*SparkLabs.Worker$' | head -1)"
echo ""
echo "Endpoints:"
echo "  ProfileApi: http://localhost:5001"
echo "  PhotoApi:   http://localhost:5002"
echo "  Swagger:    http://localhost:5001/swagger"
echo ""
echo "To attach debugger:"
echo "  Rider:   Run -> Attach to Process"
echo "  VS Code: Run -> Attach to .NET Process (or use PID)"
echo ""
echo "Press Ctrl+C to stop all services"

wait
