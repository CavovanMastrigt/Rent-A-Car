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
