using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIISMinimalAPI.Features.StudentImport;

public static class StudentImportEndpoint
{
    public static IEndpointRouteBuilder MapStudentImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/student-import")
            .WithTags("StudentImport")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapPost("/", [Authorize(Roles = "Admin")] async Task<IResult>([FromForm] IFormFile file, IStudentImportService service, CancellationToken ct) =>
        {
            if (file is null)
            {
                return Results.BadRequest("File is required.");
            }

            try
            {
                var result = await service.ImportAsync(file, ct);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Import failed: {ex.Message}");
            }
        })
        .Accepts<IFormFile>("multipart/form-data")
        .WithName("ImportStudents")
        .Produces<StudentImportResultDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();

        return app;
    }
}
