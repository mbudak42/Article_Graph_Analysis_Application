using Article_Graph_Analysis_Application.Core;

namespace Article_Graph_Analysis_Application.Services
{
    public class StatisticsService
    {
        public static string GetGraphStatistics(Graph graph)
        {
            int totalNodes = graph.GetTotalNodes();
            int totalEdges = graph.GetTotalEdges();

            int totalOutgoingRefs = 0;
            int totalIncomingRefs = 0;

            foreach (var node in graph.Nodes.Values)
            {
                totalOutgoingRefs += node.GetOutDegree();
                totalIncomingRefs += node.GetInDegree();
            }

            var mostCited = graph.GetMostCitedNode();
            var mostReferencing = graph.GetMostReferencingNode();

            string stats = $"Toplam Makale: {totalNodes}\n";
            stats += $"Toplam Referans (Kenar): {totalEdges}\n";
            stats += $"Toplam Verilen Referans: {totalOutgoingRefs}\n";
            stats += $"Toplam Alınan Referans: {totalIncomingRefs}\n";

            if (mostCited != null)
            {
                stats += $"En Çok Atıf Alan: {mostCited.Id} ({mostCited.GetInDegree()} atıf)\n";
            }

            if (mostReferencing != null)
            {
                stats += $"En Çok Referans Veren: {mostReferencing.Id} ({mostReferencing.GetOutDegree()} referans)\n";
            }

            return stats;
        }
    }
}