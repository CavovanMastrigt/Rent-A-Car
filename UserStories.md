# Userstories Mark (Serdar)

* Als operational manager wil ik een handig overzicht zien van welke auto's er vandaag verhuurd zijn en aan wie, zodat ik de planning goed kan regelen*
* Als operational manager wil ik de status van een auto (zoals 'beschikbaar', 'verhuurd' of 'in onderhoud') snel kunnen aanpassen, zodat iedereen direct de juiste informatie ziet*
* Als operational manager wil ik dat alle reserveringen en formulieren digitaal gaan, zodat we minder papierwerk op kantoor hebben*
* Als operational manager wil ik onze openingstijden en haal- en brengtijden kunnen instellen, zodat klanten alleen binnen die tijden een auto kunnen reserveren*

# Userstories Klant (Cavo)
* Als klant wil ik een account kunnen aanmaken.
* Als klant wil ik kunnen inloggen met mijn e-mailadres en wachtwoord, zodat ik toegang krijg tot mijn persoonlijke omgeving.
* Als klant wil ik mijn persoonlijke gegevens kunnen bekijken en wijzigen, zodat mijn gegevens altijd up-to-date zijn
* Als klant wil ik beschikbare auto's kunnen bekijken, zodat ik kan kiezen welke auto ik wil huren.
* Als klant wil ik de prijs en informatie van een auto kunnen bekijken, zodat ik weet wat de huur van de auto kost.
* Als klant wil ik een auto kunnen reserveren voor een bepaalde periode, zodat ik een auto kan huren wanneer ik deze nodig heb.
* Als klant wil ik kunnen aangeven wanneer ik de auto wil ophalen en terugbrengen, zodat de huurperiode duidelijk is.
* Als klant wil ik extra opties kunnen toevoegen aan mijn reservering, zodat ik mijn huur kan aanpassen aan mijn wensen.
* Als klant wil ik tijdens het reserveren kunnen zien of een auto beschikbaar is, zodat ik geen auto reserveer die al verhuurd, in onderhoud of buiten gebruik is.
* Als klant wil ik een overzicht van mijn reserveringen kunnen bekijken, zodat ik kan zien welke auto's ik heb gereserveerd en voor welke periode.
* Als klant wil ik mijn reservering kunnen wijzigen, zodat ik de reservering kan aanpassen wanneer mijn plannen veranderen.
* Als klant wil ik mijn reservering kunnen annuleren, zodat ik een reservering kan verwijderen wanneer ik de auto niet meer nodig heb.
* Als klant wil ik de totale prijs van mijn reservering kunnen zien, zodat ik vooraf weet hoeveel de huur gaat kosten.
* Als klant wil ik mijn eigen facturen kunnen bekijken, zodat ik kan controleren waarvoor ik heb betaald.
* Als klant wil ik mijn factuur als PDF kunnen downloaden, zodat ik deze digitaal kan bewaren.
* Als klant wil ik kunnen zien wat de openingstijden en haal- en brengtijden zijn, zodat ik weet wanneer ik mijn auto kan ophalen en terugbrengen.
* Als klant wil ik duidelijke meldingen krijgen wanneer ik iets verkeerd invoer of een reservering niet mogelijk is, zodat ik weet wat ik moet aanpassen.
* Als klant wil ik alleen toegang hebben tot mijn eigen gegevens en reserveringen, zodat mijn persoonlijke gegevens beschermd blijven.

# Userstories Laura (Gio)

## Userstory 1 – Prestaties en omzet
* Als manager wil ik inzicht hebben in prestaties en omzet, zodat ik weet of mijn bedrijf er goed voor staat

**Acceptatiecriteria**
* Er is een dashboard dat alleen toegankelijk is voor de manager
* Het dashboard toont de totale omzet per dag, week, maand en jaar
* De manager kan een periode kiezen met een begin- en einddatum
* Het dashboard toont het aantal verhuringen in de gekozen periode
* Het dashboard toont de bezettingsgraad van de auto's (% verhuurd t.o.v. beschikbaar)
* Het dashboard toont een top 5 van de meest verhuurde auto's
* De omzet over tijd wordt weergegeven in minimaal één grafiek
* De getoonde cijfers komen overeen met de facturen en verhuringen in de database
* Als er geen gegevens zijn voor de gekozen periode, verschijnt de melding "Geen gegevens voor deze periode" in plaats van een foutmelding

**Definition of Done**
* Gebruikers zonder managerrol krijgen geen toegang tot het dashboard
* De berekeningen zijn getest met testdata waarvan de uitkomst vooraf bekend is
* Het dashboard laadt binnen 3 seconden

## Userstory 2 – Overzicht auto's en status aanpassen
* Als manager wil ik een overzicht kunnen zien van alle auto's en hier hun status aanpassen zodat ik mijn auto's kan beheren

**Acceptatiecriteria**
* Het overzicht toont alle auto's met kenteken, merk, model en status
* Er zijn vaste statussen: Beschikbaar, Verhuurd, In onderhoud en Buiten gebruik
* De manager kan de status direct vanuit het overzicht aanpassen
* Een statuswijziging wordt direct opgeslagen en is meteen zichtbaar in het overzicht
* De manager kan filteren op status en zoeken op kenteken, merk of model
* Een auto met een actieve verhuring kan niet op Beschikbaar gezet worden; het systeem toont dan een melding
* Auto's met de status In onderhoud of Buiten gebruik kunnen niet door klanten geboekt worden

**Definition of Done**
* De statussen worden afgedwongen in de database (enum of check constraint)
* Een statuswijziging past de beschikbaarheid voor klanten direct aan

## Userstory 3 – Auto's toevoegen, bewerken en verwijderen
* Als manager wil ik auto's kunnen toevoegen, bewerken, en verwijderen zodat ik auto's kan aanmaken, aanpassen en verwijderen

**Acceptatiecriteria**
* De manager kan een auto toevoegen via een formulier met kenteken, merk, model, bouwjaar, brandstof, prijs per dag en (optioneel) een foto
* Verplichte velden worden gecontroleerd; bij lege of ongeldige invoer verschijnt een duidelijke foutmelding
* Een kenteken moet uniek zijn; bij een dubbel kenteken verschijnt de melding "Kenteken bestaat al"
* De manager kan alle gegevens van een bestaande auto bewerken en de wijzigingen zijn direct zichtbaar
* Voor het verwijderen vraagt het systeem om een bevestiging ("Weet je het zeker?")
* Een auto met een actieve of toekomstige reservering kan niet verwijderd worden; het systeem toont dan een melding
* Oude verhuringen en facturen van een verwijderde auto blijven bewaard (soft delete / archiveren)

**Definition of Done**
* Toevoegen, bekijken, bewerken en verwijderen zijn getest, ook met foute invoer
* Validatie vindt plaats in de interface én op de server/in de database

## Userstory 4 – Klanten registreren
* Als manager wil ik klanten kunnen registreren, zodat ik een klant kan verder helpen met het maken van een account

**Acceptatiecriteria**
* De manager kan een klant registreren met voornaam, achternaam, e-mail, telefoonnummer, adres en geboortedatum
* Het e-mailadres moet geldig en uniek zijn
* De klant moet minimaal 18 jaar zijn (of de minimumleeftijd die het bedrijf hanteert)
* Na registratie ontvangt de klant een e-mail om zelf een wachtwoord in te stellen; de manager ziet of kiest het wachtwoord nooit
* Een nieuwe klant krijgt automatisch de status Niet geverifieerd
* Na het opslaan verschijnt een bevestiging en staat de klant in het klantenoverzicht

**Definition of Done**
* Wachtwoorden worden alleen gehasht opgeslagen
* Persoonsgegevens worden AVG-proof opgeslagen (alleen wat echt nodig is)
* Getest met geldige invoer, een dubbel e-mailadres en ongeldige invoer

## Userstory 5 – Status van klanten aanpassen
* Als manager wil ik klanten hun status kunnen aanpassen, zodat ik mijn klanten kan beheren

**Acceptatiecriteria**
* Er is een klantenoverzicht met naam, e-mail en status, waarin gezocht en gefilterd kan worden
* Er zijn vaste statussen: Niet geverifieerd, Actief, Geblokkeerd en Inactief
* De manager kan de status van een klant wijzigen
* Bij het blokkeren van een klant is het invullen van een reden verplicht
* Een geblokkeerde klant kan geen nieuwe reservering maken en krijgt hierover een melding
* Lopende verhuringen van een geblokkeerde klant blijven zichtbaar voor de manager
* Bij elke statuswijziging wordt vastgelegd wie de wijziging deed en wanneer

**Definition of Done**
* De rechten per status zijn getest (een geblokkeerde klant kan echt niets boeken)
* De log van statuswijzigingen is opvraagbaar

## Userstory 6 – Openingstijden en bedrijfsinstellingen
* Als manager wil ik openingstijden en bedrijfsinstellingen kunnen aanpassen, zodat ik de klanten informatie kan geven via de homepage over veranderingen

**Acceptatiecriteria**
* Er is een instellingenpagina die alleen toegankelijk is voor de manager
* De manager kan per dag van de week de openingstijden instellen of de dag op Gesloten zetten
* De manager kan afwijkende dagen toevoegen (bijv. feestdagen of vakantiesluiting)
* De manager kan bedrijfsgegevens aanpassen: naam, adres, telefoonnummer en e-mail
* De manager kan een mededeling/banner instellen voor de homepage, met een begin- en einddatum
* Een sluitingstijd mag niet vóór de openingstijd liggen; anders verschijnt een foutmelding
* Na het opslaan zijn de wijzigingen direct zichtbaar op de homepage, zonder dat er code aangepast hoeft te worden

**Definition of Done**
* Instellingen worden opgeslagen in de database en staan niet hardcoded in de code
* De homepage is na een wijziging gecontroleerd op desktop en mobiel

## Userstory 7 – Facturen automatisch genereren
* Als manager wil ik facturen automatisch kunnen genereren via mijn programma, zodat ik zelf weinig papierwerk hoef te doen en het automatisch gaat

**Acceptatiecriteria**
* Na het afronden van een verhuring wordt automatisch een factuur aangemaakt
* De factuur bevat: een uniek oplopend factuurnummer, factuurdatum, bedrijfsgegevens, klantgegevens, auto, huurperiode, prijs per dag, eventuele extra kosten, subtotaal, btw en totaalbedrag
* Het totaalbedrag wordt automatisch en correct berekend (aantal dagen × dagprijs + extra kosten + btw)
* De factuur kan als PDF gedownload worden
* De factuur wordt automatisch naar het e-mailadres van de klant verstuurd
* De manager kan alle facturen bekijken in een overzicht en zoeken op factuurnummer, klant of datum
* De manager kan de betaalstatus van een factuur aanpassen (Open, Betaald, Te laat)
* Een verstuurde factuur kan niet meer gewijzigd worden; correcties gaan via een creditfactuur

**Definition of Done**
* De berekeningen zijn getest met meerdere scenario's (1 dag, meerdere dagen, met en zonder extra kosten)
* Factuurnummers zijn gegarandeerd uniek, ook als er twee facturen tegelijk worden aangemaakt
* De PDF is gecontroleerd op opmaak en volledigheid

## Userstory 8 – Klant verifiëren via ID-kaart
* Als manager wil ik een klant verifiëren via een ID kaart, zodat ik zeker weet wie deze klant is

**Acceptatiecriteria**
* De manager kan bij een klant een ID-kaart (foto/scan) uploaden, of de klant kan deze zelf uploaden
* Alleen afbeeldingen en PDF's tot een maximale bestandsgrootte (bijv. 5 MB) worden geaccepteerd
* De manager kan de gegevens van de ID-kaart vergelijken met de klantgegevens (naam en geboortedatum)
* De manager kan de klant goedkeuren (status wordt Actief) of afkeuren met een verplichte reden
* Een klant met de status Niet geverifieerd kan geen auto huren
* Er wordt vastgelegd wie de verificatie heeft uitgevoerd en wanneer
* Het documentnummer wordt niet volledig opgeslagen; de ID-scan wordt na verificatie verwijderd of afgeschermd

**Definition of Done**
* ID-bestanden zijn alleen toegankelijk voor de manager en worden beveiligd opgeslagen
* De verwerking van ID-gegevens voldoet aan de AVG (BSN wordt niet opgeslagen)
* Getest dat een niet-geverifieerde klant echt geen auto kan huren

## Algemene Definition of Done (geldt voor alle userstories)
* Alle acceptatiecriteria zijn behaald
* De code staat op GitHub en is gemerged naar de hoofdbranch
* De code is gereviewd door minimaal één teamlid
* De functionaliteit is getest (handmatig en waar mogelijk met unit tests) en alle tests slagen
* Er zijn geen bekende bugs of foutmeldingen
* Alleen gebruikers met de juiste rol hebben toegang tot de functionaliteit
* De interface is gebruiksvriendelijk en werkt op desktop en mobiel
* De code is netjes, leesbaar en voorzien van commentaar waar nodig
* De userstory is gedemonstreerd aan en goedgekeurd door de product owner
* De userstory is op Done gezet in het scrumbord
