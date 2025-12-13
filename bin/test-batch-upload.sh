#!/bin/bash
# Test batch photo upload against running ProfileApi
# Usage: ./bin/test-batch-upload.sh [photo_count]
#
# Prerequisites:
#   - Infrastructure running: ./bin/start-infra.sh
#   - PhotoApi running: dotnet run --project src/SparkLabs.PhotoApi
#   - ProfileApi running: dotnet run --project src/SparkLabs.ProfileApi

set -e

PHOTO_COUNT=${1:-15}
BASE_URL="http://localhost:5001"
USER_ID="11111111-1111-1111-1111-111111111111"
BRAND="kindling"
PHOTO_DIR="test/fixtures/photos"

# Build the curl command with photos
CURL_ARGS=(-X POST "${BASE_URL}/${BRAND}/photos/batch")
CURL_ARGS+=(-H "X-Impersonate-User: ${USER_ID}")

# Add photos to the request (cycle through 15 female photos)
for i in $(seq 1 $PHOTO_COUNT); do
    photo_num=$(( ((i - 1) % 15) + 1 ))
    CURL_ARGS+=(-F "photos=@${PHOTO_DIR}/${photo_num}f.jpg")
done

echo "=== Batch Photo Upload Test ==="
echo "Photos: ${PHOTO_COUNT}"
echo "Brand: ${BRAND}"
echo "User: ${USER_ID}"
echo ""
echo "Starting upload..."
echo ""

# Run the upload and time it
RESPONSE=$(curl "${CURL_ARGS[@]}" -w "\n%{http_code}" -s)
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
JSON_BODY=$(echo "$RESPONSE" | sed '$d')

echo "$JSON_BODY" | jq . 2>/dev/null || echo "$JSON_BODY"
echo ""
echo "HTTP Status: $HTTP_CODE"

echo ""
echo "=== Test Complete ==="
