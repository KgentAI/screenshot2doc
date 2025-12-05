using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SETUNA.Main.AI.Exceptions;
using SETUNA.Main.AI.Export;
using SETUNA.Main.AI.Models;
using SETUNA.Main.AI.Services;
using SETUNA.Main.AI.UI;
using SETUNA.Main.Option;
using AISummaryConfig = SETUNA.Main.Option.SetunaOption.AISummaryConfig;

namespace SETUNA.Main.AI
{
    /// <summary>
    /// Main form for AI Screenshot Summary feature
    /// </summary>
    public partial class AISummaryForm : BaseForm
    {
        private AISummaryConfig _config;
        private List<ScrapBase> _screenshots;
        private MultimodalResponse _currentResponse;
        private CancellationTokenSource _cancellationTokenSource;
        private string _markdownCacheFile;

        // UI Controls
        private SplitContainer splitContainer;
        private FlowLayoutPanel screenshotPanel;
        private TableLayoutPanel controlBar;
        private RichTextBox markdownDisplay;
        private StatusStrip statusBar;
        private ComboBox engineComboBox;
        private Button analyzeButton;
        private Button exportMarkdownButton;
        private Button exportExcelButton;
        private ProgressBar progressBar;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel modelLabel;
        private ToolStripStatusLabel timeLabel;
        private TextBox contextTextBox;

        public AISummaryForm()
        {
            // Set up cache file path
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(exePath);
            _markdownCacheFile = System.IO.Path.Combine(exeDir, "ai_summary_cache.md");
            
            InitializeComponent();
            InitializeControls();
            LoadConfiguration();
            LoadScreenshots();
            LoadCachedMarkdown();
        }

        private void InitializeComponent()
        {
            this.Text = "AI Screenshot Summary";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
        }

        private void InitializeControls()
        {
            // Create split container
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 360,
                FixedPanel = FixedPanel.Panel1
            };

            // Left panel: Screenshot selection
            screenshotPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                BackColor = SystemColors.Control,
                Padding = new Padding(10)
            };
            splitContainer.Panel1.Controls.Add(screenshotPanel);

            // Right panel setup
            var rightPanel = new Panel { Dock = DockStyle.Fill };

            // Control bar
            controlBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                ColumnCount = 6,
                Padding = new Padding(5)
            };
            controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // Controls in control bar
            var modelLabel = new Label { Text = "Model:", AutoSize = true, Anchor = AnchorStyles.Left };
            engineComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
            engineComboBox.Items.Add("MiniCPM-V-4.5 (Local)");
            engineComboBox.Items.Add("Qwen3-VL-Flash (Cloud)");
            
            analyzeButton = new Button { Text = "Analyze", Width = 100 };
            analyzeButton.Click += AnalyzeButton_Click;
            
            exportMarkdownButton = new Button { Text = "Export Markdown", Width = 130, Enabled = false };
            exportMarkdownButton.Click += ExportMarkdownButton_Click;
            
            exportExcelButton = new Button { Text = "Export Excel", Width = 100, Enabled = false };
            exportExcelButton.Click += ExportExcelButton_Click;
            
            progressBar = new ProgressBar { Visible = false, Width = 100, Style = ProgressBarStyle.Marquee };

            controlBar.Controls.Add(modelLabel, 0, 0);
            controlBar.Controls.Add(engineComboBox, 1, 0);
            controlBar.Controls.Add(analyzeButton, 2, 0);
            controlBar.Controls.Add(exportMarkdownButton, 3, 0);
            controlBar.Controls.Add(exportExcelButton, 4, 0);
            controlBar.Controls.Add(progressBar, 5, 0);

            // Markdown display
            markdownDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };

            // Status bar
            statusBar = new StatusStrip();
            statusLabel = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            this.modelLabel = new ToolStripStatusLabel { TextAlign = ContentAlignment.MiddleRight };
            timeLabel = new ToolStripStatusLabel { TextAlign = ContentAlignment.MiddleRight };
            statusBar.Items.Add(statusLabel);
            statusBar.Items.Add(this.modelLabel);
            statusBar.Items.Add(timeLabel);

            // Add context panel (optional user context)
            var contextPanel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(5) };
            var contextLabel = new Label { Text = "上下文（可选）：", AutoSize = true, Anchor = AnchorStyles.Left };
            contextTextBox = new TextBox { Multiline = true, Height = 40, Dock = DockStyle.Fill };
            contextPanel.Controls.Add(contextLabel);
            contextPanel.Controls.Add(contextTextBox);
            contextLabel.Location = new Point(5, 10);
            contextTextBox.Location = new Point(contextLabel.Right + 8, 8);

            // Add to right panel
            rightPanel.Controls.Add(contextPanel);
            rightPanel.Controls.Add(markdownDisplay);
            rightPanel.Controls.Add(controlBar);
            rightPanel.Controls.Add(statusBar);
            splitContainer.Panel2.Controls.Add(rightPanel);

            // Add to form
            this.Controls.Add(splitContainer);
        }

        private void LoadConfiguration()
        {
            _config = Mainform.Instance.optSetuna.AISummary;
            if (_config == null)
            {
                _config = new AISummaryConfig();
                Mainform.Instance.optSetuna.AISummary = _config;
            }

            // Set engine selection
            if (_config.EngineType == "cloud")
                engineComboBox.SelectedIndex = 1;
            else
                engineComboBox.SelectedIndex = 0;
        }

        private void LoadScreenshots()
        {
            _screenshots = new List<ScrapBase>();
            
            var scrapBook = Mainform.Instance.scrapBook;
            if (scrapBook == null)
                return;

            foreach (var scrap in scrapBook)
            {
                if (scrap.Image != null)
                {
                    _screenshots.Add(scrap);
                }
            }

            // Sort by DateTime descending
            _screenshots = _screenshots.OrderByDescending(s => s.DateTime).Take(_config.MaxImages).ToList();

            // Create thumbnails
            for (int i = 0; i < _screenshots.Count; i++)
            {
                var pictureBox = CreateThumbnail(_screenshots[i], i + 1);
                screenshotPanel.Controls.Add(pictureBox);
            }

            statusLabel.Text = $"Loaded {_screenshots.Count} screenshot(s)";
        }

        private void LoadCachedMarkdown()
        {
            try
            {
                if (System.IO.File.Exists(_markdownCacheFile))
                {
                    var cachedContent = System.IO.File.ReadAllText(_markdownCacheFile, System.Text.Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(cachedContent))
                    {
                        MarkdownRenderer.Render(markdownDisplay, cachedContent);
                        exportMarkdownButton.Enabled = true;
                        statusLabel.Text = "Loaded cached analysis from previous session";
                        
                        // Create a dummy response object for export functionality
                        _currentResponse = new MultimodalResponse
                        {
                            Success = true,
                            MarkdownContent = cachedContent,
                            ProcessingTimeMs = 0
                        };
                    }
                }
            }
            catch
            {
                // Silently ignore cache loading errors
            }
        }

        private PictureBox CreateThumbnail(ScrapBase scrap, int index)
        {
            var pb = new PictureBox
            {
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = scrap.Image,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            // Add badge with index
            pb.Paint += (s, e) =>
            {
                var g = e.Graphics;
                var badgeSize = 25;
                var x = pb.Width - badgeSize - 5;
                var y = pb.Height - badgeSize - 5;
                
                g.FillEllipse(new SolidBrush(Color.FromArgb(217, 255, 255, 255)), x, y, badgeSize, badgeSize);
                g.DrawEllipse(new Pen(Color.Gray, 2), x, y, badgeSize, badgeSize);
                
                var font = new Font("Arial", 12, FontStyle.Bold);
                var text = index.ToString();
                var textSize = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.Black, x + (badgeSize - textSize.Width) / 2, y + (badgeSize - textSize.Height) / 2);
            };

            return pb;
        }

        private async void AnalyzeButton_Click(object sender, EventArgs e)
        {
            if (_screenshots == null || _screenshots.Count == 0)
            {
                MessageBox.Show("No screenshots available to analyze.", "No Screenshots", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            progressBar.Visible = true;
            statusLabel.Text = "Analyzing screenshots...";

            try
            {
                // Update config from UI (store model name in ModelName instead of EngineType)
                _config.ModelName = engineComboBox.SelectedIndex == 1 ? "qwen3-vl-flash" : "minicpm-v4.5";
                _config.EngineType = engineComboBox.SelectedIndex == 1 ? "cloud" : "local";

                // Create cancellation token
                _cancellationTokenSource = new CancellationTokenSource();

                // Perform analysis
                _currentResponse = await PerformAnalysisAsync(_cancellationTokenSource.Token);

                if (_currentResponse.Success)
                {
                    MarkdownRenderer.Render(markdownDisplay, _currentResponse.MarkdownContent);
                    exportMarkdownButton.Enabled = true;
                    exportExcelButton.Enabled = true;
                    statusLabel.Text = "Analysis complete";
                    timeLabel.Text = $"{_currentResponse.ProcessingTimeMs / 1000.0:F2}s";
                    
                    // Save to cache
                    SaveMarkdownCache(_currentResponse.MarkdownContent);
                }
                else
                {
                    MessageBox.Show(_currentResponse.ErrorMessage, "Analysis Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Analysis failed";
                }
            }
            catch (AIServiceException ex)
            {
                MessageBox.Show(ex.Message, "Service Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Analysis failed";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Analysis failed";
            }
            finally
            {
                SetControlsEnabled(true);
                progressBar.Visible = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task<MultimodalResponse> PerformAnalysisAsync(CancellationToken cancellationToken)
        {
            // Compose prompt with optional user context and metrics-first directive (Chinese output enforced)
            var basePrompt = _config.PromptTemplate ?? string.Empty;
            var extraDirective = "请优先抽取通用/可比的核心指标，并以一个或多个 Markdown 表格输出；如维度不同（页面/模块、时间、业务类别），请拆分为多张表。";
            var userContext = contextTextBox?.Text?.Trim();
            var composedCore = string.IsNullOrEmpty(userContext)
                ? $"{basePrompt}\n{extraDirective}"
                : $"分析上下文：{userContext}\n\n{basePrompt}\n{extraDirective}";
            var composedPrompt = $"{composedCore}\n请严格按照以上要求以中文输出。";

            var request = new MultimodalRequest
            {
                Prompt = composedPrompt,
                MaxTokens = 2000,
                Temperature = 0.7f
            };

            foreach (var screenshot in _screenshots)
            {
                request.Images.Add(screenshot.Image);
            }

            var service = await MultimodalServiceFactory.GetServiceAsync(_config);
            modelLabel.Text = service.ModelName;
            
            return await service.AnalyzeImagesAsync(request, cancellationToken);
        }

        private void ExportMarkdownButton_Click(object sender, EventArgs e)
        {
            if (_currentResponse == null || !_currentResponse.Success)
                return;

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Markdown Files (*.md)|*.md|All Files (*.*)|*.*";
                dialog.DefaultExt = "md";
                dialog.FileName = MarkdownExporter.GetDefaultFilename();
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var metadata = new AISummaryMetadata
                    {
                        GeneratedAt = DateTime.Now,
                        ModelName = modelLabel.Text,
                        ImageCount = _screenshots.Count,
                        ProcessingTime = TimeSpan.FromMilliseconds(_currentResponse.ProcessingTimeMs)
                    };

                    if (MarkdownExporter.ExportToFile(_currentResponse.MarkdownContent, dialog.FileName, metadata))
                    {
                        MessageBox.Show($"Exported successfully to:\n{dialog.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to export markdown file.", "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportExcelButton_Click(object sender, EventArgs e)
        {
            // Excel export is temporarily disabled (requires EPPlus library)
            MessageBox.Show(
                "Excel export is not available in this version.\n" +
                "Please use Markdown export instead.",
                "Feature Not Available",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SetControlsEnabled(bool enabled)
        {
            engineComboBox.Enabled = enabled;
            analyzeButton.Enabled = enabled;
            exportMarkdownButton.Enabled = enabled && _currentResponse != null && _currentResponse.Success;
            // Excel export disabled - requires EPPlus library
            exportExcelButton.Enabled = false;
        }

        private void SaveMarkdownCache(string content)
        {
            try
            {
                System.IO.File.WriteAllText(_markdownCacheFile, content, System.Text.Encoding.UTF8);
            }
            catch
            {
                // Silently ignore cache saving errors
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
                _screenshots?.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
