using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Task1.Filters;

namespace Task1
{
    public partial class FilterEditorWindow : Window
    {
        public event Action<CurveFilter> FilterApplyRequested;

        public event Action<CurveFilter> FilterSaveRequested;

        private readonly List<CurveFilter> _filters;

        private CurveFilter _current;
        private int _dragIndex = -1;
        private bool _preventNameUpdate;

        private const double PointRadius = 5.0;
        private const double HitRadius = 7.0;

        public FilterEditorWindow(List<CurveFilter> sharedFilters)
        {
            InitializeComponent();
            FilterNameBox.FlowDirection = FlowDirection.LeftToRight;
            _filters = sharedFilters;
            RefreshFilterList();
            if (_filters.Count > 0)
                FilterListBox.SelectedIndex = 0;
        }

        // Filter list

        private void RefreshFilterList()
        {
            int prev = FilterListBox.SelectedIndex;
            FilterListBox.ItemsSource = null;
            FilterListBox.ItemsSource = _filters;
            if (prev >= 0 && prev < _filters.Count)
                FilterListBox.SelectedIndex = prev;
        }

        private void SelectFilter(CurveFilter filter)
        {
            _current = filter;
            bool hasFilter = filter != null;

            _preventNameUpdate = true;
            FilterNameBox.Text = filter?.Name ?? "";
            _preventNameUpdate = false;

            ApplyButton.IsEnabled = hasFilter;
            SaveToPanelButton.IsEnabled = hasFilter;
            DeleteFilterButton.IsEnabled = hasFilter && !IsPredefined(filter);

            RedrawCanvas();
        }

        private static bool IsPredefined(CurveFilter f) => f.Name == "Inversion" || f.Name == "Brightness" || f.Name == "Contrast";

        private void FilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectFilter(FilterListBox.SelectedItem as CurveFilter);
        }

        private void NewFilterButton_Click(object sender, RoutedEventArgs e)
        {
            int customCount = _filters.Count(f => !IsPredefined(f));
            var filter = new CurveFilter($"Custom {customCount + 1}");
            _filters.Add(filter);
            RefreshFilterList();
            FilterListBox.SelectedItem = filter;
        }

        private void DeleteFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null || IsPredefined(_current)) return;

            _filters.Remove(_current);
            RefreshFilterList();
            FilterListBox.SelectedIndex = _filters.Count > 0 ? 0 : -1;
        }

        private void FilterNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_preventNameUpdate || _current == null) return;

            _current.Name = FilterNameBox.Text;

            int caret = FilterNameBox.CaretIndex;
            int idx = FilterListBox.SelectedIndex;

            _preventNameUpdate = true;
            RefreshFilterList();
            FilterListBox.SelectedIndex = idx;
            _preventNameUpdate = false;

            FilterNameBox.CaretIndex = caret;
        }

        // Buttons

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_current != null)
                FilterApplyRequested?.Invoke(_current.Clone());
        }

        private void SaveToPanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_current != null)
                FilterSaveRequested?.Invoke(_current.Clone());
        }

        // Canvas drawing

        private void RedrawCanvas()
        {
            EditorCanvas.Children.Clear();
            DrawGrid();
            if (_current == null) return;
            DrawCurve();
            DrawControlPoints();
        }

        private void DrawGrid()
        {
            for (int i = 0; i <= 256; i += 64)
            {
                EditorCanvas.Children.Add(new Line
                { X1 = i, Y1 = 0, X2 = i, Y2 = 256, Stroke = Brushes.LightGray, StrokeThickness = 1 });
                EditorCanvas.Children.Add(new Line
                { X1 = 0, Y1 = i, X2 = 256, Y2 = i, Stroke = Brushes.LightGray, StrokeThickness = 1 });
            }

            EditorCanvas.Children.Add(new Line
            {
                X1 = 0,
                Y1 = 255,
                X2 = 255,
                Y2 = 0,
                Stroke = Brushes.LightSteelBlue,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 4 }
            });
        }

        private void DrawCurve()
        {
            EditorCanvas.Children.Add(new Polyline
            {
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 1.5,
                Points = new PointCollection(_current.Points.Select(ValueToCanvas))
            });
        }

        private void DrawControlPoints()
        {
            for (int i = 0; i < _current.Points.Count; i++)
            {
                Point cp = ValueToCanvas(_current.Points[i]);
                bool endpoint = (i == 0 || i == _current.Points.Count - 1);

                var ellipse = new Ellipse
                {
                    Width = PointRadius * 2,
                    Height = PointRadius * 2,
                    Fill = endpoint ? Brushes.OrangeRed : Brushes.DodgerBlue,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.5,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(ellipse, cp.X - PointRadius);
                Canvas.SetTop(ellipse, cp.Y - PointRadius);
                EditorCanvas.Children.Add(ellipse);
            }
        }

        // Mouse interaction

        private void EditorCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_current == null) return;

            Point pos = ClampToCanvas(e.GetPosition(EditorCanvas));
            int hit = HitTest(pos);

            if (hit >= 0)
            {
                _dragIndex = hit;
                EditorCanvas.CaptureMouse();
            }
            else
            {
                AddPoint(CanvasToValue(pos));
            }
        }

        private void EditorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragIndex < 0 || _current == null) return;

            MovePoint(_dragIndex, CanvasToValue(ClampToCanvas(e.GetPosition(EditorCanvas))));
            RedrawCanvas();
        }

        private void EditorCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragIndex = -1;
            EditorCanvas.ReleaseMouseCapture();
        }

        private void EditorCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_current == null) return;

            int hit = HitTest(e.GetPosition(EditorCanvas));

            if (hit > 0 && hit < _current.Points.Count - 1)
            {
                _current.Points.RemoveAt(hit);
                RedrawCanvas();
            }
        }

        // Point operations

        private void AddPoint(Point valuePoint)
        {
            int x = Clamp(valuePoint.X);
            int y = Clamp(valuePoint.Y);

            if (_current.Points.Any(p => (int)Math.Round(p.X) == x))
                return;

            int insertAt = _current.Points.Count;
            for (int i = 0; i < _current.Points.Count; i++)
            {
                if (_current.Points[i].X > x) { insertAt = i; break; }
            }

            _current.Points.Insert(insertAt, new Point(x, y));
            RedrawCanvas();
        }

        private void MovePoint(int index, Point valuePoint)
        {
            List<Point> pts = _current.Points;
            double newY = Math.Max(0, Math.Min(255, valuePoint.Y));
            double newX;

            if (index == 0)
                newX = 0;  
            else if (index == pts.Count - 1)
                newX = 255;  
            else
            {
                newX = Math.Max(pts[index - 1].X + 1, Math.Min(pts[index + 1].X - 1, valuePoint.X));
            }

            pts[index] = new Point(Math.Round(newX), Math.Round(newY));
        }

        private static Point ValueToCanvas(Point v) => new Point(v.X, 255.0 - v.Y);
        private static Point CanvasToValue(Point c) => new Point(c.X, 255.0 - c.Y);

        private static Point ClampToCanvas(Point p) =>
            new Point(Math.Max(0, Math.Min(255, p.X)), Math.Max(0, Math.Min(255, p.Y)));

        private static int Clamp(double v) => (int)Math.Max(0, Math.Min(255, Math.Round(v)));

        private int HitTest(Point canvasPos)
        {
            for (int i = 0; i < _current.Points.Count; i++)
            {
                Point cp = ValueToCanvas(_current.Points[i]);
                double dx = cp.X - canvasPos.X;
                double dy = cp.Y - canvasPos.Y;
                if (Math.Sqrt(dx * dx + dy * dy) <= HitRadius)
                    return i;
            }
            return -1;
        }
    }
}