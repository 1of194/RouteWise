using System;
using System.Net.WebSockets;
using server.Controllers;
using server.Models;

namespace server.Services
{
    public class RouteOptimization
    {

        public object RouteOptimazer(List<DeliveryAddress> deliveryAddrTable, List<GeoLocation> geoList)
        {
            var addresses = deliveryAddrTable.ToList();
            var allprioritygeosid = addresses
                .Where(a => a.Priority == DeliveryAddress.PriorityLevel.High)
                .Select(addr => addr.GeolocationId)
                .ToHashSet();

            var geos = geoList.ToList();
            //->
            // user first added address geolocation
            var startAddress = addresses.FirstOrDefault(a => a.Priority == DeliveryAddress.PriorityLevel.Start);

            GeoLocation sourceGeo = null;

            if (startAddress != null)
            {
                // Step 2: map it to GeoLocation
                sourceGeo = geos.FirstOrDefault(g => g.Id == startAddress.GeolocationId);
                Console.WriteLine($"Source Geo: {sourceGeo?.Id}, Lat: {sourceGeo?.Latitude}, Lng: {sourceGeo?.Longitude}");
            }
            else
            {
                Console.WriteLine("No Start address found.");
            }

            var sortedgeos = geos.OrderBy(g => g.Id == sourceGeo.Id ? DeliveryAddress.PriorityLevel.Start: allprioritygeosid.Contains(g.Id) ? DeliveryAddress.PriorityLevel.High:
             DeliveryAddress.PriorityLevel.Normal).ToList();

            addresses = addresses
            .GroupBy(a => a.GeolocationId)
            .Select(g => g.First())
            .ToList();

            var addressMap = addresses.ToDictionary(a => a.GeolocationId);

            foreach (var geo in sortedgeos)
            {
                if (addressMap.TryGetValue(geo.Id, out var address))
                {
                    Console.WriteLine($"Geo Id: {geo.Id}, Street: {address.Street}");
                }
                else
                {
                    Console.WriteLine($"Geo Id: {geo.Id}, Street: N/A");
                }
            }

            if (addresses.Count == 0)
            {
                return "Addresses were not found.";
            }

            if (geos.Count == 0)
            {
                return "Geolocations were not found.";
            }

            Dictionary<int, int> idToIndex = new Dictionary<int, int>();

            // Step 2: Build the mapping from GeoLocation.Id to graph index
            for (int i = 0; i < geos.Count; i++)
            {
                idToIndex[geos[i].Id] = i;
            }

            // Step 3: Create the graph using the number of vertices
            Graph graph = new Graph(geos);

            // Step 4: Add edges using mapped indices
            for (int i = 0; i < geos.Count; i++)
            {
                for (int j = 0; j < geos.Count; j++)
                {
                    var source = geos[i];
                    var destination = geos[j];

                    // You can now safely use idToIndex[source.Id] or idToIndex[destination.Id] if needed
                    if (i != j)
                    {
                        graph.AddEdge(source, destination);
                    }
                }
            }

            List<int> ids = ShortestPath.TSP(graph, sourceGeo.Id);
            // Debug: print all IDs returned by TSP
            Console.WriteLine("TSP IDs: " + string.Join(", ", ids));
            Console.WriteLine("AddressMap keys: " + string.Join(", ", addressMap.Keys));


            // Get the mapping from index to GeoLocation.Id
            var indexToGeoId = geos.Select(g => g.Id).ToList();

            // TSP returns indices, not GeoLocation IDs
            var orderedAddresses = ids
                .Select(idx => indexToGeoId[idx])
                .Where(id => addressMap.ContainsKey(id))
                .Select(id => addressMap[id])
                .ToList();

            return orderedAddresses;
        }
        


        

    }
}

