namespace WindTriangles.Navigation;

/// <summary>
/// A 2D vector in navigation coordinates: East and North components.
/// Bearings are measured in degrees clockwise from true north.
/// </summary>
public readonly record struct Vec(double East, double North)
{
    public static Vec FromPolar(double bearingDegrees, double length)
    {
        var rad = Angles.ToRadians(bearingDegrees);
        return new Vec(length * Math.Sin(rad), length * Math.Cos(rad));
    }

    public double Length => Math.Sqrt(East * East + North * North);

    public double Bearing => Angles.Normalize(Angles.ToDegrees(Math.Atan2(East, North)));

    public static Vec operator +(Vec a, Vec b) => new(a.East + b.East, a.North + b.North);
    public static Vec operator -(Vec a, Vec b) => new(a.East - b.East, a.North - b.North);
    public static Vec operator -(Vec a) => new(-a.East, -a.North);
    public static Vec operator *(Vec a, double k) => new(a.East * k, a.North * k);
}

public static class Angles
{
    public static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    public static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    /// <summary>Normalizes to the range [0, 360).</summary>
    public static double Normalize(double degrees)
    {
        var d = degrees % 360.0;
        return d < 0 ? d + 360.0 : d;
    }

    /// <summary>Normalizes to the range (-180, 180].</summary>
    public static double Signed(double degrees)
    {
        var d = Normalize(degrees);
        return d > 180.0 ? d - 360.0 : d;
    }
}
