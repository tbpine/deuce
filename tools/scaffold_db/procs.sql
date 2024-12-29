DELIMITER //


DROP PROCEDURE IF EXISTS `sp_get_account`//

CREATE PROCEDURE `sp_get_account`(
)
BEGIN

	SELECT `id`,`player`,`club`,`password`,`salt`,`active`,`updated_datetime`,`created_datetime`
	FROM `account`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_account`//

CREATE PROCEDURE `sp_set_account`(
IN p_id INT,
IN p_player INT,
IN p_club INT,
IN p_password VARBINARY(48),
IN p_salt VARBINARY(8),
IN p_active INT)

BEGIN

INSERT INTO `account`(`id`,`player`,`club`,`password`,`salt`,`active`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_player, p_club, p_password, p_salt, p_active, NOW(), NOW())
ON DUPLICATE KEY UPDATE `player` = p_player,`club` = p_club,`password` = p_password,`salt` = p_salt,`active` = p_active,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_address`//

CREATE PROCEDURE `sp_get_address`(
)
BEGIN

	SELECT `id`,`street`,`suburb`,`state`,`country`,`contact`,`email`,`player`,`club`,`tournament`,`updated_datetime`,`created_datetime`
	FROM `address`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_address`//

CREATE PROCEDURE `sp_set_address`(
IN p_id INT,
IN p_street VARCHAR(150),
IN p_suburb VARCHAR(50),
IN p_state VARCHAR(30),
IN p_country VARCHAR(30),
IN p_contact VARCHAR(40),
IN p_email VARCHAR(100),
IN p_player INT,
IN p_club INT,
IN p_tournament INT)

BEGIN

INSERT INTO `address`(`id`,`street`,`suburb`,`state`,`country`,`contact`,`email`,`player`,`club`,`tournament`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_street, p_suburb, p_state, p_country, p_contact, p_email, p_player, p_club, p_tournament, NOW(), NOW())
ON DUPLICATE KEY UPDATE `street` = p_street,`suburb` = p_suburb,`state` = p_state,`country` = p_country,`contact` = p_contact,`email` = p_email,`player` = p_player,`club` = p_club,`tournament` = p_tournament,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_club`//

CREATE PROCEDURE `sp_get_club`(
)
BEGIN

	SELECT `id`,`name`,`owner`,`abn`,`active`,`updated_datetime`,`created_datetime`
	FROM `club`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_club`//

CREATE PROCEDURE `sp_set_club`(
IN p_id INT,
IN p_name VARCHAR(300),
IN p_owner VARCHAR(100),
IN p_abn VARCHAR(300),
IN p_active INT)

BEGIN

INSERT INTO `club`(`id`,`name`,`owner`,`abn`,`active`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_name, p_owner, p_abn, p_active, NOW(), NOW())
ON DUPLICATE KEY UPDATE `name` = p_name,`owner` = p_owner,`abn` = p_abn,`active` = p_active,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_entry`//

CREATE PROCEDURE `sp_get_entry`(
)
BEGIN

	SELECT `id`,`tournament`,`player`,`active`
	FROM `entry`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_entry`//

CREATE PROCEDURE `sp_set_entry`(
IN p_id INT,
IN p_tournament INT,
IN p_player INT,
IN p_active INT)

BEGIN

INSERT INTO `entry`(`id`,`tournament`,`player`,`active`) VALUES (p_id, p_tournament, p_player, p_active)
ON DUPLICATE KEY UPDATE `tournament` = p_tournament,`player` = p_player,`active` = p_active;

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_interval`//

CREATE PROCEDURE `sp_get_interval`(
)
BEGIN

	SELECT `id`,`label`
	FROM `interval`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_interval`//

CREATE PROCEDURE `sp_set_interval`(
IN p_id INT,
IN p_label CHAR(20))

BEGIN

INSERT INTO `interval`(`id`,`label`) VALUES (p_id, p_label)
ON DUPLICATE KEY UPDATE `label` = p_label;

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_match`//

CREATE PROCEDURE `sp_get_match`(
)
BEGIN

	SELECT `id`,`player_home`,`player_away`,`round`,`tournament`,`updated_datetime`,`created_datetime`
	FROM `match`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_match`//

CREATE PROCEDURE `sp_set_match`(
IN p_id INT,
IN p_player_home INT,
IN p_player_away INT,
IN p_round INT,
IN p_tournament INT)

BEGIN

INSERT INTO `match`(`id`,`player_home`,`player_away`,`round`,`tournament`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_player_home, p_player_away, p_round, p_tournament, NOW(), NOW())
ON DUPLICATE KEY UPDATE `player_home` = p_player_home,`player_away` = p_player_away,`round` = p_round,`tournament` = p_tournament,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_player`//

CREATE PROCEDURE `sp_get_player`(
)
BEGIN

	SELECT `id`,`first_name`,`last_name`,`utr`,`updated_datetime`,`created_datetime`
	FROM `player`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_player`//

CREATE PROCEDURE `sp_set_player`(
IN p_id INT,
IN p_first_name VARCHAR(100),
IN p_last_name VARCHAR(100),
IN p_utr DECIMAL(6,2))

BEGIN

INSERT INTO `player`(`id`,`first_name`,`last_name`,`utr`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_first_name, p_last_name, p_utr, NOW(), NOW())
ON DUPLICATE KEY UPDATE `first_name` = p_first_name,`last_name` = p_last_name,`utr` = p_utr,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_result`//

CREATE PROCEDURE `sp_get_result`(
)
BEGIN

	SELECT `id`,`tournament`,`player_1`,`player_2`,`score`,`updated_datetime`,`created_datetime`
	FROM `result`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_result`//

CREATE PROCEDURE `sp_set_result`(
IN p_id INT,
IN p_tournament INT,
IN p_player_1 INT,
IN p_player_2 INT,
IN p_score VARCHAR(300))

BEGIN

INSERT INTO `result`(`id`,`tournament`,`player_1`,`player_2`,`score`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_tournament, p_player_1, p_player_2, p_score, NOW(), NOW())
ON DUPLICATE KEY UPDATE `tournament` = p_tournament,`player_1` = p_player_1,`player_2` = p_player_2,`score` = p_score,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_round`//

CREATE PROCEDURE `sp_get_round`(
)
BEGIN

	SELECT `id`,`index`,`team_home`,`team_away`,`tournament`,`updated_datetime`,`created_datetime`
	FROM `round`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_round`//

CREATE PROCEDURE `sp_set_round`(
IN p_id INT,
IN p_index INT,
IN p_team_home INT,
IN p_team_away VARCHAR(300),
IN p_tournament INT)

BEGIN

INSERT INTO `round`(`id`,`index`,`team_home`,`team_away`,`tournament`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_index, p_team_home, p_team_away, p_tournament, NOW(), NOW())
ON DUPLICATE KEY UPDATE `index` = p_index,`team_home` = p_team_home,`team_away` = p_team_away,`tournament` = p_tournament,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_team`//

CREATE PROCEDURE `sp_get_team`(
)
BEGIN

	SELECT `id`,`label`,`updated_datetime`,`created_datetime`
	FROM `team`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_team`//

CREATE PROCEDURE `sp_set_team`(
IN p_id INT,
IN p_label INT)

BEGIN

INSERT INTO `team`(`id`,`label`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_label, NOW(), NOW())
ON DUPLICATE KEY UPDATE `label` = p_label,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_team_player`//

CREATE PROCEDURE `sp_get_team_player`(
)
BEGIN

	SELECT `team`,`player`
	FROM `team_player`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_team_player`//

CREATE PROCEDURE `sp_set_team_player`(
IN p_team INT,
IN p_player INT)

BEGIN

INSERT INTO `team_player`(`team`,`player`) VALUES (p_team, p_player)
ON DUPLICATE KEY UPDATE `team` = p_team,`player` = p_player;

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_tournament`//

CREATE PROCEDURE `sp_get_tournament`(
)
BEGIN

	SELECT `id`,`label`,`start`,`end`,`interval`,`steps`,`type`,`max`,`fee`,`prize`,`seedings`,`updated_datetime`,`created_datetime`
	FROM `tournament`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_tournament`//

CREATE PROCEDURE `sp_set_tournament`(
IN p_id INT,
IN p_label VARCHAR(300),
IN p_start DATETIME,
IN p_end DATETIME,
IN p_interval INT,
IN p_steps INT,
IN p_type INT,
IN p_max INT,
IN p_fee DECIMAL(10,2),
IN p_prize DECIMAL(10,2),
IN p_seedings INT)

BEGIN

INSERT INTO `tournament`(`id`,`label`,`start`,`end`,`interval`,`steps`,`type`,`max`,`fee`,`prize`,`seedings`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_label, p_start, p_end, p_interval, p_steps, p_type, p_max, p_fee, p_prize, p_seedings, NOW(), NOW())
ON DUPLICATE KEY UPDATE `label` = p_label,`start` = p_start,`end` = p_end,`interval` = p_interval,`steps` = p_steps,`type` = p_type,`max` = p_max,`fee` = p_fee,`prize` = p_prize,`seedings` = p_seedings,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_tournament_type`//

CREATE PROCEDURE `sp_get_tournament_type`(
)
BEGIN

	SELECT `id`,`label`
	FROM `tournament_type`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_tournament_type`//

CREATE PROCEDURE `sp_set_tournament_type`(
IN p_id INT,
IN p_label CHAR(20))

BEGIN

INSERT INTO `tournament_type`(`id`,`label`) VALUES (p_id, p_label)
ON DUPLICATE KEY UPDATE `label` = p_label;

SELECT LAST_INSERT_ID() 'id';

END//

DELIMITER ;


