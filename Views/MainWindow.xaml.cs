using System.Windows;
using System.Windows.Controls;
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

			if (viewer != null)
			{
				// ÖNEMLİ: Tooltip'i silmemek için GraphHost'u değil GraphContainer'ı temizle
				GraphContainer.Children.Clear();
			}

			viewer = new GraphViewer();
			viewer.BindToPanel(GraphContainer);

			viewer.MouseDown += Viewer_MouseDown;
			viewer.MouseMove += Viewer_MouseMove;

			SwitchMode(currentMode);
		}

		private void Viewer_MouseMove(object? sender, Microsoft.Msagl.Drawing.MsaglMouseEventArgs e)
		{
			try
			{
				if (viewer?.ObjectUnderMouseCursor?.DrawingObject is Microsoft.Msagl.Drawing.Node msaglNode)
				{
					ShowNodeTooltip(msaglNode.Id);
				}
				else
				{
					NodeTooltip.Visibility = Visibility.Collapsed;
				}
			}
			catch
			{
				// Tooltip hatasını sessizce göz ardı et
			}
		}

		private void Viewer_MouseDown(object? sender, Microsoft.Msagl.Drawing.MsaglMouseEventArgs e)
		{
			try
			{
				if (viewer?.ObjectUnderMouseCursor?.DrawingObject is Microsoft.Msagl.Drawing.Node node)
				{
					ExpandGraphWithHCore(node.Id);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Bu makale için h-index hesaplanamıyor!\n\n{ex.Message}",
					"Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void ShowNodeTooltip(string nodeId)
		{
			var node = displayGraph.GetNode(nodeId);
			if (node == null)
			{
				NodeTooltip.Visibility = Visibility.Collapsed;
				return;
			}

			var paper = node.Paper;

			TooltipTitle.Text = paper.Title;

			TooltipAuthors.Text = string.Join(", ", paper.Authors.Take(3));
			if (paper.Authors.Count > 3)
				TooltipAuthors.Text += $" (+{paper.Authors.Count - 3} daha)";

			TooltipYear.Text = $"Yıl: {paper.Year}";
			TooltipCitations.Text = $"Atıf Sayısı: {paper.InCitationCount}";
			TooltipReferences.Text = $"Referans Sayısı: {paper.ReferencedWorks.Count}";
			TooltipId.Text = $"ID: {paper.Id}";

			var mousePos = Mouse.GetPosition(GraphHost);

			double tooltipWidth = 300;
			double tooltipHeight = 200;

			double left = mousePos.X + 15;
			double top = mousePos.Y + 15;

			if (left + tooltipWidth > GraphHost.ActualWidth)
				left = mousePos.X - tooltipWidth - 15;

			if (top + tooltipHeight > GraphHost.ActualHeight)
				top = mousePos.Y - tooltipHeight - 15;

			left = Math.Max(10, left);
			top = Math.Max(10, top);

			// Grid içinde Canvas.SetLeft/Top çalışmadığı için Margin ile konumlandır
			NodeTooltip.HorizontalAlignment = HorizontalAlignment.Left;
			NodeTooltip.VerticalAlignment = VerticalAlignment.Top;
			NodeTooltip.Margin = new Thickness(left, top, 0, 0);

			NodeTooltip.Visibility = Visibility.Visible;
		}

		private void SwitchMode(int mode)
		{
			currentMode = mode;

			switch (mode)
			{
				case 1:
					displayGraph = GraphBuilder.BuildFilteredGraph(allPapers, 50);
					HeaderText.Text = "Graf - 50 Makale (F1-F2-F3-F4: Seviyeler) / (F5: Yenile)";
					break;
				case 2:
					displayGraph = GraphBuilder.BuildFilteredGraph(allPapers, 200);
					HeaderText.Text = "Graf - 200 Makale (F1-F2-F3-F4: Seviyeler) / (F5: Yenile)";
					break;
				case 3:
					displayGraph = GraphBuilder.BuildFilteredGraph(allPapers, 1000);
					HeaderText.Text = "Graf - Tüm Makaleler (F1-F2-F3-F4: Seviyeler) / (F5: Yenile)";
					break;
				case 4:
					displayGraph = GraphBuilder.BuildTopCitedGraph(allPapers, 10);
					HeaderText.Text = "Graf - En Çok Atıf Alan 10 Makale (F1-F2-F3-F4: Seviyeler) / (F5: Yenile)";
					break;
				default:
					displayGraph = GraphBuilder.BuildFilteredGraph(allPapers, 50);
					HeaderText.Text = "Graf - 50 Makale (F1-F2-F3-F4: Seviyeler) / (F5: Yenile)";
					break;
			}

			if (viewer != null)
			{
				// ÖNEMLİ: Tooltip'i silmemek için GraphHost'u değil GraphContainer'ı temizle
				GraphContainer.Children.Clear();

				viewer = new GraphViewer();
				viewer.BindToPanel(GraphContainer);
				viewer.MouseDown += Viewer_MouseDown;
				viewer.MouseMove += Viewer_MouseMove;

				graphController = new MsaglGraphController(viewer);
				graphExpander = new GraphExpander(mainGraph, displayGraph);

				graphController.DrawGraph(displayGraph);

				UpdateStatistics();

				HIndexPanel.Visibility = Visibility.Collapsed;
				BetweennessPanel.Visibility = Visibility.Collapsed;
				NodeTooltip.Visibility = Visibility.Collapsed;

				StatusText.Text = $"Mod {mode} aktif - {displayGraph.GetTotalNodes()} düğüm gösteriliyor";
			}
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
					if (currentMode != 1) SwitchMode(1);
					break;
				case Key.F2:
					if (currentMode != 2) SwitchMode(2);
					break;
				case Key.F3:
					if (currentMode != 3) SwitchMode(3);
					break;
				case Key.F4:
					if (currentMode != 4) SwitchMode(4);
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
				try
				{
					graphExpander.ExpandWithHCore(hIndexResult.HCore);

					if (displayGraph.GetTotalNodes() > 1000)
					{
						MessageBox.Show($"Graf çok büyüdü ({displayGraph.GetTotalNodes()} düğüm)!\nMSAGL render edemeyebilir.\n\nF1/F2/F3 ile baştan başlayın.",
							"Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}

					graphController.DrawGraph(displayGraph);
					graphController.HighlightHCore(hIndexResult.HCore, nodeId);
					UpdateStatistics();
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Graf çizim hatası!\n\n{ex.Message}\n\nF5'e basarak yenileyin.",
						"Hata", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			StatusText.Text = $"H-Core gösterildi: {nodeId} - H-Index: {hIndexResult.HIndex}";
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
			StatusText.Text = "Betweenness Centrality hesaplanıyor... (Büyük graflarda 1-2 dakika sürebilir)";
			BetweennessPanel.Visibility = Visibility.Visible;
			BetweennessResultText.Text = "⏳ Hesaplanıyor...\nLütfen bekleyin.";

			Task.Run(() =>
			{
				var centrality = BetweennessCentrality.Calculate(displayGraph);

				Dispatcher.Invoke(() =>
				{
					var sortedResults = centrality.OrderByDescending(kvp => kvp.Value);

					BetweennessResultText.Text = "Betweenness Centrality (0'dan büyük):\n\n";
					int rank = 1;
					foreach (var kvp in sortedResults)
					{
						BetweennessResultText.Text += $"{rank}. {kvp.Key}: {kvp.Value:F4}\n";
						rank++;
					}

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
