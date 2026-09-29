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
* Userstories Mark (Serdar)
* Userstories Klant (Cavo)
* Userstories Laura (Gio)

# Opdrachtbeschrijving: Rent-a-Car 
(serdar)

In opdracht van de eigenaren Laura Beekman en Mark van Tessel wordt voor het autoverhuurbedrijf *Rent-a-Car* een professionele C#-desktopapplicatie ontwikkeld, ondersteund door een MySQL-database. Het doel van het project is het moderniseren en automatiseren van de interne bedrijfsprocessen en de dienstverlening richting klanten.

De applicatie omvat de volgende kernfunctionaliteiten:
* **Wagenparkbeheer:** Het beheren van het voertuigbestand (inclusief merk, model, kenteken, type, dagprijs en statussen zoals beschikbaar, verhuurd, in onderhoud of buiten gebruik). Ook het instellen van openingstijden, haal- en brengtijden en optionele extra's behoort hiertoe.
* **Reserveringssysteem:** Klanten kunnen auto's reserveren voor een specifieke periode, waarbij het systeem automatisch de beschikbaarheid controleert (rekening houdend met openingstijden, verhuurperiodes en datums). Gemaakte reserveringen kunnen daarnaast worden ingezien, gewijzigd of geannuleerd.
* **Facturatie:** Het automatisch genereren van facturen op basis van de reserveringsperiode, kosten en unieke factuurnummers, die als PDF kunnen worden opgeslagen of geprint.
* **Klantenbeheer:** Het registreren en beheren van klanten (inclusief NAW-gegevens, e-mailadres, wachtwoord en klantstatus zoals actief, gepauzeerd of beëindigd).
* **Dashboard en Rapportages:** Een beveiligd dashboard exclusief voor beheerders met overzichten van onder andere reserveringen per maand, omzet per maand, bezettingsgraad, meest verhuurde auto's en terugkerende klanten.

# Functioneel ontwerp

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

* **Risico:** Fouten in de code bij het mergen
* **Gevolg:** Meerdere aspecten van het programma kunnen kapot gaan
* **Maatregel:** Ieder een eigen branche dat uiteindelijk merged naar de main, in deze branch word ook getest

* **Risico:** Merge conflicten verwijderen belangrijke code
* **Gevolg:** Het programma breekt en we moeten changes terug zetten
* **Maatregel:** Met elkaar goed afstemmen wie bezig is in welke bestand, zodat het zo min mogelijk gebeurt

* **Risico:** Afwezigheid vanwege ziekte
* **Gevolg:** Contact in persoon en problemen kunnen mogelijk niet opgelost worden
* **Maatregel:** Zodra de persoon beter is, met elkaar bellen en hier zoveel mogelijk inhalen

* **Risico:** Onvoldoende communicatie in het projectteam
* **Gevolg:** Er gaan problemen komen doordat wij niet goed de functies hergebruiken
* **Maatregel:** Elke dinsdag kijken wie wat heeft gedaan en waar ze mee bezig gaan

* **Risico:** Onvoldoende tijd spenderen aan bug-testen
* **Gevolg:** Er komen bugs tijdens de presentaties waardoor het programma er niet af uit ziet
* **Maatregel:** Op tijd beginnen, elkaar vragen voor hulp als je vast zit

* **Risico:** Achterlopen van de planning.
* **Gevolg:** Het programma gaat mogelijk niet af zijn voor de deadlines
* **Maatregel:** Samen als team kijken waarom we achterlopen en hoe wij dit samen oplossen
