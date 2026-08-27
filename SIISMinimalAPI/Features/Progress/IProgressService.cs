using SIISMinimalAPI.Features.Progress.GetProgress;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Progress;

public interface IProgressService
{
    Task<ProgressDto> GetProgressByStudentUuid(Guid studentUuid, CancellationToken ct);
}
