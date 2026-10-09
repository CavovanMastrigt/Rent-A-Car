# Normalisatie (Cavo)
# Normalisatie (Serdar)
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

## 2NV
* Role: ID, Name
* User: ID, Identity_ID, Role, Name, LastName, Address, city, password
* Identity: ID, Front_photo, Back_photo
* Car: ID, License_plate, Brand, Model, Price
* Rental: ID, UID, CID,
* Invoice: ID, RID

## 3NV
* Role: ID, Name
* User: ID, Identity_ID, Created_At, Role, Name, LastName, Address, city, password, Email, Phone_number, Status
* Identity: ID, Front_photo, Back_photo, Reviewed_By, Expiration_Date, Status
* Car: ID, License_plate, Brand, Model, Build_date, Price, Status
* Rental: ID, UID, CID, Price_Per_Day, Date_Started, Date_Planned_Ended, Date_End
* Invoice: ID, RID, Total_Price, Expiration_Date, status
