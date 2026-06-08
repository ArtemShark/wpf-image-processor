using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Task1.Filters
{
    public static class PixelHelper
    {
        public static byte Clamp(int value) => (byte)Math.Max(0, Math.Min(255, value));

        public static PixelData FromBitmapSource(BitmapSource source)
        {
            BitmapSource bgra = source;
            if (source.Format != PixelFormats.Bgra32)
                bgra = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int stride = bgra.PixelWidth * 4;
            byte[] pixels = new byte[bgra.PixelHeight * stride];
            bgra.CopyPixels(pixels, stride, 0);

            return new PixelData(pixels, bgra.PixelWidth, bgra.PixelHeight, source.DpiX, source.DpiY);
        }

        public static WriteableBitmap ToWriteableBitmap(PixelData data)
        {
            var wb = new WriteableBitmap(data.Width, data.Height, data.DpiX, data.DpiY, PixelFormats.Bgra32, null);

            wb.WritePixels(new Int32Rect(0, 0, data.Width, data.Height), data.Pixels, data.Stride, 0);

            return wb;
        }
    }
}