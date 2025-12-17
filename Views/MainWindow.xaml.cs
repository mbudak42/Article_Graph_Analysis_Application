using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Article_Graph_Analysis_Application.Helpers;
using Article_Graph_Analysis_Application.Models;
using Microsoft.Msagl.Drawing;

namespace Article_Graph_Analysis_Application.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CreateGraph();
        }

        private void CreateGraph()
        {
            try
            {
                // 1. JSON dosyasını bul
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string jsonPath = Path.Combine(basePath, "Assets", "DataJson.json");

                // 2. Verileri Oku
                JsonParser parser = new JsonParser();
                List<Article> articles = parser.ParseArticles(jsonPath);

                // 3. Graf Nesnesini Oluştur
                Graph graph = new Graph("citationGraph");

                // -- DÜĞÜMLERİ EKLE --
                foreach (var article in articles)
                {
                    // Id boş değilse ekle
                    if (!string.IsNullOrEmpty(article.Id))
                    {
                        Node node = graph.AddNode(article.Id);
                        node.LabelText = article.Id;
                    }
                }

                // -- SİYAH KENARLAR (Atıflar) --
                foreach (var article in articles)
                {
                    if (string.IsNullOrEmpty(article.Id)) continue;

                    foreach (var refId in article.ReferencedWorks)
                    {
                        // Atıf yapılan makale bizim listemizde var mı?
                        if (articles.Any(a => a.Id == refId))
                        {
                            Edge edge = graph.AddEdge(article.Id, refId);
                            edge.Attr.Color = Color.Black;
                        }
                    }
                }

                // -- YEŞİL KENARLAR (Sıralama) --
                // Id'si boş olmayanları alıp sıralıyoruz
                var sortedArticles = articles
                                     .Where(a => !string.IsNullOrEmpty(a.Id))
                                     .OrderBy(a => a.Id)
                                     .ToList();

                for (int i = 0; i < sortedArticles.Count - 1; i++)
                {
                    // "!" işareti ile null olmadığından eminim diyoruz
                    string sourceId = sortedArticles[i].Id!;
                    string targetId = sortedArticles[i + 1].Id!;

                    Edge edge = graph.AddEdge(sourceId, targetId);
                    edge.Attr.Color = Color.Green;
                }

                // 4. Grafiği Ekrana Bas
                GraphViewer.Graph = graph;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
        }
    }
}