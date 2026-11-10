-- User story 3 - Auto's toevoegen, bewerken en verwijderen.
-- Adds the extra details the add/edit form needs plus a soft-delete flag so an
-- archived car keeps its old rentals and invoices. Run once on rentacar.
ALTER TABLE `car`
  ADD COLUMN `Fuel_Type` varchar(30) NULL,
  ADD COLUMN `Photo` longblob NULL,
  ADD COLUMN `Archived` tinyint(1) NOT NULL DEFAULT 0;
