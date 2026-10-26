namespace WindTriangles.Navigation;

public sealed class NavigationInput
{
    /// <summary>Desired course over ground, degrees true.</summary>
    public double DesiredCourse { get; set; } = 45;

    /// <summary>Boat speed through the water, knots.</summary>
    public double BoatSpeed { get; set; } = 6;

    /// <summary>Distance to go, nautical miles.</summary>
    public double Distance { get; set; } = 12;

    /// <summary>Direction the current flows towards, degrees true.</summary>
    public double CurrentSet { get; set; } = 135;

    /// <summary>Current speed, knots.</summary>
    public double CurrentDrift { get; set; } = 1.5;

    /// <summary>Direction the true wind blows from, degrees true.</summary>
    public double WindDirection { get; set; } = 340;

    /// <summary>True wind speed, knots.</summary>
    public double WindSpeed { get; set; } = 12;

    /// <summary>Leeway angle in degrees; the boat is pushed to the lee side by this much.</summary>
    public double Leeway { get; set; } = 5;

    public double Variation { get; set; } = 4;
    public bool VariationEast { get; set; } = true;

    public double Deviation { get; set; } = 2;
    public bool DeviationEast { get; set; } = false;

    /// <summary>Variation with east positive.</summary>
    public double SignedVariation => VariationEast ? Variation : -Variation;

    /// <summary>Deviation with east positive.</summary>
    public double SignedDeviation => DeviationEast ? Deviation : -Deviation;
}
