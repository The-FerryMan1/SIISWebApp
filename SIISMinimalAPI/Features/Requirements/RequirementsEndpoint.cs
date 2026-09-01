using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Requirements;

public static class RequirementsEndpoint
{
    public static IEndpointRouteBuilder MapToRequirements(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/requirements")
            .WithTags("Requirements")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapGet("/", [Authorize] async Task<IResult>(CancellationToken ct, AppDbContext context) =>
        {
            var requirements = await context.Requirements
                .Include(r => r.Student)
                    .ThenInclude(s => s!.Application)
                .Include(r => r.Student)
                    .ThenInclude(s => s!.Placement)
                        .ThenInclude(p => p!.Office)
                .AsNoTracking()
                .AsSplitQuery()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);

            var data = requirements.Select(r => new RequirementDto
            {
                Id = r.Id,
                FileName = r.FileName,
                FileType = r.FileType,
                FilePath = r.FilePath,
                RequirementType = r.RequirementType,
                CreatedAt = r.CreatedAt,
                StudentId = r.StudentId,
                StudentName = r.Student != null ? $"{r.Student.FirstName} {r.Student.LastName}" : "Unknown",
                StudentEmail = r.Student?.Email ?? "N/A",
                OfficeName = r.Student?.Placement?.Office?.OfficeName ?? "N/A",
                Status = r.Student?.Application?.Status.ToString() ?? "N/A"
            }).ToList();

            return TypedResults.Ok(data);
        });

        return app;
    }
}

public class RequirementDto
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public RequirementTypeEnum RequirementType { get; set; }
    public DateTime CreatedAt { get; set; }
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string OfficeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
