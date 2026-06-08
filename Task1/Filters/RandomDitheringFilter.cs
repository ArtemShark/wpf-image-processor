namespace Task1.Filters
{
    public class RandomDitheringFilter : IFilter
    {
        public int LevelsPerChannel { get; }

        public string Name => $"Random Dithering (K={LevelsPerChannel})";

        public RandomDitheringFilter(int levelsPerChannel)
        {
            LevelsPerChannel = Math.Max(2, levelsPerChannel);
        }

        public PixelData Apply(PixelData source)
        {
            int k = LevelsPerChannel;
            byte[] levels = new byte[k];
            for (int j = 0; j < k; j++)
                levels[j] = (byte)Math.Round(j * 255.0 / (k - 1));

            byte[] pixels = (byte[])source.Pixels.Clone();
            var rng = new Random();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                double threshold = rng.NextDouble();

                for (int ch = 0; ch < 3; ch++)
                {
                    double normalized = pixels[i + ch] / 255.0;

                    int col = (int)Math.Floor((k - 1) * normalized);
                    col = Math.Min(col, k - 2);

                    double remainder = (k - 1) * normalized - col;

                    if (remainder >= threshold)
                        col++;

                    pixels[i + ch] = levels[col];
                }
            }

            return new PixelData(pixels, source.Width, source.Height, source.DpiX, source.DpiY);
        }
    }
}