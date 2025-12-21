using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
	public class GraphBuilder
	{
		public static Graph BuildGraphFromPapers(List<Paper> papers)
		{
			var graph = new Graph();
			var paperIds = papers.Select(p => p.Id).ToHashSet();

			// Önce tüm düğümleri ekle
			foreach (var paper in papers)
			{
				var node = new GraphNode(paper);
				graph.AddNode(node);
			}

			// Sadece mevcut makaleler arasındaki referansları ekle
			foreach (var paper in papers)
			{
				var sourceNode = graph.GetNode(paper.Id);
				if (sourceNode != null)
				{
					foreach (var refId in paper.ReferencedWorks)
					{
						// ÖNEMLI: Sadece papers listesinde olan referanslar
						if (paperIds.Contains(refId))
						{
							var targetNode = graph.GetNode(refId);
							if (targetNode != null)
							{
								graph.AddEdge(sourceNode, targetNode);
								targetNode.Paper.InCitationCount++;
							}
						}
					}
				}
			}

			return graph;
		}
		public static Graph BuildFilteredGraph(List<Paper> papers, int maxPapers)
		{
			var sortedPapers = papers
				.OrderByDescending(p => p.InCitationCount)
				.Take(maxPapers)
				.ToList();

			var graph = new Graph();
			var selectedIds = sortedPapers.Select(p => p.Id).ToHashSet();

			// Sadece seçili makaleleri ekle
			foreach (var paper in sortedPapers)
			{
				var node = new GraphNode(paper);
				graph.AddNode(node);
			}

			// Sadece ikisi de seçili olan referansları ekle
			foreach (var paper in sortedPapers)
			{
				var sourceNode = graph.GetNode(paper.Id);
				if (sourceNode != null)
				{
					foreach (var refId in paper.ReferencedWorks)
					{
						// ÖNEMLI: Sadece seçili makaleler arasındaki referanslar
						if (selectedIds.Contains(refId))
						{
							var targetNode = graph.GetNode(refId);
							if (targetNode != null)
							{
								graph.AddEdge(sourceNode, targetNode);
								targetNode.Paper.InCitationCount++;
							}
						}
					}
				}
			}

			return graph;
		}

		public static Graph BuildTopCitedGraph(List<Paper> papers, int topCount)
		{
			var topPapers = papers.OrderByDescending(p => p.InCitationCount).Take(topCount).ToList();
			return BuildGraphFromPapers(topPapers);
		}
	}
}