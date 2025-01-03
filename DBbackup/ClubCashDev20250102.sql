-- MariaDB dump 10.19  Distrib 10.11.6-MariaDB, for debian-linux-gnu (x86_64)
--
-- Host: localhost    Database: ClubCash
-- ------------------------------------------------------
-- Server version	10.11.6-MariaDB-0+deb12u1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `BasicUnits`
--

DROP TABLE IF EXISTS `BasicUnits`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `BasicUnits` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext NOT NULL,
  `CostUnitId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_BasicUnits_CostUnitId` (`CostUnitId`),
  CONSTRAINT `FK_BasicUnits_CostUnits_CostUnitId` FOREIGN KEY (`CostUnitId`) REFERENCES `CostUnits` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `BasicUnits`
--

LOCK TABLES `BasicUnits` WRITE;
/*!40000 ALTER TABLE `BasicUnits` DISABLE KEYS */;
/*!40000 ALTER TABLE `BasicUnits` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `CashRegisters`
--

DROP TABLE IF EXISTS `CashRegisters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CashRegisters` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext NOT NULL,
  `CurrentBalance` decimal(10,2) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `CashRegisters`
--

LOCK TABLES `CashRegisters` WRITE;
/*!40000 ALTER TABLE `CashRegisters` DISABLE KEYS */;
/*!40000 ALTER TABLE `CashRegisters` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `CostUnits`
--

DROP TABLE IF EXISTS `CostUnits`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CostUnits` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CostUnitName` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `CostUnits`
--

LOCK TABLES `CostUnits` WRITE;
/*!40000 ALTER TABLE `CostUnits` DISABLE KEYS */;
/*!40000 ALTER TABLE `CostUnits` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `SpecialItems`
--

DROP TABLE IF EXISTS `SpecialItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SpecialItems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext NOT NULL,
  `Betrag` decimal(10,2) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `SpecialItems`
--

LOCK TABLES `SpecialItems` WRITE;
/*!40000 ALTER TABLE `SpecialItems` DISABLE KEYS */;
/*!40000 ALTER TABLE `SpecialItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Transactions`
--

DROP TABLE IF EXISTS `Transactions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Transactions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` date NOT NULL,
  `Documentnumber` int(11) NOT NULL,
  `Description` longtext NOT NULL,
  `Sum` decimal(10,2) NOT NULL,
  `AccountMovement` decimal(10,2) NOT NULL,
  `CostUnitID` int(11) NOT NULL,
  `BasicUnitID` int(11) NOT NULL,
  `UnitDetailsId` int(11) DEFAULT NULL,
  `CashRegisterID` int(11) NOT NULL,
  `SpecialItemID` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Transactions_BasicUnitID` (`BasicUnitID`),
  KEY `IX_Transactions_CashRegisterID` (`CashRegisterID`),
  KEY `IX_Transactions_CostUnitID` (`CostUnitID`),
  KEY `IX_Transactions_SpecialItemID` (`SpecialItemID`),
  KEY `IX_Transactions_UnitDetailsId` (`UnitDetailsId`),
  CONSTRAINT `FK_Transactions_BasicUnits_BasicUnitID` FOREIGN KEY (`BasicUnitID`) REFERENCES `BasicUnits` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Transactions_CashRegisters_CashRegisterID` FOREIGN KEY (`CashRegisterID`) REFERENCES `CashRegisters` (`ID`) ON DELETE CASCADE,
  CONSTRAINT `FK_Transactions_CostUnits_CostUnitID` FOREIGN KEY (`CostUnitID`) REFERENCES `CostUnits` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Transactions_SpecialItems_SpecialItemID` FOREIGN KEY (`SpecialItemID`) REFERENCES `SpecialItems` (`Id`),
  CONSTRAINT `FK_Transactions_UnitDetails_UnitDetailsId` FOREIGN KEY (`UnitDetailsId`) REFERENCES `UnitDetails` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Transactions`
--

LOCK TABLES `Transactions` WRITE;
/*!40000 ALTER TABLE `Transactions` DISABLE KEYS */;
/*!40000 ALTER TABLE `Transactions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UnitDetails`
--

DROP TABLE IF EXISTS `UnitDetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `UnitDetails` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CostDetails` longtext NOT NULL,
  `BasicUnitId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UnitDetails_BasicUnitId` (`BasicUnitId`),
  CONSTRAINT `FK_UnitDetails_BasicUnits_BasicUnitId` FOREIGN KEY (`BasicUnitId`) REFERENCES `BasicUnits` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UnitDetails`
--

LOCK TABLES `UnitDetails` WRITE;
/*!40000 ALTER TABLE `UnitDetails` DISABLE KEYS */;
/*!40000 ALTER TABLE `UnitDetails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__EFMigrationsHistory`
--

LOCK TABLES `__EFMigrationsHistory` WRITE;
/*!40000 ALTER TABLE `__EFMigrationsHistory` DISABLE KEYS */;
/*!40000 ALTER TABLE `__EFMigrationsHistory` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-01-02 10:32:54
