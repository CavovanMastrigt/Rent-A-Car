-- Test data for the Rent-A-Car app.
-- Three accounts (all with the password "password"):
--   laura    -> RID 1 (Admin)
--   mark     -> RID 2 (Employee)
--   customer -> RID 0 (Customer)
-- Plus sample rentals + invoices so the manager dashboard has data to show.
-- Run once on the rentacar database. Safe to re-run (idempotent).

SET SESSION sql_mode = 'NO_AUTO_VALUE_ON_ZERO';

-- Make sure all three roles exist.
INSERT INTO `role` (`ID`, `rolename`) VALUES
  (0, 'Customer'), (1, 'Admin'), (2, 'Employee')
ON DUPLICATE KEY UPDATE `rolename` = VALUES(`rolename`);

-- --- Test users -------------------------------------------------------------
-- The password hashes below are PBKDF2-SHA256 (same as AuthHelper) for "password".
INSERT INTO `user`
  (`RID`, `Created_At`, `Password`, `Name`, `LastName`, `Address`, `City`, `Email`, `Phone_Number`, `User_Status`)
VALUES
  (1, CURDATE(), '7/FIgNLKwV97IunQtMH8hQ==.NsvBoIFRPSh/vkwbgBK8QN+Ni947HfEe4JVEPaTeP+0=',
   'Laura', 'Admin', 'Dorpsstraat 1', 'Amsterdam', 'laura@rentacar.com', '0612345601', 'active'),
  (2, CURDATE(), 'lqA5wDrcbcmZvnGw/uKHGQ==.E56OqE4/cUj2cVXd6CJer0LEE/m5RVOAEZPkGsgLnaE=',
   'Mark', 'Employee', 'Kerkweg 2', 'Rotterdam', 'mark@rentacar.com', '0612345602', 'active'),
  (0, CURDATE(), 'Y8wVE0OgXDJFYhh7X1bnWg==.WiyBpfZ7RVM5EZJ82Cmt9TMQ+MZ/j2fpWrPRMsmN66g=',
   'Chris', 'Customer', 'Marktplein 3', 'Utrecht', 'customer@rentacar.com', '0612345603', 'active')
ON DUPLICATE KEY UPDATE
  `RID` = VALUES(`RID`), `Password` = VALUES(`Password`), `Name` = VALUES(`Name`),
  `LastName` = VALUES(`LastName`), `User_Status` = 'active';

SET @cust := (SELECT ID FROM `user` WHERE Email = 'customer@rentacar.com');

-- --- Clean out any previous test rentals/invoices so re-running stays tidy ---
DELETE i FROM `invoice` i
  JOIN `rental` r ON r.ID = i.Rental_ID
  WHERE r.notes LIKE 'TESTDATA%';
DELETE FROM `rental` WHERE notes LIKE 'TESTDATA%';

-- Reset any car we parked in "rented" back to available before re-seeding.
UPDATE `car` SET Car_Status = 'active' WHERE Car_Status = 'rented';

-- --- Completed rentals (these drive revenue + top 5) -------------------------
INSERT INTO `rental`
  (`UserID`, `CarID`, `Price_Per_Day`, `Date_Started`, `Date_Planned_Ending`, `Date_Picked_Up`, `Date_Ended`, `notes`, `Created_By`, `Status`, `Total_Price`)
VALUES
  (@cust,  8, 110.00, '2026-11-02', '2026-11-05', '2026-11-02', '2026-11-05', 'TESTDATA completed', 1, 'completed', 330.00),
  (@cust,  8, 110.00, '2026-10-10', '2026-10-14', '2026-10-10', '2026-10-14', 'TESTDATA completed', 1, 'completed', 440.00),
  (@cust,  8, 110.00, '2026-09-01', '2026-09-03', '2026-09-01', '2026-09-03', 'TESTDATA completed', 1, 'completed', 220.00),
  (@cust,  2,  75.00, '2026-11-03', '2026-11-06', '2026-11-03', '2026-11-06', 'TESTDATA completed', 1, 'completed', 225.00),
  (@cust,  2,  75.00, '2026-10-20', '2026-10-23', '2026-10-20', '2026-10-23', 'TESTDATA completed', 1, 'completed', 225.00),
  (@cust,  2,  75.00, '2026-08-15', '2026-08-18', '2026-08-15', '2026-08-18', 'TESTDATA completed', 1, 'completed', 225.00),
  (@cust,  1,  45.00, '2026-11-01', '2026-11-03', '2026-11-01', '2026-11-03', 'TESTDATA completed', 1, 'completed',  90.00),
  (@cust,  1,  45.00, '2026-10-05', '2026-10-08', '2026-10-05', '2026-10-08', 'TESTDATA completed', 1, 'completed', 135.00),
  (@cust,  4,  80.00, '2026-11-07', '2026-11-09', '2026-11-07', '2026-11-09', 'TESTDATA completed', 1, 'completed', 160.00),
  (@cust, 11,  90.00, '2026-10-25', '2026-10-28', '2026-10-25', '2026-10-28', 'TESTDATA completed', 1, 'completed', 270.00),
  (@cust, 13,  70.00, '2026-09-20', '2026-09-24', '2026-09-20', '2026-09-24', 'TESTDATA completed', 1, 'completed', 280.00),
  (@cust,  5,  35.00, '2026-07-10', '2026-07-15', '2026-07-10', '2026-07-15', 'TESTDATA completed', 1, 'completed', 175.00),
  (@cust, 15,  85.00, '2026-06-01', '2026-06-04', '2026-06-01', '2026-06-04', 'TESTDATA completed', 1, 'completed', 255.00),
  (@cust,  3,  65.00, '2026-11-09', '2026-11-10', '2026-11-09', '2026-11-10', 'TESTDATA completed', 1, 'completed',  65.00);

-- One car currently out on rent -> shows up as "Verhuurd" in the overview.
INSERT INTO `rental`
  (`UserID`, `CarID`, `Price_Per_Day`, `Date_Started`, `Date_Planned_Ending`, `Date_Picked_Up`, `Date_Ended`, `notes`, `Created_By`, `Status`, `Total_Price`)
VALUES
  (@cust, 2, 75.00, '2026-11-09', '2026-11-13', '2026-11-09', NULL, 'TESTDATA active', 1, 'active', 0.00);

UPDATE `car` SET Car_Status = 'rented' WHERE ID = 2;

-- --- Invoices for the completed rentals -------------------------------------
INSERT INTO `invoice` (`Rental_ID`, `Total_Price`, `Expiration_Date`)
SELECT r.ID, r.Total_Price, DATE_ADD(r.Date_Ended, INTERVAL 14 DAY)
FROM `rental` r
WHERE r.notes = 'TESTDATA completed';
