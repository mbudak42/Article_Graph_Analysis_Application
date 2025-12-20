using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
    public class GraphBuilder
    {
        public static Graph BuildGraphFromPapers(List<Paper> papers)
        {
            var graph = new Graph();

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
                        var targetNode = graph.GetNode(refId);
                        if (targetNode != null)
                        {
                            graph.AddEdge(sourceNode, targetNode);
                            targetNode.Paper.InCitationCount++;
                        }
                    }
                }
            }

            return graph;
        }

        public static Graph BuildFilteredGraph(List<Paper> papers, int maxPapers)
        {
            var sortedPapers = papers.OrderByDescending(p => p.InCitationCount).Take(maxPapers).ToList();
            return BuildGraphFromPapers(sortedPapers);
        }

        public static Graph BuildTopCitedGraph(List<Paper> papers, int topCount)
        {
            var topPapers = papers.OrderByDescending(p => p.InCitationCount).Take(topCount).ToList();
            var relatedIds = new HashSet<string>(topPapers.Select(p => p.Id));

            foreach (var paper in topPapers)
            {
                foreach (var refId in paper.ReferencedWorks)
                {
                    relatedIds.Add(refId);
                }
            }

            var relatedPapers = papers.Where(p => relatedIds.Contains(p.Id)).ToList();
            return BuildGraphFromPapers(relatedPapers);
        }
    }
}