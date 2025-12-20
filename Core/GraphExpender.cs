using System;
using System.Collections.Generic;
using System.Linq;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core
{
    public class GraphExpansionResult
    {
        public string ClickedId { get; }
        public IReadOnlyList<string> NewlyAddedNodeIds { get; }

        public GraphExpansionResult(string clickedId, IReadOnlyList<string> newlyAddedNodeIds)
        {
            ClickedId = clickedId;
            NewlyAddedNodeIds = newlyAddedNodeIds;
        }
    }

    /// <summary>
    /// FullGraph'tan referans alarak ViewGraph'ı (ekranda gösterilen graf) genişletir.
    /// </summary>
    public class GraphExpander
    {
        private readonly Graph _fullGraph;

        public GraphExpander(Graph fullGraph)
        {
            _fullGraph = fullGraph ?? throw new ArgumentNullException(nameof(fullGraph));
        }

        /// <summary>
        /// Click edilen makale ve h-core kümesini ViewGraph'a entegre eder.
        /// Ayrıca FullGraph'tan, ViewGraph'ta kalan düğümler arasındaki kenarları ekler.
        /// </summary>
        public GraphExpansionResult ExpandByHCore(Graph viewGraph, string clickedId, IReadOnlyCollection<string> hCoreIds)
        {
            if (viewGraph == null) throw new ArgumentNullException(nameof(viewGraph));
            if (string.IsNullOrWhiteSpace(clickedId)) throw new ArgumentException("clickedId boş olamaz.", nameof(clickedId));
            if (hCoreIds == null) throw new ArgumentNullException(nameof(hCoreIds));

            // 1) Eski UI flag'lerini temizle
            foreach (var n in viewGraph.GetAllNodes())
            {
                n.IsSelected = false;
                n.IsNewlyAdded = false;
            }

            // 2) Clicked + hCore düğümlerini view'e ekle
            var newlyAdded = new List<string>();

            EnsureNodeInView(viewGraph, clickedId, newlyAdded);
            foreach (var id in hCoreIds)
                EnsureNodeInView(viewGraph, id, newlyAdded);

            // 3) Clicked işaretle
            var clickedNode = viewGraph.GetNode(clickedId);
            if (clickedNode != null)
                clickedNode.IsSelected = true;

            // 4) ViewGraph'taki düğümler arasındaki ilişki kenarlarını FullGraph'tan kopyala
            var viewIds = new HashSet<string>(viewGraph.GetAllNodes().Select(x => x.Id));

            // 4a) Citation kenarları
            foreach (var e in _fullGraph.GetAllEdges().Where(e => e.Type == EdgeType.Citation))
            {
                if (viewIds.Contains(e.Source.Id) && viewIds.Contains(e.Target.Id))
                    viewGraph.AddEdge(e.Source.Id, e.Target.Id, EdgeType.Citation, isDirected: true);
            }

            // 4b) Sequential kenarları
            foreach (var e in _fullGraph.GetAllEdges().Where(e => e.Type == EdgeType.Sequential))
            {
                if (viewIds.Contains(e.Source.Id) && viewIds.Contains(e.Target.Id))
                    viewGraph.AddEdge(e.Source.Id, e.Target.Id, EdgeType.Sequential, isDirected: true);
            }

            return new GraphExpansionResult(clickedId, newlyAdded);
        }

        private void EnsureNodeInView(Graph viewGraph, string id, List<string> newlyAdded)
        {
            if (viewGraph.ContainsNode(id))
                return;

            var fullNode = _fullGraph.GetNode(id);
            if (fullNode == null)
                return;

            // Paper referansını paylaş: tooltip / citation tutarlılığı
            var newNode = new GraphNode(fullNode.Paper)
            {
                IsNewlyAdded = true
            };

            viewGraph.AddNode(newNode);
            newlyAdded.Add(id);
        }
    }
}
