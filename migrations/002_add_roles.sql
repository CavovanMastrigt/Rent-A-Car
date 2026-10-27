-- Adds the two roles the app uses: 0 = Customer, 1 = Admin.
-- Run once on the rentacar database (e.g. in phpMyAdmin > SQL).

-- ID is AUTO_INCREMENT, so without this MySQL would turn ID 0 into the next number.
SET SESSION sql_mode = 'NO_AUTO_VALUE_ON_ZERO';

INSERT INTO `role` (`ID`, `rolename`) VALUES (0, 'Customer'), (1, 'Admin')
ON DUPLICATE KEY UPDATE `rolename` = VALUES(`rolename`);

-- To make someone an admin, register them in the app and then run:
-- UPDATE `user` SET `RID` = 1 WHERE `Email` = 'their@email.com';
