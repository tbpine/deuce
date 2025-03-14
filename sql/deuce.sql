/*****************************************************
* Schema for deuce. Tournaments.
* Version 1.0 Tong Pine (1)
* Dec 2024
*******************************************************/
-- drop table if exists  `tournament`;
-- drop table if exists  `tournament_detail`;
-- drop table if exists  `tournament_type`;
-- drop table if exists  `tournament_venue`;
-- drop table if exists  `player`;
-- drop table if exists  `team`;
-- drop table if exists  `team_player`;
-- drop table if exists  `match`;
-- drop table if exists  `match_player`;
-- drop table if exists  `sport`;
-- drop table if exists  `settings`;
-- drop table if exists  `member`;

-- settings for the while application

CREATE TABLE IF NOT EXISTS `iso_3166` (
    `name` VARCHAR(100),
    `alpha-2` CHAR(2),
    `alpha-3` CHAR(3),
    `country-code` INT,
    `iso_3166-2` VARCHAR(100),
    `region` VARCHAR(50),
    `sub-region` VARCHAR(50),
    `intermediate-region` VARCHAR(50),
    `region-code` INT,
    `sub-region-code` INT,
	`intermediate-region-code` INT
);

CREATE TABLE IF NOT EXISTS `state` (
    `id` 			INT PRIMARY KEY,
    `country` 		INT,
    `key` 			VARCHAR(50),
    `value` 		VARCHAR(300)
);


CREATE TABLE IF NOT EXISTS `settings` (
    `id` INT PRIMARY KEY,
    `key` VARCHAR(50),
    `value` VARCHAR(300)
);

CREATE TABLE IF NOT EXISTS `tournament_type` (
    `id` INT,
    `label` VARCHAR(20),
    `name` VARCHAR(100),
    `key` VARCHAR(100),
    `icon` VARCHAR(300)
);

CREATE TABLE IF NOT EXISTS `interval` (
    `id` INT,
    `label` CHAR(20)
);

CREATE TABLE IF NOT EXISTS `sport` (
    `id` INT,
    `label` CHAR(100),
    `name` CHAR(100),
    `key` CHAR(100),
    `icon` CHAR(200)
);


CREATE TABLE IF NOT EXISTS `tournament` (
    `id` 				INT PRIMARY KEY AUTO_INCREMENT,
    `label` 			VARCHAR(300),
    `start` 			DATETIME,
    `end` 				DATETIME,
    `interval` 			INT,
    `steps`				INT,
    `type`				INT,
    `max` 				INT,
    `fee` 				DECIMAL(10,2),
    `prize` 			DECIMAL(10,2),
    `seedings` 			INT,
    `sport`				INT,
    `organization`		INT,
    `entry_type`		INT,
    `updated_datetime`	TIMESTAMP,
    `created_datetime`	TIMESTAMP,
    `status`		    INT DEFAULT(0)
);

ALTER TABLE `tournament` AUTO_INCREMENT = 100;

CREATE TABLE IF NOT EXISTS `tournament_detail` (
    `tournament` 		INT PRIMARY KEY,
    `no_entries` 		INT,
    `sets` 				INT,
    `games` 			INT,
    `custom_games`		INT,
    `team_size`			INT,
    `no_singles`		INT,
    `no_doubles`		INT,
    `updated_datetime`	TIMESTAMP,
    `created_datetime`	TIMESTAMP
);

CREATE TABLE IF NOT EXISTS `tournament_venue` (
    `id` 				INT PRIMARY KEY AUTO_INCREMENT,
    `tournament` 		INT,
    `street`			VARCHAR(300),
    `suburb`			VARCHAR(100),
	`state`				VARCHAR(100),
    `post_code`			INT,
    `country-code`		INT,
    `updated_datetime`	TIMESTAMP,
    `created_datetime`	TIMESTAMP
);


-- members

CREATE TABLE IF NOT EXISTS `member` (
    `id` 						INT PRIMARY KEY AUTO_INCREMENT,
    `first_name` 				VARCHAR(100),
    `middle_name` 				VARCHAR(100),
    `last_name` 				VARCHAR(100),
    `utr` 						DECIMAL(6,2),
    `country-code`				INT,
    `updated_datetime` 			TIMESTAMP,
    `created_datetime` 			TIMESTAMP
);

-- players involved in a tournament

CREATE TABLE IF NOT EXISTS `player` (
    `id` 						INT PRIMARY KEY AUTO_INCREMENT,
    `first_name` 				VARCHAR(100),
    `middle_name` 				VARCHAR(100),
    `last_name` 				VARCHAR(100),
	`tournament`				INT,
    `utr` 						DECIMAL(6,2),
    `updated_datetime` 			TIMESTAMP,
    `created_datetime` 			TIMESTAMP
);

ALTER TABLE `player` auto_increment = 1000;

CREATE TABLE IF NOT EXISTS `result` (
    `id` 					INT PRIMARY KEY AUTO_INCREMENT,
    `tournament` 			INT NOT NULL,
    `player_1` 				INT NOT NULL,
    `player_2` 				INT NOT NULL,
    `score` 				VARCHAR(300),
    `updated_datetime` 		TIMESTAMP,
    `created_datetime` 		TIMESTAMP
);


CREATE TABLE IF NOT EXISTS `address` (
    `id` 					INT PRIMARY KEY AUTO_INCREMENT,
    `street`				VARCHAR(150),
    `suburb` 				VARCHAR(50),
    `state` 				VARCHAR(30),
    `country` 				VARCHAR(30),
    `contact` 				VARCHAR(40),
    `email` 				VARCHAR(100),
    `player` 				INT,
    `organization` 			INT,
    `tournament` 			INT,
    `updated_datetime` 		TIMESTAMP,
    `created_datetime` 		TIMESTAMP
);


CREATE TABLE IF NOT EXISTS `account` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `player` INT ,
    `organization`   INT ,
    `password` VARBINARY(48),
    `salt` VARBINARY(8),
    `active` INT,
    `updated_datetime` TIMESTAMP,
    `created_datetime` TIMESTAMP
);


CREATE TABLE IF NOT EXISTS `organization` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `name` VARCHAR(300),
    `owner` VARCHAR(100),
    `abn` VARCHAR(300),
    `active` INT,
    `updated_datetime` TIMESTAMP,
    `created_datetime` TIMESTAMP
);
ALTER TABLE `organization` AUTO_INCREMENT = 100;

-- Collection of players/player
-- with a label.

CREATE TABLE IF NOT EXISTS `team` (
    `id` 				INT PRIMARY KEY AUTO_INCREMENT,
    `organization`		INT,
    `tournament`		INT,
    `index`				INT,
	`label` 		    VARCHAR(200),
    `updated_datetime` 	TIMESTAMP,
    `created_datetime` 	TIMESTAMP
);

CREATE TABLE IF NOT EXISTS `team_player` (
    `id` 				INT PRIMARY KEY AUTO_INCREMENT,
    `team`				INT,
    `player`			INT,
    `index`				INT,
    `tournament`		INT,
    `updated_datetime` 	TIMESTAMP,
    `created_datetime` 	TIMESTAMP
);


-- Contest between two players
--
--
CREATE TABLE IF NOT EXISTS `match` (
    `id` 				INT PRIMARY KEY AUTO_INCREMENT,
    `permutation`		INT,
    `round` 			INT,
    `tournament` 		INT,
    `updated_datetime` 	TIMESTAMP,
    `created_datetime` 	TIMESTAMP
);

CREATE TABLE IF NOT EXISTS `match_player` (
    `id` 				INT PRIMARY KEY AUTO_INCREMENT,
    `match`				INT,
    `player_home` 		INT,
    `player_away` 		INT,
    `tournament`		INT,
    `updated_datetime` 	TIMESTAMP,
    `created_datetime` 	TIMESTAMP
);



