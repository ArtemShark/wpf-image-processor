using System.Windows;

namespace Task1.Filters
{
    // A functional filter defined by a polyline of control points
    public class CurveFilter : IFilter
    {
        public string Name { get; set; }

        public List<Point> Points { get; private set; }

        public CurveFilter(string name)
        {
            Name = name;
            Points = new List<Point> { new Point(0, 0), new Point(255, 255) };
        }

        public CurveFilter(string name, List<Point> points)
        {
            Name = name;
            Points = points.OrderBy(p => p.X).ToList();
        }

        public PixelData Apply(PixelData source)
        {
            byte[] lut = BuildLut();
            byte[] pixels = (byte[])source.Pixels.Clone();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = lut[pixels[i]];      
                pixels[i + 1] = lut[pixels[i + 1]];  
                pixels[i + 2] = lut[pixels[i + 2]];
            }

            return new PixelData(pixels, source.Width, source.Height, source.DpiX, source.DpiY);
        }

        // Build a lookup table 
        public byte[] BuildLut()
        {
            byte[] lut = new byte[256];

            for (int x = 0; x < 256; x++)
            {
                Point left = Points.First();
                Point right = Points.Last();

                for (int i = 0; i < Points.Count - 1; i++)
                {
                    if (Points[i].X <= x && x <= Points[i + 1].X)
                    {
                        left = Points[i];
                        right = Points[i + 1];
                        break;
                    }
                }

                double t = (right.X == left.X) ? 0.0 : (x - left.X) / (right.X - left.X);
                double y = left.Y + t * (right.Y - left.Y);
                lut[x] = (byte)Math.Max(0, Math.Min(255, Math.Round(y)));
            }

            return lut;
        }

        public CurveFilter Clone() => new CurveFilter(Name, new List<Point>(Points));

        public static CurveFilter FromInversion() =>
            new CurveFilter("Inversion", new List<Point>
            {
                new Point(0, 255),
                new Point(255, 0),
            });

        public static CurveFilter FromBrightness(int value = 50)
        {
            if (value >= 0)
            {
                int clipX = Math.Min(255, 255 - value);
                return new CurveFilter("Brightness", new List<Point>
                {
                    new Point(0, Math.Min(255, value)),
                    new Point(clipX, 255),
                    new Point(255, 255),
                });
            }
            else
            {
                int zeroX = Math.Min(255, -value);
                return new CurveFilter("Brightness", new List<Point>
                {
                    new Point(0, 0),
                    new Point(zeroX, 0),
                    new Point(255, Math.Max(0, 255 + value)),
                });
            }
        }

        public static CurveFilter FromContrast(double factor = 1.5)
        {
            int xLow = (int)Math.Max(0, Math.Min(255, Math.Round(128.0 - 128.0 / factor)));
            int xHigh = (int)Math.Max(0, Math.Min(255, Math.Round(128.0 + 127.0 / factor)));

            var pts = new List<Point> { new Point(0, 0) };
            if (xLow > 0) pts.Add(new Point(xLow, 0));
            pts.Add(new Point(128, 128));
            if (xHigh < 255) pts.Add(new Point(xHigh, 255));
            pts.Add(new Point(255, 255));

            return new CurveFilter("Contrast", pts);
        }
    }
}