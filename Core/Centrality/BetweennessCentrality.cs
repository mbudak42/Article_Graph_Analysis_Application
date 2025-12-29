using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core.Centrality
{
    public class BetweennessCentrality
    {
        public static Dictionary<string, double> Calculate(Graph graph)
        {
            var centrality = new Dictionary<string, double>();
            foreach (var nodeId in graph.Nodes.Keys)
            {
                centrality[nodeId] = 0.0;
            }

            var nodesList = graph.Nodes.Values.ToList();

            // Her düğüm için BFS
            foreach (var source in nodesList)
            {
                var stack = new Stack<GraphNode>();
                var paths = new Dictionary<string, List<GraphNode>>();
                var dist = new Dictionary<string, int>();
                var sigma = new Dictionary<string, int>();
                var delta = new Dictionary<string, double>();

                foreach (var node in nodesList)
                {
                    paths[node.Id] = new List<GraphNode>();
                    dist[node.Id] = -1;
                    sigma[node.Id] = 0;
                    delta[node.Id] = 0.0;
                }

                dist[source.Id] = 0;
                sigma[source.Id] = 1;

                var queue = new Queue<GraphNode>();
                queue.Enqueue(source);

                // BFS
                while (queue.Count > 0)
                {
                    var v = queue.Dequeue();
                    stack.Push(v);

                    foreach (var neighbor in GetNeighbors(v))
                    {
                        // İlk kez bulundu
                        if (dist[neighbor.Id] < 0)
                        {
                            queue.Enqueue(neighbor);
                            dist[neighbor.Id] = dist[v.Id] + 1;
                        }

                        // En kısa yol
                        if (dist[neighbor.Id] == dist[v.Id] + 1)
                        {
                            sigma[neighbor.Id] += sigma[v.Id];
                            paths[neighbor.Id].Add(v);
                        }
                    }
                }

                // Geri dönüş
                while (stack.Count > 0)
                {
                    var w = stack.Pop();
                    
                    foreach (var v in paths[w.Id])
                    {
                        delta[v.Id] += (sigma[v.Id] / (double)sigma[w.Id]) * (1.0 + delta[w.Id]);
                    }

                    if (w.Id != source.Id)
                    {
                        centrality[w.Id] += delta[w.Id];
                    }
                }
            }

            return centrality;
        }

        private static List<GraphNode> GetNeighbors(GraphNode node)
        {
            var neighbors = new HashSet<GraphNode>();
            neighbors.UnionWith(node.OutgoingNodes);
            neighbors.UnionWith(node.IncomingNodes);
            return neighbors.ToList();
        }
    }
}