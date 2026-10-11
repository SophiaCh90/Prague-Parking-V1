using System;

class Program
{
    static void Main()
    {
        // Väljer bakgrundsfärg och textfärg.
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;

        // Fyller konsolfönstret med den valda bakgrundsfärgen.
        Console.Clear();

        //skapar en array med 100 parkeringsrutor
        string[] parking = new string[100];

        // gör alla parkeringsrutor tomma
        for (int i = 0; i < parking.Length; i++)
        {
            parking[i] = "";
        }

        bool running = true;

        while (running)
        {
            Console.WriteLine();
            // Visar rubriken i cyan.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("PRAGUE PARKING");
            // Återgår till vit text för resten av menyn.
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Parkera fordon");
            Console.WriteLine("2. Flytta fordon");
            Console.WriteLine("3. Hämta ut fordon");
            Console.WriteLine("4. Sök fordon");
            Console.WriteLine("0. Avsluta");
            Console.WriteLine("Välj:");

            string choice = Console.ReadLine()??"";

            switch (choice)
            {
                case "1":
                    {
                        // Läs vilken typ av fordon som ska parkeras.
                        Console.Write("Fordonstyp (CAR för bil, MC för motorcykel): ");
                        string vehicleType = (Console.ReadLine() ?? "").Trim().ToUpper();

                        if (vehicleType != "CAR" && vehicleType != "MC")
                        {
                            Console.WriteLine("Ange CAR eller MC.");
                            break;
                        }

                        Console.Write("Ange registreringsnummer: ");
                        string registration = (Console.ReadLine() ?? "").Trim().ToUpper();

                        if (registration.Length == 0 || registration.Length > 10)
                        {
                            Console.WriteLine("Registreringsnumret måste ha 1–10 tecken.");
                            break;
                        }

                        // Tillåter bokstäver och siffror, inte skiljetecken eller mellanslag.
                        bool validRegistration = true;

                        foreach (char character in registration)
                        {
                            if (!char.IsLetterOrDigit(character))
                            {
                                validRegistration = false;
                                break;
                            }
                        }

                        if (!validRegistration)
                        {
                            Console.WriteLine("Använd bara bokstäver och siffror.");
                            break;
                        }

                        bool alreadyParked = false;

                        // Kontrollerar alla fordon, även när två MC står på samma ruta. 
                        for (int i = 0; i < parking.Length; i++)
                        {
                            if (parking[i] == "")
                            {
                                continue;
                            }

                            string[] vehicles = parking[i].Split('|');

                            foreach (string vehicle in vehicles)
                            {
                                string[] parts = vehicle.Split('#');

                                if (parts[1] == registration)
                                {
                                    alreadyParked = true;
                                    break;
                                }
                            }

                            if (alreadyParked)
                            {
                                break;
                            }
                        }

                        if (alreadyParked)
                        {
                            Console.WriteLine("Registreringsnumret finns redan på parkeringen.");
                            break;
                        }

                        int freeSpace = -1;
                        // Hittar första rutan med ledig plats för fordonet. 
                        for (int i = 0; i < parking.Length; i++)
                        {
                            if (parking[i] == "")
                            {
                                freeSpace = i;
                                break;
                            }

                            // En MC kan också parkeras med en annan MC.
                            if (vehicleType == "MC"
                                && parking[i].StartsWith("MC#")
                                && parking[i].Split('|').Length == 1)
                            {
                                freeSpace = i;
                                break;
                            }
                        }

                        if (freeSpace == -1)
                        {
                            Console.WriteLine("Det finns ingen plats för denna fordonstyp.");
                        }
                        else
                        {
                            string newVehicle = vehicleType + "#" + registration;

                            if (parking[freeSpace] == "")
                            {
                                parking[freeSpace] = newVehicle;
                            }
                            else
                            {
                                //  Separera motorcyklarnas uppgifter med | när de delar en ruta.
                                parking[freeSpace] += "|" + newVehicle;
                            }

                            Console.WriteLine(
                                $"Fordonet har parkerats på plats {freeSpace + 1}."
                            );
                        }

                        break;
                    }
                case "2":
                    {
                        Console.Write("Registreringsnummer på fordonet som ska flyttas: ");
                        string registration =
                            (Console.ReadLine() ?? "").Trim().ToUpper();

                        // -1 betyder att fordonets ruta inte har hittats.
                        int foundSpace = -1;
                        string vehicleToMove = "";

                        for (int i = 0; i < parking.Length; i++)
                        {
                            // Hoppar över tomma parkeringsrutor.
                            if (parking[i] == "")
                            {
                                continue;
                            }

                            string[] vehicles = parking[i].Split('|');

                            foreach (string vehicle in vehicles)
                            {
                                string[] parts = vehicle.Split('#');

                                if (parts[1] == registration)
                                {
                                    foundSpace = i;

                                    // Sparar bara det fordon som ska flyttas.
                                    vehicleToMove = vehicle;
                                    break;
                                }
                            }

                            if (foundSpace != -1)
                            {
                                break;
                            }
                        }

                        if (foundSpace == -1)
                        {
                            Console.WriteLine("Fordonet hittades inte.");
                            break;
                        }

                        Console.Write("Flytta till plats (1–100): ");

                        if (!int.TryParse(Console.ReadLine(), out int newPlace)
                            || newPlace < 1
                            || newPlace > 100)
                        {
                            Console.WriteLine("Ange ett platsnummer mellan 1 och 100.");
                            break;
                        }

                        // Omvandlar platsnumret till arrayindex.
                        int newIndex = newPlace - 1;

                        if (newIndex == foundSpace)
                        {
                            Console.WriteLine("Fordonet står redan på den platsen.");
                            break;
                        }

                        string vehicleType = vehicleToMove.Split('#')[0];

                        // Alla fordon får flyttas till en tom ruta.
                        bool canMove = parking[newIndex] == "";

                        // En MC får också flyttas till en ruta med en ensam MC.
                        if (vehicleType == "MC"
                            && parking[newIndex].StartsWith("MC#")
                            && parking[newIndex].Split('|').Length == 1)
                        {
                            canMove = true;
                        }

                        // Avbryter innan något ändras om fordonet inte får plats.
                        if (!canMove)
                        {
                            Console.WriteLine("Fordonet får inte plats i den rutan.");
                            break;
                        }

                        string remainingVehicle = "";
                        string[] sourceVehicles = parking[foundSpace].Split('|');

                        foreach (string vehicle in sourceVehicles)
                        {
                            string[] parts = vehicle.Split('#');

                            // Behåller eventuell annan MC på den gamla platsen.
                            if (parts[1] != registration)
                            {
                                remainingVehicle = vehicle;
                            }
                        }

                        parking[foundSpace] = remainingVehicle;

                        if (parking[newIndex] == "")
                        {
                            parking[newIndex] = vehicleToMove;
                        }
                        else
                        {
                            // Separerar två MC med | när de delar en ruta.
                            parking[newIndex] += "|" + vehicleToMove;
                        }

                        Console.WriteLine(
                            $"Fordonet har flyttats från plats {foundSpace + 1} " +
                            $"till plats {newPlace}."
                        );

                        break;
                    }
                case "3":
                    {
                        Console.Write("Ange registreringsnummer att hämta ut: ");
                        string registration =
                            (Console.ReadLine() ?? "").Trim().ToUpper();

                        // -1 betyder att fordonets ruta inte har hittats.
                        int foundSpace = -1;

                        for (int i = 0; i < parking.Length; i++)
                        {
                            // Hoppar över tomma rutor.
                            if (parking[i] == "")
                            {
                                continue;
                            }

                            string[] vehicles = parking[i].Split('|');

                            foreach (string vehicle in vehicles)
                            {
                                string[] parts = vehicle.Split('#');

                                if (parts[1] == registration)
                                {
                                    foundSpace = i;
                                    break;
                                }
                            }

                            if (foundSpace != -1)
                            {
                                break;
                            }
                        }

                        if (foundSpace == -1)
                        {
                            Console.WriteLine("Fordonet hittades inte.");
                        }
                        else
                        {
                            // Om rutan inte inehåller annat fordon blir den ledig. 
                            string remainingVehicle = "";

                            string[] vehicles = parking[foundSpace].Split('|');

                            foreach (string vehicle in vehicles)
                            {
                                string[] parts = vehicle.Split('#');

                                // Behåller fordonet som inte ska hämtas ut.
                                if (parts[1] != registration)
                                {
                                    remainingVehicle = vehicle;
                                }
                            }

                            parking[foundSpace] = remainingVehicle;

                            Console.WriteLine(
                                $"Fordonet har hämtats ut från plats {foundSpace + 1}."
                            );
                        }

                        break;
                    }
                case "4":
                    {
                        Console.Write("Ange registreringsnummer att söka efter: ");
                        string registration = (Console.ReadLine() ?? "").Trim().ToUpper();

                        // -1 betyder att fordonets parkeringsruta inte har hittats.
                        int foundSpace = -1;

                        for (int i = 0; i < parking.Length; i++)
                        {
                            // Hoppar över tomma parkeringsrutor.
                            if (parking[i] == "")
                            {
                                continue;
                            }

                            // Delar upp rutan i ett eller två fordon.
                            string[] vehicles = parking[i].Split('|');

                            foreach (string vehicle in vehicles)
                            {
                                // Delar upp fordonstyp och registreringsnummer.
                                string[] parts = vehicle.Split('#');

                                if (parts[1] == registration)
                                {
                                    foundSpace = i;
                                    break;
                                }
                            }

                            // Avslutar sökloopen när fordonet har hittats.
                            if (foundSpace != -1)
                            {
                                break;
                            }
                        }

                        if (foundSpace == -1)
                        {
                            Console.WriteLine("Fordonet hittades inte.");
                        }
                        else
                        {
                            // Omvandlar arrayindex till platsnummer för användaren, exempelvis index 9 till plats 10.
                            Console.WriteLine(
                                $"Fordonet står på plats {foundSpace + 1}."
                            );
                        }

                        break;
                    }

                case "0":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Ogiltigt val. Försök igen.");
                    break;
            } // Slut på switch
        } // Slut på while

        // Väntar på en tangent innan programmet avslutas. 
        Console.Write("\nTryck på en tangent för att avsluta...");
        Console.ReadKey();
    } // Slut på Main
} // Slut på Programmet

               

 

