namespace Task1.Filters
{
    public class MedianCutQuantizer : IFilter
    {
        public int NumColors { get; }

        public (byte r, byte g, byte b)[] LastPalette { get; private set; }

        public string Name => $"Median Cut ({NumColors} colours)";

        public MedianCutQuantizer(int numColors)
        {
            NumColors = Math.Max(2, numColors);
        }

        public PixelData Apply(PixelData source)
        {
            int numPixels = source.Width * source.Height;
            byte[] src = source.Pixels;

            var allIndices = Enumerable.Range(0, numPixels).ToList();

            var queue = new Queue<ColorBox>();
            var finalBoxes = new List<ColorBox>();

            queue.Enqueue(new ColorBox(allIndices, src));

            while (queue.Count + finalBoxes.Count < NumColors && queue.Count > 0)
            {
                ColorBox current = queue.Dequeue();

                if (!current.CanSplit)
                {
                    finalBoxes.Add(current);
                    continue;
                }

                var (left, right) = current.Split(src);
                queue.Enqueue(left);
                queue.Enqueue(right);
            }

            while (queue.Count > 0 && finalBoxes.Count < NumColors)
            {
                finalBoxes.Add(queue.Dequeue());
            }

            var palette = finalBoxes.Select(b => b.AverageColor(src)).ToArray();
            LastPalette = palette;

            byte[] dst = new byte[src.Length];
            for (int i = 0; i < numPixels; i++)
            {
                int bi = i * 4;
                byte pb = src[bi];
                byte pg = src[bi + 1];
                byte pr = src[bi + 2];

                var (nr, ng, nb) = FindNearest(pr, pg, pb, palette);

                dst[bi] = nb;
                dst[bi + 1] = ng;
                dst[bi + 2] = nr;
                dst[bi + 3] = src[bi + 3]; 
            }

            return new PixelData(dst, source.Width, source.Height, source.DpiX, source.DpiY);
        }
        
        private static (byte r, byte g, byte b) FindNearest(byte r, byte g, byte b, (byte r, byte g, byte b)[] palette)
        {
            int bestDist = int.MaxValue;
            var best = palette[0];

            foreach (var p in palette)
            {
                int dr = r - p.r;
                int dg = g - p.g;
                int db = b - p.b;
                int dist = dr * dr + dg * dg + db * db;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = p;

                    if (bestDist == 0)
                        break;
                }
            }

            return best;
        }

        // Represents one colour box in the RGB space
        private class ColorBox
        {
            // Pixel indices in the array
            public List<int> Indices;

            public byte MinR, MaxR;
            public byte MinG, MaxG;
            public byte MinB, MaxB;

            public int LargestRange => Math.Max(MaxR - MinR, Math.Max(MaxG - MinG, MaxB - MinB));

            public bool CanSplit => Indices.Count > 1 && LargestRange > 0;

            public ColorBox(List<int> indices, byte[] pixels)
            {
                Indices = indices;
                ComputeBounds(pixels);
            }

            private void ComputeBounds(byte[] pixels)
            {
                MinR = MaxR = pixels[Indices[0] * 4 + 2];
                MinG = MaxG = pixels[Indices[0] * 4 + 1];
                MinB = MaxB = pixels[Indices[0] * 4];

                foreach (int idx in Indices)
                {
                    int bi = idx * 4;
                    byte b = pixels[bi];
                    byte g = pixels[bi + 1];
                    byte r = pixels[bi + 2];

                    if (r < MinR) MinR = r;
                    if (r > MaxR) MaxR = r;

                    if (g < MinG) MinG = g;
                    if (g > MaxG) MaxG = g;

                    if (b < MinB) MinB = b;
                    if (b > MaxB) MaxB = b;
                }
            }

            public (ColorBox left, ColorBox right) Split(byte[] pixels)
            {
                int rangeR = MaxR - MinR;
                int rangeG = MaxG - MinG;
                int rangeB = MaxB - MinB;

                int splitChannel; 
                if (rangeR >= rangeG && rangeR >= rangeB)
                    splitChannel = 2;
                else if (rangeG >= rangeB)
                    splitChannel = 1;
                else
                    splitChannel = 0;

                Indices.Sort((a, b) => pixels[a * 4 + splitChannel].CompareTo(pixels[b * 4 + splitChannel]));

                int mid = Indices.Count / 2;

                var leftIndices = Indices.GetRange(0, mid);
                var rightIndices = Indices.GetRange(mid, Indices.Count - mid);

                return (new ColorBox(leftIndices, pixels), new ColorBox(rightIndices, pixels));
            }

            public (byte r, byte g, byte b) AverageColor(byte[] pixels)
            {
                long sumR = 0, sumG = 0, sumB = 0;

                foreach (int idx in Indices)
                {
                    int bi = idx * 4;
                    sumB += pixels[bi];
                    sumG += pixels[bi + 1];
                    sumR += pixels[bi + 2];
                }

                int count = Indices.Count;

                return ((byte)(sumR / count), (byte)(sumG / count), (byte)(sumB / count));
            }
        }
    }
}