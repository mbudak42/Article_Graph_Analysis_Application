using System;
using System.Collections.Generic;
using System.Linq;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core.HIndex
{
    /// <summary>
    /// Bir makalenin h-index, h-core ve h-median değerlerini hesaplar.
    ///
    /// Tanım (yönerge ile uyumlu):
    /// - Hedef makaleye atıf yapan makaleler alınır.
    /// - Bu makalelerin her biri için "kendilerine yapılan atıf sayısı" (in-citation) bulunur.
    /// - Bu sayılar büyükten küçüğe sıralanır; h-index hesaplanır.
    /// - h-core: En çok atıf alan h adet makale.
    /// - h-median: h-core içindeki atıf sayılarının medyanı.
    /// </summary>
    public class HIndexCalculator
    {
        private readonly Graph _graph;

        public HIndexCalculator(Graph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        public HIndexResult Calculate(string targetPaperId)
        {
            if (string.IsNullOrWhiteSpace(targetPaperId))
                throw new ArgumentException("Target paper id cannot be null/empty.", nameof(targetPaperId));

            var targetNode = _graph.GetNode(targetPaperId);
            if (targetNode == null)
                throw new InvalidOperationException($"Target paper not found in graph: {targetPaperId}");

            // 1) Hedef makaleye atıf yapan (incoming) makaleleri bul (sadece Citation kenarları)
            //    Duplicate engeli için Dictionary/HashSet kullanıyoruz.
            var incomingCiterIds = _graph.GetAllEdges()
                .Where(e => e.Type == EdgeType.Citation && e.Target.Id == targetPaperId)
                .Select(e => e.Source.Id)
                .Distinct()
                .ToList();

            // 2) Her citer makale için kendi in-citation sayısını hesapla (Citation edges üzerinden)
            //    Güvenli yaklaşım: Paper.InCitationCount'a bağımlı kalmıyoruz.
            //    inCitationsOfX = count( edge: Citation && edge.Target == X )
            var inCitationMap = BuildInCitationMapForCitationEdges();

            var citerWithCounts = incomingCiterIds
                .Select(id => (PaperId: id, InCitations: inCitationMap.TryGetValue(id, out var c) ? c : 0))
                // Büyükten küçüğe, eşitlikte id ile deterministik sırala
                .OrderByDescending(x => x.InCitations)
                .ThenBy(x => x.PaperId, StringComparer.Ordinal)
                .ToList();

            // 3) h-index hesapla
            int h = 0;
            for (int i = 0; i < citerWithCounts.Count; i++)
            {
                int rank = i + 1;
                if (citerWithCounts[i].InCitations >= rank)
                    h = rank;
                else
                    break;
            }

            // 4) h-core: ilk h makale (zaten büyükten küçüğe sıralı)
            var hcore = (h == 0)
                ? new List<(string PaperId, int InCitations)>()
                : citerWithCounts.Take(h).ToList();

            // 5) h-median: h-core içindeki in-citation değerlerinin medyanı
            double hMedian = ComputeMedian(hcore.Select(x => x.InCitations));

            var hCoreIds = hcore.Select(x => x.PaperId).ToList();
            var hCoreDetails = hcore.Select(x => (x.PaperId, x.InCitations)).ToList();

            return new HIndexResult(
                targetPaperId: targetPaperId,
                hIndex: h,
                hMedian: hMedian,
                hCorePaperIds: hCoreIds,
                hCoreDetails: hCoreDetails);
        }

        private Dictionary<string, int> BuildInCitationMapForCitationEdges()
        {
            var map = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var edge in _graph.GetAllEdges().Where(e => e.Type == EdgeType.Citation))
            {
                var targetId = edge.Target.Id;
                if (!map.TryGetValue(targetId, out var current))
                    map[targetId] = 1;
                else
                    map[targetId] = current + 1;
            }

            return map;
        }

        private static double ComputeMedian(IEnumerable<int> values)
        {
            var arr = values.OrderBy(x => x).ToArray();
            if (arr.Length == 0) return 0;

            int mid = arr.Length / 2;

            // tek sayıda eleman: ortadaki
            if (arr.Length % 2 == 1)
                return arr[mid];

            // çift sayıda eleman: ortadaki iki değerin ortalaması
            return (arr[mid - 1] + arr[mid]) / 2.0;
        }
    }
}
