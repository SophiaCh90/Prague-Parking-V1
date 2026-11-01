using System.Collections.Generic;

namespace PragueParking_V2
{
    public class ParkingSpot
    {
        public int Number { get; set; }

        public List<Vehicle> Vehicles { get; set; }

        public ParkingSpot(int number)
        {
            Number = number;
            Vehicles = new List<Vehicle>();
        }

        public bool CanPark(Vehicle vehicle, ParkingSettings settings)
        {
            string vehicleType;

            if (vehicle is Car)
            {
                vehicleType = "CAR";
            }
            else if (vehicle is MC)
            {
                vehicleType = "MC";
            }
            else
            {
                return false;
            }

            if (!settings.VehiclesPerSpot.TryGetValue(
                vehicleType, out int maximumVehicles)
                || maximumVehicles < 1)
            {
                return false;
            }

            // Fordon som delar en plats ska vara av samma typ.
            foreach (Vehicle parkedVehicle in Vehicles)
            {
                if (parkedVehicle.GetType() != vehicle.GetType())
                {
                    return false;
                }
            }

            return Vehicles.Count < maximumVehicles;
        }
    }
}