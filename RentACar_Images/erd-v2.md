# ERD v2 – Rent-a-Car (samengevoegd)

Basis: ERD van Cavo, aangevuld met Identity (Gio), autotype, extra's, `CreatedBy` en geplande/werkelijke datums.

```mermaid
erDiagram
    Role ||--o{ User : "has"
    User ||--o{ Identity : "verified by"
    User ||--o{ Identity : "reviews"
    User ||--o{ Rental : "rents"
    User ||--o{ Rental : "created by (staff)"
    CarType ||--o{ Car : "classifies"
    Car ||--o{ Rental : "is rented"
    Rental ||--o| Invoice : "is invoiced"
    Rental ||--o{ RentalOption : "includes"
    ExtraOption ||--o{ RentalOption : "is chosen"

    Role {
        int ID PK
        varchar Name UK "customer, employee, admin"
    }

    User {
        int ID PK
        int RoleID FK
        varchar FirstName
        varchar LastName
        varchar Email UK
        varchar PasswordHash
        varchar PhoneNumber
        varchar Address
        varchar PostalCode
        varchar City
        enum Status "active, paused, terminated"
        datetime CreatedAt
    }

    Identity {
        int ID PK
        int UserID FK
        int ReviewedBy FK "nullable"
        varchar FrontPhoto
        varchar BackPhoto
        date ExpirationDate
        enum Status "pending, verified, failed"
    }

    CarType {
        int ID PK
        varchar Name UK "sedan, cabrio, SUV, luxury"
    }

    Car {
        int ID PK
        int CarTypeID FK
        varchar LicensePlate UK
        varchar Brand
        varchar Model
        int BuildYear
        varchar Fuel
        decimal PricePerDay
        enum Status "available, rented, maintenance, out_of_service"
        boolean IsArchived "soft delete"
    }

    Rental {
        int ID PK
        int UserID FK "customer"
        int CarID FK
        int CreatedBy FK "nullable, staff member"
        datetime DatePlannedStart
        datetime DatePlannedEnd
        datetime DatePickedUp "nullable"
        datetime DateReturned "nullable"
        enum Status "reserved, ongoing, completed, cancelled"
        decimal PricePerDay "snapshot"
        decimal TotalPrice
        datetime CreatedAt
    }

    ExtraOption {
        int ID PK
        varchar Name
        varchar Description
        decimal Price
        boolean IsActive
    }

    RentalOption {
        int RentalID PK, FK
        int OptionID PK, FK
        decimal OptionPrice "snapshot"
    }

    Invoice {
        int ID PK
        int RentalID FK, UK "1 invoice per rental"
        varchar InvoiceNumber UK
        date InvoiceDate
        date PeriodStart
        date PeriodEnd
        date ExpirationDate
        decimal TotalPrice
        enum Status "open, paid, overdue"
    }

    BusinessHours {
        int ID PK
        tinyint DayOfWeek UK "1 = Monday ... 7 = Sunday"
        time OpeningTime "NULL if closed"
        time ClosingTime "NULL if closed"
        boolean IsClosed
    }
```

## Wijzigingen t.o.v. Cavo's ERD

| Wijziging | Reden |
|---|---|
| `ExtraOption` + `RentalOption` toegevoegd aan het diagram | Extra's staan in de casus |
| `CarType` + `Car.CarTypeID` | Autotype staat in de casus |
| `Rental.CreatedBy` | Reservering namens klant door medewerker (US3) |
| `DatePlannedStart/End` + `DatePickedUp/DateReturned` | Ophalen/terugbrengen registreren en te laat inleveren (US5) |
| `Car.IsArchived` | Auto's met reserveringen niet echt verwijderen |
| `User.PostalCode` | NAW-gegevens volledig |
| `Identity` (van Gio) | Optioneel, alleen als ID-verificatie gebouwd wordt |
| `Invoice.PeriodStart/PeriodEnd` | Casus vraagt periodedatum op de factuur |
| `BusinessHours.DayOfWeek` als tinyint | Was varchar(20) |

## Opmerkingen

- `Invoice.RentalID` is uniek, dus een reservering heeft maximaal één factuur.
- `ExtraOption.Price` is een bedrag per reservering (zoals bij Cavo). Wil je team een prijs per dag, pas dan de berekening in `TotalPrice` aan.
- Overlapcontrole bij reserveren (reserveringen met status ≠ cancelled):

```sql
SELECT 1 FROM Rental
WHERE CarID = @carId
  AND Status <> 'cancelled'
  AND DatePlannedStart < @newEnd
  AND DatePlannedEnd   > @newStart;
```
