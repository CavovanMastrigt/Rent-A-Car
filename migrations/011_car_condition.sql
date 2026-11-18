-- Car condition tracking: odometer, tyre pressure and a condition status on the
-- car, plus a maintenance/damage log. Idempotent; safe to run more than once.

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

CALL add_col_if_missing('car', 'Odometer',       'int NULL');
CALL add_col_if_missing('car', 'Tire_Pressure',  'varchar(20) NULL');
CALL add_col_if_missing('car', 'Car_Condition',  'varchar(20) NOT NULL DEFAULT ''ok''');

DROP PROCEDURE IF EXISTS add_col_if_missing;

CREATE TABLE IF NOT EXISTS `car_maintenance` (
  `ID`            int NOT NULL AUTO_INCREMENT,
  `CarID`         int NOT NULL,
  `Created_At`    datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `By_Name`       varchar(100) NULL,
  `Note`          varchar(500) NULL,
  `Odometer`      int NULL,
  `Tire_Pressure` varchar(20) NULL,
  `Car_Condition` varchar(20) NULL,
  PRIMARY KEY (`ID`)
);
