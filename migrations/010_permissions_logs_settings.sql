-- Three new tables for: employee permissions, an activity log, and app settings
-- (used for the SMTP e-mail configuration). Idempotent; safe to run more than once.

CREATE TABLE IF NOT EXISTS `user_permission` (
  `UserID` int NOT NULL,
  `Perm`   varchar(40) NOT NULL,
  PRIMARY KEY (`UserID`, `Perm`)
);

CREATE TABLE IF NOT EXISTS `activity_log` (
  `ID`         int NOT NULL AUTO_INCREMENT,
  `Created_At` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `User_ID`    int NULL,
  `User_Name`  varchar(100) NULL,
  `Action`     varchar(60) NOT NULL,
  `Details`    varchar(255) NULL,
  PRIMARY KEY (`ID`)
);

CREATE TABLE IF NOT EXISTS `app_setting` (
  `Setting_Key`   varchar(60) NOT NULL,
  `Setting_Value` varchar(500) NULL,
  PRIMARY KEY (`Setting_Key`)
);

-- Give existing employees the default rights so they keep their current access.
INSERT INTO `user_permission` (`UserID`, `Perm`)
SELECT u.ID, p.perm
FROM `user` u
CROSS JOIN (SELECT 'reservations' AS perm UNION SELECT 'verification' UNION SELECT 'invoices') p
WHERE u.RID = 2
  AND NOT EXISTS (SELECT 1 FROM `user_permission` up WHERE up.UserID = u.ID);
