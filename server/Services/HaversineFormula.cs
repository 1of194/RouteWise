using Microsoft.Extensions.Diagnostics.HealthChecks;
using server.Models;
using System;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;

namespace server.Services
{
    public class HaversineFormula
    {
        public static double Haversine(GeoLocation coord1, GeoLocation coord2)
        {
            const double R = 6371000; // Radius of Earth in meters

            // Convert degrees to radians
            var phi1 = coord1.Latitude * (Math.PI / 180);
            var phi2 = coord2.Latitude * (Math.PI / 180);

            var deltaPhi = (coord2.Latitude - coord1.Latitude) * (Math.PI / 180);
            var deltaLambda = (coord2.Longitude - coord1.Longitude) * (Math.PI / 180);

            // Haversine formula
            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var meters = R * c;   // distance in meters
            var km = Math.Round(meters / 1000, 3); // distance in kilometers, rounded to 3 decimals

            Console.WriteLine($"from {coord1.Id} to {coord2.Id}  Distance: {km} km");
            return km;
        }
    }
}
