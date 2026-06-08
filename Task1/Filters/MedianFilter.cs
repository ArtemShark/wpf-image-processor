namespace Task1.Filters
{
    public class MedianFilter : IFilter
    {
        public string Name => "Median";

        public PixelData Apply(PixelData source)
        {
            int width = source.Width;
            int height = source.Height;
            int stride = source.Stride;

            byte[] src = source.Pixels;
            byte[] dst = new byte[src.Length];

            byte[] window = new byte[9];

            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    for (int ch = 0; ch < 3; ch++)
                    {
                        int k = 0;
                        for(int ky = -1;  ky <= 1; ky++)
                        {
                            for(int kx =  -1; kx <= 1; kx++)
                            {
                                int nx = Math.Max(0, Math.Min(width - 1, x + kx));
                                int ny = Math.Max(0, Math.Min(height - 1, y + ky));
                                window[k++] = src[ny * stride + nx * 4 + ch];
                            }
                        }
                        Array.Sort(window);
                        dst[y * stride + x * 4 + ch] = window[4];
                    }
                    dst[y * stride + x * 4 + 3] = src[y * stride + x * 4 + 3];
                }
            }
            return new PixelData(dst, width, height, source.DpiX, source.DpiY);
        }
    }
}
