/******************************************************
* Schema for deuce. Tournaments.
* Version 1.0 Tong Pine (1)
* Dec 2024
*******************************************************/
-- drop table `tournament`;
-- drop table `venue`;
-- drop table `player`;
CREATE TABLE IF NOT EXISTS `tournament_type` (
    `id` INT,
    `label` CHAR(20)
);

CREATE TABLE IF NOT EXISTS `interval` (
    `id` INT,
    `label` CHAR(20)
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
    `updated_datetime`	TIMESTAMP,
    `created_datetime`	TIMESTAMP
);


CREATE TABLE IF NOT EXISTS `player` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `first_name` VARCHAR(100),
    `last_name` VARCHAR(100),
    `utr` DECIMAL(6,2),
    `updated_datetime` TIMESTAMP,
    `created_datetime` TIMESTAMP
);

CREATE TABLE IF NOT EXISTS `entry` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `tournament` INT NOT NULL,
    `player` INT NOT NULL,
    `active` INT
);


CREATE TABLE IF NOT EXISTS `result` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `tournament` INT NOT NULL,
    `player_1` INT NOT NULL,
    `player_2` INT NOT NULL,
    `score` VARCHAR(300),
    `updated_datetime` TIMESTAMP,
    `created_datetime` TIMESTAMP
);


CREATE TABLE IF NOT EXISTS `address` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `street` VARCHAR(150),
    `suburb` VARCHAR(50),
    `state` VARCHAR(30),
    `country` VARCHAR(30),
    `contact` VARCHAR(40),
    `email` VARCHAR(100),
    `player` INT,
    `club` INT,
    `tournament` INT,
    `updated_datetime` TIMESTAMP,
    `created_datetime` TIMESTAMP
);


CREATE TABLE IF NOT EXISTS `account` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `player` INT ,
    `club`   INT ,
    `password` VARBINARY(48),
    `salt` VARBINARY(8),
    `active` INT,
    `updated_datetime` TIMESTAMP,
    `created_datetime` TIMESTAMP
);


CREATE TABLE IF NOT EXISTS `club` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `name` VARCHAR(300),
    `owner` VARCHAR(100),
    `abn` VARCHAR(300),
    `active` INT,
    `updated_datetime` TIMESTAMP,
    `created_datetime` TIMESTAMP
);


DROP VIEW IF EXISTS `tournament_details`;

CREATE VIEW `tournament_details` AS
    SELECT 
        t.id 'tournament_id',
        t.label,
        p.id 'player_id',
        p.first_name,
        p.last_name,
        p.utr,
        e.`active`
    FROM
        `entry` e
            JOIN
        `tournament` t ON t.id = e.tournament
            JOIN
        `player` p ON p.id = e.player
    ORDER BY p.first_name , p.last_name;


