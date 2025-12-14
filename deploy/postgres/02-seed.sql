-- SparkLabs Seed Data
-- Test users and interactions for development/interview scenarios

-- ============================================================================
-- SEED USERS
-- ============================================================================

-- Kindling users (Brand 1 - Astrology)
INSERT INTO user_profiles (id, brand_id, email, display_name, date_of_birth, bio, location, gender, seek_gender, created_at, updated_at, last_active_at, is_active) VALUES
('11111111-1111-1111-1111-111111111111', 1, 'luna@kindling.example', 'Luna', '1995-03-21', 'Aries sun, Scorpio rising. Looking for cosmic connections.', 'Los Angeles, CA', 2, 1, NOW() - INTERVAL '30 days', NOW() - INTERVAL '1 hour', NOW() - INTERVAL '1 hour', TRUE),
('11111111-1111-1111-1111-111111111112', 1, 'orion@kindling.example', 'Orion', '1992-08-15', 'Leo energy. Love stargazing and deep conversations.', 'Los Angeles, CA', 1, 2, NOW() - INTERVAL '25 days', NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours', TRUE),
('11111111-1111-1111-1111-111111111113', 1, 'celeste@kindling.example', 'Celeste', '1998-12-22', 'Capricorn with a Pisces moon. Creative soul.', 'San Francisco, CA', 2, 1, NOW() - INTERVAL '20 days', NOW() - INTERVAL '3 hours', NOW() - INTERVAL '3 hours', TRUE),
('11111111-1111-1111-1111-111111111114', 1, 'atlas@kindling.example', 'Atlas', '1990-06-10', 'Gemini who loves variety. Mercury retrograde survivor.', 'San Francisco, CA', 1, 2, NOW() - INTERVAL '15 days', NOW() - INTERVAL '5 hours', NOW() - INTERVAL '5 hours', TRUE);

-- Spark users (Brand 2 - Hobbies)
INSERT INTO user_profiles (id, brand_id, email, display_name, date_of_birth, bio, location, gender, seek_gender, created_at, updated_at, last_active_at, is_active) VALUES
('22222222-2222-2222-2222-222222222221', 2, 'alex@spark.example', 'Alex', '1993-04-12', 'Rock climbing, hiking, craft beer enthusiast.', 'Denver, CO', 1, 2, NOW() - INTERVAL '28 days', NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes', TRUE),
('22222222-2222-2222-2222-222222222222', 2, 'jamie@spark.example', 'Jamie', '1996-09-05', 'Weekend warrior. Skiing in winter, kayaking in summer.', 'Denver, CO', 2, 1, NOW() - INTERVAL '22 days', NOW() - INTERVAL '1 hour', NOW() - INTERVAL '1 hour', TRUE),
('22222222-2222-2222-2222-222222222223', 2, 'morgan@spark.example', 'Morgan', '1991-01-30', 'Board game nights and coffee shop hopping.', 'Seattle, WA', 3, 3, NOW() - INTERVAL '18 days', NOW() - INTERVAL '4 hours', NOW() - INTERVAL '4 hours', TRUE),
('22222222-2222-2222-2222-222222222224', 2, 'taylor@spark.example', 'Taylor', '1994-07-22', 'Marathon runner, amateur chef, dog parent.', 'Seattle, WA', 2, 1, NOW() - INTERVAL '12 days', NOW() - INTERVAL '6 hours', NOW() - INTERVAL '6 hours', TRUE);

-- Flame users (Brand 3 - Lifestyle)
INSERT INTO user_profiles (id, brand_id, email, display_name, date_of_birth, bio, location, gender, seek_gender, created_at, updated_at, last_active_at, is_active) VALUES
('33333333-3333-3333-3333-333333333331', 3, 'jordan@flame.example', 'Jordan', '1988-11-15', 'Ready for something real. Family-oriented.', 'Chicago, IL', 1, 2, NOW() - INTERVAL '35 days', NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours', TRUE),
('33333333-3333-3333-3333-333333333332', 3, 'casey@flame.example', 'Casey', '1990-05-28', 'Looking for my person. Love kids, have two cats.', 'Chicago, IL', 2, 1, NOW() - INTERVAL '30 days', NOW() - INTERVAL '45 minutes', NOW() - INTERVAL '45 minutes', TRUE),
('33333333-3333-3333-3333-333333333333', 3, 'riley@flame.example', 'Riley', '1987-02-14', 'Divorced, one kid. Looking for a partner in life.', 'Austin, TX', 1, 2, NOW() - INTERVAL '40 days', NOW() - INTERVAL '3 hours', NOW() - INTERVAL '3 hours', TRUE),
('33333333-3333-3333-3333-333333333334', 3, 'sam@flame.example', 'Sam', '1992-10-08', 'Career-focused but ready to settle down.', 'Austin, TX', 2, 1, NOW() - INTERVAL '25 days', NOW() - INTERVAL '1 hour', NOW() - INTERVAL '1 hour', TRUE);

-- ============================================================================
-- SEED INTERACTIONS
-- ============================================================================

-- Kindling interactions (today)
INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp) VALUES
-- Luna likes Orion, Orion likes Luna = MutualMatch
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa001', '11111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111112', 1, 1, NOW() - INTERVAL '5 hours'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002', '11111111-1111-1111-1111-111111111112', '11111111-1111-1111-1111-111111111111', 1, 1, NOW() - INTERVAL '4 hours'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003', '11111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111112', 3, 1, NOW() - INTERVAL '4 hours'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa004', '11111111-1111-1111-1111-111111111112', '11111111-1111-1111-1111-111111111111', 3, 1, NOW() - INTERVAL '4 hours'),
-- Luna passes on Atlas
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005', '11111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111114', 2, 1, NOW() - INTERVAL '3 hours'),
-- Celeste likes Orion (no match yet)
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa006', '11111111-1111-1111-1111-111111111113', '11111111-1111-1111-1111-111111111112', 1, 1, NOW() - INTERVAL '2 hours');

-- Spark interactions (today)
INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp) VALUES
-- Alex and Jamie mutual match
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', '22222222-2222-2222-2222-222222222221', '22222222-2222-2222-2222-222222222222', 1, 2, NOW() - INTERVAL '6 hours'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222221', 1, 2, NOW() - INTERVAL '5 hours'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', '22222222-2222-2222-2222-222222222221', '22222222-2222-2222-2222-222222222222', 3, 2, NOW() - INTERVAL '5 hours'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb004', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222221', 3, 2, NOW() - INTERVAL '5 hours'),
-- Morgan likes Taylor
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb005', '22222222-2222-2222-2222-222222222223', '22222222-2222-2222-2222-222222222224', 1, 2, NOW() - INTERVAL '3 hours'),
-- Alex passes on Morgan
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb006', '22222222-2222-2222-2222-222222222221', '22222222-2222-2222-2222-222222222223', 2, 2, NOW() - INTERVAL '2 hours'),
-- Taylor very active today
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb007', '22222222-2222-2222-2222-222222222224', '22222222-2222-2222-2222-222222222221', 1, 2, NOW() - INTERVAL '1 hour'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb008', '22222222-2222-2222-2222-222222222224', '22222222-2222-2222-2222-222222222223', 1, 2, NOW() - INTERVAL '45 minutes');

-- Flame interactions (today)
INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp) VALUES
-- Jordan and Casey mutual match
('cccccccc-cccc-cccc-cccc-ccccccccc001', '33333333-3333-3333-3333-333333333331', '33333333-3333-3333-3333-333333333332', 1, 3, NOW() - INTERVAL '8 hours'),
('cccccccc-cccc-cccc-cccc-ccccccccc002', '33333333-3333-3333-3333-333333333332', '33333333-3333-3333-3333-333333333331', 1, 3, NOW() - INTERVAL '7 hours'),
('cccccccc-cccc-cccc-cccc-ccccccccc003', '33333333-3333-3333-3333-333333333331', '33333333-3333-3333-3333-333333333332', 3, 3, NOW() - INTERVAL '7 hours'),
('cccccccc-cccc-cccc-cccc-ccccccccc004', '33333333-3333-3333-3333-333333333332', '33333333-3333-3333-3333-333333333331', 3, 3, NOW() - INTERVAL '7 hours'),
-- Riley likes Sam, Sam passes
('cccccccc-cccc-cccc-cccc-ccccccccc005', '33333333-3333-3333-3333-333333333333', '33333333-3333-3333-3333-333333333334', 1, 3, NOW() - INTERVAL '4 hours'),
('cccccccc-cccc-cccc-cccc-ccccccccc006', '33333333-3333-3333-3333-333333333334', '33333333-3333-3333-3333-333333333333', 2, 3, NOW() - INTERVAL '3 hours');

-- ============================================================================
-- ADDITIONAL INTERACTIONS FOR "MOST ACTIVE USERS" EXERCISE
-- ============================================================================
-- This data ensures meaningful results for the GetMostActiveUsers query.
-- Activity counts after seeding (by brand):
--
-- Brand 1 (Kindling):
--   Luna:    12 interactions
--   Orion:    5 interactions
--   Celeste:  1 interaction
--
-- Brand 2 (Spark):
--   Alex:     8 interactions
--   Taylor:   4 interactions
--   Jamie:    2 interactions
--   Morgan:   1 interaction
--
-- Brand 3 (Flame):
--   Jordan:   4 interactions
--   Casey:    2 interactions
--   Riley:    1 interaction
--   Sam:      1 interaction

-- Luna's additional activity (Kindling power user)
INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp) VALUES
('dddddddd-dddd-dddd-dddd-ddddddddd001', '11111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111113', 1, 1, NOW() - INTERVAL '6 hours'),
('dddddddd-dddd-dddd-dddd-ddddddddd002', '11111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111113', 3, 1, NOW() - INTERVAL '6 hours'),
('dddddddd-dddd-dddd-dddd-ddddddddd003', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222221', 1, 1, NOW() - INTERVAL '5 hours'),
('dddddddd-dddd-dddd-dddd-ddddddddd004', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', 1, 1, NOW() - INTERVAL '4 hours'),
('dddddddd-dddd-dddd-dddd-ddddddddd005', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222223', 2, 1, NOW() - INTERVAL '3 hours'),
('dddddddd-dddd-dddd-dddd-ddddddddd006', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222224', 1, 1, NOW() - INTERVAL '2 hours'),
('dddddddd-dddd-dddd-dddd-ddddddddd007', '11111111-1111-1111-1111-111111111111', '33333333-3333-3333-3333-333333333331', 1, 1, NOW() - INTERVAL '1 hour'),
('dddddddd-dddd-dddd-dddd-ddddddddd008', '11111111-1111-1111-1111-111111111111', '33333333-3333-3333-3333-333333333332', 2, 1, NOW() - INTERVAL '30 minutes'),
('dddddddd-dddd-dddd-dddd-ddddddddd009', '11111111-1111-1111-1111-111111111111', '33333333-3333-3333-3333-333333333333', 1, 1, NOW() - INTERVAL '15 minutes');

-- Alex's additional activity (Spark power user)
INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp) VALUES
('eeeeeeee-eeee-eeee-eeee-eeeeeeeee001', '22222222-2222-2222-2222-222222222221', '11111111-1111-1111-1111-111111111111', 1, 2, NOW() - INTERVAL '7 hours'),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeee002', '22222222-2222-2222-2222-222222222221', '11111111-1111-1111-1111-111111111112', 1, 2, NOW() - INTERVAL '6 hours'),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeee003', '22222222-2222-2222-2222-222222222221', '11111111-1111-1111-1111-111111111113', 2, 2, NOW() - INTERVAL '4 hours'),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeee004', '22222222-2222-2222-2222-222222222221', '33333333-3333-3333-3333-333333333331', 1, 2, NOW() - INTERVAL '2 hours'),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeee005', '22222222-2222-2222-2222-222222222221', '33333333-3333-3333-3333-333333333332', 1, 2, NOW() - INTERVAL '1 hour');

-- Taylor's additional activity
INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp) VALUES
('ffffffff-ffff-ffff-ffff-fffffffffff1', '22222222-2222-2222-2222-222222222224', '11111111-1111-1111-1111-111111111111', 1, 2, NOW() - INTERVAL '8 hours'),
('ffffffff-ffff-ffff-ffff-fffffffffff2', '22222222-2222-2222-2222-222222222224', '11111111-1111-1111-1111-111111111112', 2, 2, NOW() - INTERVAL '6 hours');

-- Orion's additional activity
INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp) VALUES
('00000000-0000-0000-0000-000000000a01', '11111111-1111-1111-1111-111111111112', '22222222-2222-2222-2222-222222222221', 1, 1, NOW() - INTERVAL '5 hours'),
('00000000-0000-0000-0000-000000000a02', '11111111-1111-1111-1111-111111111112', '22222222-2222-2222-2222-222222222222', 2, 1, NOW() - INTERVAL '3 hours'),
('00000000-0000-0000-0000-000000000a03', '11111111-1111-1111-1111-111111111112', '33333333-3333-3333-3333-333333333332', 1, 1, NOW() - INTERVAL '1 hour');

-- Jordan's additional activity
INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp) VALUES
('00000000-0000-0000-0000-000000000b01', '33333333-3333-3333-3333-333333333331', '11111111-1111-1111-1111-111111111111', 1, 3, NOW() - INTERVAL '4 hours'),
('00000000-0000-0000-0000-000000000b02', '33333333-3333-3333-3333-333333333331', '22222222-2222-2222-2222-222222222221', 2, 3, NOW() - INTERVAL '2 hours');
