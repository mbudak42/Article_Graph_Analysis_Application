using System.Windows;
using System.Windows.Input;
using Article_Graph_Analysis_Application.Core;
using Article_Graph_Analysis_Application.Core.HIndex;
using Article_Graph_Analysis_Application.Core.Centrality;
using Article_Graph_Analysis_Application.Core.KCore;
using Article_Graph_Analysis_Application.Models;
using Article_Graph_Analysis_Application.Services;
using Article_Graph_Analysis_Application.Services.Json;
using Article_Graph_Analysis_Application.Visualization.Msagl;
using Microsoft.Msagl.WpfGraphControl;

namespace Article_Graph_Analysis_Application.Views
{
	public partial class MainWindow : Window
	{
		private List<Paper> allPapers = new();
		private Core.Graph mainGraph = new();
		private Core.Graph displayGraph = new();
		private MsaglGraphController? graphController;
		private GraphExpander? graphExpander;
		private GraphViewer? viewer;
		private string? selectedNodeId = null;
		private int currentMode = 1;

		public MainWindow()
		{
			InitializeComponent();
			LoadData();
			InitializeGraph();
		}

		private void LoadData()
		{
			try
			{
				StatusText.Text = "Veriler yükleniyor...";
				string jsonPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "articles.json");
				allPapers = JsonPaperLoader.LoadPapers(jsonPath);
				StatusText.Text = $"Yüklendi: {allPapers.Count} makale";
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
				StatusText.Text = "Veri yüklenemedi!";
			}
		}

		private void InitializeGraph()
		{
			mainGraph = GraphBuilder.BuildGraphFromPapers(allPapers);

			// Eski viewer'ı temizle
			if (viewer != null)
			{
				GraphHost.Children.Clear();
			}

			viewer = new GraphViewer();
			viewer.BindToPanel(GraphHost);

			// Mevcut modu koru
			SwitchMode(currentMode);
		}

		private void SwitchMode(int mode)
		{
			currentMode = mode;

			switch (mode)
			{
				case 1:
					displayGraph = GraphBuilder.BuildFilteredGraph(allPapers, 50);
					HeaderText.Text = "Graf [Overview] - 50 Makale gösteriliyor.";
					break;
				case 2:
					displayGraph = GraphBuilder.BuildFilteredGraph(allPapers, 150);
					HeaderText.Text = "Graf [Overview] - 100 Makale (F1-F2-F3: Seviyeler)";
					break;
				case 3:
					displayGraph = mainGraph;
					HeaderText.Text = "Graf [Overview] - Tüm Makaleler";
					break;
				case 4:
					displayGraph = GraphBuilder.BuildTopCitedGraph(allPapers, 5);
					HeaderText.Text = "Graf [Overview] - En Çok Atıf Alan 5";
					break;
				default:
					displayGraph = GraphBuilder.BuildFilteredGraph(allPapers, 50);
					HeaderText.Text = "Graf [Overview] - 50 Makale gösteriliyor.";
					break;
			}

			graphController = new MsaglGraphController(viewer);
			graphExpander = new GraphExpander(mainGraph, displayGraph);

			graphController.DrawGraph(displayGraph);
			UpdateStatistics();

			HIndexPanel.Visibility = Visibility.Collapsed;
			BetweennessPanel.Visibility = Visibility.Collapsed;

			StatusText.Text = $"Mod {mode} aktif - {displayGraph.GetTotalNodes()} düğüm gösteriliyor";
		}

		private void UpdateStatistics()
		{
			StatisticsText.Text = StatisticsService.GetGraphStatistics(displayGraph);
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.F1:
					SwitchMode(1);
					break;
				case Key.F2:
					SwitchMode(2);
					break;
				case Key.F3:
					SwitchMode(3);
					break;
				case Key.F4:
					SwitchMode(4);
					break;
				case Key.F5:
					LoadData();
					InitializeGraph();
					break;
			}
		}

		private void ExpandGraphWithHCore(string nodeId)
		{
			var node = mainGraph.GetNode(nodeId);
			if (node == null)
			{
				MessageBox.Show("Düğüm bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var hIndexResult = HIndexCalculator.Calculate(node);

			HIndexResultText.Text = $"Seçilen Makale: {nodeId}\n";
			HIndexResultText.Text += $"H-Index: {hIndexResult.HIndex}\n";
			HIndexResultText.Text += $"H-Median: {hIndexResult.HMedian:F2}\n";
			HIndexResultText.Text += $"H-Core Boyutu: {hIndexResult.HCore.Count}";
			HIndexPanel.Visibility = Visibility.Visible;

			if (hIndexResult.HCore.Count > 0 && graphExpander != null && graphController != null)
			{
				graphExpander.ExpandWithHCore(hIndexResult.HCore);
				graphController.DrawGraph(displayGraph);
				graphController.HighlightHCore(hIndexResult.HCore, nodeId);
				UpdateStatistics();
			}

			StatusText.Text = $"Genişletildi: {nodeId} - H-Index: {hIndexResult.HIndex}";
		}

		private void CalculateHIndex_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new Window
			{
				Title = "H-Index Hesaplama",
				Width = 350,
				Height = 180,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				Owner = this,
				ResizeMode = ResizeMode.NoResize
			};

			var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };
			stack.Children.Add(new System.Windows.Controls.TextBlock
			{
				Text = "Makale ID'sini girin:",
				Margin = new Thickness(0, 0, 0, 10),
				FontSize = 13
			});

			var textBox = new System.Windows.Controls.TextBox
			{
				Margin = new Thickness(0, 0, 0, 15),
				Padding = new Thickness(5),
				FontSize = 12
			};
			stack.Children.Add(textBox);

			var buttonPanel = new System.Windows.Controls.StackPanel
			{
				Orientation = System.Windows.Controls.Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Right
			};

			var okButton = new System.Windows.Controls.Button
			{
				Content = "Tamam",
				Width = 80,
				Margin = new Thickness(0, 0, 10, 0),
				Padding = new Thickness(10, 5, 10, 5),
				IsDefault = true
			};
			okButton.Click += (s, args) => dialog.DialogResult = true;

			var cancelButton = new System.Windows.Controls.Button
			{
				Content = "İptal",
				Width = 80,
				Padding = new Thickness(10, 5, 10, 5),
				IsCancel = true
			};
			cancelButton.Click += (s, args) => dialog.DialogResult = false;

			buttonPanel.Children.Add(okButton);
			buttonPanel.Children.Add(cancelButton);
			stack.Children.Add(buttonPanel);

			dialog.Content = stack;

			bool? result = dialog.ShowDialog();

			if (result != true) return;

			string input = textBox.Text.Trim();

			if (string.IsNullOrEmpty(input)) return;

			var node = mainGraph.GetNode(input);
			if (node == null)
			{
				MessageBox.Show("Makale bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			selectedNodeId = input;
			ExpandGraphWithHCore(input);
		}

		private void CalculateBetweenness_Click(object sender, RoutedEventArgs e)
		{
			StatusText.Text = "Betweenness Centrality hesaplanıyor...";

			Task.Run(() =>
			{
				var centrality = BetweennessCentrality.Calculate(displayGraph);

				Dispatcher.Invoke(() =>
				{
					var sortedResults = centrality.OrderByDescending(kvp => kvp.Value).Take(20);

					BetweennessResultText.Text = "Top 20 Düğüm (Betweenness):\n\n";
					int rank = 1;
					foreach (var kvp in sortedResults)
					{
						BetweennessResultText.Text += $"{rank}. {kvp.Key}: {kvp.Value:F2}\n";
						rank++;
					}

					BetweennessPanel.Visibility = Visibility.Visible;
					StatusText.Text = "Betweenness Centrality hesaplandı";
				});
			});
		}

		private void ApplyKCore_Click(object sender, RoutedEventArgs e)
		{
			if (!int.TryParse(KValueTextBox.Text, out int k) || k < 1)
			{
				MessageBox.Show("Geçerli bir K değeri girin (1 veya daha büyük)", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			StatusText.Text = $"K-Core (k={k}) hesaplanıyor...";

			Task.Run(() =>
			{
				var kCoreNodes = KCoreDecomposition.FindKCore(displayGraph, k);

				Dispatcher.Invoke(() =>
				{
					if (kCoreNodes.Count == 0)
					{
						MessageBox.Show($"K={k} için K-Core bulunamadı. Daha küçük bir K değeri deneyin.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
						StatusText.Text = $"K-Core bulunamadı (k={k})";
						return;
					}

					graphController?.HighlightKCore(kCoreNodes);
					StatusText.Text = $"K-Core uygulandı (k={k}) - {kCoreNodes.Count} düğüm";

					MessageBox.Show($"K-Core sonucu:\n{kCoreNodes.Count} düğüm bulundu", "K-Core", MessageBoxButton.OK, MessageBoxImage.Information);
				});
			});
		}
	}
}