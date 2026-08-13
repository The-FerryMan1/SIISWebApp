using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Logs;

namespace SIISMinimalAPI.Features.PlacementTransfer;

public static class PlacementTransferEndpoint
{
    public static IEndpointRouteBuilder MapToPlacementTransfer(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/placement")
            .WithTags("PlacementTransfer")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapPut("/{studentUuid:guid}", async Task<IResult>(
            Guid studentUuid,
            UpdatePlacementDto dto,
            CancellationToken ct,
            AppDbContext context,
            ILogService logService) =>
        {
            try
            {
                var student = await context.Students
                    .Include(t => t.Placement)
                    .FirstOrDefaultAsync(t => t.StudentUUID == studentUuid && !t.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Student not found");

                if (student.Placement == null)
                    throw new KeyNotFoundException("Placement record not found");

                var oldOfficeId = student.Placement.OfficeId;
                var newOffice = await context.Offices
                    .FirstOrDefaultAsync(o => o.OfficeName == dto.Office && !o.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Office not found");

                student.Placement.OfficeId = newOffice.Id;
                student.Placement.StartDate = dto.StartDate;
                student.Placement.EstimatedEndDate = dto.EstimatedEndDate;
                student.Placement.AccumulatedHours = dto.AccumulatedHours;
                student.Placement.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync(ct);

                var changes = new List<string>();
                if (oldOfficeId != newOffice.Id)
                {
                    changes.Add($"office changed to {newOffice.OfficeName}");
                }
                if (student.Placement.StartDate != dto.StartDate)
                {
                    changes.Add($"start date updated");
                }
                if (student.Placement.EstimatedEndDate != dto.EstimatedEndDate)
                {
                    changes.Add($"end date updated");
                }
                if (student.Placement.AccumulatedHours != dto.AccumulatedHours)
                {
                    changes.Add($"accumulated hours updated to {dto.AccumulatedHours}");
                }

                await logService.WriteAsync(
                    "Transfer",
                    "Placement",
                    student.Placement.Id,
                    "admin",
                    $"Transferred {student.FullName} to {newOffice.OfficeName}. Changes: {string.Join(", ", changes)}"
                );

                return TypedResults.Ok(new
                {
                    message = "Placement updated successfully",
                    studentName = student.FullName,
                    office = newOffice.OfficeName
                });
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}

public class UpdatePlacementDto
{
    public string Office { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EstimatedEndDate { get; set; }
    public int AccumulatedHours { get; set; }
}
