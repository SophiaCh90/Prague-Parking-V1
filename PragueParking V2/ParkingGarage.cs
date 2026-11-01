using System.Collections.Generic;

namespace PragueParking_V2
{
    public class ParkingGarage
    {
        public List<ParkingSpot> ParkingSpots { get; set; }

        public ParkingSettings Settings { get; private set; }

        public ParkingGarage(ParkingSettings settings)
        {
            Settings = settings;
            ParkingSpots = new List<ParkingSpot>();

            for (int number = 1; number <= settings.NumberOfSpots; number++)
            {
                ParkingSpots.Add(new ParkingSpot(number));
            }
        }

        // Gör att tidigare tester med ett antal platser fungerar.
        public ParkingGarage(int numberOfSpots)
            : this(new ParkingSettings { NumberOfSpots = numberOfSpots })
        {
        }

        public int FindVehicle(string registrationNumber)
        {
            foreach (ParkingSpot spot in ParkingSpots)
            {
                foreach (Vehicle vehicle in spot.Vehicles)
                {
                    if (vehicle.RegistrationNumber == registrationNumber)
                    {
                        return spot.Number;
                    }
                }
            }

            return 0;
        }

        public int ParkVehicle(Vehicle vehicle)
        {
            if (FindVehicle(vehicle.RegistrationNumber) != 0)
            {
                return -1;
            }

            foreach (ParkingSpot spot in ParkingSpots)
            {
                if (spot.CanPark(vehicle, Settings))
                {
                    spot.Vehicles.Add(vehicle);
                    return spot.Number;
                }
            }

            return 0;
        }

        public int RemoveVehicle(string registrationNumber)
        {
            foreach (ParkingSpot spot in ParkingSpots)
            {
                for (int i = 0; i < spot.Vehicles.Count; i++)
                {
                    if (spot.Vehicles[i].RegistrationNumber
                        == registrationNumber)
                    {
                        spot.Vehicles.RemoveAt(i);
                        return spot.Number;
                    }
                }
            }

            return 0;
        }

        public string MoveVehicle(
            string registrationNumber, int newSpotNumber)
        {
            int currentSpotNumber = FindVehicle(registrationNumber);

            if (currentSpotNumber == 0)
            {
                return "Fordonet hittades inte.";
            }

            if (newSpotNumber < 1 || newSpotNumber > ParkingSpots.Count)
            {
                return "Ogiltigt platsnummer.";
            }

            if (currentSpotNumber == newSpotNumber)
            {
                return "Fordonet står redan på den platsen.";
            }

            ParkingSpot source = ParkingSpots[currentSpotNumber - 1];
            ParkingSpot destination = ParkingSpots[newSpotNumber - 1];

            for (int i = 0; i < source.Vehicles.Count; i++)
            {
                Vehicle vehicle = source.Vehicles[i];

                if (vehicle.RegistrationNumber == registrationNumber)
                {
                    if (!destination.CanPark(vehicle, Settings))
                    {
                        return "Fordonet får inte plats i den rutan.";
                    }

                    destination.Vehicles.Add(vehicle);
                    source.Vehicles.RemoveAt(i);

                    return $"Fordonet har flyttats från plats " +
                        $"{currentSpotNumber} till plats {newSpotNumber}.";
                }
            }

            return "Fordonet hittades inte.";
        }
    }
}