-- Prevent double-booking a car for an overlapping period, and store a reason
-- when a reservation is cancelled. Idempotent.

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
CALL add_col_if_missing('rental', 'Cancel_Reason', 'varchar(255) NULL');
DROP PROCEDURE IF EXISTS add_col_if_missing;

-- A car may not have two overlapping, non-cancelled rentals.
DROP TRIGGER IF EXISTS rental_no_overlap;
DELIMITER //
CREATE TRIGGER rental_no_overlap BEFORE INSERT ON rental
FOR EACH ROW
BEGIN
    IF EXISTS (
        SELECT 1 FROM rental r
        WHERE r.CarID = NEW.CarID
          AND r.Status <> 'cancelled'
          AND r.Date_Started < NEW.Date_Planned_Ending
          AND r.Date_Planned_Ending > NEW.Date_Started
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Deze auto is in deze periode al verhuurd.';
    END IF;
END //
DELIMITER ;
