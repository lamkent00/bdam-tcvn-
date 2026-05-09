namespace BDAM.Core.Models;

public sealed record BeamInput(
    string BeamId,
    double WidthMm,
    double HeightMm,
    double ClearSpanMm,
    double CoverMm,
    double Scale);
