-- User story 8 - the reviewer takes over the key details from the ID card.
-- Stores the name exactly as it appears on the ID, next to the birth date,
-- expiry date and (masked) document number that are already captured.
-- Run once on the rentacar database, after 004_id_verification.sql.
ALTER TABLE `identity`
  ADD COLUMN `ID_First_Name` varchar(100) NULL,
  ADD COLUMN `ID_Last_Name` varchar(100) NULL;
