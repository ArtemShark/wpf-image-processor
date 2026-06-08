namespace Task1.Filters
{
    public interface IFilter
    {
        string Name { get; }
        PixelData Apply(PixelData source);
    }
}