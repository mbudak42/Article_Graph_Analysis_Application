using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Helpers
{
    public class JsonParser
    {
        /// <summary>
        /// JSON dosyasını okur ve Article listesine çevirir.
        /// Hazır kütüphane (Newtonsoft vb.) KULLANILMAMIŞTIR.
        /// </summary>
        public List<Article> ParseArticles(string filePath)
        {
            var articles = new List<Article>();

            // 1. Dosya var mı kontrolü
            if (!File.Exists(filePath))
                throw new FileNotFoundException("JSON dosyası bulunamadı!", filePath);

            // 2. Tüm metni oku
            string jsonContent = File.ReadAllText(filePath);

            // 3. Veriyi temizle (Baştaki '[' ve sondaki ']' karakterlerini at)
            jsonContent = jsonContent.Trim();
            if (jsonContent.StartsWith("[")) jsonContent = jsonContent.Substring(1);
            if (jsonContent.EndsWith("]")) jsonContent = jsonContent.Substring(0, jsonContent.Length - 1);

            // 4. Nesneleri ayır: "}," ifadesine göre bölerek her bir makaleyi ayırıyoruz
            // Not: Bu basit bir ayırıcıdır, iç içe objelerde daha karmaşık yapı gerekir ama proje verisi için yeterlidir.
            string[] objectBlocks = jsonContent.Split(new string[] { "}," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var block in objectBlocks)
            {
                // Son kalan süslü parantezleri temizle
                string cleanBlock = block.Replace("{", "").Replace("}", "").Trim();

                Article article = new Article();

                // Manuel veri ayıklama (Parsing)
                article.Id = ExtractString(cleanBlock, "id");
                article.Title = ExtractString(cleanBlock, "title");
                
                // Yıl bilgisini sayıya çevirme
                string yearStr = ExtractString(cleanBlock, "year");
                int.TryParse(yearStr, out int year);
                article.Year = year;

                // Liste olan verileri ayıklama (Authors ve ReferencedWorks)
                article.Authors = ExtractList(cleanBlock, "authors");
                article.ReferencedWorks = ExtractList(cleanBlock, "referenced_works");

                // Atıf sayısını varsayılan 0 yapıyoruz (Sonra hesaplanacak)
                article.CitationCount = 0;

                articles.Add(article);
            }

            return articles;
        }

        /// <summary>
        /// Metin içinden "key": "value" yapısındaki değeri bulur.
        /// </summary>
        private string ExtractString(string source, string key)
        {
            string searchKey = $"\"{key}\":";
            int startIndex = source.IndexOf(searchKey);
            
            if (startIndex == -1) return ""; // Bulamazsa boş dön

            startIndex += searchKey.Length; // Anahtar kelimenin sonuna git

            // Değerin nerede bittiğini bul (virgül veya satır sonu)
            // Tırnak işaretli stringleri ayırmak için basit mantık
            int valueStart = source.IndexOf("\"", startIndex); 
            if (valueStart == -1) 
            {
                // Eğer tırnak yoksa (örn: sayı ise) direkt al
                valueStart = startIndex; 
            }
            else 
            {
                valueStart++; // İlk tırnağı geç
            }

            // Bitiş noktasını bul
            int valueEnd;
            if (source.Substring(startIndex).Trim().StartsWith("\""))
            {
                // String ise kapanış tırnağını ara
                valueEnd = source.IndexOf("\"", valueStart);
            }
            else
            {
                // Sayı ise virgülü ara
                valueEnd = source.IndexOf(",", valueStart);
                if (valueEnd == -1) valueEnd = source.Length; // Blok sonu
            }

            if (valueEnd == -1) return "";

            return source.Substring(valueStart, valueEnd - valueStart).Trim();
        }

        /// <summary>
        /// Köşeli parantez içindeki listeleri ["a", "b"] ayıklar.
        /// </summary>
        private List<string> ExtractList(string source, string key)
        {
            List<string> result = new List<string>();
            string searchKey = $"\"{key}\":";
            int startIndex = source.IndexOf(searchKey);

            if (startIndex == -1) return result;

            // Listenin başladığı yeri bul '['
            int openBracket = source.IndexOf("[", startIndex);
            int closeBracket = source.IndexOf("]", openBracket);

            if (openBracket == -1 || closeBracket == -1) return result;

            // İçeriği al: "Yazar 1", "Yazar 2"
            string content = source.Substring(openBracket + 1, closeBracket - openBracket - 1);

            // Virgülle ayır ve temizle
            var items = content.Split(',');
            foreach (var item in items)
            {
                string cleanItem = item.Trim().Replace("\"", "");
                if (!string.IsNullOrWhiteSpace(cleanItem))
                {
                    result.Add(cleanItem);
                }
            }

            return result;
        }
    }
}