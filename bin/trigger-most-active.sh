#!/bin/bash
set -e

# Trigger GetMostActiveUsers message to Kafka
# Usage: ./trigger-most-active.sh [limit]
#
# Simulates a CRM system requesting the most active users for reporting.
# The CorrelationId allows the CRM to match the async response to this request.

LIMIT=${1:-10}
TOPIC="message-processing"
KAFKA_CONTAINER="sparklabs-kafka"

# Generate a correlation ID (simulating what the CRM would send)
CORRELATION_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')

MESSAGE=$(cat <<EOF
{"CorrelationId":"$CORRELATION_ID","Limit":$LIMIT}
EOF
)

echo "Publishing GetMostActiveUsers message to Kafka..."
echo "  Topic: $TOPIC"
echo "  CorrelationId: $CORRELATION_ID"
echo "  Limit: $LIMIT"
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
