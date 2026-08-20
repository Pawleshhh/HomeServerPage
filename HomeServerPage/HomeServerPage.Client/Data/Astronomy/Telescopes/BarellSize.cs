namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public class BarellSizes
{

    public double Value { get; set; }

    private BarellSizes(double v)
    {
        Value = v;
    }

    public static BarellSizes Size125Inches => new(1.25);
    public static BarellSizes Size2Inches => new(2.0);

    public static implicit operator double(BarellSizes size) => size.Value;

    public static explicit operator BarellSizes(double value) => new(value);

}
