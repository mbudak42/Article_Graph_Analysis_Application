namespace Article_Graph_Analysis_Application.Models
{
    /// <summary>
    /// Graf üzerindeki bir kenarı temsil eder.
    /// Kaynak -> Hedef ilişkisini tutar.
    /// </summary>
    public class GraphEdge
    {
        // === DÜĞÜMLER ===
        public GraphNode Source { get; }
        public GraphNode Target { get; }

        // === KENAR ÖZELLİKLERİ ===

        // true  -> yönlü (atıf ilişkisi)
        // false -> yönsüz (analiz grafı)
        public bool IsDirected { get; }

        // Atıf kenarı mı (siyah) yoksa
        // ID sıralama kenarı mı (yeşil)
        public EdgeType Type { get; }

        // === CONSTRUCTOR ===
        public GraphEdge(
            GraphNode source,
            GraphNode target,
            EdgeType type,
            bool isDirected = true)
        {
            Source = source;
            Target = target;
            Type = type;
            IsDirected = isDirected;
        }
    }

    /// <summary>
    /// Kenarın anlamını belirtir.
    /// </summary>
    public enum EdgeType
    {
        Citation,      // Siyah kenar (referans)
        Sequential     // Yeşil kenar (artan ID bağlantısı)
    }
}
