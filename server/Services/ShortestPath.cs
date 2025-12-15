using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using server.Models;
using System.ComponentModel.DataAnnotations;

namespace server.Services
{
    public class ShortestPath
    {



        public static double[] Dijkstra(Graph graph, int V, int sourceId)
        {
            int sourceIdx = graph.IdtoIndex[sourceId];
            double[] dist = new double[V];
            var edges = graph.adj[sourceIdx];

            // Initialize distances
            for (int i = 0; i < V; i++)
                dist[i] = double.MaxValue;

            dist[sourceIdx] = 0;

            // PriorityQueue: <distance, nodeIdx>
            PriorityQueue<int, double> heap = new PriorityQueue<int, double>();
            heap.Enqueue(sourceIdx, 0);

            while (heap.Count > 0)
            {
                int u = heap.Dequeue();

                foreach (var edge in graph.adj[u])
                {
                    int v = graph.IdtoIndex[edge.Destination.Id];
                    double weight = edge.Weight;

                    if (dist[u] + weight < dist[v])
                    {
                        dist[v] = dist[u] + weight;
                        heap.Enqueue(v, dist[v]);
                    }
                }
            }

            return dist;
        }


        public static List<int> TSP(Graph graph, int sourceId)
        {
            int sourceIdx = graph.IdtoIndex[sourceId];
            int V = graph.adj.Length;
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

            bool[] visited = new bool[V];
            List<int> tour = new List<int>();
            int current = sourceIdx;
            tour.Add(current);
            visited[current] = true;

            for (int step = 1; step < V; step++)
            {
                double minDist = double.MaxValue;
                int next = -1;

                for (int j = 0; j < V; j++)
                {
                    if (!visited[j] && dist[current, j] < minDist)
                    {
                        minDist = dist[current, j];
                        next = j;
                    }
                }

                if (next == -1) break;
                tour.Add(next);
                visited[next] = true;
                current = next;
            }

            tour.Add(sourceIdx);
            return tour;
        }
    }
}