# Userstories Mark (Serdar)

US1 – Dagoverzicht
Als medewerker wil ik een automatisch overzicht van de reserveringen van vandaag, zodat ik weet welke auto’s er worden opgehaald en teruggebracht.

Toont klant, auto, ophaal- en retourtijd, status
Datum is te wisselen (gisteren, vandaag, morgen)
Gesorteerd op tijd

US2 – Planning van auto’s
Als operationeel manager wil ik een planning (agenda) per auto, zodat ik zie wie welke auto wanneer heeft.

Weekweergave met alle auto’s
Bezet, vrij, onderhoud en buiten gebruik zijn te onderscheiden
Klik op een blok geeft de reserveringsdetails

US3 – Reservering maken namens klant
Als medewerker wil ik een reservering maken voor een klant aan de balie of telefonisch, zodat klanten niet zelf online hoeven te boeken.

Klant zoeken, auto, periode en extra’s kiezen
Beschikbaarheidscontrole, openingstijden en 1 jaar-limiet gelden ook hier
Een gepauzeerde of geblokkeerde klant kan niet reserveren
Factuur wordt automatisch aangemaakt

US4 – Reservering wijzigen of annuleren namens klant
Als medewerker wil ik reserveringen wijzigen of annuleren, zodat ik klanten kan helpen die dat telefonisch vragen.

Beschikbaarheid wordt opnieuw gecontroleerd en de prijs herberekend
Annuleren volgt de 24-uursregel, met bevestiging

US5 – Ophalen en terugbrengen registreren
Als medewerker wil ik vastleggen dat een auto is opgehaald of teruggebracht, zodat de status altijd klopt.

Reserveringsstatus: Gereserveerd → Opgehaald → Teruggebracht (of Geannuleerd)
Bij ophalen wordt de auto Verhuurd, bij terugbrengen Beschikbaar
Ophalen en terugbrengen alleen binnen openingstijden
Bij schade kan de auto direct op Buiten gebruik worden gezet, met een korte opmerking

US6 – Extra’s en dagprijzen beheren
Als beheerder wil ik extra’s (navigatie, kinderstoeltje, luxe-pakket) en dagprijzen beheren, zodat het aanbod actueel is.

Extra’s toevoegen, bewerken en uit gebruik nemen
Prijswijzigingen gelden alleen voor nieuwe reserveringen (bestaande reserveringen houden hun prijs)

US7 – Rollen en inloggen medewerkers
Als beheerder wil ik medewerkers accounts met een rol geven, zodat iedereen alleen ziet wat hij nodig heeft.

Rollen: klant, medewerker, beheerder
Medewerkers kunnen niet bij dashboard en rapportages (alleen eigenaren)

US8 – Klantgegevens inzien door medewerker
Als medewerker wil ik klantgegevens kunnen zoeken en bekijken, zodat ik snel een klant kan helpen.

Zoeken op naam of e-mail, met overzicht van de reserveringen van de klant
Dit vult doc 2 US4/US5 aan, dus stem af zodat je geen dubbel werk doet

# Userstories Klant (Cavo)

## Userstory 1 – Account aanmaken

*Als klant wil ik een account kunnen aanmaken, zodat ik gebruik kan maken van de website en mijn reserveringen kan beheren.*

**Acceptatiecriteria**

- De klant kan een account aanmaken via een registratieformulier.
- Het formulier bevat minimaal een voornaam, achternaam, e-mailadres en wachtwoord.
- Alle verplichte velden moeten ingevuld zijn voordat het account aangemaakt kan worden.
- Het e-mailadres moet een geldig formaat hebben.
- Een e-mailadres dat al in gebruik is, kan niet opnieuw geregistreerd worden.
- Het wachtwoord moet voldoen aan de wachtwoordvereisten van het systeem.
- Na een succesvolle registratie verschijnt een bevestigingsmelding.
- Het wachtwoord wordt veilig opgeslagen en is niet leesbaar voor anderen.

**Definition of Done**

- Het registratieformulier werkt en alle acceptatiecriteria zijn behaald.
- De invoer wordt gecontroleerd in de interface en op de server.
- Registratie is getest met geldige en ongeldige gegevens.
- Het is getest dat een dubbel e-mailadres niet gebruikt kan worden.

## Userstory 2 – Inloggen

*Als klant wil ik kunnen inloggen met mijn e-mailadres en wachtwoord, zodat ik toegang krijg tot mijn persoonlijke omgeving.*

**Acceptatiecriteria**

- De klant kan inloggen met een geregistreerd e-mailadres en het bijbehorende wachtwoord.
- Bij een succesvolle login wordt de klant doorgestuurd naar de persoonlijke omgeving.
- Bij onjuiste inloggegevens verschijnt een duidelijke foutmelding.
- Een klant kan niet inloggen met een e-mailadres dat niet geregistreerd is.
- De klant kan uitloggen via een uitlogknop.
- Na het uitloggen is de persoonlijke omgeving niet meer toegankelijk zonder opnieuw in te loggen.

**Definition of Done**

- Inloggen en uitloggen zijn getest met geldige en ongeldige gegevens.
- Wachtwoorden worden veilig gecontroleerd en opgeslagen.
- Pagina's met persoonlijke gegevens zijn alleen toegankelijk voor ingelogde gebruikers.
- De foutmeldingen werken correct en geven geen gevoelige informatie prijs.

## Userstory 3 – Persoonlijke gegevens bekijken en wijzigen

*Als klant wil ik mijn persoonlijke gegevens kunnen bekijken en wijzigen, zodat mijn gegevens altijd up-to-date zijn.*

**Acceptatiecriteria**

- De klant kan een pagina openen waarop de eigen persoonsgegevens worden weergegeven.
- De klant kan de beschikbare persoonsgegevens aanpassen.
- Verplichte velden mogen niet leeg worden opgeslagen.
- Het e-mailadres moet een geldig formaat hebben.
- Het systeem controleert of het gewijzigde e-mailadres niet al door een andere klant wordt gebruikt.
- Na het opslaan verschijnt een bevestigingsmelding.
- Gewijzigde gegevens worden direct opgeslagen en zijn daarna zichtbaar.
- De klant kan niet de persoonsgegevens van een andere klant bekijken of wijzigen.

**Definition of Done**

- Het bekijken en wijzigen van persoonsgegevens is getest.
- De invoer wordt zowel in de interface als op de server gecontroleerd.
- De toegangsrechten zijn getest, zodat klanten uitsluitend hun eigen gegevens kunnen wijzigen.
- Foutmeldingen en bevestigingen worden correct weergegeven.

## Userstory 4 – Beschikbare auto's bekijken

*Als klant wil ik beschikbare auto's kunnen bekijken, zodat ik kan kiezen welke auto ik wil huren.*

**Acceptatiecriteria**

- Er is een overzichtspagina waarop klanten beschikbare auto's kunnen bekijken.
- Per auto worden minimaal het merk, model en de prijs per dag getoond.
- Indien beschikbaar wordt ook een foto van de auto getoond.
- Auto's die verhuurd zijn, in onderhoud zijn of buiten gebruik zijn, worden niet als beschikbaar aangeboden.
- De klant kan een auto selecteren om de bijbehorende informatie te bekijken.
- Als er geen auto's beschikbaar zijn, verschijnt de melding "Er zijn momenteel geen auto's beschikbaar".
- De getoonde beschikbaarheid komt overeen met de gegevens in de database.

**Definition of Done**

- Het auto-overzicht werkt en toont de juiste gegevens.
- De beschikbaarheidscontrole is getest met verschillende autostatussen.
- De pagina is getest wanneer er wel en geen auto's beschikbaar zijn.
- Het overzicht werkt correct op desktop en mobiel.

## Userstory 5 – Prijs en informatie van een auto bekijken

*Als klant wil ik de prijs en informatie van een auto kunnen bekijken, zodat ik weet wat de huur van de auto kost.*

**Acceptatiecriteria**

- De klant kan de detailpagina van een auto openen.
- De detailpagina toont het merk, model en de prijs per dag.
- Aanvullende beschikbare informatie, zoals bouwjaar, brandstof en kenmerken, wordt weergegeven.
- De prijs wordt duidelijk weergegeven in euro's.
- De klant kan vanuit de detailpagina verdergaan naar het reserveren van de auto.
- Als de auto niet beschikbaar is, kan de klant deze niet direct reserveren voor een onbeschikbare periode.
- De informatie komt overeen met de gegevens die bij de auto zijn opgeslagen.

**Definition of Done**

- De detailpagina toont de juiste auto-informatie en prijs.
- De getoonde gegevens zijn getest met meerdere auto's.
- De prijs wordt correct weergegeven.
- De pagina werkt op desktop en mobiel.

## Userstory 6 – Een auto reserveren

*Als klant wil ik een auto kunnen reserveren voor een bepaalde periode, zodat ik een auto kan huren wanneer ik deze nodig heb.*

**Acceptatiecriteria**

- De klant kan een beschikbare auto selecteren om te reserveren.
- De klant moet een geldige huurperiode opgeven.
- Het systeem controleert of de auto gedurende de volledige huurperiode beschikbaar is.
- Een reservering kan niet worden gemaakt als de auto in die periode al verhuurd of anderszins niet beschikbaar is.
- Een reservering kan alleen worden aangemaakt als alle verplichte gegevens zijn ingevuld.
- Na het succesvol plaatsen van een reservering verschijnt een bevestigingsmelding.
- De reservering wordt opgeslagen in de database en gekoppeld aan de ingelogde klant.
- De reservering verschijnt in het overzicht van de klant.

**Definition of Done**

- Het reserveringsproces is getest met geldige en ongeldige huurperiodes.
- De beschikbaarheid wordt gecontroleerd voordat de reservering wordt opgeslagen.
- Het systeem voorkomt dubbele reserveringen van dezelfde auto voor overlappende periodes.
- De reservering wordt correct aan de klant gekoppeld.
- De functionaliteit is getest met meerdere auto's en huurperiodes.

## Userstory 7 – Ophaal- en terugbrengtijden aangeven

*Als klant wil ik kunnen aangeven wanneer ik de auto wil ophalen en terugbrengen, zodat de huurperiode duidelijk is.*

**Acceptatiecriteria**

- De klant kan een ophaaldatum en ophaaltijd selecteren.
- De klant kan een retourdatum en retourtijd selecteren.
- De retourdatum en -tijd moeten na de ophaaldatum en -tijd liggen.
- Het systeem controleert of de gekozen tijden binnen de toegestane haal- en brengtijden vallen.
- Het systeem controleert of de auto beschikbaar is tijdens de volledige huurperiode.
- Bij een ongeldige datum of tijd verschijnt een duidelijke foutmelding.
- De gekozen tijden worden bij de reservering opgeslagen.

**Definition of Done**

- De datum- en tijdselectie werkt correct.
- Ongeldige huurperiodes en tijden buiten de toegestane tijden zijn getest.
- De controles worden ook op de server uitgevoerd.
- De opgeslagen tijden komen overeen met de gegevens in de reservering.

## Userstory 8 – Extra opties toevoegen

*Als klant wil ik extra opties kunnen toevoegen aan mijn reservering, zodat ik mijn huur kan aanpassen aan mijn wensen.*

**Acceptatiecriteria**

- De klant kan tijdens het reserveren de beschikbare extra opties bekijken.
- Per extra optie wordt de naam en de bijbehorende prijs getoond.
- De klant kan een of meerdere beschikbare extra opties selecteren.
- De klant kan een geselecteerde optie weer verwijderen voordat de reservering definitief wordt gemaakt.
- De kosten van de geselecteerde opties worden meegenomen in de totale prijs.
- De geselecteerde opties worden opgeslagen bij de reservering.
- Als er geen extra opties beschikbaar zijn, kan de klant de reservering zonder extra opties afronden.

**Definition of Done**

- Het selecteren en verwijderen van extra opties is getest.
- De prijzen van de extra opties worden correct berekend.
- De geselecteerde opties worden correct bij de reservering opgeslagen.
- De totale prijs wordt opnieuw berekend wanneer de klant opties toevoegt of verwijdert.

## Userstory 9 – Beschikbaarheid controleren tijdens het reserveren

*Als klant wil ik tijdens het reserveren kunnen zien of een auto beschikbaar is, zodat ik geen auto reserveer die al verhuurd, in onderhoud of buiten gebruik is.*

**Acceptatiecriteria**

- Het systeem controleert de beschikbaarheid van de geselecteerde auto voor de gekozen huurperiode.
- Auto's met de status In onderhoud of Buiten gebruik kunnen niet worden gereserveerd.
- Het systeem controleert of er al een reservering bestaat die overlapt met de gekozen huurperiode.
- Als een auto niet beschikbaar is, verschijnt een duidelijke melding.
- De klant kan geen reservering afronden voor een onbeschikbare auto.
- De beschikbaarheid wordt opnieuw gecontroleerd wanneer de reservering definitief wordt opgeslagen.
- Als een auto ondertussen door een andere klant is gereserveerd, wordt de nieuwe reservering geweigerd.

**Definition of Done**

- De beschikbaarheidscontrole is getest met verschillende statussen en huurperiodes.
- Overlappende reserveringen worden correct tegengehouden.
- De controle wordt uitgevoerd op de server en niet uitsluitend in de interface.
- Het systeem voorkomt dat twee klanten tegelijkertijd dezelfde auto voor overlappende periodes reserveren.

## Userstory 10 – Eigen reserveringen bekijken

*Als klant wil ik een overzicht van mijn reserveringen kunnen bekijken, zodat ik kan zien welke auto's ik heb gereserveerd en voor welke periode.*

**Acceptatiecriteria**

- De klant kan een overzicht van de eigen reserveringen openen.
- Per reservering worden minimaal de auto, ophaaldatum, retourdatum en reserveringsstatus getoond.
- De klant kan de details van een reservering bekijken.
- Het overzicht toont de reserveringen die bij het account van de klant horen.
- Als de klant nog geen reserveringen heeft, verschijnt de melding "Je hebt nog geen reserveringen".
- De reserveringen worden opgehaald uit de database.
- De klant kan geen reserveringen van andere klanten bekijken.

**Definition of Done**

- Het reserveringsoverzicht toont de juiste reserveringen en gegevens.
- Het overzicht is getest met accounts met en zonder reserveringen.
- De toegangsrechten zijn getest.
- Het overzicht werkt op desktop en mobiel.

## Userstory 11 – Een reservering wijzigen

*Als klant wil ik mijn reservering kunnen wijzigen, zodat ik de reservering kan aanpassen wanneer mijn plannen veranderen.*

**Acceptatiecriteria**

- De klant kan een eigen reservering selecteren om te wijzigen.
- De klant kan de toegestane gegevens van de reservering aanpassen, zoals de huurperiode en extra opties.
- Het systeem controleert opnieuw of de auto beschikbaar is voor de gewijzigde huurperiode.
- De gewijzigde huurperiode moet geldig zijn en binnen de toegestane haal- en brengtijden vallen.
- De totale prijs wordt opnieuw berekend wanneer de huurperiode of extra opties veranderen.
- Als de wijziging niet mogelijk is, verschijnt een duidelijke foutmelding.
- Na een succesvolle wijziging worden de gegevens opgeslagen en verschijnt een bevestiging.
- De klant kan geen reservering van een andere klant wijzigen.

**Definition of Done**

- Het wijzigen van reserveringen is getest met geldige en ongeldige wijzigingen.
- De beschikbaarheid wordt opnieuw gecontroleerd voordat wijzigingen worden opgeslagen.
- De prijs wordt na een wijziging correct berekend.
- De toegangsrechten zijn getest.
- De gewijzigde reservering wordt correct weergegeven in het reserveringsoverzicht.

## Userstory 12 – Een reservering annuleren

*Als klant wil ik mijn reservering kunnen annuleren, zodat ik een reservering kan verwijderen wanneer ik de auto niet meer nodig heb.*

**Acceptatiecriteria**

- De klant kan een eigen annuleerbare reservering selecteren.
- Voordat de reservering wordt geannuleerd, vraagt het systeem om bevestiging.
- Na bevestiging wordt de reservering geannuleerd.
- Na een succesvolle annulering verschijnt een bevestigingsmelding.
- De status van de reservering wordt aangepast naar Geannuleerd.
- De geannuleerde reservering blijft zichtbaar in het overzicht met de juiste status.
- De auto wordt weer beschikbaar voor de betreffende periode als er geen andere blokkades of reserveringen zijn.
- De klant kan geen reservering van een andere klant annuleren.
- Reserveringen die volgens de bedrijfsregels niet meer geannuleerd mogen worden, kunnen niet via deze functie worden geannuleerd.

**Definition of Done**

- Het annuleren van reserveringen is getest.
- De bevestigingsvraag werkt correct.
- De reserveringsstatus wordt correct opgeslagen.
- De beschikbaarheid van de auto wordt na annulering correct bijgewerkt.
- De toegangsrechten en eventuele annuleringsbeperkingen zijn getest.

## Userstory 13 – Totale reserveringsprijs bekijken

*Als klant wil ik de totale prijs van mijn reservering kunnen zien, zodat ik vooraf weet hoeveel de huur gaat kosten.*

**Acceptatiecriteria**

- Tijdens het reserveren wordt een overzicht van de kosten getoond.
- Het overzicht toont de huurperiode, de prijs per dag en de berekende huurkosten.
- Eventuele extra opties en bijbehorende kosten worden afzonderlijk weergegeven.
- De totale prijs wordt automatisch berekend op basis van de huurperiode en de geselecteerde opties.
- Eventuele belastingen of toeslagen worden duidelijk weergegeven als deze van toepassing zijn.
- De prijs wordt opnieuw berekend wanneer de klant de huurperiode of extra opties wijzigt.
- Het totaalbedrag komt overeen met de kosten die bij de reservering worden opgeslagen.
- De prijs wordt duidelijk weergegeven in euro's.

**Definition of Done**

- De prijsberekeningen zijn getest met verschillende huurperiodes en extra opties.
- De berekeningen zijn gecontroleerd met testgegevens waarvan de uitkomst vooraf bekend is.
- Afronding van bedragen gebeurt op een consistente manier.
- De getoonde prijs komt overeen met de opgeslagen reserveringsgegevens.

## Userstory 14 – Eigen facturen bekijken

*Als klant wil ik mijn eigen facturen kunnen bekijken, zodat ik kan controleren waarvoor ik heb betaald.*

**Acceptatiecriteria**

- De klant kan een overzicht van de eigen facturen openen.
- Per factuur worden minimaal het factuurnummer, de factuurdatum en het totaalbedrag getoond.
- De betaalstatus wordt weergegeven wanneer deze beschikbaar is.
- De klant kan een factuur openen om de volledige factuurgegevens te bekijken.
- De factuurgegevens komen overeen met de gegevens die in de database zijn opgeslagen.
- Als de klant geen facturen heeft, verschijnt een duidelijke melding.
- De klant kan geen facturen van andere klanten bekijken.

**Definition of Done**

- Het factuuroverzicht toont de juiste facturen en gegevens.
- Het overzicht is getest met accounts met en zonder facturen.
- De toegangsrechten zijn getest, zodat klanten uitsluitend hun eigen facturen kunnen bekijken.
- De bedragen en factuurgegevens zijn gecontroleerd op juistheid.

## Userstory 15 – Factuur als PDF downloaden

*Als klant wil ik mijn factuur als PDF kunnen downloaden, zodat ik deze digitaal kan bewaren.*

**Acceptatiecriteria**

- De klant kan vanuit de eigen factuur een PDF downloaden.
- De PDF bevat het factuurnummer, de factuurdatum, de bedrijfsgegevens en de klantgegevens.
- De PDF bevat de gegevens van de huur, de huurperiode, de kosten en het totaalbedrag.
- Eventuele belastingen en extra kosten worden duidelijk weergegeven.
- De gegevens in de PDF komen overeen met de opgeslagen factuur.
- De PDF wordt met een duidelijke bestandsnaam aangeboden.
- De klant kan alleen PDF-facturen downloaden die bij het eigen account horen.

**Definition of Done**

- Het downloaden van facturen als PDF is getest.
- De PDF is gecontroleerd op opmaak, leesbaarheid en volledigheid.
- De berekende bedragen komen overeen met de factuurgegevens.
- De toegangsrechten zijn getest, zodat klanten geen facturen van anderen kunnen downloaden.

## Userstory 16 – Openingstijden en haal- en brengtijden bekijken

*Als klant wil ik kunnen zien wat de openingstijden en haal- en brengtijden zijn, zodat ik weet wanneer ik mijn auto kan ophalen en terugbrengen.*

**Acceptatiecriteria**

- De openingstijden zijn zichtbaar op de website.
- De haal- en brengtijden worden duidelijk weergegeven.
- De tijden zijn per dag inzichtelijk.
- Dagen waarop het bedrijf gesloten is, worden duidelijk aangegeven.
- De weergegeven tijden komen overeen met de ingestelde bedrijfsgegevens.
- Tijdens het reserveren kan de klant geen ophaal- of retourtijd selecteren die buiten de toegestane tijden valt.
- Als er afwijkende openingstijden gelden, worden deze duidelijk weergegeven wanneer deze door het bedrijf zijn ingesteld.

**Definition of Done**

- De openingstijden en haal- en brengtijden worden correct weergegeven.
- De getoonde informatie komt overeen met de opgeslagen instellingen.
- De controles op toegestane reserveringstijden zijn getest.
- De informatie is goed leesbaar op desktop en mobiel.

## Userstory 17 – Duidelijke foutmeldingen ontvangen

*Als klant wil ik duidelijke meldingen krijgen wanneer ik iets verkeerd invoer of een reservering niet mogelijk is, zodat ik weet wat ik moet aanpassen.*

**Acceptatiecriteria**

- Bij ontbrekende verplichte gegevens verschijnt een duidelijke foutmelding.
- Bij ongeldige gegevens wordt aangegeven welk veld aangepast moet worden.
- Bij onjuiste inloggegevens verschijnt een melding dat het inloggen niet is gelukt.
- Wanneer een auto niet beschikbaar is, wordt dit duidelijk gemeld.
- Bij een ongeldige huurperiode verschijnt een melding waarin wordt uitgelegd wat er niet klopt.
- Bij een mislukte opslag krijgt de klant een melding dat de actie niet is gelukt.
- Bij een succesvolle actie, zoals het plaatsen of wijzigen van een reservering, verschijnt een bevestigingsmelding.
- Foutmeldingen zijn begrijpelijk en bevatten geen technische details of gevoelige informatie.

**Definition of Done**

- De foutmeldingen zijn getest voor de verschillende formulieren en functionaliteiten.
- De meldingen worden op het juiste moment weergegeven.
- De invoervalidatie werkt zowel in de interface als op de server.
- De meldingen zijn duidelijk en begrijpelijk voor klanten.

## Userstory 18 – Eigen gegevens en reserveringen beveiligen

*Als klant wil ik alleen toegang hebben tot mijn eigen gegevens en reserveringen, zodat mijn persoonlijke gegevens beschermd blijven.*

**Acceptatiecriteria**

- Een klant moet ingelogd zijn om persoonlijke gegevens, reserveringen en facturen te bekijken.
- Een klant kan alleen de eigen persoonsgegevens bekijken en wijzigen.
- Een klant kan alleen de eigen reserveringen bekijken, wijzigen en annuleren, voor zover deze acties zijn toegestaan.
- Een klant kan alleen de eigen facturen bekijken en downloaden.
- Het aanpassen van een URL of een identificatienummer geeft geen toegang tot gegevens van andere klanten.
- Verzoeken zonder de juiste toegangsrechten worden geweigerd.
- Na het uitloggen kan de klant de beveiligde pagina's niet opnieuw openen zonder in te loggen.
- Gevoelige gegevens worden niet onnodig getoond of opgeslagen.

**Definition of Done**

- De toegangsrechten zijn getest met meerdere klantaccounts.
- Het is getest dat klanten geen gegevens van andere klanten kunnen bekijken of aanpassen.
- De toegangscontrole wordt op de server afgedwongen.
- Niet-geautoriseerde verzoeken worden correct geweigerd.
- De beveiliging is gecontroleerd voor persoonsgegevens, reserveringen en facturen.

---

# Algemene Definition of Done (geldt voor alle klant-userstories)

- Alle acceptatiecriteria van de betreffende userstory zijn behaald.
- De code staat op GitHub en is gemerged naar de hoofdbranch.
- De code is gereviewd door minimaal één teamlid.
- De functionaliteit is getest en alle relevante tests slagen.
- Er zijn geen bekende bugs die de werking van de functionaliteit verhinderen.
- Alleen gebruikers met de juiste rechten hebben toegang tot de functionaliteit.
- De interface is gebruiksvriendelijk en werkt op desktop en mobiel.
- De code is netjes, leesbaar en voorzien van commentaar waar nodig.
- De userstory is gedemonstreerd aan en goedgekeurd door de product owner.
- De userstory is op Done gezet in het scrumbord.


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

## Userstory 4 – Klanten registreren
* Als manager wil ik klanten kunnen registreren, zodat ik een klant kan verder helpen met het maken van een account

**Acceptatiecriteria**
* De manager kan een klant registreren met voornaam, achternaam, e-mail, telefoonnummer, adres en geboortedatum
* Het e-mailadres moet geldig en uniek zijn
* De klant moet minimaal 18 jaar zijn (of de minimumleeftijd die het bedrijf hanteert)
* Na registratie ontvangt de klant een e-mail om zelf een wachtwoord in te stellen; de manager ziet of kiest het wachtwoord nooit
* Een nieuwe klant krijgt automatisch de status Niet geverifieerd
* Na het opslaan verschijnt een bevestiging en staat de klant in het klantenoverzicht

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
