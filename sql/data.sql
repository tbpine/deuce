-- ========================================================
-- deuce data
-- Dec 2024
-- ========================================================
TRUNCATE `tournament_type`;
INSERT INTO `tournament_type` VALUES (0, "Unknown");
INSERT INTO `tournament_type` VALUES (1, "Round Robin");
INSERT INTO `tournament_type` VALUES (2, "KnockOut");

TRUNCATE `interval`;
INSERT INTO `interval` VALUES (0, "Unknown");
INSERT INTO `interval` VALUES (1, "Minute");
INSERT INTO `interval` VALUES (2, "Hourly");
INSERT INTO `interval` VALUES (3, "Daily");
INSERT INTO `interval` VALUES (4, "Weekly");
INSERT INTO `interval` VALUES (5, "Fortnightly");
INSERT INTO `interval` VALUES (6, "Monthly");
INSERT INTO `interval` VALUES (7, "Annually");