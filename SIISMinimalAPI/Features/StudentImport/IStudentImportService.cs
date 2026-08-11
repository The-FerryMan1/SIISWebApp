using Microsoft.AspNetCore.Http;

namespace SIISMinimalAPI.Features.StudentImport;

public interface IStudentImportService
{
    Task<StudentImportResultDto> ImportAsync(IFormFile file, CancellationToken ct);
}
