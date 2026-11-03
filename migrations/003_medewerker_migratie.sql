-- 001_medewerker_migratie.sql
-- Migratie voor de bestaande database `rentacar` (gebaseerd op rentacar.sql van 28 okt).
-- EENMALIG uitvoeren, in MySQL Workbench / phpMyAdmin, op elke lokale database.
-- NIET rentacar.sql opnieuw importeren: dat bevat DROP TABLE en wist je gegevens.
-- Bestaande kolomnamen blijven hetzelfde, zodat de code van je teamgenoten blijft werken.

USE rentacar;

-- ---------------------------------------------------------------
-- 1. Rol voor medewerkers (0 = Customer, 1 = Admin, 2 = Employee)
-- ---------------------------------------------------------------
INSERT INTO `role` (`ID`, `rolename`) VALUES (2, 'Employee');

-- ---------------------------------------------------------------
-- 2. Rental: foute sleutels herstellen
--    - UNIQUE op UserID en CarID zorgde dat een klant maar 1 reservering
--      en een auto maar 1 reservering ooit kon hebben
--    - rental_ibfk_3 verwees de verkeerde kant op (rental -> invoice)
-- ---------------------------------------------------------------
ALTER TABLE `rental` DROP FOREIGN KEY `rental_ibfk_3`;

ALTER TABLE `rental`
  ADD KEY `idx_rental_user` (`UserID`),
  ADD KEY `idx_rental_car` (`CarID`);

ALTER TABLE `rental`
  DROP INDEX `Rental_User_Key`,
  DROP INDEX `Rental_Car_Key`;

-- Juiste richting: invoice -> rental
ALTER TABLE `invoice`
  ADD CONSTRAINT `invoice_rental_fk` FOREIGN KEY (`Rental_ID`) REFERENCES `rental` (`ID`);

-- ---------------------------------------------------------------
-- 3. Rental: tijden, status, medewerker en totaalbedrag
-- ---------------------------------------------------------------
ALTER TABLE `rental`
  MODIFY `Price_Per_Day`       DECIMAL(10,2) NOT NULL,
  MODIFY `Date_Started`        DATETIME NOT NULL,
  MODIFY `Date_Planned_Ending` DATETIME NOT NULL,
  MODIFY `Date_Ended`          DATETIME NULL,
  MODIFY `notes`               VARCHAR(255) NULL,
  ADD COLUMN `Date_Picked_Up`  DATETIME NULL AFTER `Date_Planned_Ending`,
  ADD COLUMN `Created_By`      INT NULL,
  ADD COLUMN `Status`          VARCHAR(30) NOT NULL DEFAULT 'reserved'
        COMMENT 'reserved, ongoing, completed, cancelled',
  ADD COLUMN `Total_Price`     DECIMAL(10,2) NOT NULL DEFAULT 0,
  ADD COLUMN `Created_At`      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD CONSTRAINT `rental_created_by_fk` FOREIGN KEY (`Created_By`) REFERENCES `user` (`ID`);

-- ---------------------------------------------------------------
-- 4. Extra's (navigatie, kinderstoeltje, luxe-pakket)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `extra_option` (
  `ID`          INT NOT NULL AUTO_INCREMENT,
  `Name`        VARCHAR(100) NOT NULL,
  `Description` VARCHAR(255) NULL,
  `Price`       DECIMAL(10,2) NOT NULL,
  `Is_Active`   TINYINT(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rental_option` (
  `Rental_ID`    INT NOT NULL,
  `Option_ID`    INT NOT NULL,
  `Option_Price` DECIMAL(10,2) NOT NULL,
  PRIMARY KEY (`Rental_ID`, `Option_ID`),
  CONSTRAINT `rental_option_rental_fk` FOREIGN KEY (`Rental_ID`) REFERENCES `rental` (`ID`),
  CONSTRAINT `rental_option_option_fk` FOREIGN KEY (`Option_ID`) REFERENCES `extra_option` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `extra_option` (`Name`, `Description`, `Price`) VALUES
  ('Navigatie',      'Navigatiesysteem',                10.00),
  ('Kinderstoeltje', 'Veilig kinderstoeltje',            7.50),
  ('Luxe-pakket',    'Leren bekleding en extra comfort', 25.00);

-- ---------------------------------------------------------------
-- 5. Openingstijden: primaire sleutel en gesloten-vlag
-- ---------------------------------------------------------------
ALTER TABLE `openingtimes`
  ADD COLUMN `ID` INT NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST,
  ADD COLUMN `Is_Closed` TINYINT(1) NOT NULL DEFAULT 0;

-- Alleen uitvoeren als `openingtimes` nog leeg is (anders dubbele dagen).
-- Controleer eerst hoe de beheer-kant de dagnamen opslaat (Weekday).
-- INSERT INTO `openingtimes` (`Time_Open`, `Time_Closed`, `Weekday`) VALUES
--   ('08:00:00', '20:00:00', 'Monday'),
--   ('08:00:00', '20:00:00', 'Tuesday'),
--   ('08:00:00', '20:00:00', 'Wednesday'),
--   ('08:00:00', '20:00:00', 'Thursday'),
--   ('08:00:00', '20:00:00', 'Friday'),
--   ('10:00:00', '16:00:00', 'Saturday'),
--   ('10:00:00', '16:00:00', 'Sunday');
