-- User story 8 follow-up: require a front AND back photo of the ID.
-- Front photo/type/name already exist (Front_Photo, File_Type, File_Name).
-- Add the type and file name for the back photo (Back_Photo already exists).

ALTER TABLE `identity`
  ADD COLUMN `Back_File_Type` varchar(10) NULL,
  ADD COLUMN `Back_File_Name` varchar(255) NULL;
