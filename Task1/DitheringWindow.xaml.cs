using System.Windows;
using Task1.Filters;

namespace Task1
{
    public partial class DitheringWindow : Window
    {
        public event Action<IFilter> AlgorithmApplyRequested;

        public DitheringWindow()
        {
            InitializeComponent();
        }

        private void GreyscaleButton_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmApplyRequested?.Invoke(new GreyscaleFilter());
        }

        private void ApplyDitheringButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DitherLevelsBox.Text, out int k) || k < 2 || k > 256)
            {
                MessageBox.Show("Levels per channel must be a whole number between 2 and 256.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AlgorithmApplyRequested?.Invoke(new RandomDitheringFilter(k));
        }

        private void ApplyMedianCutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(MedianColorsBox.Text, out int n) || n < 2 || n > 256)
            {
                MessageBox.Show("Number of colours must be a whole number between 2 and 256.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AlgorithmApplyRequested?.Invoke(new MedianCutQuantizer(n));
        }
    }
}