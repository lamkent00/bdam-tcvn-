namespace BDAM.Core.Configuration;

public sealed record BdamPluginConfig
{
    public const double DefaultCoverMm = 25;
    public const double DefaultAbvMm = 30;
    public const double DefaultHookLengthMm = 120;
    public const int DefaultSupportZoneDivisor = 4;
    public const double DefaultStirrupEdgeDistanceMm = 50;
    public const double DefaultMaxStockLengthMm = 11700;
    public const double DefaultLapSpliceDiameterMultiplier = 40;

    public double CoverMm { get; init; } = DefaultCoverMm;

    public double AbvMm { get; init; } = DefaultAbvMm;

    public double HookLengthMm { get; init; } = DefaultHookLengthMm;

    public int SupportZoneDivisor { get; init; } = DefaultSupportZoneDivisor;

    public double StirrupEdgeDistanceMm { get; init; } = DefaultStirrupEdgeDistanceMm;

    public double MaxStockLengthMm { get; init; } = DefaultMaxStockLengthMm;

    public double LapSpliceDiameterMultiplier { get; init; } = DefaultLapSpliceDiameterMultiplier;

    public static BdamPluginConfig CreateDefault()
    {
        return new BdamPluginConfig();
    }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        AddPositiveError(errors, CoverMm, nameof(CoverMm));
        AddPositiveError(errors, AbvMm, nameof(AbvMm));
        AddPositiveError(errors, HookLengthMm, nameof(HookLengthMm));
        AddPositiveError(errors, StirrupEdgeDistanceMm, nameof(StirrupEdgeDistanceMm));
        AddPositiveError(errors, MaxStockLengthMm, nameof(MaxStockLengthMm));
        AddPositiveError(errors, LapSpliceDiameterMultiplier, nameof(LapSpliceDiameterMultiplier));

        if (SupportZoneDivisor <= 0)
        {
            errors.Add($"{nameof(SupportZoneDivisor)} must be greater than zero.");
        }

        return errors;
    }

    private static void AddPositiveError(ICollection<string> errors, double value, string name)
    {
        if (value <= 0)
        {
            errors.Add($"{name} must be greater than zero.");
        }
    }
}
