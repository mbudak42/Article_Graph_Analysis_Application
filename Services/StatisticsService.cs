using System;
using System.Linq;
using Article_Graph_Analysis_Application.Core;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Services
{
    /// <summary>
    /// Graf üzerinden istatistiksel bilgileri hesaplar
    /// ve Paper nesnelerinin türetilmiş alanlarını günceller.
    /// </summary>
    public class StatisticsService
    {
        private readonly Graph _graph;

        public StatisticsService(Graph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        // =========================
        // GENEL SAYILAR
        // =========================

        public int TotalNodeCount => _graph.TotalNodeCount;

        public int TotalEdgeCount =>
            _graph.GetAllEdges().Count(e => e.Type == EdgeType.Citation);

        // =========================
        // CITATION BİLGİLERİ
        // =========================

        /// <summary>
        /// Tüm Paper nesnelerinin CitedBy kümelerini günceller.
        /// InCitationCount bu kümeden otomatik türetilir.
        /// </summary>
        public void UpdateCitationData()
        {
            // 1️⃣ Önce temizle
            foreach (var node in _graph.GetAllNodes())
            {
                node.Paper.CitedBy.Clear();
            }

            // 2️⃣ Sadece atıf (siyah) kenarları işle
            foreach (var edge in _graph.GetAllEdges()
                                       .Where(e => e.Type == EdgeType.Citation))
            {
                // source -> target
                edge.Target.Paper.CitedBy.Add(edge.Source.Id);
            }
        }

        // =========================
        // MAKSİMUM DEĞERLER
        // =========================

        public GraphNode? GetMostCitedNode()
        {
            return _graph.GetAllNodes()
                         .OrderByDescending(n => n.Paper.InCitationCount)
                         .FirstOrDefault();
        }

        public GraphNode? GetMostReferencingNode()
        {
            return _graph.GetAllNodes()
                         .OrderByDescending(n =>
                             _graph.GetOutDegree(n.Id))
                         .FirstOrDefault();
        }
    }
}
