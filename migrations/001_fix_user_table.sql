-- Fixes the user table so that registering works.
-- Run once on the rentacar database (e.g. in phpMyAdmin > SQL).

-- These foreign keys pointed the wrong way: a user could only be inserted
-- after an identity row existed, and a role only after a user had it.
ALTER TABLE `user` DROP FOREIGN KEY `user_ibfk_1`;
ALTER TABLE `role` DROP FOREIGN KEY `role_ibfk_1`;
ALTER TABLE `identity` ADD CONSTRAINT `identity_user_fk` FOREIGN KEY (`UserID`) REFERENCES `user` (`ID`);

-- RID was UNIQUE, so only one user could ever have a given role.
ALTER TABLE `user` DROP INDEX `Role_ID`, ADD INDEX `Role_ID` (`RID`);

-- Room for a real street address, and keep the leading 0 of phone numbers.
ALTER TABLE `user` MODIFY `Address` varchar(100) NOT NULL;
ALTER TABLE `user` MODIFY `Phone_Number` varchar(20) NOT NULL;
