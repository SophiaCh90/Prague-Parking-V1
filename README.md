# Prague Parking V2

Parkeringsprogram i C# för bilar och motorcyklar.

## Starta programmet

Projektet använder .NET 10 och Spectre.Console.

1. Öppna Projekt Inlämningsuppgift.slnx i Visual Studio
   med stöd för .NET 10.
2. Högerklicka på PragueParking V2 och välj
   Set as Startup Project.
3. Starta med Ctrl+F5.

NuGet-paket återställs normalt automatiskt.
PragueParking.Data är klassbiblioteket för filhantering.
V1 finns kvar i lösningen som den tidigare versionen.

## Funktioner

- Parkera, söka, flytta och hämta ut fordon.
- En bil eller två motorcyklar per plats som standard.
- Ankomsttid, parkeringstid och avgift.
- Automatisk sparning efter parkering, flytt och uthämtning.
- Parkeringskarta med Spectre.Console.

Rosa betyder ledigt, vitt delvis upptaget och blått fullt.

## Filer

- settings.json: antal platser och antal fordon per plats.
- prices.txt: timpriser i CZK. Text efter # är kommentarer.
- parking.json: parkerade fordon och ankomsttider.

Filerna kopieras till programmets körmapp vid bygge om
källfilen är nyare. Programmet använder filerna i körmappen.
Menyval 7 visar deras sökvägar.

Ändra prisfilen i körmappen och välj menyval 6 för att läsa
in priserna igen utan omstart.

Konfigurationen läses vid start. Antalet platser och
platsreglerna måste stämma med befintliga parkeringsdata.

## Avgifter

Bil: 20 CZK per påbörjad timme.
MC: 10 CZK per påbörjad timme.

Högst tio minuter är gratis. Därefter räknas påbörjade
timmar från ankomsten, utan avdrag för de första tio minuterna.

## Loggbok

Loggboken för V2 lämnas med som PDF.