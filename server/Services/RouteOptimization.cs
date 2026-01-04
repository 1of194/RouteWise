using server.Models;

namespace server.Services
{
    public class RouteOptimization
    {
        /// The main entry point for route logic. 
        /// Converts a list of delivery addresses into an optimized sequence.
        public object RouteOptimazer(List<DeliveryAddress> deliveryAddrTable, List<GeoLocation> geoList)
        {
            var addresses = deliveryAddrTable.ToList();

            // 1. PRIORITY HANDLING: Identify "High Priority" locations
            // storing these in a HashSet for better lookup speed during sorting.
            var allprioritygeosid = addresses
                .Where(a => a.Priority == DeliveryAddress.PriorityLevel.High)
                .Select(addr => addr.GeolocationId)
                .ToHashSet();

            var geos = geoList.ToList();

            // STARTING POINT: Find the address designated as the 'Start'
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

            // Remove potential duplicate addresses for the same coordinate
            // This ensures our graph doesn't have redundant vertices.
            addresses = addresses
            .GroupBy(a => a.GeolocationId)
            .Select(g => g.First())
            .ToList();

            // Create a dictionary for fast address retrieval once the algorithm finishes
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

            // Basic Validation: Ensure we actually have data to process
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

            // We create a "Complete Graph" where every location is connected to every other location.
            Graph graph = new Graph(geos);

            for (int i = 0; i < geos.Count; i++)
            {
                for (int j = 0; j < geos.Count; j++)
                {
                    var source = geos[i];
                    var destination = geos[j];

                    // Don't create an edge from a point to itself
                    if (i != j)
                    {
                        graph.AddEdge(source, destination);
                    }
                }
            }

            // Run the Traveling Salesperson Problem (TSP)
            // This returns a list of integer indices (0, 3, 1, 2...) representing the visit order.

            List<int> ids = ShortestPath.TSP(graph, sourceGeo.Id);

            // Debug: print all IDs returned by TSP
            Console.WriteLine("TSP IDs: " + string.Join(", ", ids));
            Console.WriteLine("AddressMap keys: " + string.Join(", ", addressMap.Keys));


            // Convert internal graph indices back to Real Address Objects
            var indexToGeoId = geos.Select(g => g.Id).ToList();

            var orderedAddresses = ids
                .Select(idx => indexToGeoId[idx]) // Map Index -> GeoId
                .Where(id => addressMap.ContainsKey(id))
                .Select(id => addressMap[id]) // Map GeoId -> Full Address Object
                .ToList();

            // Return the optimized list to the controller (which sends it to the UI)
            return orderedAddresses;
        }
        


        

    }
}

