using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Article_Graph_Analysis_Application.Models;

namespace Article_Graph_Analysis_Application.Services.Json
{
    /// <summary>
    /// articles.json dosyasını okuyup Paper listesi üretir
    /// </summary>
    public class JsonPaperLoader
    {
        public List<Paper> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("articles.json bulunamadı", filePath);

            var json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var papers = JsonSerializer.Deserialize<List<Paper>>(json, options);

            return papers ?? new List<Paper>();
        }
    }
}
