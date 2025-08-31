/*M!999999\- enable the sandbox mode */ 
-- MariaDB dump 10.19  Distrib 10.11.11-MariaDB, for debian-linux-gnu (x86_64)
--
-- Host: 127.0.0.1    Database: ClubCash
-- ------------------------------------------------------
-- Server version	10.11.11-MariaDB-0+deb12u1

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
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `BasicUnits` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext NOT NULL,
  `CostUnitId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_BasicUnits_CostUnitId` (`CostUnitId`),
  CONSTRAINT `FK_BasicUnits_CostUnits_CostUnitId` FOREIGN KEY (`CostUnitId`) REFERENCES `CostUnits` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=71 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `BasicUnits`
--

LOCK TABLES `BasicUnits` WRITE;
/*!40000 ALTER TABLE `BasicUnits` DISABLE KEYS */;
INSERT INTO `BasicUnits` VALUES
(1,'Beiträge',37),
(2,'Beiträge Berge Westerbauer',37),
(3,'Beitragsrückbuchungen',37),
(4,'Steuerberater',37),
(5,'Bank',37),
(6,'Zinsen-Kosten',37),
(7,'Gerichtskosten',37),
(8,'WTTV',38),
(9,'Bezirk,Kreis,SSB',38),
(10,'LSB',38),
(11,'Hallennutzung',38),
(12,'Kreis',38),
(13,'Ehrenamtspauschale',38),
(14,'sonstiges',38),
(15,'SSK Hagen',38),
(16,'Sparda Bank',38),
(17,'Mitglieder',38),
(18,'Spende I',38),
(19,'Spende II',38),
(20,'Provider',39),
(21,'Änderungen',39),
(22,'Sommerfest',41),
(24,'Reinigung',42),
(25,'Planung',43),
(26,'Beraterkosten',43),
(27,'Trainingsanzüge',44),
(28,'Hemden',44),
(29,'Trikots-Hemden Ersatz',44),
(30,'Beflockung',44),
(31,'Trainingsbälle',44),
(32,'Netze',44),
(33,'Tische,Umrandungen',44),
(34,'Kosten',45),
(35,'Seniorentraining',45),
(36,'Damentraining',45),
(37,'Hobbytraining',45),
(38,'1.Herrenmannschaft',45),
(39,'Fahrtkosten',45),
(40,'Hotelkosten',45),
(41,'Spielbälle',45),
(42,'Turnierstartgelder',45),
(43,'Rückstellungen Spielbetrieb',45),
(44,'Lehrgangsgebühren',45),
(45,'Stadtsportbund',45),
(46,'Sporthilfe',45),
(47,'GF Ausgaben',45),
(48,'Geschenke',43),
(49,'GEMA',45),
(50,'Inventarversicherung',45),
(51,'Hygienemaßnahmen',45),
(52,'Übungsleiter',46),
(53,'Assistenztrainer',46),
(54,'Jugendfahrten',46),
(55,'Jugendausflüge',46),
(56,'Materialgestellung',46),
(57,'Catering',47),
(58,'Einnahmen',47),
(59,'Kosten Planung',47),
(60,'Kosten',47),
(62,'Verkaufsstand',47),
(63,'Ordnungsstrafen',45),
(64,'Weihnachtsfeier',41),
(65,'Undefined',41),
(66,'Altherrenstammtisch',41),
(67,'Mannschaftsessen',41),
(68,'Anwalt',37),
(70,'Umbuchung',37);
/*!40000 ALTER TABLE `BasicUnits` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `CashRegisters`
--

DROP TABLE IF EXISTS `CashRegisters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `CashRegisters` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(150) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `CashRegisters`
--

LOCK TABLES `CashRegisters` WRITE;
/*!40000 ALTER TABLE `CashRegisters` DISABLE KEYS */;
INSERT INTO `CashRegisters` VALUES
(1,'TTC Hagen e.V.');
/*!40000 ALTER TABLE `CashRegisters` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `CostUnits`
--

DROP TABLE IF EXISTS `CostUnits`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `CostUnits` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CostUnitName` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `CostUnits`
--

LOCK TABLES `CostUnits` WRITE;
/*!40000 ALTER TABLE `CostUnits` DISABLE KEYS */;
INSERT INTO `CostUnits` VALUES
(37,'Verwaltung'),
(38,'Zuschüsse'),
(39,'Internet'),
(40,'Jahresabschlussessen'),
(41,'Sonderveranstaltungen'),
(42,'Raumkosten'),
(43,'Verein'),
(44,'Vereinsbedarf'),
(45,'Spielbetrieb'),
(46,'Jugendarbeit'),
(47,'Turnier'),
(48,'Turnier Helferfest');
/*!40000 ALTER TABLE `CostUnits` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PositionDetails`
--

DROP TABLE IF EXISTS `PositionDetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `PositionDetails` (
  `BasicUnitsId` int(11) NOT NULL,
  `CostUnitDetailsId` int(11) NOT NULL,
  PRIMARY KEY (`BasicUnitsId`,`CostUnitDetailsId`),
  KEY `IX_PositionDetails_CostUnitDetailsId` (`CostUnitDetailsId`),
  CONSTRAINT `FK_PositionDetails_BasicUnits_BasicUnitsId` FOREIGN KEY (`BasicUnitsId`) REFERENCES `BasicUnits` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_PositionDetails_UnitDetails_CostUnitDetailsId` FOREIGN KEY (`CostUnitDetailsId`) REFERENCES `UnitDetails` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PositionDetails`
--

LOCK TABLES `PositionDetails` WRITE;
/*!40000 ALTER TABLE `PositionDetails` DISABLE KEYS */;
INSERT INTO `PositionDetails` VALUES
(1,6),
(1,7),
(1,8),
(1,9),
(1,18),
(1,30),
(1,35),
(1,47),
(3,37),
(5,16),
(5,17),
(5,26),
(5,27),
(5,48),
(9,14),
(13,1),
(13,10),
(13,38),
(13,46),
(18,10),
(18,11),
(18,24),
(18,27),
(18,29),
(18,42),
(18,45),
(19,13),
(21,1),
(22,15),
(22,48),
(27,15),
(29,15),
(30,1),
(33,23),
(34,22),
(34,25),
(34,27),
(34,28),
(34,34),
(35,12),
(36,5),
(37,13),
(38,1),
(38,20),
(38,21),
(38,28),
(38,31),
(38,33),
(38,41),
(38,43),
(39,1),
(39,2),
(42,15),
(42,39),
(42,40),
(44,1),
(45,36),
(45,44),
(48,48),
(52,1),
(52,5),
(53,2),
(53,3),
(53,4),
(54,2),
(58,37),
(63,22),
(64,15),
(64,48),
(66,48),
(68,48);
/*!40000 ALTER TABLE `PositionDetails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `SpecialItems`
--

DROP TABLE IF EXISTS `SpecialItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `SpecialItems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `SpecialItems`
--

LOCK TABLES `SpecialItems` WRITE;
/*!40000 ALTER TABLE `SpecialItems` DISABLE KEYS */;
INSERT INTO `SpecialItems` VALUES
(1,'Damentraining');
/*!40000 ALTER TABLE `SpecialItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Transactions`
--

DROP TABLE IF EXISTS `Transactions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `Transactions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` date NOT NULL,
  `Documentnumber` int(11) NOT NULL,
  `Description` varchar(300) DEFAULT NULL,
  `Sum` decimal(10,2) NOT NULL,
  `AccountMovement` decimal(10,2) NOT NULL,
  `CostUnitId` int(11) NOT NULL,
  `BasicUnitId` int(11) NOT NULL,
  `UnitDetailsId` int(11) DEFAULT NULL,
  `CashRegisterId` int(11) NOT NULL,
  `SpecialItemId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Transactions_BasicUnitId` (`BasicUnitId`),
  KEY `IX_Transactions_CashRegisterId` (`CashRegisterId`),
  KEY `IX_Transactions_CostUnitId` (`CostUnitId`),
  KEY `IX_Transactions_SpecialItemId` (`SpecialItemId`),
  KEY `IX_Transactions_UnitDetailsId` (`UnitDetailsId`),
  CONSTRAINT `FK_Transactions_BasicUnits_BasicUnitId` FOREIGN KEY (`BasicUnitId`) REFERENCES `BasicUnits` (`Id`),
  CONSTRAINT `FK_Transactions_CashRegisters_CashRegisterId` FOREIGN KEY (`CashRegisterId`) REFERENCES `CashRegisters` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Transactions_CostUnits_CostUnitId` FOREIGN KEY (`CostUnitId`) REFERENCES `CostUnits` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Transactions_SpecialItems_SpecialItemId` FOREIGN KEY (`SpecialItemId`) REFERENCES `SpecialItems` (`Id`),
  CONSTRAINT `FK_Transactions_UnitDetails_UnitDetailsId` FOREIGN KEY (`UnitDetailsId`) REFERENCES `UnitDetails` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=788 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Transactions`
--

LOCK TABLES `Transactions` WRITE;
/*!40000 ALTER TABLE `Transactions` DISABLE KEYS */;
INSERT INTO `Transactions` VALUES
(501,'2024-01-02',2002,'Mitgliedsbeitrag Familie Murjan ',50.00,50.00,37,1,6,1,NULL),
(502,'2024-01-03',2003,'Zahlung Anzeige Udo Majus RG 004/122023',150.00,150.00,47,58,NULL,1,NULL),
(503,'2024-01-03',2004,'Einzahlung Wechselgeldkasse ',10.00,10.00,47,58,NULL,1,NULL),
(504,'2024-01-03',2005,'RG 016/122023 Gottfried Schutz Vertriebs GmbH ',150.00,150.00,47,58,NULL,1,NULL),
(505,'2024-01-04',2006,'RG 019/122023 Weidlich GmbH ',200.00,200.00,47,58,NULL,1,NULL),
(506,'2024-01-08',2025,'Einzahlung Kleingeldkasse Turnier ',438.63,438.63,47,58,NULL,1,NULL),
(507,'2024-01-08',2027,'RG008/122023 Albuschat Turnieranzeige ',150.00,150.00,47,58,NULL,1,NULL),
(508,'2024-01-15',2031,'Rech. 014/122023 Paul Garthe GmbH ',100.00,100.00,47,58,NULL,1,NULL),
(509,'2024-01-15',2032,'Rech. 003/122023BSH GmbH ',150.00,150.00,47,58,NULL,1,NULL),
(510,'2024-01-15',2034,'Gutschrift Weidlich 903620360',47.98,47.98,47,58,NULL,1,NULL),
(511,'2024-01-17',2035,'Spende Dr. Padeck ',1000.00,1000.00,38,18,11,1,NULL),
(512,'2024-02-02',2041,'Beitragrückstand Murjan ',50.00,50.00,37,1,6,1,NULL),
(513,'2024-02-05',2042,'Beitragseinzug 1.HJ.2024',5202.00,5202.00,37,1,18,1,NULL),
(514,'2024-02-05',2043,'Spende Klaus Winkler',100.00,100.00,38,17,NULL,1,NULL),
(515,'2024-02-05',2044,'Rech.005/122023 Alexandrios Grill Anzeige Turnier ',125.00,125.00,47,58,NULL,1,NULL),
(516,'2024-02-12',2050,'Beitragszahlung R.Biggemann',48.00,48.00,37,1,35,1,NULL),
(517,'2024-02-12',2051,'Beiträge Neue Mitglieder',319.00,319.00,37,1,47,1,NULL),
(518,'2024-02-13',2052,'Turnieranzeige 2023 Dr. Padeck ',75.00,75.00,47,58,NULL,1,NULL),
(519,'2024-02-13',2053,'Turnieranzeige 2023 La Veranda ',125.00,125.00,47,58,NULL,1,NULL),
(520,'2024-02-13',2054,'Spende Haus Schneppendahl',125.00,125.00,47,58,NULL,1,NULL),
(521,'2024-03-01',2060,'SSK-Spende Integration TT-Sport ',1500.00,1500.00,38,15,NULL,1,NULL),
(522,'2024-03-09',2062,'Eigenanteil Trikots R.Kleinevoß',60.00,60.00,44,29,15,1,NULL),
(523,'2024-03-09',2063,'Mitgliedsbeitrag Familie Murjan Rate ',50.00,50.00,37,1,6,1,NULL),
(524,'2024-03-15',2067,'Beiträge 1 HJ.2024 Berge Westerbauer',580.20,580.20,37,2,NULL,1,NULL),
(525,'2024-03-15',2068,'Spende Dr. Padeck ',2000.00,2000.00,38,18,11,1,NULL),
(526,'2024-03-29',2074,'Spende Trainingsleistung 1.Quartal Willi Brandt ',360.00,360.00,38,19,13,1,NULL),
(527,'2024-03-31',2075,'Rechnung 012/122023 La Candela-Anzeige 2023',100.00,100.00,47,58,NULL,1,NULL),
(528,'2024-03-31',2076,'Spende Leistungstraining M.+G.Elsenbruch ',100.00,100.00,38,18,24,1,1),
(529,'2024-04-03',2078,'Beitrag Familie Murjan Ratenzahlung',50.00,50.00,37,1,6,1,NULL),
(530,'2024-04-10',2084,'Ekrem Abazi (RG.018/122023)',150.00,150.00,47,58,NULL,1,NULL),
(531,'2024-04-24',2089,'Mark-E, RG 001/122023',150.00,150.00,47,58,NULL,1,NULL),
(532,'2024-04-26',2090,'Rückzahlung R.Kleinevoss, Sicherheit zur Abrechnung ',500.00,500.00,45,38,33,1,NULL),
(533,'2024-04-27',2091,'Nachbuchung Beitrag P.Muß + Anmeldegebühr',87.00,87.00,37,1,47,1,NULL),
(534,'2024-04-27',2092,'Nachbuchung Beitrag neue Mitglieder + Anmeldegebühr',140.00,140.00,37,1,47,1,NULL),
(535,'2024-05-03',2095,'Spende Helmut Diegel',2000.00,2000.00,38,18,10,1,NULL),
(536,'2024-05-06',2096,'Beitragszahlung H.Murjan Ratenzahlung',50.00,50.00,37,1,6,1,NULL),
(537,'2024-05-22',2099,'Eingang Eigenanteil Trikots Nachbestellung',187.00,187.00,44,29,15,1,NULL),
(538,'2024-05-25',2103,'Spendenquittung Thönniges-Differenz 2401032834',360.50,360.50,38,18,10,1,NULL),
(539,'2024-05-25',2105,'Verzicht auf Ehrenamtspauschale ',840.00,840.00,38,13,46,1,NULL),
(540,'2024-05-31',2109,'Verzicht auf Ehrenamtspauschale F.Melerra',840.00,840.00,38,13,46,1,NULL),
(541,'2024-06-03',2110,'Beitragszahlung H.Murjan Ratenzahlung',50.00,50.00,37,1,6,1,NULL),
(542,'2024-06-13',2111,'Spende fuer Jugendabteilung aus Spendenschwein',319.57,319.57,38,18,45,1,NULL),
(543,'2024-07-09',2125,'Beitragszahlung H.Murjan Ratenzahlung',50.00,50.00,37,1,6,1,NULL),
(544,'2024-08-02',2130,'Beitragszahlung H.Murjan Ratenzahlung',50.00,50.00,37,1,6,1,NULL),
(545,'2024-08-16',2131,'Eigenanteil Trikots',74.00,74.00,44,27,15,1,NULL),
(546,'2024-08-19',2132,'Eigenanteil Sommerfest',190.00,190.00,41,22,15,1,NULL),
(547,'2024-08-27',2135,'Mitgliedsbeiträge',7820.00,7820.00,37,1,18,1,NULL),
(548,'2024-08-27',2136,'Ursula Hohoff TTC/ Reinbacher',672.00,672.00,37,1,35,1,NULL),
(549,'2024-09-02',2141,'Beitragszahlung H.Murjan Ratenzahlung',50.00,50.00,37,1,6,1,NULL),
(550,'2024-09-05',2143,'Spende fuer 1. Herren Sebastian Dringenberg',360.00,360.00,38,18,42,1,NULL),
(551,'2024-09-08',2145,'Spende W.Brandt ',270.00,270.00,38,19,13,1,NULL),
(552,'2024-10-01',2151,'Beitragszahlung H.Murjan Ratenzahlung',50.00,50.00,37,1,6,1,NULL),
(553,'2024-10-18',2157,'Landessportbund NRW',97.26,97.26,38,10,NULL,1,NULL),
(554,'2024-10-22',2158,'Spende fuer 1. Herren Sebastian Dringenberg',270.00,270.00,38,18,42,1,NULL),
(555,'2024-10-22',2159,'Marko Lukac ReNr 2024-009 (Turnier) Fehlbuchung',100.00,100.00,47,58,37,1,NULL),
(556,'2024-10-30',2160,'Eigenanteil Trikots Nachbestellung',484.00,484.00,44,29,NULL,1,NULL),
(557,'2024-11-11',2166,'Mitgliedsbeitrag Maryna Povalii',80.84,80.84,37,1,35,1,NULL),
(558,'2024-11-19',2168,'Fehlbuchung Carl Dörken Stiftung Turnier',500.00,500.00,47,58,37,1,NULL),
(559,'2024-11-20',2169,'Spende Helmut Diegel 1.Herren Essen',100.00,100.00,38,18,10,1,NULL),
(560,'2024-11-25',2174,'Startgebühren BM Erwachsene Eigenanteil',24.00,24.00,45,42,15,1,NULL),
(561,'2024-11-25',2175,'Spende fuer Jugendabteilung aus Spendenschwein',150.00,150.00,38,18,27,1,NULL),
(562,'2024-11-25',2176,'Startgebühren BM Leistungsklasse Eigenanteil',24.00,24.00,45,42,15,1,NULL),
(563,'2024-12-02',2178,'Spende fuer 1. Herren Sebastian Dringenberg',270.00,270.00,38,18,42,1,NULL),
(564,'2024-12-06',2179,'Spende fuer 1.Herren Helmut Diegel',1000.00,1000.00,38,18,10,1,NULL),
(565,'2024-12-12',2180,'Sportfoerderung Stadt Hagen',123.40,123.40,38,9,NULL,1,NULL),
(566,'2024-12-17',2182,'Fehlbuchung Sparkasse Rech.Nr.:2024-002',500.00,500.00,47,58,37,1,NULL),
(567,'2024-12-27',2185,'Spende der Ehrenamtspauschale Corinne Elsenbruch',840.00,840.00,38,13,38,1,NULL),
(568,'2024-12-30',2189,'Spende W.Brandt ',322.50,322.50,38,19,13,1,NULL),
(569,'2024-12-30',2190,'Spede der Ehrenamtsoauschale Thorsten Hoedtke',840.00,840.00,38,13,1,1,NULL),
(570,'2025-01-02',2200,'Spende für die Jugend Hagen open 2024',26.00,26.00,38,14,NULL,1,NULL),
(571,'2025-01-08',2202,'Beitragszahlung H.Murjan Ratenzahlung',102.00,102.00,37,1,6,1,NULL),
(572,'2025-01-14',2208,'Eigenanteil Trainingsanzüge Mike S. Und Vladimir F.',70.00,70.00,44,27,15,1,NULL),
(574,'2025-01-21',2211,'Spende Damenmannschaft Manfred Elsenbruch',180.00,180.00,38,18,24,1,1),
(575,'2025-01-28',2213,'Kostenerstattung TuS Sundern',600.00,600.00,45,34,28,1,NULL),
(576,'2025-02-04',2220,'Mitgliedsbeiträge 1. Hj 2025',7147.00,7147.00,37,1,18,1,NULL),
(577,'2025-02-10',2225,'½ Tuniereinnahmen Hagen Open 2024',4800.00,4800.00,47,58,NULL,1,NULL),
(578,'2025-02-19',2230,'Ratenzahlung Familie Hrek',65.20,65.20,37,1,7,1,NULL),
(579,'2025-02-26',2232,'Beitragsnachzahlung Toni-Ivano Ramljak',76.00,76.00,37,1,35,1,NULL),
(580,'2025-02-27',2233,'Beitragsnachzahlung Raphaela Schmitz',196.00,196.00,37,1,35,1,NULL),
(581,'2025-03-05',2239,'Ratenzahlung Familie Hrek',35.20,35.20,37,1,7,1,NULL),
(582,'2025-03-06',2240,'Beitragseinzug neue Mitglieder',155.00,155.00,37,1,47,1,NULL),
(583,'2025-03-26',2243,'Kontoauflösung Turnierkonto',19.72,19.72,47,58,NULL,1,NULL),
(584,'2025-04-01',2246,'Ratenzahlung Familie Hrek',35.20,35.20,37,1,7,1,NULL),
(585,'2025-04-02',2247,'Rueckzahlung Bargeldauslage 1.Herren',45.00,45.00,45,38,NULL,1,NULL),
(586,'2025-04-07',2252,'Spende W.Brandt ',337.50,337.50,38,19,13,1,NULL),
(587,'2024-12-06',2254,'Spende Materialkosten Vereinsmeisterschaften',105.60,105.60,38,17,NULL,1,NULL),
(588,'2025-05-02',2268,'Beitragszahlung H.Murjan Ratenzahlung',78.00,78.00,37,1,6,1,NULL),
(589,'2025-05-05',2269,'Ratenzahlung Familie Hrek',35.20,35.20,37,1,7,1,NULL),
(590,'2025-05-27',2272,'Spende Helmut Diegel',2400.00,2400.00,38,18,10,1,NULL),
(591,'2025-06-05',2276,'Spende Bernd Müler',50.00,50.00,38,18,29,1,NULL),
(592,'2025-06-03',2277,'Ratenzahlung Familie Hrek',35.20,35.20,37,1,7,1,NULL),
(593,'2025-06-13',2278,'Kostenerstattung Palii',100.00,100.00,45,34,28,1,NULL),
(594,'2024-01-04',2007,'RG 840651-47 Wir machen Druck-Urkunden Turnier',49.42,-49.42,47,60,NULL,1,NULL),
(595,'2024-01-04',2008,'RG Amazon-Gewebeband Reparatur Banden',9.98,-9.98,44,33,NULL,1,NULL),
(596,'2024-01-04',2009,'Rechnung WTTV Jahresgebühr 2024',1473.09,-1473.09,38,8,NULL,1,NULL),
(597,'2024-01-05',2010,'Rechnung Gali RE2310541',149.63,-149.63,47,57,NULL,1,NULL),
(598,'2024-01-05',2011,'Training + Betreuung Damen 4.Quartal 2023',90.00,-90.00,45,36,5,1,1),
(599,'2024-01-05',2012,'Training + Betreuung Jugend  4.Quartal 2023',1140.00,-1140.00,46,52,NULL,1,NULL),
(600,'2024-01-06',2013,'Übernachtung L.Marcona 10.11.2023 Bar R.Kleinevoss',60.00,-60.00,45,40,NULL,1,NULL),
(601,'2024-01-06',2014,'Übernachtung L.Marcona 18.11.2023 Bar H.Diegel',69.00,-69.00,45,40,NULL,1,NULL),
(602,'2024-01-06',2015,'Übernachtung L.Marcona 25.11.2023 Bar H.Diegel',75.00,-75.00,45,40,NULL,1,NULL),
(603,'2024-01-06',2016,'Übernachtung L.Marcona 02.12..2023 Bar H.Diegel',75.00,-75.00,45,40,NULL,1,NULL),
(604,'2024-01-06',2017,'Barauslage H.Diegel Dank Weihnachtsfeier ',37.05,-37.05,41,64,48,1,NULL),
(605,'2024-01-06',2018,'Barauslage H.Diegel Dank Weihnachtsfeier ',43.99,-43.99,41,64,48,1,NULL),
(606,'2024-01-06',2019,'Barauslage H.Diegel Dank Weihnachtsfeier ',9.99,-9.99,41,64,48,1,NULL),
(607,'2024-01-06',2020,'Barauslage H.Diegel Dönninghaus ',57.00,-57.00,45,47,NULL,1,NULL),
(608,'2024-01-06',2021,'Postwertzeichen H.Diegel',6.40,-6.40,46,56,NULL,1,NULL),
(609,'2024-01-06',2022,'RG:414716 Schöler & Micke ',32.00,-32.00,47,60,NULL,1,NULL),
(610,'2024-01-06',2023,'Barauslage H.Diegel Abbau Turnier Helfer ',71.30,-71.30,45,48,NULL,1,NULL),
(611,'2024-01-06',2024,'Übernachtung L.Marcano 13.01.2024',65.00,-65.00,45,40,NULL,1,NULL),
(612,'2024-01-08',2026,'Einzahlungsgebühr SSK für Kleingeld',7.50,-7.50,37,5,27,1,NULL),
(613,'2024-01-09',2028,'3.Abschlagszahlung L.Marcano ',1500.00,-1500.00,45,39,NULL,1,NULL),
(614,'2024-01-09',2029,'3.Abschlagszahlung D.Seidel',1000.00,-1000.00,45,39,NULL,1,NULL),
(615,'2024-01-11',2030,'Training + Betreuung 4.Quartal Maik Israel',300.00,-300.00,46,53,NULL,1,NULL),
(616,'2024-01-15',2033,'Rech. Weidlich 903602633',866.40,-866.40,47,57,NULL,1,NULL),
(617,'2024-01-21',2036,'Rechnung 1363088 Butterfly Trikots',125.59,-125.59,44,28,NULL,1,NULL),
(618,'2024-01-21',2037,'Rech.202401 Th.Hödtke Flock Trikots',36.00,-36.00,44,30,NULL,1,NULL),
(619,'2024-01-21',2038,'Paket Kosten-Rückversand Trikots Auslage Th.Hödtke',6.99,-6.99,44,28,NULL,1,NULL),
(620,'2024-02-01',2039,'Entgeldabschluss SSK ',15.70,-15.70,37,5,16,1,NULL),
(621,'2024-02-02',2040,'Stadt Hagen Hallennutzung 01.07.23-31.12.2023',556.50,-556.50,38,11,NULL,1,NULL),
(622,'2024-02-06',2045,'Rückbelastung Beiträge R.Biggemann/Kontoänderung',50.94,-50.94,37,3,37,1,NULL),
(623,'2024-02-06',2046,'Rücklastschrift 404 Oksana Strelchenko',62.94,-62.94,37,3,37,1,NULL),
(624,'2024-02-07',2047,'Tineon AG-Vereinsberatung',166.80,-166.80,37,5,17,1,NULL),
(625,'2024-02-08',2048,'Fahrtkosten Maik Israel',514.50,-514.50,45,39,NULL,1,NULL),
(626,'2024-02-12',2049,'3.Abschlagszahlung Fahrtkosten o. Dethiarov ',1000.00,-1000.00,45,39,NULL,1,NULL),
(627,'2024-02-16',2055,'Rechnung Schöler & Micke R 422357',60.00,-60.00,45,38,NULL,1,NULL),
(628,'2024-02-18',2056,'Rücküberweisung Farideh Rostami Beitrag 1.HJ24 (V.Hadi)',60.00,-60.00,37,1,30,1,NULL),
(629,'2024-02-25',2057,'Übernachtung L.Marcano 24.02.-25.02.2024',65.00,-65.00,45,40,NULL,1,NULL),
(630,'2024-02-25',2058,'4.Abschlagzahlung L.Marcano ',600.00,-600.00,45,39,NULL,1,NULL),
(631,'2024-02-29',2059,'Entgeldabschluss SSK ',50.60,-50.60,37,5,16,1,NULL),
(632,'2024-03-09',2061,'Jahresrechnung WTTV 2024',305.00,-305.00,38,8,NULL,1,NULL),
(633,'2024-03-12',2064,'4.Abschlagszahlung Fahrtkosten o. Dethiarov ',1000.00,-1000.00,45,39,NULL,1,NULL),
(634,'2024-03-12',2065,'4.Abschlagszahlung D.Seidel',540.00,-540.00,45,39,NULL,1,NULL),
(635,'2024-03-13',2066,'Verbandsabgabe Turnier Nr. 2023/031',368.00,-368.00,47,60,NULL,1,NULL),
(636,'2024-03-24',2069,'RG Th.Hödtke 202403-Pflege Internetseite 2023 ',350.00,-350.00,39,21,NULL,1,NULL),
(637,'2024-03-24',2070,'Beleg Alexandros Grill ',173.50,-173.50,45,38,NULL,1,NULL),
(638,'2024-03-26',2071,'Training + Betreuung 1.Quartal2024 Ben Runde ',310.50,-310.50,46,53,NULL,1,NULL),
(639,'2024-03-29',2072,'Training + Betreuung 1.Quartal2024 Oxana Fadeeva',1350.00,-1350.00,46,52,NULL,1,NULL),
(640,'2024-03-29',2073,'Training + Betreuung 1.Quartal 2024 Willi Brandt ',360.00,-360.00,45,37,NULL,1,NULL),
(641,'2024-03-31',2077,'Entgeldabschluss SSK 28.03.2024',18.25,-18.25,37,5,16,1,NULL),
(642,'2024-04-05',2079,'Fahrtkosten M.Israel 1.Quartal 2024',400.80,-400.80,45,39,NULL,1,NULL),
(643,'2024-04-05',2080,'Training M.Israel 1.Quartal ',165.00,-165.00,46,53,NULL,1,NULL),
(644,'2024-04-05',2081,'Fahrtkosten + Restzahlung Spielertraining ',600.00,-600.00,45,39,NULL,1,NULL),
(645,'2024-04-05',2082,'Fahtkosten O-Dethiarov ',1300.00,-1300.00,45,39,NULL,1,NULL),
(646,'2024-04-08',2083,'Training + Betreuung I. Quartal 2024- Th. Hödtke',327.30,-327.30,46,52,NULL,1,NULL),
(647,'2024-04-12',2085,'City Hotel by Celina (L.Marcano)',65.00,-65.00,45,40,NULL,1,NULL),
(648,'2024-04-12',2086,'Butterfly RG 1367703',164.07,-164.07,44,29,NULL,1,NULL),
(649,'2024-04-18',2087,'RG:91196520 Dogado Domain ',15.48,-15.48,39,20,NULL,1,NULL),
(650,'2024-04-22',2088,'Beleg vom 13.04.2024-Aufstiegsessen ',100.00,-100.00,45,34,NULL,1,NULL),
(651,'2024-05-02',2093,'Entgeldabschluss SSK ',9.75,-9.75,37,5,16,1,NULL),
(652,'2024-05-03',2094,'myTischtennis GmbH-TXID1048821231',15.00,-15.00,45,34,NULL,1,NULL),
(653,'2024-05-15',2097,'Barauslage Kuhnert-Vorhängeschloss',3.49,-3.49,45,47,NULL,1,NULL),
(654,'2024-05-15',2098,'Ordnungsstrafe IV. Mannschaft',10.00,-10.00,45,34,NULL,1,NULL),
(655,'2024-05-25',2100,'Endrangliste Mittleres Ruhrgebiet Jugendliche ',9.00,-9.00,45,34,NULL,1,NULL),
(657,'2024-05-25',2104,'Ehrenamtspauschale H.Diegel als 1.Vors.',840.00,-840.00,38,13,NULL,1,NULL),
(658,'2024-05-31',2107,'Entgeldabschluss Sparkasse ',8.07,-8.07,37,5,16,1,NULL),
(659,'2024-05-31',2108,'Ehrenamtspauschale F.Melerra als 2.Vors.',840.00,-840.00,38,13,NULL,1,NULL),
(660,'2024-06-28',2112,'Beitragsrechnung Stadtsport Bund',111.60,-111.60,45,45,44,1,NULL),
(661,'2024-06-28',2113,'Fahrkostenabrechnung Thorsten Hödtke Q1',106.80,-106.80,45,38,1,1,NULL),
(662,'2024-06-28',2114,'Vorhängeschloss',12.14,-12.14,45,34,NULL,1,NULL),
(663,'2024-06-28',2115,'Fahrkostenabrechnung Thorsten Hödtke Q2',16.80,-16.80,45,38,1,1,NULL),
(664,'2024-06-28',2116,'Kostenerstattung Rogelio Dringenberg',400.00,-400.00,45,38,28,1,NULL),
(665,'2024-06-28',2117,'Fahrkostenabrechnung Maik Israel Q2',50.10,-50.10,45,38,43,1,NULL),
(666,'2024-06-28',2118,'Kosten Hallenschlüssel',45.00,-45.00,45,34,27,1,NULL),
(667,'2024-07-01',2119,'Entgeldabschluss Sparkasse ',8.25,-8.25,37,5,16,1,NULL),
(668,'2024-07-16',2120,'Training + Betreuung 2.Quartal2024 Ben Runde ',245.80,-245.80,46,53,2,1,NULL),
(669,'2024-07-16',2121,'Training + Jugend 2.Quartal2024 Oxana Fadeeva',1260.00,-1260.00,46,52,5,1,NULL),
(670,'2024-07-16',2122,'Training + Betreuung 2.Quartal 2024 Th. Hödtke',356.10,-356.10,46,52,1,1,NULL),
(671,'2024-07-16',2123,'Ordnungsstrafe Berzirksjugendtag (VS07)',20.00,-20.00,45,34,NULL,1,NULL),
(672,'2024-07-16',2124,'Gamilec Goran Fahrkosten',150.00,-150.00,45,38,33,1,NULL),
(673,'2024-07-25',2126,'Spinfactory R.-Nr.: 100085279',150.00,-150.00,44,31,NULL,1,NULL),
(674,'2024-07-29',2127,'BWL-Rechtsanwaelte',101.96,-101.96,37,68,48,1,NULL),
(675,'2024-07-29',2128,'Abrechnung Training Uyanik Furkan',30.00,-30.00,45,35,12,1,NULL),
(676,'2024-08-01',2129,'Entgeldabschluss Sparkasse ',8.07,-8.07,37,5,16,1,NULL),
(678,'2024-08-26',2134,'Ursula Hohof Z-74/23-13',15.00,-15.00,37,68,48,1,NULL),
(679,'2024-08-28',2137,'Beitragsrückbuchung Frank Melerra',33.01,-33.01,37,3,37,1,NULL),
(680,'2024-08-29',2138,'Beitragsrückbuchung Maryna Povalii',80.84,-80.84,37,3,37,1,NULL),
(681,'2024-08-29',2139,'Beitragsrückbuchung Ilona Latta',80.94,-80.94,37,3,37,1,NULL),
(682,'2024-09-02',2140,'Entgeldabschluss Sparkasse ',24.55,-24.55,37,5,16,1,NULL),
(683,'2024-09-04',2142,'Vorschuss Auslagen 1.Herren',1640.00,-1640.00,45,38,33,1,NULL),
(684,'2024-09-08',2144,'Training+Betreuung W.Brandt 2. Quartal 2024',270.00,-270.00,45,37,13,1,NULL),
(685,'2024-09-30',2146,'Tamasu Butterfly R.Nr: 1374272',209.32,-209.32,44,29,NULL,1,NULL),
(686,'2024-09-30',2147,'Sommerfest TTC Hagen R.-Nr.: 47/2024',436.50,-436.50,41,22,48,1,NULL),
(687,'2024-09-30',2148,'Stadt Hagen Hallennutzung R.-Nr.: 2024-0093',801.00,-801.00,45,45,36,1,NULL),
(688,'2024-09-30',2149,'Rechnung LSB R.-Nr.: 2024/20407',246.51,-246.51,45,46,NULL,1,NULL),
(689,'2024-10-01',2150,'Entgeldabschluss Sparkasse ',8.12,-8.12,37,5,16,1,NULL),
(690,'2024-10-10',2152,'Vorschuss Auslagen 1.Herren',385.00,-385.00,45,38,33,1,NULL),
(691,'2024-10-15',2153,'Training + Betreuung 3.Quartal2024 Ben Runde ',209.50,-209.50,46,53,2,1,NULL),
(692,'2024-10-15',2154,'Training + Betreuung 3.Quartal 2024 Th. Hödtke',366.00,-366.00,46,52,1,1,NULL),
(693,'2024-10-15',2155,'Training Jugend 3.Quartal2024 Evgeny Fadeeva',840.00,-840.00,46,52,5,1,NULL),
(694,'2024-10-15',2156,'Training Damen 3.Quartal2024 Evgeny Fadeeva',90.00,-90.00,45,36,5,1,1),
(696,'2024-10-31',2162,'Tamasu Butterfly R.Nr: 1378974',356.33,-356.33,44,29,NULL,1,NULL),
(697,'2024-10-31',2163,'Fahrkosten Th. Hödtke 3.Quartal',25.80,-25.80,45,39,1,1,NULL),
(698,'2024-11-04',2164,'Entgeldabschluss Sparkasse ',8.97,-8.97,37,5,16,1,NULL),
(699,'2024-11-04',2165,'Uebertrag von B2159 auf Turnierkonto',100.00,-100.00,47,58,37,1,NULL),
(700,'2024-11-18',2167,'Dogado Gmbh ReNr: RG 91356873',143.89,-143.89,39,20,NULL,1,NULL),
(701,'2024-11-22',2170,'Auslagenerstattung Th. Hödtke 1.Herren',2095.00,-2095.00,45,38,33,1,NULL),
(702,'2024-11-25',2171,'Startgebühren BM Jugend + Strafe',64.00,-64.00,45,42,39,1,NULL),
(703,'2024-11-25',2172,'Uebertrag von B2168 auf Turnierkonto',500.00,-500.00,47,58,37,1,NULL),
(704,'2024-11-25',2173,'StartgebührenBM Erwachsene',24.00,-24.00,45,42,40,1,NULL),
(705,'2024-12-02',2177,'Entgeldabschluss Sparkasse ',8.72,-8.72,37,5,16,1,NULL),
(706,'2024-12-13',2181,'Carsten Zulauf ReNr: 2024/34',175.00,-175.00,45,38,41,1,NULL),
(707,'2024-12-17',2183,'Startgebühren Quali DM',34.00,-34.00,45,42,40,1,NULL),
(708,'2024-12-17',2184,'Startgebühren Jugend Rangliste',18.00,-18.00,45,42,39,1,NULL),
(709,'2024-12-30',2186,'Ehrenamtspauschale Corinne Elsenbruch',840.00,-840.00,38,13,38,1,NULL),
(710,'2024-12-30',2187,'Ehrenamtspauschale Thorsten Hoedtke',840.00,-840.00,38,13,1,1,NULL),
(711,'2024-12-30',2188,'Training+Betreuung W.Brandt 4. Quartal 2024',322.50,-322.50,45,37,13,1,NULL),
(712,'2024-12-30',2191,'Entgeldabschluss Sparkasse ',8.77,-8.77,37,5,16,1,NULL),
(713,'2025-01-02',2192,'Butterfly Rechnung 1382877',250.58,-250.58,44,27,NULL,1,NULL),
(715,'2025-01-02',2194,'Fahrkosten Th. Hödtke 4.Quartal',83.40,-83.40,45,39,1,1,NULL),
(716,'2025-01-02',2195,'Training + Betreuung 4.Quartal 2024 Th. Hödtke',332.70,-332.70,46,52,1,1,NULL),
(717,'2025-01-02',2196,'Training 4.Quartal 2024 Mike Rybinski ',75.00,-75.00,46,53,4,1,NULL),
(718,'2025-01-02',2197,'Training + Betreuung 4.Quartal 2024 Ben Runde ',354.90,-354.90,46,53,2,1,NULL),
(719,'2025-01-02',2198,'Training Damen 4.Quartal 2024 Oxana Fadeeva',15.00,-15.00,45,36,5,1,1),
(720,'2025-01-02',2199,'Training Jugend 4.Quartal 2024 Oxana Fadeeva',120.00,-120.00,46,52,5,1,NULL),
(721,'2025-01-02',2201,'Uebertrag von B2182 auf Turnierkonto',500.00,-500.00,47,58,37,1,NULL),
(722,'2025-01-10',2203,'Geburtstagskarten Mitglieder',9.90,-9.90,45,48,NULL,1,NULL),
(723,'2025-01-10',2204,'Th. Hoedtke RchNr. 202409 Beflockung Trikots',222.00,-222.00,44,30,1,1,NULL),
(724,'2025-01-10',2205,'Th. Hoedtke RchNr. 202408 Pflege Internetseite',350.00,-350.00,39,21,1,1,NULL),
(725,'2025-01-10',2206,'WTTV Ordnungsstrafe',20.00,-20.00,45,63,22,1,NULL),
(726,'2025-01-13',2207,'Bargeldauslage 1.Herren Thomas Sander',650.00,-650.00,45,38,33,1,NULL),
(728,'2025-01-23',2212,'Bargeldauszahlung 1. Herren',255.00,-255.00,45,38,33,1,NULL),
(729,'2025-01-31',2214,'Bargeldauszahlung 1. Herren',300.00,-300.00,45,38,33,1,NULL),
(730,'2025-02-03',2215,'Entgeldabschluss Sparkasse ',10.75,-10.75,37,5,16,1,NULL),
(731,'2025-02-03',2216,'Schoeler&Micke BelegNr:481075',88.14,-88.14,45,38,20,1,NULL),
(732,'2025-02-03',2217,'Fahrkosten Furkan Uyanik',70.80,-70.80,45,38,33,1,NULL),
(733,'2025-02-03',2218,'Rechnung Filament Gleiter Tische',31.98,-31.98,44,33,23,1,NULL),
(734,'2025-02-04',2219,'Tinen AG Vereinsverwaltung',166.80,-166.80,37,5,17,1,NULL),
(735,'2025-02-05',2221,'Beitragsrückbuchung Toni-Ivano Ramljak',67.90,-67.90,37,3,37,1,NULL),
(736,'2025-02-05',2222,'WTTV Jahresrechnung 2025 RechNr: 43.565',1673.39,-1673.39,45,34,22,1,NULL),
(737,'2025-02-06',2223,'Beitragsrueckbuchung Raphaela Schmitz',188.84,-188.84,37,3,37,1,NULL),
(738,'2025-02-07',2224,'Bargeldauszahlung 1. Herren',300.00,-300.00,45,38,33,1,NULL),
(739,'2025-02-14',2226,'Hallennutzungsgebuehr RechNr: 2024-0216',561.00,-561.00,45,45,36,1,NULL),
(740,'2025-02-17',2227,'Bargeldauszahlung 1. Herren',320.00,-320.00,45,38,33,1,NULL),
(741,'2025-02-17',2228,'Bargeldauszahlung 1. Herren',1000.00,-1000.00,45,38,33,1,NULL),
(742,'2025-02-18',2229,'Hotelkosten Plamen Vrabonov',207.00,-207.00,45,38,31,1,NULL),
(743,'2025-02-21',2231,'Bargeldauszahlung 1. Herren',300.00,-300.00,45,38,33,1,NULL),
(744,'2025-03-03',2234,'Entgeldabschluss Sparkasse ',24.97,-24.97,37,5,16,1,NULL),
(745,'2025-03-05',2235,'Bargeldauszahlung 1. Herren',300.00,-300.00,45,38,33,1,NULL),
(746,'2025-03-05',2236,'Ordnungsstrafe 2. Herren',10.00,-10.00,45,63,NULL,1,NULL),
(747,'2025-03-05',2237,'Jahrersrechnung Bezirk Mittleres Ruhrgebiet',330.00,-330.00,45,34,34,1,NULL),
(748,'2025-03-05',2238,'Kostenerstattung Beläge Furkan Uyanik',179.75,-179.75,45,38,20,1,NULL),
(749,'2025-03-10',2241,'Fahrkosten Furkan Uyanik',93.90,-93.90,45,38,33,1,NULL),
(750,'2025-03-17',2242,'Bargeldauszahlung 1. Herren',300.00,-300.00,45,38,33,1,NULL),
(751,'2025-03-31',2244,'Bargeldauszahlung 1. Herren',605.00,-605.00,45,38,33,1,NULL),
(752,'2025-04-01',2245,'Entgeldabschluss Sparkasse ',9.17,-9.17,37,5,16,1,NULL),
(753,'2025-04-02',2248,'Hotelkosten Plamen Vrabonov Rg.Nr.:2225013145',207.00,-207.00,45,38,31,1,NULL),
(754,'2025-04-02',2249,'Butterfly Rechnung 13879553',135.90,-135.90,45,35,NULL,1,NULL),
(755,'2025-04-02',2250,'Beitragserstattung Hans-Werner Trost',66.00,-66.00,37,1,30,1,NULL),
(756,'2025-04-07',2251,'Training+Betreuung W.Brandt 1. Quartal 2025',337.50,-337.50,45,37,13,1,NULL),
(757,'2024-12-06',2253,'Vereinsmeisterschaften B18716 + B18718',105.60,-105.60,45,35,NULL,1,NULL),
(758,'2025-04-07',2255,'Training Jugend 1.Quartal 2025 Oxana Fadeeva',180.00,-180.00,46,52,5,1,NULL),
(759,'2025-04-07',2256,'Training Jugend 1.Quartal 2025 Mike Rybinski',125.00,-125.00,46,53,4,1,NULL),
(760,'2025-04-07',2257,'Training Damen 1.Quartal 2025 Oxana Fadeeva',120.00,-120.00,46,52,5,1,NULL),
(761,'2025-04-11',2258,'WTTV Ordnungsstrafen',20.00,-20.00,45,63,NULL,1,NULL),
(762,'2025-04-11',2259,'Training Jugend 1.Quartal 2025 Gabriel Rodouniklis',115.00,-115.00,46,53,3,1,NULL),
(763,'2025-04-11',2260,'Training Jugend + Betreuung 1.Quartal 2025 Ben Runde',336.70,-336.70,46,53,2,1,NULL),
(764,'2025-04-11',2261,'Tamasu Butterfly R.Nr: 1388582',53.84,-53.84,44,28,NULL,1,NULL),
(765,'2025-04-11',2262,'Fahrkosten 1.Herren Saison 24/25 Reinhold Kleinevoss',474.30,-474.30,45,38,21,1,NULL),
(766,'2025-04-22',2263,'Spinfactory R.-Nr.: 100091937',131.30,-131.30,44,31,NULL,1,NULL),
(767,'2025-04-24',2264,'Dogado GmbH ReNr: 91478143',16.68,-16.68,39,20,NULL,1,NULL),
(768,'2025-04-24',2265,'Training + Betreuung 1.Quartal 2025 Th. Hödtke',407.40,-407.40,46,52,1,1,NULL),
(769,'2025-04-24',2266,'Fahrkosten 1. Herren Q1 2025 Th. Hödtke',80.40,-80.40,45,38,1,1,NULL),
(770,'2025-05-02',2267,'Entgeldabschluss Sparkasse ',9.75,-9.75,37,5,16,1,NULL),
(772,'2025-05-05',2271,'myTischtennis Mitgliedschaft',15.00,-15.00,45,34,27,1,NULL),
(773,'2025-05-30',2273,'Landessportbund NRW Renr 2025/15603',221.05,-221.05,45,45,NULL,1,NULL),
(774,'2025-05-30',2274,'WTTV ReNr 25.072 Spielgemeinschaft',50.00,-50.00,45,34,22,1,NULL),
(775,'2025-06-02',2275,'Entgeldabschluss Sparkasse ',8.07,-8.07,37,5,16,1,NULL),
(776,'2025-06-16',2279,'Aufstiegsprämie 3.Herren',50.00,-50.00,45,34,25,1,NULL),
(777,'2025-06-16',2280,'Aufstiegsprämie 1.Damen',50.00,-50.00,45,34,25,1,NULL),
(778,'2025-06-17',2281,'Kartengebueheren Sprarkasse',9.00,-9.00,37,5,26,1,NULL),
(779,'2024-08-23',2133,'Barauslage Altherrenstammtisch',25.10,-25.10,41,66,48,1,NULL),
(780,'2024-10-31',2161,'Barauslage Altherrenstammtisch 25.10.2024',38.40,-38.40,41,66,48,1,NULL),
(781,'2025-01-02',2193,'Auslagenerstattung Th. Hödtke 1.Herren',100.00,-100.00,41,67,NULL,1,NULL),
(782,'2025-01-15',2209,'Eigenanteil Weihnachtsfeier',400.00,400.00,41,64,15,1,NULL),
(783,'2025-01-20',2210,'Weihnachtsfeier TTC Hagen (Kolpinghaus)',800.00,-800.00,41,64,48,1,NULL),
(784,'2025-05-05',2270,'Bargeldauslage Altherrenstammtisch 25.24.2025',36.70,-36.70,41,66,48,1,NULL),
(786,'2024-01-01',1,'Umbuchung aus Geschäftsjahr 2023',5096.66,5096.66,37,70,NULL,1,NULL),
(787,'2024-05-25',2101,'Thönniges RG: 2401032834 ',535.50,-535.50,47,60,NULL,1,NULL);
/*!40000 ALTER TABLE `Transactions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UnitDetails`
--

DROP TABLE IF EXISTS `UnitDetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `UnitDetails` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CostDetails` longtext NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UnitDetails`
--

LOCK TABLES `UnitDetails` WRITE;
/*!40000 ALTER TABLE `UnitDetails` DISABLE KEYS */;
INSERT INTO `UnitDetails` VALUES
(1,'Thorsten Hoedtke'),
(2,'Ben Runde'),
(3,'Gabriel Rodouniklis'),
(4,'Mike Rybinski'),
(5,'Oxana Fadeev'),
(6,'Murjan'),
(7,'Familie Hrek'),
(8,'Mitglieder'),
(9,'Liubov Kyselova'),
(10,'Helmut Diegel'),
(11,'Eckhard Padeck'),
(12,'Furkan Uyanik'),
(13,'Wilhelm Brandt'),
(14,'Hallennutzung'),
(15,'Eigenanteil'),
(16,'Entgeldabschluss'),
(17,'S-Verein'),
(18,'Halbjahresbeitrag'),
(20,'Material'),
(21,'Reinhold Kleinevoss'),
(22,'WTTV'),
(23,'Ersatzteile'),
(24,'Manfred Elsenbruch'),
(25,'Prämie'),
(26,'Kartengebühr'),
(27,'sonstiges'),
(28,'Spielerwechsel'),
(29,'Bernd Müller'),
(30,'Erstattung'),
(31,'Hotelkosten'),
(33,'Fahrkosten'),
(34,'Bezirk'),
(35,'Nachzahlung'),
(36,'Hallennutzung'),
(37,'Fehlbuchung'),
(38,'Corinne Elsenbruch'),
(39,'Jugend'),
(40,'Senioren'),
(41,'Spielerberater'),
(42,'Sebastian Dringenberg'),
(43,'Mike Israel'),
(44,'Beitragsrechnung'),
(45,'Spendenschwein'),
(46,'Frank Melerra'),
(47,'Neue Mitglieder'),
(48,'Kosten');
/*!40000 ALTER TABLE `UnitDetails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
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
INSERT INTO `__EFMigrationsHistory` VALUES
('20250416124754_NewInit','8.0.10'),
('20250819115946_PreventDeleteOfTransactionWhenBasicUnitIsDeleated','8.0.10'),
('20250824132042_AddSpecialItemBalanceAndRemoveSumFromDB','8.0.10');
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

-- Dump completed on 2025-08-29 11:13:54
