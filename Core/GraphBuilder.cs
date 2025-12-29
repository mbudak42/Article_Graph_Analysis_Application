using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
	public class GraphBuilder
	{
		public static Graph BuildGraphFromPapers(List<Paper> papers)
		{
			var graph = new Graph();
			var paperIds = papers.Select(p => p.Id).ToHashSet();

			foreach (var paper in papers)
			{
				var node = new GraphNode(paper);
				graph.AddNode(node);
			}

			foreach (var paper in papers)
			{
				var sourceNode = graph.GetNode(paper.Id);
				if (sourceNode != null)
				{
					foreach (var refId in paper.ReferencedWorks)
					{
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
				.ThenBy(p => p.Id)  // ← Deterministik sıralama
				.Take(maxPapers)
				.ToList();

			var graph = new Graph();
			var selectedIds = sortedPapers.Select(p => p.Id).ToHashSet();

			// Paper nesnelerini KOPYALA
			var paperCopies = new Dictionary<string, Paper>();
			foreach (var paper in sortedPapers)
			{
				var copy = new Paper
				{
					Id = paper.Id,
					Title = paper.Title,
					Year = paper.Year,
					Authors = paper.Authors,
					ReferencedWorks = paper.ReferencedWorks,
					InCitationCount = paper.InCitationCount  // JSON'daki değeri koru
				};
				paperCopies[paper.Id] = copy;

				var node = new GraphNode(copy);
				graph.AddNode(node);
			}

			foreach (var paperId in selectedIds)
			{
				var paper = paperCopies[paperId];
				var sourceNode = graph.GetNode(paperId);

				if (sourceNode != null)
				{
					foreach (var refId in paper.ReferencedWorks)
					{
						if (selectedIds.Contains(refId))
						{
							var targetNode = graph.GetNode(refId);
							if (targetNode != null)
							{
								graph.AddEdge(sourceNode, targetNode);
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