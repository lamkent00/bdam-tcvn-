namespace BDAM.Core.Commands;

public sealed record BdamCommandDefinition(
    string Id,
    string DisplayName,
    string Description,
    bool RequiresCadMutation,
    string PrdReference);
