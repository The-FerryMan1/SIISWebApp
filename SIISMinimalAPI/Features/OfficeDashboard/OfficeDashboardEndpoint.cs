using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.OfficeDashboard;

namespace SIISMinimalAPI.Features.OfficeDashboard;

public static class OfficeDashboardEndpoint
{
    public static IEndpointRouteBuilder MapToOfficeDashboard(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/office-dashboard")
            .WithTags("OfficeDashboard")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/{officeId:long}", async Task<IResult>(
            long officeId,
            CancellationToken ct,
            IOfficeDashboardService service) =>
        {
            try
            {
                var result = await service.GetDashboardAsync(officeId, ct);
                return TypedResults.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        });

        group.MapPut("/placement/{studentUuid:guid}", async Task<IResult>(
            Guid studentUuid,
            UpdateInternshipDatesDto dto,
            CancellationToken ct,
            AppDbContext context) =>
        {
            try
            {
                var student = await context.Students
                    .Include(t => t.Placement)
                    .FirstOrDefaultAsync(t => t.StudentUUID == studentUuid && !t.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Student not found");

                if (student.Placement == null)
                    throw new KeyNotFoundException("Placement record not found");

                student.Placement.StartDate = dto.StartDate;
                student.Placement.EstimatedEndDate = dto.EstimatedEndDate;
                student.Placement.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync(ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        });

        return app;
    }
}
