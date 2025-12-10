-- SparkLabs Database Schema
-- Run automatically on first container startup

-- ============================================================================
-- SCHEMA
-- ============================================================================

-- Core user profiles
CREATE TABLE user_profiles (
    id UUID PRIMARY KEY,
    brand_id INT NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    date_of_birth DATE NOT NULL,
    bio TEXT,
    location VARCHAR(100),
    gender INT NOT NULL,
    seek_gender INT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_active_at TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- User interactions (append-only event log)
CREATE TABLE user_interactions (
    event_id UUID PRIMARY KEY,
    from_user_id UUID NOT NULL REFERENCES user_profiles(id),
    to_user_id UUID NOT NULL REFERENCES user_profiles(id),
    interaction_type INT NOT NULL,
    brand_id INT NOT NULL,
    timestamp TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ============================================================================
-- INDEXES
-- ============================================================================

-- Profile lookups
CREATE INDEX idx_profiles_brand ON user_profiles(brand_id);
CREATE INDEX idx_profiles_email ON user_profiles(email);
CREATE INDEX idx_profiles_active ON user_profiles(is_active) WHERE is_active = TRUE;

-- Interaction queries
CREATE INDEX idx_interactions_from_user ON user_interactions(from_user_id, timestamp);
CREATE INDEX idx_interactions_to_user ON user_interactions(to_user_id, timestamp);
CREATE INDEX idx_interactions_timestamp ON user_interactions(timestamp);
CREATE INDEX idx_interactions_type_timestamp ON user_interactions(interaction_type, timestamp);

-- For "has user B liked user A?" check (mutual match detection)
CREATE INDEX idx_interactions_mutual_check ON user_interactions(to_user_id, from_user_id, interaction_type)
    WHERE interaction_type = 1;  -- Like only

-- ============================================================================
-- REFERENCE
-- ============================================================================

-- Brand enum:
--   1 = Kindling (astrology)
--   2 = Spark (hobbies)
--   3 = Flame (lifestyle)

-- Gender enum:
--   1 = Male, 2 = Female, 3 = NonBinary, 4 = Other

-- InteractionType enum:
--   1 = Like, 2 = Pass, 3 = MutualMatch

-- ============================================================================
-- USEFUL QUERIES (for reference/interview)
-- ============================================================================

-- Top 3 most active users for today:
-- SELECT from_user_id, COUNT(*) as activity_count
-- FROM user_interactions
-- WHERE timestamp >= CURRENT_DATE
-- GROUP BY from_user_id
-- ORDER BY activity_count DESC
-- LIMIT 3;

-- Check for mutual match potential:
-- SELECT EXISTS(
--     SELECT 1 FROM user_interactions
--     WHERE from_user_id = :to_user_id
--       AND to_user_id = :from_user_id
--       AND interaction_type = 1
-- );
