using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Models
{
    public class GraphNode
    {
        // === TEMEL REFERANS ===
        public Paper Paper { get; }

        // MSAGL / graf için tekil düğüm anahtarı
        public string Id => Paper.Id;

        // === UI DURUM BİLGİLERİ ===
        public bool IsSelected { get; set; }
        public bool IsNewlyAdded { get; set; }

        // === TOOLTIP (LAZY + CACHE) ===
        private string? _tooltipCache;

        public string TooltipText =>
            _tooltipCache ??= BuildTooltip();

        private string BuildTooltip()
        {
            return
                $"ID: {Paper.Id}\n" +
                $"Title: {Paper.Title}\n" +
                $"Authors: {string.Join(", ", Paper.Authors)}\n" +
                $"Year: {Paper.Year}\n" +
                $"Citations: {Paper.InCitationCount}";
        }

        // === TOOLTIP GÜNCELLEME ===
        // Paper içeriği değiştiğinde çağrılır
        public void InvalidateTooltip()
        {
            _tooltipCache = null;
        }

        // === CONSTRUCTOR ===
        public GraphNode(Paper paper)
        {
            Paper = paper;
        }
    }
}
