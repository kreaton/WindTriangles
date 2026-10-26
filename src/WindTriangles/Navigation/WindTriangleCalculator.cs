namespace WindTriangles.Navigation;

public sealed record NavigationResult
{
    // Course chain: true course -> course through water -> true heading -> magnetic -> compass.
    public required double DesiredCourse { get; init; }
    public required double CurrentCorrection { get; init; }
    public required double CourseThroughWater { get; init; }
    public required double LeewayCorrection { get; init; }
    public required double TrueHeading { get; init; }
    public required double MagneticHeading { get; init; }
    public required double CompassCourse { get; init; }

    public required double SpeedOverGround { get; init; }
    public required TimeSpan TimeToGo { get; init; }

    public required double TrueWindAngle { get; init; }
    public required double ApparentWindDirection { get; init; }
    public required double ApparentWindSpeed { get; init; }
    public required double ApparentWindAngle { get; init; }

    public required double Variation { get; init; }
    public required double Deviation { get; init; }

    // Vectors for plotting (knots).
    public required Vec Water { get; init; }
    public required Vec Current { get; init; }
    public required Vec Ground { get; init; }
    public required Vec TrueWind { get; init; }
    public required Vec ApparentWind { get; init; }

    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record CalculationOutcome(NavigationResult? Result, string? Error);

public static class WindTriangleCalculator
{
    /// <summary>Wind closer than this to the bow cannot be sailed directly.</summary>
    public const double NoGoAngle = 40;

    /// <summary>Wind further aft than this pushes the boat straight ahead, so no leeway.</summary>
    private const double NoLeewayAngle = 165;

    public static CalculationOutcome Calculate(NavigationInput input)
    {
        if (input.BoatSpeed <= 0)
            return new(null, "Boat speed through the water must be greater than 0.");
        if (input.CurrentDrift < 0 || input.WindSpeed < 0 || input.Distance < 0)
            return new(null, "Speeds and distance cannot be negative.");
        if (input.Leeway is < 0 or > 45)
            return new(null, "Leeway must be between 0° and 45°.");

        var course = Angles.Normalize(input.DesiredCourse);
        var current = Vec.FromPolar(input.CurrentSet, input.CurrentDrift);

        // Current triangle: Water + Current must lie along the desired course.
        // The cross-track part of the water vector has to cancel the cross-track part of the current.
        var currentRelative = Angles.ToRadians(input.CurrentSet - course);
        var crossTrack = input.CurrentDrift * Math.Sin(currentRelative);
        var ratio = -crossTrack / input.BoatSpeed;
        if (Math.Abs(ratio) > 1)
            return new(null, "The current is too strong across the course: the boat cannot hold this course at this speed.");

        var currentCorrection = Angles.ToDegrees(Math.Asin(ratio));
        var courseThroughWater = Angles.Normalize(course + currentCorrection);
        var speedOverGround = input.BoatSpeed * Math.Cos(Angles.ToRadians(currentCorrection))
                              + input.CurrentDrift * Math.Cos(currentRelative);
        if (speedOverGround <= 0.01)
            return new(null, "The current is stronger than the boat along the course: no progress over ground.");

        var water = Vec.FromPolar(courseThroughWater, input.BoatSpeed);
        var ground = water + current;

        // Leeway: the wind pushes the boat to leeward, so steer the leeway angle up into the wind.
        var windOnWaterCourse = Angles.Signed(input.WindDirection - courseThroughWater);
        var leewayCorrection = 0.0;
        if (input.WindSpeed > 0 && input.Leeway > 0 && Math.Abs(windOnWaterCourse) < NoLeewayAngle && windOnWaterCourse != 0)
            leewayCorrection = Math.Sign(windOnWaterCourse) * input.Leeway;

        var trueHeading = Angles.Normalize(courseThroughWater + leewayCorrection);
        var magneticHeading = Angles.Normalize(trueHeading - input.SignedVariation);
        var compassCourse = Angles.Normalize(magneticHeading - input.SignedDeviation);

        // Wind triangle: apparent wind = true wind - boat motion over ground.
        var trueWind = Vec.FromPolar(input.WindDirection + 180, input.WindSpeed);
        var apparentWind = trueWind - ground;
        var apparentFrom = Angles.Normalize(apparentWind.Bearing + 180);
        var trueWindAngle = Angles.Signed(input.WindDirection - trueHeading);
        var apparentWindAngle = apparentWind.Length < 1e-9 ? 0 : Angles.Signed(apparentFrom - trueHeading);

        var warnings = new List<string>();
        if (input.WindSpeed > 0 && Math.Abs(trueWindAngle) < NoGoAngle)
            warnings.Add($"The wind is only {Math.Abs(trueWindAngle):0}° off the bow – inside the no-go zone. You will have to tack towards the destination.");
        if (input.CurrentDrift > 0 && Math.Abs(currentCorrection) > 30)
            warnings.Add($"Large current correction ({Math.Abs(currentCorrection):0}°). Double-check the current data.");

        return new(new NavigationResult
        {
            DesiredCourse = course,
            CurrentCorrection = currentCorrection,
            CourseThroughWater = courseThroughWater,
            LeewayCorrection = leewayCorrection,
            TrueHeading = trueHeading,
            MagneticHeading = magneticHeading,
            CompassCourse = compassCourse,
            SpeedOverGround = speedOverGround,
            TimeToGo = TimeSpan.FromHours(input.Distance / speedOverGround),
            TrueWindAngle = trueWindAngle,
            ApparentWindDirection = apparentFrom,
            ApparentWindSpeed = apparentWind.Length,
            ApparentWindAngle = apparentWindAngle,
            Variation = input.SignedVariation,
            Deviation = input.SignedDeviation,
            Water = water,
            Current = current,
            Ground = ground,
            TrueWind = trueWind,
            ApparentWind = apparentWind,
            Warnings = warnings,
        }, null);
    }
}
