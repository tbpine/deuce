-- Insert 8 sample players
delete from `player`;

INSERT INTO `player` (`first_name`, `middle_name`, `last_name`, `tournament`, `utr`, `member`, `updated_datetime`, `created_datetime`) VALUES
('John', 'Michael', 'Smith', 100, 8.50, 1, NOW(), NOW()),
('Sarah', 'Anne', 'Johnson', 100, 7.25, 2, NOW(), NOW()),
('David', 'James', 'Williams', 100, 9.10, 3, NOW(), NOW()),
('Emma', 'Claire', 'Brown', 100, 6.75, 4, NOW(), NOW()),
('Michael', 'Robert', 'Davis', 100, 8.00, 5, NOW(), NOW()),
('Lisa', 'Marie', 'Wilson', 100, 7.80, 6, NOW(), NOW()),
('Alex', 'Thomas', 'Taylor', 100, 8.25, 7, NOW(), NOW()),
('Jessica', 'Nicole', 'Anderson', 100, 7.50, 8, NOW(), NOW());