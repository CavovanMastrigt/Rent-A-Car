-- Messages between customers and staff (a shared inbox: any staff member can
-- reply, and an admin can message a customer directly). One conversation per
-- customer. Idempotent.
CREATE TABLE IF NOT EXISTS `message` (
  `ID`               int NOT NULL AUTO_INCREMENT,
  `Customer_ID`      int NOT NULL,                 -- whose conversation this is
  `From_Staff`       tinyint(1) NOT NULL,          -- 1 = sent by staff, 0 = by the customer
  `Sender_ID`        int NULL,
  `Sender_Name`      varchar(100) NULL,
  `Body`             varchar(1000) NOT NULL,
  `Created_At`       datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Read_By_Staff`    tinyint(1) NOT NULL DEFAULT 0,
  `Read_By_Customer` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`ID`)
);
