namespace Task1.Filters
{
    public class ConvolutionFilter : IFilter
    {
        public string Name { get; }

        private readonly int[,] _kernel;
        private readonly int _divisor;
        private readonly int _offset;

        public ConvolutionFilter(string name, int[,] kernel, int divisor, int offset)
        {
            Name = name;
            _kernel = kernel;
            _divisor = divisor;
            _offset = offset;
        }

        public PixelData Apply(PixelData source)
        {
            int width = source.Width;
            int height = source.Height;
            int stride = source.Stride;
            int kRadius = _kernel.GetLength(0) / 2;

            byte[] src = source.Pixels;
            byte[] dst = new byte[src.Length];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int ch = 0; ch < 3; ch++)
                    {
                        int sum = 0;

                        for (int ky = -kRadius; ky <= kRadius; ky++)
                        {
                            for (int kx = -kRadius; kx <= kRadius; kx++)
                            {
                                // any coordinate that falls outside the image is clamped to the nearest edge pixel
                                int nx = Math.Max(0, Math.Min(width - 1, x + kx)); 
                                int ny = Math.Max(0, Math.Min(height - 1, y + ky));

                                sum += src[ny * stride + nx * 4 + ch] * _kernel[ky + kRadius, kx + kRadius];
                            }
                        }

                        dst[y * stride + x * 4 + ch] = PixelHelper.Clamp(_offset + sum / _divisor);
                    }

                    // Alpha channel: copy unchanged
                    dst[y * stride + x * 4 + 3] = src[y * stride + x * 4 + 3];
                }
            }

            return new PixelData(dst, width, height, source.DpiX, source.DpiY);
        }
    }
}