delete from  `player`;

-- set player auto increment to 1000
ALTER TABLE `player` AUTO_INCREMENT = 1000;

-- insert 100 test rows into the player table
INSERT INTO `player` (`first_name`, `middle_name`, `last_name`, `utr`, `member`, `updated_datetime`, `created_datetime`)
SELECT CONCAT('First', LPAD(FLOOR(RAND() * 100), 3, '0')),
       CONCAT('Middle', LPAD(FLOOR(RAND() * 100), 3, '0')),
       CONCAT('Last', LPAD(FLOOR(RAND() * 100), 3, '0')),
       ROUND(RAND() * 10, 2),
       NULL,    
         NOW(),

         NOW()
FROM (
    SELECT 1 AS dummy
    FROM information_schema.tables
    LIMIT 100
) AS temp;
