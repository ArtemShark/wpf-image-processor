namespace Task1.Filters
{
    public class GreyscaleFilter : IFilter
    {
        public string Name => "Greyscale";

        public PixelData Apply(PixelData source)
        {
            byte[] pixels = (byte[])source.Pixels.Clone();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];

                byte y = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                pixels[i] = y; 
                pixels[i + 1] = y; 
                pixels[i + 2] = y; 
            }

            return new PixelData(pixels, source.Width, source.Height, source.DpiX, source.DpiY);
        }
    }
}