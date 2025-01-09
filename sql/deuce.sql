/*****************************************************
* Schema for deuce. Tournaments.
* Version 1.0 Tong Pine (1)
* Dec 2024
*******************************************************/
-- drop table `tournament`;
-- drop table `tournament_type`;
-- drop table `player`;
-- drop table `team`;
-- drop table `match`;
-- drop table `match_player`;
-- drop table `team_player`;
-- drop table `sport`;

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
    `updated_datetime`	TIMESTAMP,
    `created_datetime`	TIMESTAMP
);


CREATE TABLE IF NOT EXISTS `player` (
    `id` 						INT PRIMARY KEY AUTO_INCREMENT,
    `first_name` 				VARCHAR(100),
    `last_name` 				VARCHAR(100),
    `organization`				INT,
    `utr` 						DECIMAL(6,2),
    `updated_datetime` 			TIMESTAMP,
    `created_datetime` 			TIMESTAMP
);

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

-- Collection of players/player
-- with a label.

CREATE TABLE IF NOT EXISTS `team` (
    `id` 				INT PRIMARY KEY AUTO_INCREMENT,
    `organization`				INT,
    `tournament`		INT,
	`label` 		    VARCHAR(200),
    `updated_datetime` 	TIMESTAMP,
    `created_datetime` 	TIMESTAMP
);

CREATE TABLE IF NOT EXISTS `team_player` (
    `id` 				INT PRIMARY KEY AUTO_INCREMENT,
    `team`				INT,
    `player`			INT,
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



