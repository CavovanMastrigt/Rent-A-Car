-- Customers get a postal code (e.g. 7606 ER), shown on the invoice under the
-- street and city. Run once on the rentacar database.
ALTER TABLE `user`
  ADD COLUMN `Postal_Code` varchar(10) NOT NULL DEFAULT '';
