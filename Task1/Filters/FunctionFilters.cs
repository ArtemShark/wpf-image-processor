namespace Task1.Filters
{
    public class InversionFilter : IFilter
    {
        public string Name => "Inversion";

        public PixelData Apply(PixelData source)
        {
            byte[] pixels = (byte[])source.Pixels.Clone();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = (byte)(255 - pixels[i]);      
                pixels[i + 1] = (byte)(255 - pixels[i + 1]);  
                pixels[i + 2] = (byte)(255 - pixels[i + 2]);
            }

            return new PixelData(pixels, source.Width, source.Height, source.DpiX, source.DpiY);
        }
    }

    public class BrightnessFilter : IFilter
    {
        private const int BrightnessValue = 50;

        public string Name => "Brightness";

        public PixelData Apply(PixelData source)
        {
            byte[] pixels = (byte[])source.Pixels.Clone();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = PixelHelper.Clamp(pixels[i] + BrightnessValue);
                pixels[i + 1] = PixelHelper.Clamp(pixels[i + 1] + BrightnessValue);
                pixels[i + 2] = PixelHelper.Clamp(pixels[i + 2] + BrightnessValue);
            }

            return new PixelData(pixels, source.Width, source.Height, source.DpiX, source.DpiY);
        }
    }

    public class ContrastFilter : IFilter
    {
        private const double ContrastFactor = 1.5;

        public string Name => "Contrast";

        public PixelData Apply(PixelData source)
        {
            byte[] pixels = (byte[])source.Pixels.Clone();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = PixelHelper.Clamp((int)((pixels[i] - 128) * ContrastFactor + 128));
                pixels[i + 1] = PixelHelper.Clamp((int)((pixels[i + 1] - 128) * ContrastFactor + 128));
                pixels[i + 2] = PixelHelper.Clamp((int)((pixels[i + 2] - 128) * ContrastFactor + 128));
            }

            return new PixelData(pixels, source.Width, source.Height, source.DpiX, source.DpiY);
        }
    }

    public class GammaFilter : IFilter
    {
        private const double GammaValue = 0.5;

        public string Name => "Gamma";

        public PixelData Apply(PixelData source)
        {
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++)
                lut[i] = PixelHelper.Clamp((int)Math.Round(255.0 * Math.Pow(i / 255.0, GammaValue)));

            byte[] pixels = (byte[])source.Pixels.Clone();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = lut[pixels[i]];
                pixels[i + 1] = lut[pixels[i + 1]];
                pixels[i + 2] = lut[pixels[i + 2]];
            }

            return new PixelData(pixels, source.Width, source.Height, source.DpiX, source.DpiY);
        }
    }
}