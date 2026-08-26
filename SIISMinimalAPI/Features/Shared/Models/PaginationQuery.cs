namespace SIISMinimalAPI.Features.Shared.Models;

public record PaginationQuery(int Page = 1, int PageSize = 10, string? Search = null)
{
    private const int MaxPageSize = 100;

    public int ValidatedPageSize => PageSize <= 0 ? 10 : PageSize > MaxPageSize ? MaxPageSize : PageSize;
    public int ValidatedPage => Page <= 0 ? 1 : Page;
}
