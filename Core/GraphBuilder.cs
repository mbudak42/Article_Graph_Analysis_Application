using System;
using System.Collections.Generic;
using System.Linq;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
    /// <summary>
    /// Paper listesinden Graph oluşturmaktan sorumludur.
    /// JSON -> Model -> Graph dönüşümü burada yapılır.
    /// </summary>
    public class GraphBuilder
    {
        public Graph BuildGraph(IEnumerable<Paper> papers)
        {
            if (papers == null)
                throw new ArgumentNullException(nameof(papers));

            var graph = new Graph();
            var paperList = papers.ToList();

            // === 1. TÜM DÜĞÜMLERİ EKLE ===
            foreach (var paper in paperList)
            {
                var node = new GraphNode(paper);
                graph.AddNode(node);
            }

            // === 2. SİYAH KENARLAR (ATIFLAR) ===
            foreach (var paper in paperList)
            {
                foreach (var referencedId in paper.ReferencedWorks)
                {
                    graph.AddEdge(paper.Id, referencedId);
                }
            }

            // === 3. YEŞİL KENARLAR (ARTAN ID) ===
            var sortedIds = paperList
                .Select(p => p.Id)
                .OrderBy(id => id)
                .ToList();

            for (int i = 0; i < sortedIds.Count - 1; i++)
            {
                graph.AddEdge(sortedIds[i], sortedIds[i + 1]);
            }

            return graph;
        }
    }
}
