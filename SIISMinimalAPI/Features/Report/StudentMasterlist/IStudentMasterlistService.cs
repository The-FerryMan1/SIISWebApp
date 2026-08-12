using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.StudentMasterlist;

public interface IStudentMasterlistService
{
    Task<byte[]> GeneratePdf(string officeName, CancellationToken ct);
    Task<byte[]> GenerateCsv(string officeName, CancellationToken ct);
}
