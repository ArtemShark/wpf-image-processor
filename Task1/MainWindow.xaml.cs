using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Task1.Filters;

namespace Task1
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap _originalBitmap;

        private WriteableBitmap _currentBitmap;

        private readonly Dictionary<string, IFilter> _filters = new Dictionary<string, IFilter>();

        private FilterEditorWindow _editorWindow;

        private DitheringWindow _ditheringWindow;

        private readonly List<CurveFilter> _curveFilters = new List<CurveFilter>
        {
            CurveFilter.FromInversion(),
            CurveFilter.FromBrightness(),
            CurveFilter.FromContrast(),
        };

        public MainWindow()
        {
            InitializeComponent();
            BuildFilterToolbar();
        }

        // Toolbar setup

        private void BuildFilterToolbar()
        {
            FiltersPanel.Children.Add(MakeLabel("Function Filters:"));
            foreach (var filter in FilterFactory.CreateFunctionFilters())
            {
                _filters[filter.Name] = filter;
                FiltersPanel.Children.Add(MakeFilterButton(filter.Name));
            }

            FiltersPanel.Children.Add(new Separator
            {
                Style = (Style)FindResource(ToolBar.SeparatorStyleKey),
                Margin = new Thickness(6, 0, 6, 0),
                Height = 20
            });

            FiltersPanel.Children.Add(MakeLabel("Convolution Filters:"));
            foreach (var filter in FilterFactory.CreateConvolutionFilters())
            {
                _filters[filter.Name] = filter;
                FiltersPanel.Children.Add(MakeFilterButton(filter.Name));
            }
        }

        private static TextBlock MakeLabel(string text) => new TextBlock
        {
            Text = text,
            FontWeight = FontWeights.SemiBold,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 6, 0)
        };

        private Button MakeFilterButton(string filterName)
        {
            var btn = new Button
            {
                Content = filterName,
                Tag = filterName,
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(2, 1, 2, 1),
                FontSize = 12
            };
            btn.Click += FilterButton_Click;
            return btn;
        }

        // File operations

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Image",
                Filter = "Image files (*.bmp;*.png;*.jpg;*.jpeg;*.tiff)|*.bmp;*.png;*.jpg;*.jpeg;*.tiff|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                BitmapDecoder decoder = BitmapDecoder.Create(new Uri(dialog.FileName), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                _originalBitmap = PixelHelper.ToWriteableBitmap(PixelHelper.FromBitmapSource(decoder.Frames[0]));
                _currentBitmap = new WriteableBitmap(_originalBitmap);

                OriginalImage.Source = _originalBitmap;
                ResultImage.Source = _currentBitmap;

                SaveButton.IsEnabled = true;
                ResetButton.IsEnabled = true;

                UpdateStatus($"Loaded: {Path.GetFileName(dialog.FileName)}  —  " +
                             $"{_originalBitmap.PixelWidth} × {_originalBitmap.PixelHeight} px");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load image:\n{ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBitmap == null) return;

            var dialog = new SaveFileDialog
            {
                Title = "Save Result Image",
                Filter = "Bitmap (*.bmp)|*.bmp",
                DefaultExt = "bmp"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_currentBitmap));
                using (FileStream fs = File.OpenWrite(dialog.FileName))
                    encoder.Save(fs);

                UpdateStatus($"Saved: {Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save image:\n{ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_originalBitmap == null) return;

            _currentBitmap = new WriteableBitmap(_originalBitmap);
            ResultImage.Source = _currentBitmap;
            UpdateStatus("Reset to original.");
        }

        // Applying filters
        // Pixels are extracted on the UI thread, processed on a background thread and then the result is written back on the UI thread

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            string filterName = (string)((Button)sender).Tag;
            if (_filters.TryGetValue(filterName, out IFilter filter))
                await ApplyFilterAsync(filter);
        }

        private async void OnCurveFilterApply(CurveFilter filter)
        {
            await ApplyFilterAsync(filter);
        }

        private async Task ApplyFilterAsync(IFilter filter)
        {
            if (_currentBitmap == null)
            {
                MessageBox.Show("Please load an image first.", "No Image", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PixelData input = PixelHelper.FromBitmapSource(_currentBitmap);

            SetUiEnabled(false);
            Cursor = Cursors.Wait;
            UpdateStatus($"Applying: {filter.Name} …");

            try
            {
                PixelData result = await Task.Run(() => filter.Apply(input));

                _currentBitmap = PixelHelper.ToWriteableBitmap(result);
                ResultImage.Source = _currentBitmap;
                UpdateStatus($"Applied: {filter.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying '{filter.Name}':\n{ex.Message}", "Filter Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Filter failed.");
            }
            finally
            {
                Cursor = null;
                SetUiEnabled(true);
            }
        }

        private void SetUiEnabled(bool enabled)
        {
            LoadButton.IsEnabled = enabled;
            SaveButton.IsEnabled = enabled && _currentBitmap != null;
            ResetButton.IsEnabled = enabled && _originalBitmap != null;
            foreach (UIElement child in FiltersPanel.Children)
                if (child is Button btn) btn.IsEnabled = enabled;
        }

        // Filter Editor window

        private void OpenEditorButton_Click(object sender, RoutedEventArgs e)
        {
            _editorWindow = new FilterEditorWindow(_curveFilters);
            _editorWindow.FilterApplyRequested += OnCurveFilterApply;
            _editorWindow.FilterSaveRequested += OnFilterSaved;
            _editorWindow.Show();
        }

        // Dithering window

        private void OpenDitheringButton_Click(object sender, RoutedEventArgs e)
        {
            _ditheringWindow = new DitheringWindow();
            _ditheringWindow.AlgorithmApplyRequested += async filter => await ApplyFilterAsync(filter);
            _ditheringWindow.Show();
        }

        private void OnFilterSaved(CurveFilter filter)
        {
            CurveFilter snapshot = filter.Clone();

            foreach (UIElement child in FiltersPanel.Children)
            {
                if (child is Button btn && btn.Content as string == filter.Name)
                {
                    RewireButton(btn, snapshot);
                    UpdateStatus($"Filter '{filter.Name}' updated in panel.");
                    return;
                }
            }

            bool hasSavedLabel = false;
            foreach (UIElement child in FiltersPanel.Children)
            {
                if (child is TextBlock tb && tb.Text == "Saved Filters:")
                {
                    hasSavedLabel = true;
                    break;
                }
            }

            if (!hasSavedLabel)
            {
                FiltersPanel.Children.Add(new Separator
                {
                    Style = (Style)FindResource(ToolBar.SeparatorStyleKey),
                    Margin = new Thickness(6, 0, 6, 0),
                    Height = 20
                });
                FiltersPanel.Children.Add(MakeLabel("Saved Filters:"));
            }

            var newBtn = new Button
            {
                Content = filter.Name,
                Tag = filter.Name,
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(2, 1, 2, 1),
                FontSize = 12
            };
            RewireButton(newBtn, snapshot);
            FiltersPanel.Children.Add(newBtn);
            UpdateStatus($"Filter '{filter.Name}' added to panel.");
        }

        private void RewireButton(Button btn, CurveFilter snapshot)
        {
            if (btn.Tag is CurveButtonHandler old)
                btn.Click -= old.Handler;

            btn.Click -= FilterButton_Click;

            var wrapper = new CurveButtonHandler(async (s, ev) =>
                await ApplyFilterAsync(snapshot));

            btn.Tag = wrapper;
            btn.Click += wrapper.Handler;
        }

        // Wrapper class that stores a reference to the handler delegate so it can be removed later when the button is reassigned
        private class CurveButtonHandler
        {
            public RoutedEventHandler Handler { get; }
            public CurveButtonHandler(RoutedEventHandler handler) { Handler = handler; }
        }

        private void UpdateStatus(string message) => StatusText.Text = message;
    }
}