using server.Models;

namespace server.Services

{
    /// Represents a connection between two geographical points.
    /// In graph theory, this is a "Weighted Edge" where the weight is physical distance.

    public class WeightedEdge
    {
        public GeoLocation Source { get; set; }
        public GeoLocation Destination { get; set; }
        public double Weight { get; set; } // The distance of this path in km 

        public WeightedEdge(GeoLocation source, GeoLocation destination, double weight)
        {
            Source = source;
            Destination = destination;
            Weight = weight;
        }
    }

    /// A Graph structure representing a network of locations.
    /// Uses an Adjacency List for efficient memory usage when nodes have few connections.
    public class Graph
    {
        private int vertices;  // Total number of locations (nodes)

        // Maps Database IDs (which could be non-sequential) to array indexes
        public Dictionary<int, int> IdtoIndex = new Dictionary<int, int>();

        //The Adjacency List: An array where each index contains a list of edges originating from that node.
        public List<WeightedEdge>[] adj;

        public Graph(List<GeoLocation> geos)
        {
            this.vertices = geos.Count;
            adj = new List<WeightedEdge>[vertices];

            for (int i = 0; i < vertices; i++)
            {
                adj[i] = new List<WeightedEdge>();
                IdtoIndex[geos[i].Id] = i;   // Store the mapping so we can find this GeoLocation's index later using its ID
            }
        }

        /// Creates a bidirectional connection between two points.
        /// Uses the Haversine Formula to calculate the great-circle distance (curvature of the Earth).
        public void AddEdge(GeoLocation source, GeoLocation destination)
        {
            // Map GeoLocation.Id to array index
            if (!IdtoIndex.ContainsKey(source.Id) || !IdtoIndex.ContainsKey(destination.Id))
                throw new ArgumentException("GeoLocation.Id not found in graph mapping.");

            int sourceIdx = IdtoIndex[source.Id];
            int destIdx = IdtoIndex[destination.Id];

            // Calculate distance with Haversine
            double distance = HaversineFormula.Haversine(source, destination);

            // Add edge from source to destination
            WeightedEdge edge1 = new WeightedEdge(source, destination, distance);
            adj[sourceIdx].Add(edge1);

            // Add edge from destination to source
            WeightedEdge edge2 = new WeightedEdge(destination, source, distance);
            adj[destIdx].Add(edge2);
        }

        /// Debugging tool to visualize the graph structure in the console.
        public void PrintGraph()
        {
            for (int i = 0; i < vertices; i++)
            {
                foreach (var edge in adj[i])
                {
                    Console.WriteLine(
                        $"From Node {edge.Source.Id} ({edge.Source.Latitude},{edge.Source.Longitude}) " +
                        $"to Node {edge.Destination.Id} ({edge.Destination.Latitude},{edge.Destination.Longitude}) " +
                        $"= {edge.Weight} km"
                    );
                }
            }
        }
    }
}