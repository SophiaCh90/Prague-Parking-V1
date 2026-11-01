using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PragueParking.Data;
using Spectre.Console;

namespace PragueParking_V2
{
    class Program
    {
        static readonly JsonFileStorage storage = new JsonFileStorage();

        static readonly string settingsPath = Path.Combine(
            AppContext.BaseDirectory, "settings.json");

        static readonly string pricesPath = Path.Combine(
            AppContext.BaseDirectory, "prices.txt");

        static readonly string parkingPath = Path.Combine(
            AppContext.BaseDirectory, "parking.json");

        static ParkingSettings settings = new ParkingSettings();
        static ParkingGarage garage = null!;

        static Dictionary<string, decimal> prices =
            new Dictionary<string, decimal>();

        static void Main()
        {
            try
            {
                LoadGarage();

                bool running = true;

                while (running)
                {
                    Console.Clear();
                    ShowMap();

                    Console.WriteLine();
                    Console.WriteLine("1. Parkera fordon");
                    Console.WriteLine("2. Flytta fordon");
                    Console.WriteLine("3. Hämta ut fordon");
                    Console.WriteLine("4. Sök fordon");
                    Console.WriteLine("5. Visa prislista");
                    Console.WriteLine("6. Läs in prislistan på nytt");
                    Console.WriteLine("7. Visa filsökvägar");
                    Console.WriteLine("0. Avsluta");
                    Console.Write("Välj: ");

                    string choice = Console.ReadLine() ?? "0";

                    switch (choice)
                    {
                        case "1":
                            ParkVehicle();
                            break;

                        case "2":
                            MoveVehicle();
                            break;

                        case "3":
                            CollectVehicle();
                            break;

                        case "4":
                            SearchVehicle();
                            break;

                        case "5":
                            ShowPrices();
                            break;

                        case "6":
                            ReloadPrices();
                            break;

                        case "7":
                            ShowPaths();
                            break;

                        case "0":
                            running = false;
                            break;

                        default:
                            Console.WriteLine("Ogiltigt menyval.");
                            break;
                    }

                    if (running)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Tryck på Enter för att fortsätta.");
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception exception)
            {
                // Avslutar vid exempelvis ett sparfel, så att användaren
                // inte fortsätter arbeta i tron att ändringarna sparats.
                AnsiConsole.MarkupLine("[red]Programmet stoppades på grund av ett fel.[/]");
                Console.WriteLine(exception.Message);
                Console.WriteLine(
                    "Om felet uppstod vid sparning kan den senaste " +
                    "ändringen saknas i filen.");
                Console.WriteLine("Tryck på Enter för att avsluta.");
                Console.ReadLine();
            }
        }

        static void LoadGarage()
        {
            if (!File.Exists(settingsPath))
            {
                storage.Save(settingsPath, new ParkingSettings());
            }

            settings = storage.Load<ParkingSettings>(settingsPath);
            ValidateSettings();

            prices = storage.LoadPrices(pricesPath);
            garage = new ParkingGarage(settings);

            if (File.Exists(parkingPath))
            {
                List<ParkingSpot> savedSpots =
                    storage.Load<List<ParkingSpot>>(parkingPath);

                ValidateParkingData(savedSpots);
                garage.ParkingSpots = savedSpots;
            }
            else
            {
                SaveGarage();
            }
        }

        static void ValidateSettings()
        {
            if (settings.NumberOfSpots < 1)
            {
                throw new InvalidDataException(
                    "Antalet parkeringsplatser måste vara minst 1.");
            }

            if (settings.VehiclesPerSpot == null
                || !settings.VehiclesPerSpot.ContainsKey("CAR")
                || !settings.VehiclesPerSpot.ContainsKey("MC"))
            {
                throw new InvalidDataException(
                    "Konfigurationen måste innehålla CAR och MC.");
            }

            foreach (var entry in settings.VehiclesPerSpot)
            {
                if (entry.Value < 1)
                {
                    throw new InvalidDataException(
                        "Antalet fordon per plats måste vara minst 1.");
                }
            }
        }

        static void ValidateParkingData(List<ParkingSpot> spots)
        {
            if (spots.Count != settings.NumberOfSpots)
            {
                throw new InvalidDataException(
                    "Antalet platser i datafilen stämmer inte med " +
                    "konfigurationen. Återställ inställningen innan start.");
            }

            HashSet<string> registrations =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < spots.Count; i++)
            {
                ParkingSpot spot = spots[i];

                if (spot == null || spot.Number != i + 1
                    || spot.Vehicles == null)
                {
                    throw new InvalidDataException(
                        "Datafilen innehåller en ogiltig parkeringsplats.");
                }

                ParkingSpot checkSpot = new ParkingSpot(spot.Number);

                foreach (Vehicle vehicle in spot.Vehicles)
                {
                    if (vehicle == null
                        || !IsValidRegistration(vehicle.RegistrationNumber)
                        || !registrations.Add(vehicle.RegistrationNumber))
                    {
                        throw new InvalidDataException(
                            "Datafilen innehåller ett ogiltigt eller " +
                            "dubblerat registreringsnummer.");
                    }

                    if (!checkSpot.CanPark(vehicle, settings))
                    {
                        throw new InvalidDataException(
                            $"Fordonen på plats {spot.Number} " +
                            "stämmer inte med platsreglerna.");
                    }

                    if (vehicle.ArrivalTime == default
                        || vehicle.ArrivalTime > DateTime.Now)
                    {
                        throw new InvalidDataException(
                            "Datafilen innehåller en ogiltig ankomsttid.");
                    }

                    checkSpot.Vehicles.Add(vehicle);
                }
            }
        }

        static void SaveGarage()
        {
            storage.Save(parkingPath, garage.ParkingSpots);
        }

        static string ReadRegistration()
        {
            Console.Write("Registreringsnummer: ");

            return (Console.ReadLine() ?? "")
                .Trim()
                .ToUpperInvariant();
        }

        static bool IsValidRegistration(string registration)
        {
            return !string.IsNullOrWhiteSpace(registration)
                && registration.Length <= 10
                && registration.All(char.IsLetterOrDigit);
        }

        static void ParkVehicle()
        {
            Console.Write("Fordonstyp (CAR eller MC): ");

            string type = (Console.ReadLine() ?? "")
                .Trim()
                .ToUpperInvariant();

            if (type != "CAR" && type != "MC")
            {
                Console.WriteLine("Ange CAR eller MC.");
                return;
            }

            string registration = ReadRegistration();

            if (!IsValidRegistration(registration))
            {
                Console.WriteLine(
                    "Använd 1–10 tecken, endast bokstäver och siffror.");
                return;
            }

            Vehicle vehicle;

            if (type == "CAR")
            {
                vehicle = new Car(registration);
            }
            else
            {
                vehicle = new MC(registration);
            }

            int result = garage.ParkVehicle(vehicle);

            if (result == -1)
            {
                Console.WriteLine("Registreringsnumret finns redan.");
            }
            else if (result == 0)
            {
                Console.WriteLine("Det finns ingen plats för fordonet.");
            }
            else
            {
                SaveGarage();
                Console.WriteLine($"Fordonet parkerades på plats {result}.");
            }
        }

        static void MoveVehicle()
        {
            string registration = ReadRegistration();

            Console.Write(
                $"Flytta till plats (1–{garage.ParkingSpots.Count}): ");

            if (!int.TryParse(Console.ReadLine(), out int destination))
            {
                Console.WriteLine("Platsnumret måste vara ett heltal.");
                return;
            }

            string message = garage.MoveVehicle(
                registration, destination);

            SaveGarage();
            Console.WriteLine(message);
        }

        static void CollectVehicle()
        {
            string registration = ReadRegistration();
            int spotNumber = garage.FindVehicle(registration);

            if (spotNumber == 0)
            {
                Console.WriteLine("Fordonet hittades inte.");
                return;
            }

            Vehicle vehicle = garage.ParkingSpots[spotNumber - 1]
                .Vehicles.First(v => v.RegistrationNumber == registration);

            DateTime departureTime = DateTime.Now;
            TimeSpan duration = vehicle.GetParkingDuration(departureTime);
            string type = vehicle is Car ? "CAR" : "MC";

            decimal fee = vehicle.CalculateFee(
                prices[type], departureTime);

            garage.RemoveVehicle(registration);
            SaveGarage();

            Console.WriteLine($"Fordonet hämtades ut från plats {spotNumber}.");
            Console.WriteLine(
                $"Parkeringstid: {(int)duration.TotalHours} timmar " +
                $"och {duration.Minutes} minuter.");
            Console.WriteLine($"Avgift: {fee} CZK.");
        }

        static void SearchVehicle()
        {
            string registration = ReadRegistration();
            int spotNumber = garage.FindVehicle(registration);

            if (spotNumber == 0)
            {
                Console.WriteLine("Fordonet hittades inte.");
                return;
            }

            Vehicle vehicle = garage.ParkingSpots[spotNumber - 1]
                .Vehicles.First(v => v.RegistrationNumber == registration);

            Console.WriteLine($"Fordonet står på plats {spotNumber}.");
            Console.WriteLine(
                $"Ankomst: {vehicle.ArrivalTime:yyyy-MM-dd HH:mm:ss}");
        }

        static void ShowPrices()
        {
            Table table = new Table();
            table.AddColumn("Fordon");
            table.AddColumn("CZK per påbörjad timme");

            table.AddRow("Bil", prices["CAR"].ToString());
            table.AddRow("MC", prices["MC"].ToString());

            AnsiConsole.Write(table);
            Console.WriteLine("Högst 10 minuter är gratis.");
        }

        static void ReloadPrices()
        {
            try
            {
                // Ersätter priserna först när hela filen har lästs.
                Dictionary<string, decimal> updatedPrices =
                    storage.LoadPrices(pricesPath);

                prices = updatedPrices;
                Console.WriteLine("Prislistan har lästs in på nytt.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Prislistan kunde inte läsas: {exception.Message}");
                Console.WriteLine("De tidigare priserna används fortfarande.");
            }
        }

        static void ShowPaths()
        {
            Console.WriteLine($"Konfiguration: {settingsPath}");
            Console.WriteLine($"Prislista: {pricesPath}");
            Console.WriteLine($"Parkeringsdata: {parkingPath}");
        }

        static void ShowMap()
        {
            AnsiConsole.MarkupLine("[bold cyan]PRAGUE PARKING V2[/]");

            Table map = new Table();
            map.Border(TableBorder.Square);
            map.ShowRowSeparators();
            map.HideHeaders();

            for (int column = 0; column < 10; column++)
            {
                map.AddColumn(new TableColumn("").Centered());
            }

            int empty = 0;
            int partial = 0;
            int full = 0;

            List<string> row = new List<string>();

            foreach (ParkingSpot spot in garage.ParkingSpots)
            {
                string color;
                string status;

                if (spot.Vehicles.Count == 0)
                {
                    color = "hotpink";
                    status = "LEDIG";
                    empty++;
                }
                else
                {
                    string type = spot.Vehicles[0] is Car ? "CAR" : "MC";
                    int maximum = settings.VehiclesPerSpot[type];

                    if (spot.Vehicles.Count < maximum)
                    {
                        color = "white";
                        status = "DEL";
                        partial++;
                    }
                    else
                    {
                        color = "dodgerblue1";
                        status = "FULL";
                        full++;
                    }
                }

                string numberLine = $"  {spot.Number:D3}  ";
                string statusLine = $" {status.PadRight(5)} ";

                row.Add(
                    $"[black on {color}]{numberLine}\n{statusLine}[/]");

                if (row.Count == 10)
                {
                    map.AddRow(row.ToArray());
                    row.Clear();
                }
            }

            if (row.Count > 0)
            {
                while (row.Count < 10)
                {
                    row.Add("");
                }

                map.AddRow(row.ToArray());
            }

            AnsiConsole.Write(map);

            AnsiConsole.MarkupLine(
      $"[hotpink]Tomma: {empty}[/]  " +
      $"[white]Delvis upptagna: {partial}[/]  " +
      $"[dodgerblue1]Fulla: {full}[/]");
        }
    }
}