namespace BDAM.Core.Models;

public sealed record BeamGeometry(
    string BeamId,
    double StartX,
    double StartY,
    double EndX,
    double EndY,
    double WidthMm,
    double HeightMm);
