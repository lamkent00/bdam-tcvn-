namespace BDAM.Core.XData;

public sealed record RebarXDataRecord(
    string BeamId,
    string Mark,
    int Diam,
    int Qty,
    double LengthMM,
    string ShapeString,
    string NumCK,
    string BarType,
    string SourceCommand,
    int SchemaVersion)
{
    public const string AppId = "ST_BIM_REBAR";
    public const int CurrentSchemaVersion = 1;
}
