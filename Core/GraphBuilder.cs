using System;
using System.Collections.Generic;
using System.Linq;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
    public class GraphBuilder
    {
        public Graph BuildGraph(IEnumerable<Paper> papers)
        {
            if (papers == null)
                throw new ArgumentNullException(nameof(papers));

            var graph = new Graph();
            var paperList = papers.ToList();

            foreach (var paper in paperList)
            {
                var node = new GraphNode(paper);
                graph.AddNode(node);
            }

            foreach (var paper in paperList)
            {
                foreach (var referencedId in paper.ReferencedWorks)
                {
                    graph.AddEdge(paper.Id, referencedId, EdgeType.Citation, isDirected: true);
                }
            }

            return graph;
        }
    }
}