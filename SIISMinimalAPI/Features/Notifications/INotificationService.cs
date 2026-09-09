namespace SIISMinimalAPI.Features.Notifications;

public interface INotificationService
{
    Task NotifyOfficeAsync(
        long officeId,
        string type,
        string title,
        string description,
        string actionUrl,
        string priority,
        CancellationToken ct = default);

    Task NotifyOfficeForStudentAsync(
        long studentId,
        string type,
        string title,
        string description,
        string actionUrl,
        string priority,
        CancellationToken ct = default);
}