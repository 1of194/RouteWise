using server.Models;

namespace server.Services

{

    public class WeightedEdge
    {
        public GeoLocation Source { get; set; }
        public GeoLocation Destination { get; set; }
        public double Weight { get; set; } // distance in km   

        public WeightedEdge(GeoLocation source, GeoLocation destination, double weight)
        {
            Source = source;
            Destination = destination;
            Weight = weight;
        }
    }

    public class Graph
    {
        private int vertices;
        public Dictionary<int, int> IdtoIndex = new Dictionary<int, int>();
        public List<WeightedEdge>[] adj;

        public Graph(List<GeoLocation> geos)
        {
            this.vertices = geos.Count;
            adj = new List<WeightedEdge>[vertices];

            for (int i = 0; i < vertices; i++)
            {
                adj[i] = new List<WeightedEdge>();
                IdtoIndex[geos[i].Id] = i;
            }
        }

        // Add an undirected edge (both directions)
        // Add an undirected edge (both directions)
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

        // Print all edges in the graph
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