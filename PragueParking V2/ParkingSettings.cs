using System.Collections.Generic;

namespace PragueParking_V2
{
    public class ParkingSettings
    {
        // Standardvärdet är 100 parkeringsplatser.
        public int NumberOfSpots { get; set; } = 100;

        // Anger hur många fordon av samma typ som ryms på en plats.
        public Dictionary<string, int> VehiclesPerSpot { get; set; }
            = new Dictionary<string, int>
            {
                { "CAR", 1 },
                { "MC", 2 }
            };
    }
}