namespace Task1.Filters
{
    // Place to register all filters and change their parameters

    public static class FilterFactory
    {
        public static List<IFilter> CreateFunctionFilters()
        {
            return new List<IFilter>
            {
                new InversionFilter(),
                new BrightnessFilter(),
                new ContrastFilter(),
                new GammaFilter(),
            };
        }

        public static List<IFilter> CreateConvolutionFilters()
        {
            return new List<IFilter>
            {
                new ConvolutionFilter("Box Blur",
                    kernel: new int[,]
                    {
                        { 1, 1, 1 },
                        { 1, 1, 1 },
                        { 1, 1, 1 },
                    },
                    divisor: 9, offset: 0),
 
                new ConvolutionFilter("Gaussian Blur",
                    kernel: new int[,]
                    {
                        { 0, 1, 0 },
                        { 1, 4, 1 },
                        { 0, 1, 0 },
                    },
                    divisor: 8, offset: 0),

                new ConvolutionFilter("Sharpen",
                    kernel: new int[,]
                    {
                        {  0, -1,  0 },
                        { -1,  5, -1 },
                        {  0, -1,  0 },
                    },
                    divisor: 1, offset: 0),

                new ConvolutionFilter("Edge Detection",
                    kernel: new int[,]
                    {
                        {  0, -1,  0 },
                        {  0,  1,  0 },
                        {  0,  0,  0 },
                    },
                    divisor: 1, offset: 128),

                new ConvolutionFilter("Emboss",
                    kernel: new int[,]
                    {
                        { -1,  0,  1 },
                        { -1,  1,  1 },
                        { -1,  0,  1 },
                    },
                    divisor: 1, offset: 0),

                new MedianFilter(),
            };
        }
    }
}