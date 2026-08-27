namespace SIISMinimalAPI.Features.Progress.GetProgress;

public record ProgressDto(
    Guid StudentUuid,
    string StudentName,
    string Office,
    int TotalHours,
    int AccumulatedHours,
    int RemainingHours,
    int TrainingHoursRendered,
    int TrainingHoursForWeek,
    double ProgressPercent,
    string PlacementStatus
);
