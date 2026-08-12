using Microsoft.AspNetCore.Http;

namespace SIISMinimalAPI.Features.StudentImport;

public interface IStudentImportService
{
    Task<StudentImportResultDto> ImportAsync(IFormFile file, string userId, CancellationToken ct);
}
