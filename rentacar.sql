-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1:3306
-- Gegenereerd op: 28 okt 2026 om 12:13
-- Serverversie: 9.1.0
-- PHP-versie: 8.2.26

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `rentacar`
--

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `car`
--

DROP TABLE IF EXISTS `car`;
CREATE TABLE IF NOT EXISTS `car` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `License_Plate` varchar(50) NOT NULL,
  `Brand` varchar(50) NOT NULL,
  `Model` varchar(50) NOT NULL,
  `Build_Date` date NOT NULL,
  `Daily_Price` double NOT NULL,
  `Car_Status` varchar(50) NOT NULL COMMENT 'active, disabled, maintenance',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `licenseplate` (`License_Plate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `identity`
--

DROP TABLE IF EXISTS `identity`;
CREATE TABLE IF NOT EXISTS `identity` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `Front_Photo` longblob NOT NULL,
  `Back_Photo` longblob NOT NULL,
  `Reviewed_By` varchar(100) NOT NULL,
  `Expiration_Date` date NOT NULL,
  `Identity_Status` varchar(100) NOT NULL COMMENT 'Pending, Verified, Failed',
  `Document_Number` varchar(255) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Identity_Users_Key` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `invoice`
--

DROP TABLE IF EXISTS `invoice`;
CREATE TABLE IF NOT EXISTS `invoice` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Rental_ID` int NOT NULL,
  `Total_Price` double NOT NULL,
  `Expiration_Date` date NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Invoice_Rental_Key` (`Rental_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `openingtimes`
--

DROP TABLE IF EXISTS `openingtimes`;
CREATE TABLE IF NOT EXISTS `openingtimes` (
  `Time_Open` time NOT NULL,
  `Time_Closed` time NOT NULL,
  `Weekday` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `rental`
--

DROP TABLE IF EXISTS `rental`;
CREATE TABLE IF NOT EXISTS `rental` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `CarID` int NOT NULL,
  `Price_Per_Day` int NOT NULL,
  `Date_Started` date NOT NULL,
  `Date_Planned_Ending` date NOT NULL,
  `Date_Ended` date NOT NULL,
  `notes` varchar(255) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Rental_User_Key` (`UserID`),
  UNIQUE KEY `Rental_Car_Key` (`CarID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `role`
--

DROP TABLE IF EXISTS `role`;
CREATE TABLE IF NOT EXISTS `role` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `rolename` varchar(40) NOT NULL COMMENT '0 = customer\r\n1 = admin',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Gegevens worden geëxporteerd voor tabel `role`
--

INSERT INTO `role` (`ID`, `rolename`) VALUES
(0, 'Customer'),
(1, 'Admin');

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `user`
--

DROP TABLE IF EXISTS `user`;
CREATE TABLE IF NOT EXISTS `user` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `RID` int NOT NULL,
  `Created_At` date NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Name` varchar(40) NOT NULL,
  `LastName` varchar(40) NOT NULL,
  `Address` varchar(100) NOT NULL,
  `City` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Phone_Number` varchar(20) NOT NULL,
  `User_Status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'active, disabled',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Email_key` (`Email`),
  KEY `Role_ID` (`RID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Gegevens worden geëxporteerd voor tabel `user`
--

INSERT INTO `user` (`ID`, `RID`, `Created_At`, `Password`, `Name`, `LastName`, `Address`, `City`, `Email`, `Phone_Number`, `User_Status`) VALUES
(1, 1, '2026-10-27', 'zwjialoyNO0JL+RtMIeSVg==.3atqV7VeUCx9vRQbwPgi7mkgNz/M4JEYUihLaZOLd9M=', 'gio', 'gio', 'test', 'test', 'gio@gio.com', '223122131', 'active'),
(2, 0, '2026-10-27', 'LHn8n9ZXFXclp4Xo0RYVRg==.BhBbbOnfpYWiRN4tTQQzqPuIfcDI+yCB5HvYTN5DuiY=', 'cavo', 'cavo', 'ijdwaodja', 'djwaidojaw', 'cavo@cavo.com', '2821312312', 'active');

--
-- Beperkingen voor geëxporteerde tabellen
--

--
-- Beperkingen voor tabel `identity`
--
ALTER TABLE `identity`
  ADD CONSTRAINT `identity_user_fk` FOREIGN KEY (`UserID`) REFERENCES `user` (`ID`);

--
-- Beperkingen voor tabel `rental`
--
ALTER TABLE `rental`
  ADD CONSTRAINT `rental_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `user` (`ID`),
  ADD CONSTRAINT `rental_ibfk_2` FOREIGN KEY (`CarID`) REFERENCES `car` (`ID`),
  ADD CONSTRAINT `rental_ibfk_3` FOREIGN KEY (`ID`) REFERENCES `invoice` (`Rental_ID`);

--
-- Beperkingen voor tabel `user`
--
ALTER TABLE `user`
  ADD CONSTRAINT `user_ibfk_1` FOREIGN KEY (`RID`) REFERENCES `role` (`ID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
