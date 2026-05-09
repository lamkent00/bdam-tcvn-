namespace BDAM.Core.Commands;

public static class BdamCommandIds
{
    public const string DrawBeam = "BDAM_TCVN";
    public const string AnnotateSupplementaryRebar = "GT";
    public const string CreateCadStatisticsTable = "TKTD";
    public const string ExportToExcel = "XTE";

    public static readonly IReadOnlyList<string> All =
    [
        DrawBeam,
        AnnotateSupplementaryRebar,
        CreateCadStatisticsTable,
        ExportToExcel,
    ];
}
