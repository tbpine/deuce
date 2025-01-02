-- ========================================================
-- deuce data
-- Dec 2024
-- ========================================================
TRUNCATE `tournament_type`;
INSERT INTO `tournament_type` VALUES (0, "Unknown", "category","unknown","");
INSERT INTO `tournament_type` VALUES (1, "Round Robin", "category","rr","");
INSERT INTO `tournament_type` VALUES (2, "KnockOut", "category","ko","");

TRUNCATE `interval`;
INSERT INTO `interval` VALUES (0, "Unknown");
INSERT INTO `interval` VALUES (1, "Minute");
INSERT INTO `interval` VALUES (2, "Hourly");
INSERT INTO `interval` VALUES (3, "Daily");
INSERT INTO `interval` VALUES (4, "Weekly");
INSERT INTO `interval` VALUES (5, "Fortnightly");
INSERT INTO `interval` VALUES (6, "Monthly");
INSERT INTO `interval` VALUES (7, "Annually");

DELETE FROM `organization` WHERE id = 1;
insert into `organization` values (null, 'test_organization', 'tester', null, 1, now(), now());

TRUNCATE `sport`;

INSERT INTO `sport` VALUES (0, "Unknown", "","", "");
INSERT INTO `sport` VALUES (1, "Tennis", "type", "tennis", "assets/icons/tennis.svg");
INSERT INTO `sport` VALUES (2, "Martial Arts","type", "ma","assets/icons/martial_arts.svg");
INSERT INTO `sport` VALUES (3, "Badminton", "type", "badminton","assets/icons/tennis.svg");
INSERT INTO `sport` VALUES (4, "Table Tennis", "type", "table_tennis","assets/icons/tennis.svg");


