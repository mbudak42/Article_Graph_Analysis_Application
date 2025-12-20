using System.Collections.Generic;

namespace Article_Graph_Analysis_Application.Core.HIndex
{
    /// <summary>
    /// Bir makale için h-index hesaplama çıktıları.
    /// </summary>
    public class HIndexResult
    {
        public string TargetPaperId { get; }
        public int HIndex { get; }
        public double HMedian { get; }

        /// <summary>
        /// h-core kümesindeki makalelerin ID'leri (h adet).
        /// </summary>
        public IReadOnlyList<string> HCorePaperIds { get; }

        /// <summary>
        /// H-core içindeki her makale için (PaperId, InCitationCount) bilgisi.
        /// UI'da tablo/tooltip/detay göstermek için kullanışlıdır.
        /// </summary>
        public IReadOnlyList<(string PaperId, int InCitationCount)> HCoreDetails { get; }

        public HIndexResult(
            string targetPaperId,
            int hIndex,
            double hMedian,
            IReadOnlyList<string> hCorePaperIds,
            IReadOnlyList<(string PaperId, int InCitationCount)> hCoreDetails)
        {
            TargetPaperId = targetPaperId;
            HIndex = hIndex;
            HMedian = hMedian;
            HCorePaperIds = hCorePaperIds;
            HCoreDetails = hCoreDetails;
        }
    }
}
