using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Progress;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Progress;

public static class ProgressEndpoint
{
    public static IEndpointRouteBuilder MapToProgress(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/progress")
            .WithTags("Progress")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/{studentUuid:guid}", [Authorize] async Task<IResult>(
            Guid studentUuid,
            CancellationToken ct,
            IProgressService service) =>
        {
            try
            {
                var result = await service.GetProgressByStudentUuid(studentUuid, ct);
                return TypedResults.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        group.MapGet("/student/{studentUuid:guid}", [Authorize] async Task<IResult>(
            Guid studentUuid,
            CancellationToken ct,
            AppDbContext db) =>
        {
            var student = await db.Students
                .Where(s => s.StudentUUID == studentUuid && !s.IsDeleted)
                .Select(s => new
                {
                    s.StudentUUID,
                    s.FullName,
                    s.TotalInternshipHours,
                    s.Email
                })
                .FirstOrDefaultAsync(ct);

            if (student == null)
                return TypedResults.NotFound("Student not found");

            return TypedResults.Ok(student);
        }).RequireAuthorization();

        return app;
    }
}
