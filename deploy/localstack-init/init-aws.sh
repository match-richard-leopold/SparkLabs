#!/bin/bash

echo "Initializing LocalStack resources..."

# =============================================================================
# S3
# =============================================================================

awslocal s3 mb s3://sparklabs-photos
echo "Created S3 bucket: sparklabs-photos"

# =============================================================================
# DynamoDB - Photo Metadata
# pk = {brandId}#{userId}, sk = {photoId}
# =============================================================================

awslocal dynamodb create-table \
    --table-name PhotoMetadata \
    --attribute-definitions \
        AttributeName=pk,AttributeType=S \
        AttributeName=sk,AttributeType=S \
    --key-schema \
        AttributeName=pk,KeyType=HASH \
        AttributeName=sk,KeyType=RANGE \
    --billing-mode PAY_PER_REQUEST

echo "Created DynamoDB table: PhotoMetadata"

# =============================================================================
# DynamoDB - Brand Extensions
# Each brand has its own table - models real-world where brand teams own their
# schema and can iterate independently. Key is UserId (1:1 with core profile).
# =============================================================================

# Kindling - Astrology-focused dating
awslocal dynamodb create-table \
    --table-name KindlingExtensions \
    --attribute-definitions \
        AttributeName=UserId,AttributeType=S \
    --key-schema \
        AttributeName=UserId,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST

echo "Created DynamoDB table: KindlingExtensions"

# Spark - Hobbies & activities focused
awslocal dynamodb create-table \
    --table-name SparkExtensions \
    --attribute-definitions \
        AttributeName=UserId,AttributeType=S \
    --key-schema \
        AttributeName=UserId,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST

echo "Created DynamoDB table: SparkExtensions"

# Flame - Lifestyle & values focused
awslocal dynamodb create-table \
    --table-name FlameExtensions \
    --attribute-definitions \
        AttributeName=UserId,AttributeType=S \
    --key-schema \
        AttributeName=UserId,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST

echo "Created DynamoDB table: FlameExtensions"

# =============================================================================
# Seed Data - Brand Extensions
# =============================================================================

echo "Seeding extension data..."

# Kindling extensions (zodiac data for astrology-focused users)
# ZodiacSign: 1=Aries, 2=Taurus, 3=Gemini, 4=Cancer, 5=Leo, 6=Virgo, 7=Libra, 8=Scorpio, 9=Sagittarius, 10=Capricorn, 11=Aquarius, 12=Pisces

awslocal dynamodb put-item --table-name KindlingExtensions --item '{
  "UserId": {"S": "11111111-1111-1111-1111-111111111111"},
  "SunSign": {"N": "1"},
  "RisingSign": {"N": "8"},
  "MoonSign": {"N": "12"},
  "BelievesInAstrology": {"BOOL": true},
  "CompatibleSigns": {"NS": ["5", "9", "7"]},
  "CreatedAt": {"S": "2024-11-01T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name KindlingExtensions --item '{
  "UserId": {"S": "11111111-1111-1111-1111-111111111112"},
  "SunSign": {"N": "5"},
  "RisingSign": {"N": "1"},
  "MoonSign": {"N": "7"},
  "BelievesInAstrology": {"BOOL": true},
  "CompatibleSigns": {"NS": ["1", "9", "3"]},
  "CreatedAt": {"S": "2024-11-05T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name KindlingExtensions --item '{
  "UserId": {"S": "11111111-1111-1111-1111-111111111113"},
  "SunSign": {"N": "10"},
  "RisingSign": {"N": "6"},
  "MoonSign": {"N": "12"},
  "BelievesInAstrology": {"BOOL": true},
  "CompatibleSigns": {"NS": ["2", "6", "8"]},
  "CreatedAt": {"S": "2024-11-10T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name KindlingExtensions --item '{
  "UserId": {"S": "11111111-1111-1111-1111-111111111114"},
  "SunSign": {"N": "3"},
  "RisingSign": {"N": "11"},
  "MoonSign": {"N": "7"},
  "BelievesInAstrology": {"BOOL": false},
  "CompatibleSigns": {"NS": ["7", "11", "1"]},
  "CreatedAt": {"S": "2024-11-15T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

echo "Seeded KindlingExtensions"

# Spark extensions (hobbies/activities data)
# ActivityLevel: 1=Sedentary, 2=LightlyActive, 3=ModeratelyActive, 4=VeryActive, 5=ExtremelyActive
# WeekendStyle: 1=Homebody, 2=Adventurer, 3=Social, 4=Mixed

awslocal dynamodb put-item --table-name SparkExtensions --item '{
  "UserId": {"S": "22222222-2222-2222-2222-222222222221"},
  "Hobbies": {"SS": ["rock climbing", "hiking", "craft beer"]},
  "ActivityLevel": {"N": "5"},
  "WeekendStyle": {"N": "2"},
  "FavoriteActivities": {"SS": ["bouldering", "14ers", "brewery tours"]},
  "OpenToNewHobbies": {"BOOL": true},
  "CreatedAt": {"S": "2024-11-02T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name SparkExtensions --item '{
  "UserId": {"S": "22222222-2222-2222-2222-222222222222"},
  "Hobbies": {"SS": ["skiing", "kayaking", "yoga"]},
  "ActivityLevel": {"N": "4"},
  "WeekendStyle": {"N": "2"},
  "FavoriteActivities": {"SS": ["backcountry skiing", "whitewater kayaking"]},
  "OpenToNewHobbies": {"BOOL": true},
  "CreatedAt": {"S": "2024-11-08T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name SparkExtensions --item '{
  "UserId": {"S": "22222222-2222-2222-2222-222222222223"},
  "Hobbies": {"SS": ["board games", "coffee", "reading"]},
  "ActivityLevel": {"N": "2"},
  "WeekendStyle": {"N": "3"},
  "FavoriteActivities": {"SS": ["D&D", "coffee shop hopping", "book clubs"]},
  "OpenToNewHobbies": {"BOOL": true},
  "CreatedAt": {"S": "2024-11-12T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name SparkExtensions --item '{
  "UserId": {"S": "22222222-2222-2222-2222-222222222224"},
  "Hobbies": {"SS": ["running", "cooking", "dogs"]},
  "ActivityLevel": {"N": "5"},
  "WeekendStyle": {"N": "4"},
  "FavoriteActivities": {"SS": ["marathon training", "meal prep", "dog park"]},
  "OpenToNewHobbies": {"BOOL": false},
  "CreatedAt": {"S": "2024-11-18T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

echo "Seeded SparkExtensions"

# Flame extensions (lifestyle/values data)
# RelationshipGoal: 1=Casual, 2=LongTerm, 3=Marriage, 4=Undecided
# FamilyPlans: 1=WantsChildren, 2=DoesNotWant, 3=HasChildren, 4=OpenToChildren, 5=Undecided
# PoliticalLeaning: 1=Liberal, 2=Conservative, 3=Moderate, 4=Libertarian, 5=Apolitical, 6=Other
# Religion: 1=Christian, 2=Jewish, 3=Muslim, 4=Hindu, 5=Buddhist, 6=Spiritual, 7=Agnostic, 8=Atheist, 9=Other
# DietaryPreference: 1=NoRestrictions, 2=Vegetarian, 3=Vegan, 4=Pescatarian, 5=Kosher, 6=Halal, 7=GlutenFree, 8=Other

awslocal dynamodb put-item --table-name FlameExtensions --item '{
  "UserId": {"S": "33333333-3333-3333-3333-333333333331"},
  "RelationshipGoal": {"N": "3"},
  "FamilyPlans": {"N": "1"},
  "PoliticalLeaning": {"N": "3"},
  "Religion": {"N": "1"},
  "WantsPets": {"BOOL": true},
  "DietaryPreference": {"N": "1"},
  "CreatedAt": {"S": "2024-10-25T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name FlameExtensions --item '{
  "UserId": {"S": "33333333-3333-3333-3333-333333333332"},
  "RelationshipGoal": {"N": "3"},
  "FamilyPlans": {"N": "4"},
  "PoliticalLeaning": {"N": "1"},
  "Religion": {"N": "6"},
  "WantsPets": {"BOOL": true},
  "DietaryPreference": {"N": "2"},
  "CreatedAt": {"S": "2024-11-01T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name FlameExtensions --item '{
  "UserId": {"S": "33333333-3333-3333-3333-333333333333"},
  "RelationshipGoal": {"N": "2"},
  "FamilyPlans": {"N": "3"},
  "PoliticalLeaning": {"N": "3"},
  "Religion": {"N": "7"},
  "WantsPets": {"BOOL": false},
  "DietaryPreference": {"N": "1"},
  "CreatedAt": {"S": "2024-10-20T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

awslocal dynamodb put-item --table-name FlameExtensions --item '{
  "UserId": {"S": "33333333-3333-3333-3333-333333333334"},
  "RelationshipGoal": {"N": "2"},
  "FamilyPlans": {"N": "5"},
  "PoliticalLeaning": {"N": "1"},
  "Religion": {"N": "8"},
  "WantsPets": {"BOOL": true},
  "DietaryPreference": {"N": "3"},
  "CreatedAt": {"S": "2024-11-05T00:00:00Z"},
  "UpdatedAt": {"S": "2024-12-01T00:00:00Z"}
}'

echo "Seeded FlameExtensions"

echo "LocalStack initialization complete!"
