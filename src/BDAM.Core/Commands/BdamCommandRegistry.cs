namespace BDAM.Core.Commands;

public sealed class BdamCommandRegistry
{
    private static readonly BdamCommandDefinition[] DefaultDefinitions =
    [
        new(
            BdamCommandIds.DrawBeam,
            "Vẽ dầm BTCT theo TCVN",
            "Mở workflow nhập liệu và vẽ hình học dầm, thép, đai, leader và metadata.",
            RequiresCadMutation: true,
            PrdReference: "PRD #1/#2/#3/#4/#5"),
        new(
            BdamCommandIds.AnnotateSupplementaryRebar,
            "Gán thông tin thép bổ sung",
            "Chọn entity thép vẽ bổ sung, nhập Mark/Diam/Qty/ShapeString và gắn XData/leader.",
            RequiresCadMutation: true,
            PrdReference: "PRD #3/#5"),
        new(
            BdamCommandIds.CreateCadStatisticsTable,
            "Tạo bảng thống kê CAD",
            "Đọc XData ST_BIM_REBAR, gom nhóm và sinh CAD Table thống kê.",
            RequiresCadMutation: true,
            PrdReference: "PRD #6"),
        new(
            BdamCommandIds.ExportToExcel,
            "Xuất Excel TKCT",
            "Xuất dataset thống kê sang Excel sheet TKCT bằng COM Interop/Value2.",
            RequiresCadMutation: false,
            PrdReference: "PRD #7"),
    ];

    private readonly Dictionary<string, BdamCommandDefinition> definitions;

    public BdamCommandRegistry(IEnumerable<BdamCommandDefinition>? definitions = null)
    {
        var source = definitions?.ToArray() ?? DefaultDefinitions;
        this.definitions = source.ToDictionary(definition => definition.Id, StringComparer.OrdinalIgnoreCase);
        EnsureRequiredCommandsRegistered(this.definitions);
    }

    public IReadOnlyCollection<BdamCommandDefinition> All => definitions.Values.OrderBy(definition => definition.Id).ToArray();

    public BdamCommandDefinition GetRequired(string commandId)
    {
        if (!definitions.TryGetValue(commandId, out var definition))
        {
            throw new KeyNotFoundException($"Command '{commandId}' is not registered.");
        }

        return definition;
    }

    public bool TryGet(string commandId, out BdamCommandDefinition definition)
    {
        return definitions.TryGetValue(commandId, out definition!);
    }

    private static void EnsureRequiredCommandsRegistered(IReadOnlyDictionary<string, BdamCommandDefinition> definitions)
    {
        var missing = BdamCommandIds.All.Where(commandId => !definitions.ContainsKey(commandId)).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Missing required BDAM commands: {string.Join(", ", missing)}.");
        }
    }
}
