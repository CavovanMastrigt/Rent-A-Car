-- User story 8 - Klant verifiëren via ID-kaart.
-- Extends the `identity` table so a customer can upload an ID scan and an admin
-- can review it. Run once on the rentacar database.

-- The scan is uploaded first and reviewed later, so most fields become optional.
ALTER TABLE `identity`
  MODIFY `Front_Photo` longblob NULL,
  MODIFY `Back_Photo` longblob NULL,
  MODIFY `Reviewed_By` varchar(100) NULL,
  MODIFY `Expiration_Date` date NULL,
  MODIFY `Document_Number` varchar(255) NULL,
  MODIFY `Identity_Status` varchar(100) NOT NULL DEFAULT 'Pending';

-- New fields:
--   Date_Of_Birth : birth date the customer states from the ID (for comparison)
--   File_Type     : 'image' or 'pdf'
--   File_Name     : original file name of the upload
--   Reviewed_At   : when the admin reviewed it (audit)
--   Reject_Reason : mandatory reason when rejected
ALTER TABLE `identity`
  ADD COLUMN `Date_Of_Birth` date NULL,
  ADD COLUMN `File_Type` varchar(10) NULL,
  ADD COLUMN `File_Name` varchar(255) NULL,
  ADD COLUMN `Reviewed_At` datetime NULL,
  ADD COLUMN `Reject_Reason` varchar(255) NULL;
