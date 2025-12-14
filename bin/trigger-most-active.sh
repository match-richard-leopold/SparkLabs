#!/bin/bash
set -e

# Trigger GetMostActiveUsers message to Kafka
# Usage: ./trigger-most-active.sh <brand> [limit]
#
# Brands: 1=Kindling, 2=Spark, 3=Flame
#
# Simulates a CRM system requesting the most active users for reporting.
# The CorrelationId allows the CRM to match the async response to this request.

if [ -z "$1" ]; then
    echo "Usage: ./trigger-most-active.sh <brand> [limit]"
    echo "  brand: 1=Kindling, 2=Spark, 3=Flame"
    echo "  limit: number of users to return (default: 5)"
    exit 1
fi

BRAND_ID=$1
LIMIT=${2:-5}
TOPIC="message-processing"
KAFKA_CONTAINER="sparklabs-kafka"

# Map brand ID to name for display
case $BRAND_ID in
    1) BRAND_NAME="Kindling" ;;
    2) BRAND_NAME="Spark" ;;
    3) BRAND_NAME="Flame" ;;
    *) BRAND_NAME="Unknown" ;;
esac

# Generate a correlation ID (simulating what the CRM would send)
CORRELATION_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')

MESSAGE=$(cat <<EOF
{"CorrelationId":"$CORRELATION_ID","BrandId":$BRAND_ID,"Limit":$LIMIT}
EOF
)

echo "Publishing GetMostActiveUsers message to Kafka..."
echo "  Topic: $TOPIC"
echo "  Brand: $BRAND_NAME ($BRAND_ID)"
echo "  Limit: $LIMIT"
echo "  CorrelationId: $CORRELATION_ID"
echo ""

# Use kafka-console-producer with headers
# The message-type header is required for routing in the worker
docker exec -i $KAFKA_CONTAINER \
  /opt/kafka/bin/kafka-console-producer.sh \
    --bootstrap-server localhost:9092 \
    --topic $TOPIC \
    --property "parse.headers=true" \
    --property "headers.delimiter=:" \
    --property "headers.separator=," \
    --property "headers.key.separator==" \
  <<< "message-type=GetMostActiveUsers:$MESSAGE"

echo ""
echo "Message sent!"
echo "  -> Worker will process and publish MostActiveUsersResult to 'notifications' topic"
echo "  -> Response will include CorrelationId: $CORRELATION_ID"
