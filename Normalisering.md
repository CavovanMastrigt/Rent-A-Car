# Normalisatie (Cavo)
# Normalisatie (Serdar)
# Normalisatie (Gio)
UID = UserID <br>
CID = CarID <br>
IID = InvoiceID <br>
RID = RentalID <br>

identity status = pending, disabled, enabled <br>
user status = active, disabled, temp_disabled <br>
car price = per day - > in Rental opslaan, kan later niet aangepast worden als het aangemaakt is <br>

## 1NV
* Role: ID, Name
* User: ID, Role, Name, LastName, Address, city, password
* Identity: ID, Front_Photo, Back_Photo
* Car: ID, License_plate, Brand, Model, Build_Date, Price
* Rental: ID, Status
* Invoice: ID

## 2NV
* Role: ID, Name
* User: ID, Identity_ID, Role, Name, LastName, Address, city, password
* Identity: ID, Front_photo, Back_photo
* Car: ID, License_plate, Brand, Model, Price
* Rental: ID, UID, CID, Status
* Invoice: ID, RID

## 3NV
* Role: ID, Name
* User: ID, Identity_ID, Created_At, Role, Name, LastName, Address, city, password, Status
* Identity: ID, Front_photo, Back_photo, Status
* Car: ID, License_plate, Brand, Model, Build_date, Price, Status
* Rental: ID, UID, CID, Price_Per_Day, Date_Started, Date_End, Status
* Invoice: ID, RID, Total_Price, Expiration_Date
