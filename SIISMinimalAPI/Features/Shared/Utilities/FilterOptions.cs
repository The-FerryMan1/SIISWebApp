namespace SIISMinimalAPI.Features.Shared.Utilities;

public record CommonFilterOptions(
    string? Name = null,
    string? School = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? Office = null,
    string? Status = null,
    string? PlacementStatus = null
);
