using System;
using System.Text.Json.Serialization;

namespace PragueParking_V2
{
    // Sparar fordonstypen i JSON så rätt klass kan återskapas.
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Car), "CAR")]
    [JsonDerivedType(typeof(MC), "MC")]
    public abstract class Vehicle
    {
        public string RegistrationNumber { get; set; }
        public DateTime ArrivalTime { get; set; }

        protected Vehicle(string registrationNumber)
        {
            RegistrationNumber = registrationNumber;
            ArrivalTime = DateTime.Now;
        }

        public TimeSpan GetParkingDuration(DateTime departureTime)
        {
            return departureTime - ArrivalTime;
        }

        public decimal CalculateFee(
            decimal hourlyRate, DateTime departureTime)
        {
            TimeSpan duration = GetParkingDuration(departureTime);

            if (duration.TotalMinutes <= 10)
            {
                return 0;
            }

            int startedHours = (int)Math.Ceiling(duration.TotalHours);

            return startedHours * hourlyRate;
        }
    }
}