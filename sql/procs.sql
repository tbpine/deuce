-- Procs for deuce
-- Dec 2024
-- 

DELIMITER //

DROP PROCEDURE IF EXISTS `sp_add_tournament`;
CREATE PROCEDURE `sp_add_tournament`
(
)
INSERT INTO `tournament` (`id`, `label`, `start`, `end`, `interval`, `steps`, `type`, `max`,`fee`, `prize`, `seedings`, `updated_datetime`,
`created_datetime`)
VALUES (NULL, p_label, p_start, p_end, p_interval,p_type, p_max,p_f, p_prize, p_seedings, now(), now())
ON DUPLICATE KEY SET `label` = p_label, `start` = p_start, `end`= p_end, `interval` = p_interval,
 `type` = p_type, `max` = p_max ,`fee` = p_fee, `prize` = p_prize, `seedings` = p_seedings;

BEGIN

END//

DELIMITER ;