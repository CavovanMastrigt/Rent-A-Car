-- Rent-A-Car database dump
-- Compatible with MySQL 5.7+/8.x and MariaDB 10.x (collation utf8mb4_unicode_ci).
-- Fresh setup: create an empty database 'rentacar', then import this file. No migrations needed.

-- MySQL dump 10.13  Distrib 9.1.0, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: rentacar
-- ------------------------------------------------------
-- Server version	9.1.0
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `car`
--

DROP TABLE IF EXISTS `car`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `car` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `License_Plate` varchar(50) NOT NULL,
  `Brand` varchar(50) NOT NULL,
  `Model` varchar(50) NOT NULL,
  `Build_Date` date NOT NULL,
  `Daily_Price` double NOT NULL,
  `Car_Status` varchar(50) NOT NULL COMMENT 'active, disabled, maintenance',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `licenseplate` (`License_Plate`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `car`
--

LOCK TABLES `car` WRITE;
/*!40000 ALTER TABLE `car` DISABLE KEYS */;
INSERT INTO `car` VALUES (1,'K-123-AB','Volkswagen','Golf','2020-05-14',45,'active'),(2,'P-456-CD','BMW','3 Series','2021-08-22',75,'rented'),(3,'R-789-EF','Audi','A4','2019-03-10',65,'active'),(4,'S-234-GH','Mercedes-Benz','A-Class','2022-11-05',80,'active'),(5,'T-567-JK','Toyota','Yaris','2020-07-19',35,'active'),(6,'V-890-LM','Ford','Focus','2018-12-03',40,'maintenance'),(7,'X-123-NP','Opel','Corsa','2019-06-27',32,'active'),(8,'H-456-RS','Tesla','Model 3','2023-02-15',110,'active'),(9,'J-789-TV','Peugeot','208','2021-09-30',38,'disabled'),(10,'K-234-WX','Renault','Clio','2017-04-12',30,'active'),(11,'L-567-YZ','Volvo','XC40','2022-06-08',90,'active'),(12,'M-890-AB','Hyundai','i20','2020-10-21',37,'maintenance'),(13,'N-123-CD','Kia','Sportage','2021-01-17',70,'active'),(14,'P-456-EF','Skoda','Octavia','2019-11-29',55,'disabled'),(15,'R-789-GH','Mazda','CX-5','2023-07-04',85,'active');
/*!40000 ALTER TABLE `car` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `extra_option`
--

DROP TABLE IF EXISTS `extra_option`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `extra_option` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Price` decimal(10,2) NOT NULL,
  `Is_Active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `extra_option`
--

LOCK TABLES `extra_option` WRITE;
/*!40000 ALTER TABLE `extra_option` DISABLE KEYS */;
INSERT INTO `extra_option` VALUES (1,'Navigatie','Navigatiesysteem',10.00,1),(2,'Kinderstoeltje','Veilig kinderstoeltje',7.50,1),(3,'Luxe-pakket','Leren bekleding en extra comfort',25.00,1);
/*!40000 ALTER TABLE `extra_option` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `identity`
--

DROP TABLE IF EXISTS `identity`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `identity` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `Front_Photo` longblob,
  `Back_Photo` longblob,
  `Reviewed_By` varchar(100) DEFAULT NULL,
  `Expiration_Date` date DEFAULT NULL,
  `Identity_Status` varchar(100) NOT NULL DEFAULT 'Pending',
  `Document_Number` varchar(255) DEFAULT NULL,
  `Date_Of_Birth` date DEFAULT NULL,
  `File_Type` varchar(10) DEFAULT NULL,
  `File_Name` varchar(255) DEFAULT NULL,
  `Reviewed_At` datetime DEFAULT NULL,
  `Reject_Reason` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Identity_Users_Key` (`UserID`),
  CONSTRAINT `identity_user_fk` FOREIGN KEY (`UserID`) REFERENCES `user` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `identity`
--

LOCK TABLES `identity` WRITE;
/*!40000 ALTER TABLE `identity` DISABLE KEYS */;
/*!40000 ALTER TABLE `identity` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `invoice`
--

DROP TABLE IF EXISTS `invoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoice` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Rental_ID` int NOT NULL,
  `Total_Price` double NOT NULL,
  `Expiration_Date` date NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Invoice_Rental_Key` (`Rental_ID`),
  CONSTRAINT `invoice_rental_fk` FOREIGN KEY (`Rental_ID`) REFERENCES `rental` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoice`
--

LOCK TABLES `invoice` WRITE;
/*!40000 ALTER TABLE `invoice` DISABLE KEYS */;
INSERT INTO `invoice` VALUES (1,1,330,'2026-11-19'),(2,2,440,'2026-10-28'),(3,3,220,'2026-09-17'),(4,4,225,'2026-11-20'),(5,5,225,'2026-11-06'),(6,6,225,'2026-09-01'),(7,7,90,'2026-11-17'),(8,8,135,'2026-10-22'),(9,9,160,'2026-11-23'),(10,10,270,'2026-11-11'),(11,11,280,'2026-10-08'),(12,12,175,'2026-07-29'),(13,13,255,'2026-06-18'),(14,14,65,'2026-11-24');
/*!40000 ALTER TABLE `invoice` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `openingtimes`
--

DROP TABLE IF EXISTS `openingtimes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `openingtimes` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Time_Open` time NOT NULL,
  `Time_Closed` time NOT NULL,
  `Weekday` varchar(20) NOT NULL,
  `Is_Closed` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `openingtimes`
--

LOCK TABLES `openingtimes` WRITE;
/*!40000 ALTER TABLE `openingtimes` DISABLE KEYS */;
/*!40000 ALTER TABLE `openingtimes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rental`
--

DROP TABLE IF EXISTS `rental`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rental` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `CarID` int NOT NULL,
  `Price_Per_Day` decimal(10,2) NOT NULL,
  `Date_Started` datetime NOT NULL,
  `Date_Planned_Ending` datetime NOT NULL,
  `Date_Picked_Up` datetime DEFAULT NULL,
  `Date_Ended` datetime DEFAULT NULL,
  `notes` varchar(255) DEFAULT NULL,
  `Created_By` int DEFAULT NULL,
  `Status` varchar(30) NOT NULL DEFAULT 'reserved' COMMENT 'reserved, ongoing, completed, cancelled',
  `Total_Price` decimal(10,2) NOT NULL DEFAULT '0.00',
  `Created_At` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`),
  KEY `idx_rental_user` (`UserID`),
  KEY `idx_rental_car` (`CarID`),
  KEY `rental_created_by_fk` (`Created_By`),
  CONSTRAINT `rental_created_by_fk` FOREIGN KEY (`Created_By`) REFERENCES `user` (`ID`),
  CONSTRAINT `rental_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `user` (`ID`),
  CONSTRAINT `rental_ibfk_2` FOREIGN KEY (`CarID`) REFERENCES `car` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rental`
--

LOCK TABLES `rental` WRITE;
/*!40000 ALTER TABLE `rental` DISABLE KEYS */;
INSERT INTO `rental` VALUES (1,6,8,110.00,'2026-11-02 00:00:00','2026-11-05 00:00:00','2026-11-02 00:00:00','2026-11-05 00:00:00','TESTDATA completed',1,'completed',330.00,'2026-11-10 06:55:50'),(2,6,8,110.00,'2026-10-10 00:00:00','2026-10-14 00:00:00','2026-10-10 00:00:00','2026-10-14 00:00:00','TESTDATA completed',1,'completed',440.00,'2026-11-10 06:55:50'),(3,6,8,110.00,'2026-09-01 00:00:00','2026-09-03 00:00:00','2026-09-01 00:00:00','2026-09-03 00:00:00','TESTDATA completed',1,'completed',220.00,'2026-11-10 06:55:50'),(4,6,2,75.00,'2026-11-03 00:00:00','2026-11-06 00:00:00','2026-11-03 00:00:00','2026-11-06 00:00:00','TESTDATA completed',1,'completed',225.00,'2026-11-10 06:55:50'),(5,6,2,75.00,'2026-10-20 00:00:00','2026-10-23 00:00:00','2026-10-20 00:00:00','2026-10-23 00:00:00','TESTDATA completed',1,'completed',225.00,'2026-11-10 06:55:50'),(6,6,2,75.00,'2026-08-15 00:00:00','2026-08-18 00:00:00','2026-08-15 00:00:00','2026-08-18 00:00:00','TESTDATA completed',1,'completed',225.00,'2026-11-10 06:55:50'),(7,6,1,45.00,'2026-11-01 00:00:00','2026-11-03 00:00:00','2026-11-01 00:00:00','2026-11-03 00:00:00','TESTDATA completed',1,'completed',90.00,'2026-11-10 06:55:50'),(8,6,1,45.00,'2026-10-05 00:00:00','2026-10-08 00:00:00','2026-10-05 00:00:00','2026-10-08 00:00:00','TESTDATA completed',1,'completed',135.00,'2026-11-10 06:55:50'),(9,6,4,80.00,'2026-11-07 00:00:00','2026-11-09 00:00:00','2026-11-07 00:00:00','2026-11-09 00:00:00','TESTDATA completed',1,'completed',160.00,'2026-11-10 06:55:50'),(10,6,11,90.00,'2026-10-25 00:00:00','2026-10-28 00:00:00','2026-10-25 00:00:00','2026-10-28 00:00:00','TESTDATA completed',1,'completed',270.00,'2026-11-10 06:55:50'),(11,6,13,70.00,'2026-09-20 00:00:00','2026-09-24 00:00:00','2026-09-20 00:00:00','2026-09-24 00:00:00','TESTDATA completed',1,'completed',280.00,'2026-11-10 06:55:50'),(12,6,5,35.00,'2026-07-10 00:00:00','2026-07-15 00:00:00','2026-07-10 00:00:00','2026-07-15 00:00:00','TESTDATA completed',1,'completed',175.00,'2026-11-10 06:55:50'),(13,6,15,85.00,'2026-06-01 00:00:00','2026-06-04 00:00:00','2026-06-01 00:00:00','2026-06-04 00:00:00','TESTDATA completed',1,'completed',255.00,'2026-11-10 06:55:50'),(14,6,3,65.00,'2026-11-09 00:00:00','2026-11-10 00:00:00','2026-11-09 00:00:00','2026-11-10 00:00:00','TESTDATA completed',1,'completed',65.00,'2026-11-10 06:55:50'),(15,6,2,75.00,'2026-11-09 00:00:00','2026-11-13 00:00:00','2026-11-09 00:00:00',NULL,'TESTDATA active',1,'active',0.00,'2026-11-10 06:55:50');
/*!40000 ALTER TABLE `rental` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rental_option`
--

DROP TABLE IF EXISTS `rental_option`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rental_option` (
  `Rental_ID` int NOT NULL,
  `Option_ID` int NOT NULL,
  `Option_Price` decimal(10,2) NOT NULL,
  PRIMARY KEY (`Rental_ID`,`Option_ID`),
  KEY `rental_option_option_fk` (`Option_ID`),
  CONSTRAINT `rental_option_option_fk` FOREIGN KEY (`Option_ID`) REFERENCES `extra_option` (`ID`),
  CONSTRAINT `rental_option_rental_fk` FOREIGN KEY (`Rental_ID`) REFERENCES `rental` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rental_option`
--

LOCK TABLES `rental_option` WRITE;
/*!40000 ALTER TABLE `rental_option` DISABLE KEYS */;
/*!40000 ALTER TABLE `rental_option` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `rolename` varchar(40) NOT NULL COMMENT '0 = customer\r\n1 = admin',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES (0,'Customer'),(1,'Admin'),(2,'Employee');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
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
  `User_Status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'active, disabled',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Email_key` (`Email`),
  KEY `Role_ID` (`RID`),
  CONSTRAINT `user_ibfk_1` FOREIGN KEY (`RID`) REFERENCES `role` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (1,1,'2026-10-27','zwjialoyNO0JL+RtMIeSVg==.3atqV7VeUCx9vRQbwPgi7mkgNz/M4JEYUihLaZOLd9M=','gio','gio','test','test','gio@gio.com','223122131','active'),(2,0,'2026-10-27','LHn8n9ZXFXclp4Xo0RYVRg==.BhBbbOnfpYWiRN4tTQQzqPuIfcDI+yCB5HvYTN5DuiY=','cavo','cavo','ijdwaodja','djwaidojaw','cavo@cavo.com','2821312312','active'),(3,2,'2026-11-06','FVffpOj/aUJHq/qtdEhyLg==.0HLBhhF4zOGl0jvTF2P8AQo2gTc79PXt/U3MZI0hgVg=','serdar','serdar','serdar','serdar','serdar@serdar.nl','2893012','active'),(4,1,'2026-11-10','7/FIgNLKwV97IunQtMH8hQ==.NsvBoIFRPSh/vkwbgBK8QN+Ni947HfEe4JVEPaTeP+0=','Laura','Admin','Dorpsstraat 1','Amsterdam','laura@rentacar.com','0612345601','active'),(5,2,'2026-11-10','lqA5wDrcbcmZvnGw/uKHGQ==.E56OqE4/cUj2cVXd6CJer0LEE/m5RVOAEZPkGsgLnaE=','Mark','Employee','Kerkweg 2','Rotterdam','mark@rentacar.com','0612345602','active'),(6,0,'2026-11-10','Y8wVE0OgXDJFYhh7X1bnWg==.WiyBpfZ7RVM5EZJ82Cmt9TMQ+MZ/j2fpWrPRMsmN66g=','Chris','Customer','Marktplein 3','Utrecht','customer@rentacar.com','0612345603','active');
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'rentacar'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-10  9:19:15
