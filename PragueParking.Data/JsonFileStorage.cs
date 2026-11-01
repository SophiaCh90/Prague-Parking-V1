using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace PragueParking.Data
{
    public class JsonFileStorage
    {
        public void Save<T>(string filePath, T data)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }

        public T Load<T>(string filePath)
        {
            string json = File.ReadAllText(filePath);

            T? data = JsonSerializer.Deserialize<T>(json);

            if (data is null)
            {
                throw new InvalidDataException(
                    "JSON-filen innehåller ingen giltig data.");
            }

            return data;
        }

        public Dictionary<string, decimal> LoadPrices(string filePath)
        {
            // Skapar en prislista om filen saknas.
            if (!File.Exists(filePath))
            {
                File.WriteAllLines(filePath, new[]
                {
                    "# Pris per påbörjad timme i CZK.",
                    "# Ändra beloppet efter likhetstecknet.",
                    "# Använd punkt om du skriver decimaler.",
                    "# Högst 10 minuters parkering är gratis.",
                    "CAR=20",
                    "MC=10"
                });
            }

            Dictionary<string, decimal> prices =
                new Dictionary<string, decimal>();

            int lineNumber = 0;

            foreach (string line in File.ReadAllLines(filePath))
            {
                lineNumber++;

                // Allt efter # är en kommentar.
                string text = line.Split('#')[0].Trim();

                if (text.Length == 0)
                {
                    continue;
                }

                string[] parts = text.Split('=');

                if (parts.Length != 2)
                {
                    throw new InvalidDataException(
                        $"Fel på rad {lineNumber} i prislistan. " +
                        "Använd exempelvis CAR=20.");
                }

                string vehicleType = parts[0].Trim().ToUpperInvariant();

                if (vehicleType.Length == 0
                    || !decimal.TryParse(
                        parts[1].Trim(),
                        NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out decimal hourlyRate)
                    || hourlyRate < 0)
                {
                    throw new InvalidDataException(
                        $"Ogiltigt pris på rad {lineNumber}.");
                }

                if (!prices.TryAdd(vehicleType, hourlyRate))
                {
                    throw new InvalidDataException(
                        $"Fordonstypen {vehicleType} finns flera gånger.");
                }
            }

            if (!prices.ContainsKey("CAR") || !prices.ContainsKey("MC"))
            {
                throw new InvalidDataException(
                    "Prislistan måste innehålla CAR och MC.");
            }

            return prices;
        }
    }
}