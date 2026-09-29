# PVA

## **gemaakt door:**

https://github.com/CavovanMastrigt

https://github.com/WGioW

https://github.com/Serdar1616


### **inhoudsopgave**

Inleiding (Serdar)

Opdrachtbeschrijving (Serdar)

Functioneel ontwerp (Cavo)

Technisch ontwerp (Gio)

Doelen (Cavo)

Risico's en maatregelen (Gio)

# Opdrachtbeschrijving: Rent-a-Car 
(serdar)

In opdracht van de eigenaren Laura Beekman en Mark van Tessel wordt voor het autoverhuurbedrijf *Rent-a-Car* een professionele C#-desktopapplicatie ontwikkeld, ondersteund door een MySQL-database. Het doel van het project is het moderniseren en automatiseren van de interne bedrijfsprocessen en de dienstverlening richting klanten.

De applicatie omvat de volgende kernfunctionaliteiten:
* **Wagenparkbeheer:** Het beheren van het voertuigbestand (inclusief merk, model, kenteken, type, dagprijs en statussen zoals beschikbaar, verhuurd, in onderhoud of buiten gebruik). Ook het instellen van openingstijden, haal- en brengtijden en optionele extra's behoort hiertoe.
* **Reserveringssysteem:** Klanten kunnen auto's reserveren voor een specifieke periode, waarbij het systeem automatisch de beschikbaarheid controleert (rekening houdend met openingstijden, verhuurperiodes en datums). Gemaakte reserveringen kunnen daarnaast worden ingezien, gewijzigd of geannuleerd.
* **Facturatie:** Het automatisch genereren van facturen op basis van de reserveringsperiode, kosten en unieke factuurnummers, die als PDF kunnen worden opgeslagen of geprint.
* **Klantenbeheer:** Het registreren en beheren van klanten (inclusief NAW-gegevens, e-mailadres, wachtwoord en klantstatus zoals actief, gepauzeerd of beëindigd).
* **Dashboard en Rapportages:** Een beveiligd dashboard exclusief voor beheerders met overzichten van onder andere reserveringen per maand, omzet per maand, bezettingsgraad, meest verhuurde auto's en terugkerende klanten.

### Technisch ontwerp
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
* Wachtwoorden worden gesleuteld opgeslagen

* Alleen de gebruiker kan bij hun gegevens

* Bevestiging bij verwijderen of wijzigingen
