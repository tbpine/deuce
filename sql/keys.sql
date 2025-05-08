use deuce;
-- turn off foreign key checks to avoid problems with dropping indexes
SET FOREIGN_KEY_CHECKS=0;

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

SET @n = "u_player_first_last_name_org";
SELECT COUNT(index_name) INTO @b FROM information_schema.statistics WHERE table_schema = DATABASE() AND index_name = @n;
SET @stat = IF(@b > 0, "DROP INDEX `u_player_first_last_name_org` ON `player`;", "SELECT 1 WHERE 1 = 0;");
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

SET @n = "u_account_email";
SELECT COUNT(index_name) INTO @b FROM information_schema.statistics WHERE table_schema = DATABASE() AND index_name = @n;
SET @stat = IF(@b = 0, "CREATE UNIQUE INDEX `u_account_email` ON `account` (`email`);", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


-- ---------------------------------------------------------------------------
-- Foreign key constraints
-- ---------------------------------------------------------------------------

-- Check that the foreign key constraints exists for the table score and column tournament
SET @n = "fk_score_tournament";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `score` ADD CONSTRAINT `fk_score_tournament` FOREIGN KEY (`tournament`) REFERENCES `tournament` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table score and the column match
SET @n = "fk_score_match";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `score` ADD CONSTRAINT `fk_score_match` FOREIGN KEY (`match`) REFERENCES `match` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table match and the column tournament
SET @n = "fk_match_tournament";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `match` ADD CONSTRAINT `fk_match_tournament` FOREIGN KEY (`tournament`) REFERENCES `tournament` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table match_player and the column match
SET @n = "fk_match_player_match";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `match_player` ADD CONSTRAINT `fk_match_player_match` FOREIGN KEY (`match`) REFERENCES `match` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table match_player and the column  player_home reference to player
SET @n = "fk_match_player_player_home";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `match_player` ADD CONSTRAINT `fk_match_player_player_home` FOREIGN KEY (`player_home`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table match_player and the column  player_away reference to player
SET @n = "fk_match_player_player_away";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `match_player` ADD CONSTRAINT `fk_match_player_player_away` FOREIGN KEY (`player_away`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table match_player and the column  tournament  references tournament
SET @n = "fk_match_player_tournament";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `match_player` ADD CONSTRAINT `fk_match_player_tournament` FOREIGN KEY (`tournament`) REFERENCES `tournament` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table team and the column tournament references table tournament
SET @n = "fk_team_tournament";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `team` ADD CONSTRAINT `fk_team_tournament` FOREIGN KEY (`tournament`) REFERENCES `tournament` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table team and the column organization references table tournament
SET @n = "fk_team_organization";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `team` ADD CONSTRAINT `fk_team_organization` FOREIGN KEY (`organization`) REFERENCES `organization` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table team_player and the column player references table player
SET @n = "fk_team_player_player";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `team_player` ADD CONSTRAINT `fk_team_player_player` FOREIGN KEY (`player`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check that the foreign key constraints exists for the table team_player and the column tournament references table tournament
SET @n = "fk_team_player_tournament";
SELECT COUNT(constraint_name) INTO @b FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND constraint_name = @n;
SET @stat = IF(@b = 0, "ALTER TABLE `team_player` ADD CONSTRAINT `fk_team_player_tournament` FOREIGN KEY (`tournament`) REFERENCES `tournament` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;", "SELECT 1 WHERE 1 = 0;");
PREPARE stmt FROM @stat;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- turn on foreign key checks again
SET FOREIGN_KEY_CHECKS=1;








