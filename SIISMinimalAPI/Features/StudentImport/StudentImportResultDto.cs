namespace SIISMinimalAPI.Features.StudentImport;

public class StudentImportResultDto
{
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<StudentImportErrorDto> Errors { get; set; } = new();
}

public class StudentImportErrorDto
{
    public int RowNumber { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = string.Empty;
}
