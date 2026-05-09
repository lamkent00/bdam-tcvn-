namespace BDAM.Core.Statistics;

public sealed record StatisticsRow(
    string BeamId,
    string Mark,
    int Diam,
    int Qty,
    double LengthMM,
    string ShapeString,
    string NumCK,
    string BarType);
