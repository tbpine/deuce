DELIMITER //


DROP PROCEDURE IF EXISTS `sp_get_account`//

CREATE PROCEDURE `sp_get_account`(
)
BEGIN

	SELECT `id`,`player`,`organization`,`password`,`salt`,`active`,`updated_datetime`,`created_datetime`
	FROM `account`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_account`//

CREATE PROCEDURE `sp_set_account`(
IN p_id INT,
IN p_player INT,
IN p_organization INT,
IN p_password VARBINARY(48),
IN p_salt VARBINARY(8),
IN p_active INT)

BEGIN

INSERT INTO `account`(`id`,`player`,`organization`,`password`,`salt`,`active`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_player, p_organization, p_password, p_salt, p_active, NOW(), NOW())
ON DUPLICATE KEY UPDATE `player` = p_player,`organization` = p_organization,`password` = p_password,`salt` = p_salt,`active` = p_active,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_address`//

CREATE PROCEDURE `sp_get_address`(
)
BEGIN

	SELECT `id`,`street`,`suburb`,`state`,`country`,`contact`,`email`,`player`,`organization`,`tournament`,`updated_datetime`,`created_datetime`
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
IN p_organization INT,
IN p_tournament INT)

BEGIN

INSERT INTO `address`(`id`,`street`,`suburb`,`state`,`country`,`contact`,`email`,`player`,`organization`,`tournament`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_street, p_suburb, p_state, p_country, p_contact, p_email, p_player, p_organization, p_tournament, NOW(), NOW())
ON DUPLICATE KEY UPDATE `street` = p_street,`suburb` = p_suburb,`state` = p_state,`country` = p_country,`contact` = p_contact,`email` = p_email,`player` = p_player,`organization` = p_organization,`tournament` = p_tournament,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_organization`//

CREATE PROCEDURE `sp_get_organization`(
)
BEGIN

	SELECT `id`,`name`,`owner`,`abn`,`active`,`updated_datetime`,`created_datetime`
	FROM `organization`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_organization`//

CREATE PROCEDURE `sp_set_organization`(
IN p_id INT,
IN p_name VARCHAR(300),
IN p_owner VARCHAR(100),
IN p_abn VARCHAR(300),
IN p_active INT)

BEGIN

INSERT INTO `organization`(`id`,`name`,`owner`,`abn`,`active`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_name, p_owner, p_abn, p_active, NOW(), NOW())
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
IN p_tournament int
)
BEGIN

	SELECT `id`,`permutation`,`round`,`tournament`,`updated_datetime`,`created_datetime`
	FROM `match`
    where `tournament` = p_tournament
	ORDER BY `round`, `permutation`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_match`//

CREATE PROCEDURE `sp_set_match`(
IN p_id INT,
IN p_permutation INT,
IN p_round INT,
IN p_tournament INT)

BEGIN

INSERT INTO `match`(`id`,`permutation`,`round`,`tournament`,`updated_datetime`,`created_datetime`) 
VALUES (p_id, p_permutation, p_round, p_tournament, NOW(), NOW())
ON DUPLICATE KEY UPDATE  `permutation` = p_permutation, `round` = p_round,`tournament` = p_tournament,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_player`//

CREATE PROCEDURE `sp_get_player`(
IN p_organization INT
)
BEGIN

	SELECT `id`,`organization`,`first_name`,`last_name`,`utr`,`updated_datetime`,`created_datetime`
	FROM `player`
    WHERE `organization` = p_organization OR ISNULL(p_organization)
	ORDER BY `first_name`,`last_name`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_player`//

CREATE PROCEDURE `sp_set_player`(
IN p_id INT,
IN p_organization INT,
IN p_first_name VARCHAR(100),
IN p_last_name VARCHAR(100),
IN p_utr DECIMAL(6,2))

BEGIN

INSERT INTO `player`(`id`,`organization`,`first_name`,`last_name`,`utr`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_organization,p_first_name, p_last_name, p_utr, NOW(), NOW())
ON DUPLICATE KEY UPDATE `first_name` = p_first_name,`last_name` = p_last_name,`utr` = p_utr,`organization` = p_organization, `updated_datetime` = NOW();

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

DROP PROCEDURE IF EXISTS `sp_get_permutation`//

CREATE PROCEDURE `sp_get_permutation`(
)
BEGIN

	SELECT `id`,`index`,`team_home`,`team_away`,`tournament`,`updated_datetime`,`created_datetime`
	FROM `permutation`
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_permutation`//

CREATE PROCEDURE `sp_set_permutation`(
IN p_id INT,
IN p_index INT,
IN p_team_home INT,
IN p_team_away VARCHAR(300),
IN p_tournament INT)

BEGIN

INSERT INTO `permutation`(`id`,`index`,`team_home`,`team_away`,`tournament`,`updated_datetime`,`created_datetime`) VALUES (p_id, p_index, p_team_home, p_team_away, p_tournament, NOW(), NOW())
ON DUPLICATE KEY UPDATE `index` = p_index,`team_home` = p_team_home,`team_away` = p_team_away,`tournament` = p_tournament,`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_team`//

CREATE PROCEDURE `sp_get_team`(
IN p_organization INT
)
BEGIN

	SELECT `id`,`label`,`organization`,`updated_datetime`,`created_datetime`
	FROM `team`
    WHERE `organization` = p_organization OR ISNULL(p_organization)
	ORDER BY `id`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_team`//

CREATE PROCEDURE `sp_set_team`(
IN p_id INT,
IN p_organization INT,
IN p_tournament INT,
IN p_label VARCHAR(200))

BEGIN

INSERT INTO `team` (`id`,`label`,`organization`,`tournament`,`updated_datetime`,`created_datetime`) 
VALUES (p_id, p_label, p_organization,p_tournament, NOW(), NOW())
ON DUPLICATE KEY UPDATE `label` = p_label,`organization` = p_organization, `tournament` = p_tournament, `updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_set_team_player`//

CREATE PROCEDURE `sp_set_team_player`(
IN p_team INT,
IN p_player INT,
IN p_player_first VARCHAR(100),
IN p_player_last VARCHAR(100),
IN p_tournament INT,
IN p_organization INT)

BEGIN
-- add new players 
SET @p_id = p_player;

IF @p_id < 1  THEN

	INSERT INTO `player` VALUES ( NULL, p_player_first,p_player_last, p_organization, 1.0, now(), now());
    SELECT last_insert_id() INTO @p_id;
END IF;


INSERT INTO `team_player`(`team`,`player`,`tournament`) VALUES (p_team, @p_id, p_tournament)
ON DUPLICATE KEY UPDATE `team` = p_team,`player` = @p_id, `tournament` = p_tournament;

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_team_player`//

CREATE PROCEDURE `sp_get_team_player`(
IN p_tournament INT)

BEGIN

SELECT tp.`id`, t.organization 'organization', t.tournament, t.label 'team',  t.id 'team_id',
p.id `player_id`,  p.first_name , p.last_name, p.utr
FROM `team_player` tp JOIN `team` t ON t.id = tp.team
JOIN `player` p ON p.id = tp.`player`
WHERE tp.`tournament` = p_tournament
ORDER BY `team`, `player`;

END//


DROP PROCEDURE IF EXISTS `sp_get_tournament`//

CREATE PROCEDURE `sp_get_tournament`(
in p_id int
)
BEGIN

	SELECT `id`,`label`,`start`,`end`,`interval`,`steps`,`type`,`max`,`fee`,`prize`,`seedings`,`sport`,
    `organization`,`entry_type`,`updated_datetime`,`created_datetime`
	FROM `tournament`
    WHERE `id` = p_id
	ORDER BY `id`;


END//

DROP PROCEDURE IF EXISTS `sp_get_tournament_list`//

CREATE PROCEDURE `sp_get_tournament_list`(
in p_organization int
)
BEGIN

	SELECT t.`id`,t.`label`,`start`,`end`,`interval`, i.Label 'interval_label',
    `steps`,`type`,tt.`label` 'type_label', `max`,`fee`,`prize`,`seedings`,`sport`,
    s.`label` 'sport_label', `organization`,`updated_datetime`,`created_datetime`
	FROM `tournament` t JOIN `tournament_type` tt ON tt.id = t.`type`
    JOIN `interval` i ON i.id = t.`interval`
    JOIN `sport` s ON s.id = t.`sport`
    WHERE t.`organization` = p_organization
	ORDER BY t.`start`;


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
IN p_seedings INT,
IN p_sport INT,
IN p_organization INT,
IN p_entry_type INT)

BEGIN

INSERT INTO `tournament`(`id`,`label`,`start`,`end`,`interval`,`steps`,`type`,`max`,`fee`,`prize`,`seedings`,`sport`,`organization`,`entry_type`,`updated_datetime`,`created_datetime`) 
VALUES (p_id, p_label, p_start, p_end, p_interval, p_steps, p_type, p_max, p_fee, p_prize, p_seedings, p_sport, p_organization, p_entry_type,NOW(), NOW())
ON DUPLICATE KEY UPDATE `label` = p_label,`start` = p_start,`end` = p_end,`interval` = p_interval,
`steps` = p_steps,`type` = p_type,`max` = p_max,`fee` = p_fee,`prize` = p_prize, `organization` = p_organization,
`seedings` = p_seedings,`sport` = p_sport, `entry_type` = p_entry_type ,`updated_datetime` = NOW();

if ifnull(p_id, 0)<1 then
	SELECT LAST_INSERT_ID() 'id';
else
	SELECT p_id 'id';
end if;

END//

DROP PROCEDURE IF EXISTS `sp_get_tournament_type`//

CREATE PROCEDURE `sp_get_tournament_type`(
)
BEGIN

	SELECT `id`,`label`, `name`, `key`,`icon`
	FROM `tournament_type`
    WHERE id > 0;


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


DROP PROCEDURE IF EXISTS `sp_set_match_player`//

CREATE PROCEDURE `sp_set_match_player`(
IN p_id INT,
IN p_match INT,
IN p_player_home INT,
IN p_player_away INT)

BEGIN

INSERT INTO `match_player`(`id`,`match`,`player_home`,`player_away`,`updated_datetime`, `created_datetime`)
 VALUES (p_id, p_match, p_player_home, p_player_away, NOW(), NOW())
ON DUPLICATE KEY UPDATE `match` = p_match,`player_home` = p_player_home,`player_away` = p_player_away,
`updated_datetime` = NOW();

SELECT LAST_INSERT_ID() 'id';

END//

DROP PROCEDURE IF EXISTS `sp_get_match_player`//

CREATE PROCEDURE `sp_get_match_player`(
IN p_match INT)

BEGIN

SELECT `id`, `match`, `player_home`,`player_away`, `updated_datetime`, `created_datetime`
FROM `match_player`
ORDER BY `player_home`, `player_away`;

END//



DROP PROCEDURE IF EXISTS `sp_get_tournament_schedule`//

CREATE PROCEDURE `sp_get_tournament_schedule`(
IN p_tournament INT)

BEGIN

select m.id 'match_id', m.permutation, m.round, player_home, player_away, home.team 'team_home', away.team 'team_away'
 from `match` m left join `match_player` p on p.`match`= m.id 
 left join `team_player` home on home.player = p.player_home
 left join `team_player` away on away.player = p.player_away
 order by m.round, m.permutation;
 
END//

DROP PROCEDURE IF EXISTS `sp_get_sports`//

CREATE PROCEDURE `sp_get_sports`(
)

BEGIN

SELECT `id`, `label` , `name`, `key`,`icon`
FROM sport 
WHERE `id` > 0
ORDER BY `id`;
 
END//

DROP PROCEDURE IF EXISTS `sp_delete_tournament`//

CREATE PROCEDURE `sp_delete_tournament`(
IN p_tournament INT)

BEGIN

-- TODO : Move to history
DELETE `match_player`
FROM `match_player`, `match` 
WHERE  `match`.id = `match_player`.`match` AND
`match`.`tournament` =  p_tournament;

DELETE FROM `match` where `tournament` = p_tournament;

-- Remove teams and players involved in the tournament
DELETE FROM `team_player` WHERE `tournament` = p_tournament;
DELETE FROM `team` WHERE `tournament` = p_tournament;

DELETE FROM `result` WHERE `tournament` = p_tournament;

DELETE FROM `tournament` WHERE `id` = p_tournament;
 
END//

DROP PROCEDURE IF EXISTS `sp_get_tournament_detail`//

CREATE PROCEDURE `sp_get_tournament_detail`(
IN p_tour INT
)
BEGIN

	SELECT `tournament`,`no_entries`,`sets`,`games`,`updated_datetime`,`created_datetime`
	FROM `tournament_detail`
    where `tournament` = p_tour
	ORDER BY `tournament`;


 END//


DROP PROCEDURE IF EXISTS `sp_set_tournament_detail`//

CREATE PROCEDURE `sp_set_tournament_detail`(
IN p_tournament INT,
IN p_no_entries INT,
IN p_sets INT,
IN p_games INT)

BEGIN

INSERT INTO `tournament_detail`(`tournament`,`no_entries`,`sets`,`games`,`updated_datetime`,`created_datetime`) VALUES (p_tournament, p_no_entries, p_sets, p_games, NOW(), NOW())
ON DUPLICATE KEY UPDATE `no_entries` = p_no_entries,`sets` = p_sets,`games` = p_games,`updated_datetime` = NOW();

END//

DELIMITER ;


