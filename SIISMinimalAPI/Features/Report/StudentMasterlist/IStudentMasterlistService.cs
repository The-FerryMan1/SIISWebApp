using System;
using System.Threading;
using System.Threading.Tasks;

using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.StudentMasterlist;

public interface IStudentMasterlistService
{
    Task<byte[]> GeneratePdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateCsv(CommonFilterOptions filters, CancellationToken ct);
}
