use deuce;
-- ===============================================================
-- DROP existing Uniques
-- ===============================================================

SET @n = "u_team_label";
SELECT COUNT(index_name) INTO @b FROM information_schema.statistics WHERE table_schema = DATABASE() AND index_name = @n;
SET @stat = IF(@b > 0, "DROP INDEX `u_team_label` ON `team`;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @n = "u_team_player_tour";
SELECT COUNT(index_name) INTO @b FROM information_schema.statistics WHERE table_schema = DATABASE() AND index_name = @n;
SET @stat = IF(@b > 1, "DROP INDEX `u_team_player_tour` ON `team_player`;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @n = "u_team_tour_label";
SELECT COUNT(index_name) INTO @b FROM information_schema.statistics WHERE table_schema = DATABASE() AND index_name = @n;
SET @stat = IF(@b > 0, "DROP INDEX `u_team_tour_label` ON `team`;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


-- ===============================================================
-- Uniques
-- ===============================================================
SET @n = "u_team_player_tour";
SELECT COUNT(index_name) INTO @b FROM information_schema.statistics WHERE table_schema = DATABASE() AND index_name = @n;
SET @stat = IF(@b = 0, "CREATE UNIQUE INDEX `u_team_player_tour` ON `team_player` (`team`, `player`, `tournament`);", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
