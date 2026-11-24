-- Invoices get a payment status: 'open' (default) or 'paid'. An 'open' invoice
-- past its expiration date is shown as overdue ("Verlopen") in the app.
-- Run once on the rentacar database.
ALTER TABLE `invoice`
  ADD COLUMN `Status` varchar(20) NOT NULL DEFAULT 'open';
