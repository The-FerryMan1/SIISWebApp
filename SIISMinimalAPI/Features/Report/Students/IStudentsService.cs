using System;

namespace SIISMinimalAPI.Features.Report.Students;

public interface IStudentsService
{
    Task<byte[]> GetStudentsPdf(CancellationToken ct);
    Task<byte[]> GetStudentsCsv(CancellationToken ct);
}
