namespace Task1.Filters
{
    // Pixel format is four bytes per pixel in B, G, R, A order
    public class PixelData
    {
        public byte[] Pixels { get; }
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; } 
        public double DpiX { get; }
        public double DpiY { get; }

        public PixelData(byte[] pixels, int width, int height, double dpiX, double dpiY)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
            Stride = width * 4;
            DpiX = dpiX;
            DpiY = dpiY;
        }
    }
}