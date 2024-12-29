use deuce;

-- ===============================================================
-- Uniques
-- ===============================================================
SET @n = "u_team_player";
SELECT COUNT(index_name) INTO @b FROM information_schema.statistics WHERE table_schema = DATABASE() AND index_name = @n;
SET @stat = IF(@b = 0, "CREATE UNIQUE INDEX `u_team_player` ON `team_player` (`team`, `player`);", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

