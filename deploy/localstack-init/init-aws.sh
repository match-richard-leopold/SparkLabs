#!/bin/bash

echo "Initializing LocalStack resources..."

# Create S3 bucket for photos
awslocal s3 mb s3://sparklabs-photos
echo "Created S3 bucket: sparklabs-photos"

# Create DynamoDB table for photo metadata (available for candidates to use)
awslocal dynamodb create-table \
    --table-name PhotoMetadata \
    --attribute-definitions \
        AttributeName=UserId,AttributeType=S \
        AttributeName=PhotoId,AttributeType=S \
    --key-schema \
        AttributeName=UserId,KeyType=HASH \
        AttributeName=PhotoId,KeyType=RANGE \
    --billing-mode PAY_PER_REQUEST

echo "Created DynamoDB table: PhotoMetadata"

echo "LocalStack initialization complete!"
