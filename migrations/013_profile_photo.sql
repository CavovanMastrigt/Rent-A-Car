-- A profile photo for each user, shown in the profile settings and the top bar.
-- Idempotent.
DELIMITER //
DROP PROCEDURE IF EXISTS add_col_if_missing //
CREATE PROCEDURE add_col_if_missing(IN tbl VARCHAR(64), IN col VARCHAR(64), IN def TEXT)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = tbl AND COLUMN_NAME = col) THEN
        SET @ddl = CONCAT('ALTER TABLE `', tbl, '` ADD COLUMN `', col, '` ', def);
        PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;
END //
DELIMITER ;
CALL add_col_if_missing('user', 'Profile_Photo', 'longblob NULL');
DROP PROCEDURE IF EXISTS add_col_if_missing;
