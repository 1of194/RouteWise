namespace server.Services
{
    public class ShortestPath
    {


        /// Dijkstra's Algorithm
        /// Finds the shortest distance from a starting node to ALL other nodes in the graph.
        public static double[] Dijkstra(Graph graph, int V, int sourceId)
        {
            // Map the DB ID to the internal array index
            int sourceIdx = graph.IdtoIndex[sourceId];
            double[] dist = new double[V];
            var edges = graph.adj[sourceIdx];

            // Initialize distances
            for (int i = 0; i < V; i++)
                dist[i] = double.MaxValue;

            dist[sourceIdx] = 0;

            // 2. PriorityQueue stores <NodeIndex, Distance> 
            // It automatically gives us the node with the smallest distance next
            PriorityQueue<int, double> heap = new PriorityQueue<int, double>();
            heap.Enqueue(sourceIdx, 0);

            while (heap.Count > 0)
            {
                // Pull the node with the currently shortest known distance
                int u = heap.Dequeue();

                // 3. Relaxation Step: Check all neighbors of node 'u'
                foreach (var edge in graph.adj[u])
                {

                    int v = graph.IdtoIndex[edge.Destination.Id];
                    double weight = edge.Weight;

                    // If going through 'u' to 'v' is shorter than the current known distance to 'v'
                    if (dist[u] + weight < dist[v])
                    {
                        dist[v] = dist[u] + weight;
                        heap.Enqueue(v, dist[v]);  // Update the queue with the new shorter distance
                    }
                }
            }

            return dist;
        }


        /// Traveling Salesperson Problem (TSP) - Greedy/Nearest Neighbor Approach
        /// Calculates an optimized route that visits every location and returns to the start.
        public static List<int> TSP(Graph graph, int sourceId)
        {
            int sourceIdx = graph.IdtoIndex[sourceId];
            int V = graph.adj.Length;
            // We run Dijkstra for every single node to find the shortest distance between 
            // every possible pair of locations in the graph.
            double[,] dist = new double[V, V];

            // Get all GeoLocation IDs in the graph
            var geoIds = graph.IdtoIndex.Keys.ToList();

            for (int i = 0; i < V; i++)
            {
                int geoIdI = geoIds[i];
                double[] distFromI = Dijkstra(graph, V, geoIdI);
                for (int j = 0; j < V; j++)
                    dist[i, j] = distFromI[j];
            }

            for (int i = 0; i < V; i++)
            {
                foreach (var edge in graph.adj[i])
                {
                    int destIdx = graph.IdtoIndex[edge.Destination.Id];
                    double weight = edge.Weight;
                    dist[i, destIdx] = weight;
                }
            }

            //Greedy Search(Nearest Neighbor)

            bool[] visited = new bool[V];
            List<int> tour = new List<int>();
            int current = sourceIdx;
            tour.Add(current);
            visited[current] = true;

            // Step through until every vertex is visited
            for (int step = 1; step < V; step++)
            {
                double minDist = double.MaxValue;
                int next = -1;

                // Find the closest unvisited neighbor to the current node
                for (int j = 0; j < V; j++)
                {
                    if (!visited[j] && dist[current, j] < minDist)
                    {
                        minDist = dist[current, j];
                        next = j;
                    }
                }

                if (next == -1) break; // No more reachable nodes
                
                 tour.Add(next);
                visited[next] = true;
                current = next;  // Move to the new node and repeat
            }
            // Return to Start
            tour.Add(sourceIdx);  
            return tour;
        }
    }
}