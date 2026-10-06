# PVA

## **gemaakt door:**

* https://github.com/CavovanMastrigt
* https://github.com/WGioW
* https://github.com/Serdar1616


# **inhoudsopgave**
* Opdrachtbeschrijving (Serdar)
* Functioneel ontwerp (Cavo)
* Technisch ontwerp (Gio)
* Doelen (Cavo)
* Risico's en maatregelen (Gio)
* Planning (Cavo)
* User Stories
* User Story (Cavo)
* User Story (Gio)
* User Story (Serdar)

# Opdrachtbeschrijving: Rent-a-Car 
(serdar)

OpdrachtbeschrijvingIn opdracht van de eigenaren Laura Beekman en Mark van Tessel bouwen we een C#-programma op de computer, gekoppeld aan een MySQL-database. Het doel is om het autoverhuurbedrijf Rent-a-Car te vernieuwen en alle handelingen digitaal te regelen.   Het programma krijgt de volgende belangrijke onderdelen:Wagenparkbeheer: Alle auto's netjes bijhouden (zoals merk, model, kenteken, prijs en of een auto beschikbaar, verhuurd, in onderhoud of stuk is). Ook kun je hier de openingstijden, haal- en brengtijden en extra's zoals navigatie instellen.   Reserveringssysteem: Klanten kunnen zelf auto's reserveren voor een bepaalde periode. Het programma controleert dan meteen of de auto vrij is en of het binnen de openingstijden valt. Klanten kunnen hun reservering daarna ook bekijken, wijzigen of annuleren.   Facturatie: Het programma maakt automatisch rekeningen (facturen) op basis van de gehuurde periode en kosten, met een uniek nummer. Je kunt deze facturen meteen opslaan als PDF of uitprinten.   Klantenbeheer: Het opslaan en beheren van alle klantgegevens (naam, adres, e-mail, wachtwoord) en de status van de klant (of ze actief zijn, op pauze staan of gestopt zijn).   Dashboard en Rapportages: Een speciaal overzicht alleen voor de bazen. Hierop zie je handige cijfers zoals het aantal reserveringen per maand, de totale omzet, hoe vaak auto's verhuurd zijn (bezettingsgraad), de populairste auto's en vaste klanten.  

# Functioneel ontwerp
# **rollen**

# **manager**

* autos toevoegen verwijderen en wijzigen
* de beschikbaarheid van autos beheren
* klanten bekijken en beheren
* reserveringen bekijken, wijzigen en annuleren
* Facturen bekijken en genereren.
* Openingstijden en haal- en brengtijden beheren.
* Extra opties voor verhuur beheren.
* Het dashboard en de rapportages bekijken.

# **klant**

* Een account aanmaken en inloggen.
* Persoonlijke gegevens bekijken en wijzigen.
* Beschikbare auto's bekijken.
* Een auto reserveren.
* Eigen reserveringen bekijken.
* Een reservering wijzigen of annuleren.
* Eigen facturen bekijken en downloaden.

# **inloggen**

Gebruikers moeten kunnen inloggen met hun account.
Bij het inloggen voert de gebruiker zijn e-mailadres en wachtwoord in. Het systeem controleert of de ingevoerde gegevens correct zijn.
Na het succesvol inloggen wordt de gebruiker naar de juiste omgeving gestuurd op basis van zijn rol.
Een manager krijgt toegang tot het beheergedeelte.
Een customer krijgt toegang tot het klantgedeelte.
Bij onjuiste inloggegevens wordt een foutmelding weergegeven.

# **accountgegevens klant**

De applicatie moet het mogelijk maken om klantgegevens te registreren en te beheren.
De volgende gegevens kunnen van een klant worden opgeslagen:

* Voornaam
* Achternaam
* Adres
* Postcode
* Woonplaats
* E-mailadres
* Wachtwoord
* Klantstatus

Een klant kan de eigen gegevens bekijken en wijzigen.
Een manager kan klanten bekijken en beheren.
Bij het verwijderen of wijzigen van belangrijke gegevens moet de gebruiker een bevestiging geven.


# **Auto reserveren**

Een klant moet een beschikbare auto kunnen reserveren.

Bij het maken van een reservering kiest de klant:

* De gewenste auto.
* De startdatum van de huurperiode.
* De einddatum van de huurperiode.
* Eventuele extra opties.

Het systeem controleert automatisch of de gekozen auto gedurende de gehele periode beschikbaar is.

Een auto mag niet worden gereserveerd wanneer:

* De auto al verhuurd is tijdens de gekozen periode.
* De auto in onderhoud is.
* De auto buiten gebruik is.
* De gekozen periode buiten de toegestane verhuurperiode valt.

Wanneer de reservering succesvol is aangemaakt, wordt deze opgeslagen en krijgt de klant een overzicht van de reservering.

# **Reserveringen beheren**

Klanten kunnen hun eigen reserveringen bekijken.

Bij een reservering worden onder andere de volgende gegevens weergegeven:

* Auto
* Startdatum
* Einddatum
* Totale huurprijs
* Geselecteerde extra's
* Status van de reservering

Een klant kan een reservering wijzigen of annuleren wanneer dit volgens de regels van Rent-a-Car is toegestaan.

De manager kan alle reserveringen bekijken en beheren.

# **Facturatie**

* Na het maken van een reservering kan het systeem automatisch een factuur genereren.

* De factuur bevat onder andere:

* Uniek factuurnummer
* Klantgegevens
* Gegevens van de gehuurde auto
* Huurperiode
* Dagprijs
* Eventuele extra kosten
* Totale kosten
* Factuurdatum

De factuur kan als PDF-bestand worden opgeslagen en geprint.

Iedere factuur krijgt een uniek factuurnummer zodat facturen van elkaar kunnen worden onderscheiden.


# **Openingstijden en haal- en brengtijden**

De manager kan de openingstijden van Rent-a-Car beheren.

Daarnaast kunnen de tijden worden ingesteld waarop auto's kunnen worden opgehaald en teruggebracht.

Bij het maken van een reservering wordt rekening gehouden met deze tijden.
# Technisch ontwerp
(Giovanni)

**Gebruikte technieken:**

* Database: MYSQL
* frontend: C# forms
* backend: C#
* Versiebeheer: Github

**Rollen:**

* Manager
* Customer

**belangerijke functionaliteiten**
* Inloggen met rollen
* Automatisering van facturen (pdf bestanden)
* Beheer van klanten
* Auto beheer
* Bestellen en zien van Auto's op de homepage

**beveiliging**
* Wachtwoorden worden gesleuteld opgeslagen <br>
* Alleen de gebruiker kan bij hun gegevens
* Bevestiging bij verwijderen of wijzigingen

# Doelen

# Risico's en maatregelen
(Giovanni)

**Risico:** Fouten in de code bij het mergen <br>
**Gevolg:** Meerdere aspecten van het programma kunnen kapot gaan <br>
**Maatregel:** Ieder een eigen branche dat uiteindelijk merged naar de main, in deze branch word ook getest

**Risico:** Merge conflicten verwijderen belangrijke code <br>
**Gevolg:** Het programma breekt en we moeten changes terug zetten <br>
**Maatregel:** Met elkaar goed afstemmen wie bezig is in welke bestand, zodat het zo min mogelijk gebeurt

**Risico:** Afwezigheid vanwege ziekte <br>
**Gevolg:** Contact in persoon en problemen kunnen mogelijk niet opgelost worden <br>
**Maatregel:** Zodra de persoon beter is, met elkaar bellen en hier zoveel mogelijk inhalen

**Risico:** Onvoldoende communicatie in het projectteam <br>
**Gevolg:** Er gaan problemen komen doordat wij niet goed de functies hergebruiken <br>
**Maatregel:** Elke dinsdag kijken wie wat heeft gedaan en waar ze mee bezig gaan

**Risico:** Onvoldoende tijd spenderen aan bug-testen <br>
**Gevolg:** Er komen bugs tijdens de presentaties waardoor het programma er niet af uit ziet <br>
**Maatregel:** Op tijd beginnen, elkaar vragen voor hulp als je vast zit

**Risico:** Achterlopen van de planning. <br>
**Gevolg:** Het programma gaat mogelijk niet af zijn voor de deadlines <br>
**Maatregel:** Samen als team kijken waarom we achterlopen en hoe wij dit samen oplossen
