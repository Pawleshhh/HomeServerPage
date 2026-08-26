namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public enum BarellSize
{
    Size125Inches,
    Size2Inches 
}

public static class BarellSizesExtensions
{
    extension(BarellSize barellSize)
    {
        public double Size => barellSize switch
        {
            BarellSize.Size125Inches => 1.25,
            BarellSize.Size2Inches => 2,
            _ => double.NaN
        };
    }
}