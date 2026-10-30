# Normalisatie (Cavo)

**UID = UserID**  
**CID = CarID**  
**RID = RentalID**  
**IID = InvoiceID**  
**ROID = RentalOptionID**  
**OID = OptionID**  
**RoleID = RoleID**  
**BID = BusinessHoursID**

### Statussen

- **user status** = active, disabled
- **car status** = available, rented, maintenance, out_of_service
- **rental status** = reserved, confirmed, ongoing, completed, cancelled
- **invoice status** = open, paid, overdue

## 1NV

- Role: ID, Name
- User: ID, RoleID, FirstName, LastName, Email, Password, Phone_Number, Address, City, Status
- Car: ID, License_Plate, Brand, Model, Build_Year, Fuel, Price_Per_Day, Status
- Rental: ID, UserID, CarID, FirstName, LastName, Email, License_Plate, Brand, Model, Price_Per_Day, Date_Started, Date_Planned_Ended, Date_End, Status, Total_Price
- Extra_Option: ID, Name, Description, Price
- Rental_Option: ID, RentalID, OptionID, Option_Name, Option_Price
- Invoice: ID, RentalID, UserID, Invoice_Date, Expiration_Date, Customer_Name, Customer_Address, Car_Name, Date_Started, Date_Planned_Ended, Price_Per_Day, Extra_Costs, Total_Price, Status
- Business_Hours: ID, Day_Of_Week, Opening_Time, Closing_Time, Is_Closed

## 2NV

- Role: ID, Name
- User: ID, RoleID, FirstName, LastName, Email, Password, Phone_Number, Address, City, Status
- Car: ID, License_Plate, Brand, Model, Build_Year, Fuel, Price_Per_Day, Status
- Rental: ID, UserID, CarID, Date_Started, Date_Planned_Ended, Date_End, Status, Price_Per_Day, Total_Price
- Extra_Option: ID, Name, Description, Price
- Rental_Option: ID, RentalID, OptionID, Option_Price
- Invoice: ID, RentalID, Invoice_Date, Expiration_Date, Total_Price, Status
- Business_Hours: ID, Day_Of_Week, Opening_Time, Closing_Time, Is_Closed

## 3NV

- **Role:** ID, Name
- **User:** ID, RoleID, FirstName, LastName, Email, Password, Phone_Number, Address, City, Status, Created_At
- **Car:** ID, License_Plate, Brand, Model, Build_Year, Fuel, Price_Per_Day, Status
- **Rental:** ID, UserID, CarID, Date_Started, Date_Planned_Ended, Date_End, Status, Price_Per_Day, Total_Price, Created_At
- **Extra_Option:** ID, Name, Description, Price
- **Rental_Option:** ID, RentalID, OptionID, Option_Price
- **Invoice:** ID, RentalID, Invoice_Number, Invoice_Date, Expiration_Date, Total_Price, Status
- **Business_Hours:** ID, Day_Of_Week, Opening_Time, Closing_Time, Is_Closed

# Normalisatie (Serdar)

## Afkortingen

| Afkorting | Betekenis |
|---|---|
| UID | UserID |
| CID | CarID |
| TID | CarTypeID |
| RID | RentalID |
| IID | InvoiceID |
| OID | OptionID |
| IDID | IdentityID |
| RoleID | RoleID |
| BID | BusinessHoursID |

## Statussen

- **user status** = active, paused, terminated
- **identity status** = pending, verified, failed
- **car status** = available, rented, maintenance, out_of_service
- **rental status** = reserved, ongoing, completed, cancelled
- **invoice status** = open, paid, overdue

## Uitgangspunten

- **Car price** = per dag, wordt bij het aanmaken van een Rental opgeslagen (`Price_Per_Day`) en kan daarna niet meer wijzigen
- **Option price** = wordt bij het aanmaken van een Rental opgeslagen in `Rental_Option` (`Option_Price`)
- `Date_Planned_End` = geplande einddatum, `Date_Returned` = werkelijke inleverdatum (voor te laat inleveren)
- `Date_Picked_Up` = werkelijke ophaaldatum
- `Created_By` = de medewerker die de reservering voor de klant heeft gemaakt (leeg als de klant zelf reserveert)
- Een Rental heeft maximaal één Invoice

---

## 1NV

Alle waarden zijn atomair, herhalende groepen staan in een aparte tabel en elke tabel heeft een primaire sleutel.

- **Role:** ID, Name
- **User:** ID, Role_Name, FirstName, LastName, Email, Password_Hash, Phone_Number, Address, Postal_Code, City, Status, Created_At
- **Identity:** ID, UserID, Front_Photo, Back_Photo, Reviewed_By_Name, Expiration_Date, Status
- **Car_Type:** ID, Name
- **Car:** ID, Car_Type_Name, License_Plate, Brand, Model, Build_Year, Fuel, Price_Per_Day, Status, Is_Archived
- **Rental:** ID, UserID, FirstName, LastName, Email, CarID, License_Plate, Brand, Model, Created_By_Name, Date_Planned_Start, Date_Planned_End, Date_Picked_Up, Date_Returned, Status, Price_Per_Day, Total_Price, Created_At
- **Extra_Option:** ID, Name, Description, Price, Is_Active
- **Rental_Option:** RentalID, OptionID, Option_Name, Option_Price
- **Invoice:** ID, RentalID, UserID, Customer_Name, Customer_Address, Car_Name, Invoice_Number, Invoice_Date, Period_Start, Period_End, Expiration_Date, Total_Price, Status
- **Business_Hours:** ID, Day_Of_Week, Opening_Time, Closing_Time, Is_Closed

---

## 2NV

Geen gedeeltelijke afhankelijkheid van een deel van een samengestelde sleutel.

Alleen `Rental_Option` heeft een samengestelde sleutel (RID + OID). `Option_Name` hangt alleen af van OID, dus die gaat weg. `Option_Price` blijft staan, want die hangt af van de combinatie RID + OID (prijs op het moment van reserveren). Alle andere tabellen hebben één kolom als primaire sleutel en voldoen daarmee automatisch aan de 2NV.

- **Role:** ID, Name
- **User:** ID, Role_Name, FirstName, LastName, Email, Password_Hash, Phone_Number, Address, Postal_Code, City, Status, Created_At
- **Identity:** ID, UserID, Front_Photo, Back_Photo, Reviewed_By_Name, Expiration_Date, Status
- **Car_Type:** ID, Name
- **Car:** ID, Car_Type_Name, License_Plate, Brand, Model, Build_Year, Fuel, Price_Per_Day, Status, Is_Archived
- **Rental:** ID, UserID, FirstName, LastName, Email, CarID, License_Plate, Brand, Model, Created_By_Name, Date_Planned_Start, Date_Planned_End, Date_Picked_Up, Date_Returned, Status, Price_Per_Day, Total_Price, Created_At
- **Extra_Option:** ID, Name, Description, Price, Is_Active
- **Rental_Option:** RID, OID, Option_Price
- **Invoice:** ID, RentalID, UserID, Customer_Name, Customer_Address, Car_Name, Invoice_Number, Invoice_Date, Period_Start, Period_End, Expiration_Date, Total_Price, Status
- **Business_Hours:** ID, Day_Of_Week, Opening_Time, Closing_Time, Is_Closed

---

## 3NV

Geen kolommen die afhangen van een andere niet-sleutelkolom.

### Wat verandert

- **User:** `Role_Name` hangt af van RoleID, dus vervangen door `RoleID` (FK naar Role)
- **Identity:** `Reviewed_By_Name` hangt af van de beoordelaar, dus vervangen door `Reviewed_By` (FK naar User)
- **Car:** `Car_Type_Name` hangt af van TID, dus vervangen door `CarTypeID` (FK naar Car_Type)
- **Rental:** `FirstName`, `LastName` en `Email` hangen af van UserID, en `License_Plate`, `Brand` en `Model` hangen af van CarID, dus die gaan weg. `Created_By_Name` wordt `Created_By` (FK naar User)
- **Invoice:** `Customer_Name`, `Customer_Address` en `UserID` zijn via Rental en User te vinden, en `Car_Name` via Rental en Car, dus die gaan weg

### Resultaat

- **Role:** ID, Name
- **User:** ID, RoleID, FirstName, LastName, Email, Password_Hash, Phone_Number, Address, Postal_Code, City, Status, Created_At
- **Identity:** ID, UserID, Reviewed_By, Front_Photo, Back_Photo, Expiration_Date, Status
- **Car_Type:** ID, Name
- **Car:** ID, CarTypeID, License_Plate, Brand, Model, Build_Year, Fuel, Price_Per_Day, Status, Is_Archived
- **Rental:** ID, UserID, CarID, Created_By, Date_Planned_Start, Date_Planned_End, Date_Picked_Up, Date_Returned, Status, Price_Per_Day, Total_Price, Created_At
- **Extra_Option:** ID, Name, Description, Price, Is_Active
- **Rental_Option:** RID, OID, Option_Price
- **Invoice:** ID, RentalID, Invoice_Number, Invoice_Date, Period_Start, Period_End, Expiration_Date, Total_Price, Status
- **Business_Hours:** ID, Day_Of_Week, Opening_Time, Closing_Time, Is_Closed

---

## Bewuste keuzes

- `Rental.Price_Per_Day`, `Rental_Option.Option_Price`, `Rental.Total_Price` en `Invoice.Total_Price` zijn vastgelegde waarden op het moment van reserveren of factureren. Een latere prijswijziging in Car of Extra_Option mag bestaande reserveringen en facturen niet veranderen, dus dit is geen overtollige dubbele opslag maar historie
- `Invoice.Period_Start` en `Invoice.Period_End` zijn een vastlegging van de factuurperiode (casus: periodedatum op de factuur)
- `Invoice.RentalID` is uniek: één factuur per reservering

## Sleutels

| Type | Kolommen |
|---|---|
| Primaire sleutels | Role.ID, User.ID, Identity.ID, Car_Type.ID, Car.ID, Rental.ID, Extra_Option.ID, Rental_Option.(RID + OID), Invoice.ID, Business_Hours.ID |
| Vreemde sleutels | User.RoleID, Identity.UserID, Identity.Reviewed_By, Car.CarTypeID, Rental.UserID, Rental.CarID, Rental.Created_By, Rental_Option.RID, Rental_Option.OID, Invoice.RentalID |
| Unieke kolommen | Role.Name, User.Email, Car_Type.Name, Car.License_Plate, Invoice.Invoice_Number, Invoice.RentalID, Business_Hours.Day_Of_Week |

# Normalisatie (Gio)
UID = UserID <br>
CID = CarID <br>
IID = InvoiceID <br>
RID = RentalID <br>

identity status = pending, verified, failed <br>
user status = active, disabled, temp_disabled <br>
car status = active, disabled, maintenance <br>
invoice status = pending, paid

car price = per day - > in Rental opslaan, kan later niet aangepast worden als het aangemaakt is <br>
Date planned ended is voor als er te laat een auto is ingeleverd

## 1NV
* Role: ID, Name
* User: ID, Role, Name, LastName, Address, city, password, Email, Phone_number
* Identity: ID, Front_Photo, Back_Photo, Reviewed_By, Expiration_Date
* Car: ID, License_plate, Brand, Model, Build_Date, Price
* Rental: ID
* Invoice: ID
* OpeningTimes: Time_Open, Time_Closed, Weekday

## 2NV
* Role: ID, Name
* User: ID, Identity_ID, Role, Name, LastName, Address, city, password
* Identity: ID, Front_photo, Back_photo
* Car: ID, License_plate, Brand, Model, Price
* Rental: ID, UID, CID,
* Invoice: ID, RID
* OpeningTimes: Time_Open, Time_Closed, Weekday

## 3NV
* Role: ID, Name
* User: ID, Identity_ID, Created_At, Role, Name, LastName, Address, city, password, Email, Phone_number, Status
* Identity: ID, Front_photo, Back_photo, Reviewed_By, Expiration_Date, Status
* Car: ID, License_plate, Brand, Model, Build_date, Price, Status
* Rental: ID, UID, CID, Price_Per_Day, Date_Started, Date_Planned_Ended, Date_End
* Invoice: ID, RID, Total_Price, Expiration_Date, status
* OpeningTimes: Time_Open, Time_Closed, Weekday
# ERD Gio
![Gio's ERD](/RentACar_Images/ERD_gio.png)
