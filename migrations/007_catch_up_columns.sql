-- Catch-up migration: adds every column the current app needs, but only if it
-- is still missing. Safe to run on any database, however many of migrations
-- 003-006 were already applied. Works on MySQL 5.7+/8/9 and MariaDB.
--
-- Run once on the rentacar database, e.g.:
--   mysql -u root rentacar < 007_catch_up_columns.sql

DELIMITER //
DROP PROCEDURE IF EXISTS add_col_if_missing //
CREATE PROCEDURE add_col_if_missing(IN tbl VARCHAR(64), IN col VARCHAR(64), IN def TEXT)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = tbl AND COLUMN_NAME = col
    ) THEN
        SET @ddl = CONCAT('ALTER TABLE `', tbl, '` ADD COLUMN `', col, '` ', def);
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END //
DELIMITER ;

-- identity: ID verification (user story 8)
CALL add_col_if_missing('identity', 'Date_Of_Birth',  'date NULL');
CALL add_col_if_missing('identity', 'File_Type',      'varchar(10) NULL');
CALL add_col_if_missing('identity', 'File_Name',      'varchar(255) NULL');
CALL add_col_if_missing('identity', 'Reviewed_At',    'datetime NULL');
CALL add_col_if_missing('identity', 'Reject_Reason',  'varchar(255) NULL');
CALL add_col_if_missing('identity', 'Back_File_Type', 'varchar(10) NULL');
CALL add_col_if_missing('identity', 'Back_File_Name', 'varchar(255) NULL');
CALL add_col_if_missing('identity', 'ID_First_Name',  'varchar(100) NULL');
CALL add_col_if_missing('identity', 'ID_Last_Name',   'varchar(100) NULL');

-- car: add / edit / archive cars (user story 3)
CALL add_col_if_missing('car', 'Fuel_Type', 'varchar(30) NULL');
CALL add_col_if_missing('car', 'Photo',     'longblob NULL');
CALL add_col_if_missing('car', 'Archived',  'tinyint(1) NOT NULL DEFAULT 0');

-- openingtimes: opening hours management
CALL add_col_if_missing('openingtimes', 'Is_Closed', 'tinyint(1) NOT NULL DEFAULT 0');

DROP PROCEDURE IF EXISTS add_col_if_missing;
