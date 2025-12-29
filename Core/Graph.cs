using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
	public class Graph
	{
		public Dictionary<string, GraphNode> Nodes { get; set; }
		public List<GraphEdge> Edges { get; set; }

		public Graph()
		{
			Nodes = new Dictionary<string, GraphNode>();
			Edges = new List<GraphEdge>();
		}

		public void AddNode(GraphNode node)
		{
			if (!Nodes.ContainsKey(node.Id))
			{
				Nodes[node.Id] = node;
			}
		}

		public void AddEdge(GraphNode source, GraphNode target, bool isDirected = true)
		{
			if (!source.OutgoingNodes.Contains(target))
			{
				source.OutgoingNodes.Add(target);
			}
			if (!target.IncomingNodes.Contains(source))
			{
				target.IncomingNodes.Add(source);
			}

			var edge = new GraphEdge(source, target, isDirected);
			Edges.Add(edge);
		}

		public bool ContainsNode(string nodeId)
		{
			return Nodes.ContainsKey(nodeId);
		}

		public GraphNode? GetNode(string nodeId)
		{
			return Nodes.ContainsKey(nodeId) ? Nodes[nodeId] : null;
		}

		public int GetTotalNodes()
		{
			return Nodes.Count;
		}

		public int GetTotalEdges()
		{
			return Edges.Count;
		}

		public GraphNode? GetMostCitedNode()
		{
			return Nodes.Values.OrderByDescending(n => n.GetInDegree()).FirstOrDefault();
		}

		public GraphNode? GetMostReferencingNode()
		{
			return Nodes.Values.OrderByDescending(n => n.GetOutDegree()).FirstOrDefault();
		}

		public Graph ConvertToUndirected()
		{
			var undirected = new Graph();

			// Düğümleri kopyala
			foreach (var node in Nodes.Values)  // ← DEĞİŞTİ
			{
				undirected.AddNode(new GraphNode(node.Paper));
			}

			// Her yönlü kenarı çift yönlü yap
			var addedEdges = new HashSet<(string, string)>();

			foreach (var edge in Edges)  // ← DEĞİŞTİ
			{
				string id1 = edge.Source.Id;
				string id2 = edge.Target.Id;

				// Sıralı tuple (küçük, büyük) - duplicate önleme
				var edgePair = (string.CompareOrdinal(id1, id2) < 0) ? (id1, id2) : (id2, id1);

				if (!addedEdges.Contains(edgePair))
				{
					var node1 = undirected.GetNode(id1);
					var node2 = undirected.GetNode(id2);

					if (node1 != null && node2 != null)
					{
						undirected.AddEdge(node1, node2);
						undirected.AddEdge(node2, node1); // Çift yönlü
						addedEdges.Add(edgePair);
					}
				}
			}

			return undirected;
		}
	}
}