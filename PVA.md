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
* Planning (Serdar)

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

# Functionele doelen

# Inlog- en rollensysteem

Een doel is om een veilig inlogsysteem te ontwikkelen waarbij gebruikers met hun e-mailadres en wachtwoord kunnen inloggen.
Het systeem moet onderscheid kunnen maken tussen de rollen **Manager** en **Customer**. Na het inloggen krijgt iedere gebruiker alleen toegang tot de onderdelen die bij zijn of haar rol horen.

* Managers krijgen toegang tot het beheergedeelte.
* Klanten krijgen toegang tot het klantgedeelte.
* Bij verkeerde inloggegevens wordt een duidelijke foutmelding weergegeven.
* Gebruikers mogen niet bij gegevens komen die niet voor hun rol bestemd zijn.

# Klantenbeheer

Een doel is om alle klantgegevens digitaal te kunnen opslaan en beheren.
De applicatie moet onder andere de volgende gegevens kunnen opslaan:

* Voornaam
* Achternaam
* Adres
* Postcode
* Woonplaats
* E-mailadres
* Wachtwoord
* Klantstatus

Klanten moeten hun eigen gegevens kunnen bekijken en aanpassen. Managers moeten klanten kunnen bekijken en beheren.
Bij belangrijke wijzigingen of het verwijderen van gegevens moet een bevestiging worden gevraagd om fouten te voorkomen.

# Wagenparkbeheer

Een belangrijk doel is om het volledige wagenpark digitaal te beheren.
Per auto moet relevante informatie kunnen worden opgeslagen, zoals:

* Merk
* Model
* Kenteken
* Dagprijs
* Beschikbaarheidsstatus

Daarnaast moet de status van een auto kunnen aangeven of deze:

* Beschikbaar is
* Verhuurd is
* In onderhoud is
* Buiten gebruik is

Managers moeten auto's kunnen toevoegen, wijzigen en verwijderen. Ook moet de beschikbaarheid van auto's kunnen worden aangepast.

# Reserveringssysteem

Een doel is om klanten zelfstandig auto's te laten reserveren.
Bij het maken van een reservering moet een klant kunnen aangeven:

* Welke auto gewenst is
* Wat de startdatum van de huurperiode is
* Wat de einddatum van de huurperiode is
* Welke extra opties gewenst zijn

Het systeem moet automatisch controleren of de gekozen auto beschikbaar is gedurende de volledige huurperiode.
Een reservering mag niet worden gemaakt wanneer de auto al verhuurd is, in onderhoud is, buiten gebruik is of wanneer de gekozen periode niet binnen de toegestane verhuurperiode valt.

# Reserveringen beheren

Een doel is om bestaande reserveringen overzichtelijk te kunnen beheren.
Klanten moeten hun eigen reserveringen kunnen bekijken en, wanneer dit volgens de regels is toegestaan, een reservering kunnen wijzigen of annuleren.
Bij iedere reservering moet minimaal informatie worden weergegeven over:

* De gehuurde auto
* De startdatum
* De einddatum
* De totale huurprijs
* De gekozen extra's
* De status van de reservering

Managers en Admins moeten alle reserveringen kunnen bekijken, wijzigen en annuleren.

# Automatische facturatie

Een doel is om het maken van facturen zoveel mogelijk te automatiseren.
Na het maken van een reservering moet het systeem een factuur kunnen genereren. Op de factuur moeten onder andere de volgende gegevens staan:

* Een uniek factuurnummer
* De klantgegevens
* De gegevens van de gehuurde auto
* De huurperiode
* De dagprijs
* Eventuele extra kosten
* De totale kosten
* De factuurdatum

De factuur moet als PDF kunnen worden opgeslagen en geprint. Iedere factuur moet een uniek factuurnummer krijgen zodat facturen eenvoudig van elkaar kunnen worden onderscheiden.

# Openingstijden en haal- en brengtijden

Managers moeten de openingstijden en de tijden waarop auto's kunnen worden opgehaald en teruggebracht kunnen instellen en wijzigen.
Het reserveringssysteem moet deze instellingen gebruiken bij het controleren van nieuwe reserveringen. Hierdoor kunnen klanten geen reserveringen maken die buiten de toegestane tijden vallen.

# Dashboard en rapportages

Een doel is om managers inzicht te geven in de prestaties van het verhuurbedrijf.
Het dashboard moet relevante informatie overzichtelijk kunnen weergeven, zoals:

* Het aantal reserveringen per maand
* De totale omzet
* De bezettingsgraad van auto's
* De populairste auto's
* Informatie over vaste klanten

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
