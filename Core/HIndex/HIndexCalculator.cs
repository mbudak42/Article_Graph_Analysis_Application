using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Core.HIndex
{
    public class HIndexCalculator
    {
        public static HIndexResult Calculate(GraphNode node)
        {
            var result = new HIndexResult();

            var citingPapers = node.IncomingNodes;

            if (citingPapers.Count == 0)
            {
                result.HIndex = 0;
                result.HMedian = 0;
                return result;
            }

            var citationCounts = citingPapers
                .Select(n => n.GetInDegree())
                .OrderByDescending(c => c)
                .ToList();

            int hIndex = 0;
            for (int i = 0; i < citationCounts.Count; i++)
            {
                if (citationCounts[i] >= (i + 1))
                {
                    hIndex = i + 1;
                }
                else
                {
                    break;
                }
            }

            result.HIndex = hIndex;

            if (hIndex > 0)
            {
                var hCorePapers = citingPapers
                    .OrderByDescending(n => n.GetInDegree())
                    .Take(hIndex)
                    .ToList();

                result.HCore = hCorePapers;

                var hCoreCitations = hCorePapers
                    .Select(n => n.GetInDegree())
                    .OrderBy(c => c)
                    .ToList();

                if (hCoreCitations.Count % 2 == 1)
                {
                    result.HMedian = hCoreCitations[hCoreCitations.Count / 2];
                }
                else
                {
                    int mid = hCoreCitations.Count / 2;
                    result.HMedian = (hCoreCitations[mid - 1] + hCoreCitations[mid]) / 2.0;
                }
            }

            return result;
        }
    }
}