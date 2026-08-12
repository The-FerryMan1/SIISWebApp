using System;
using System.Text;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;
using System.Globalization;

namespace SIISMinimalAPI.Features.Report.ImportAudit;

public class ImportAuditHandler(AppDbContext context) : IImportAuditService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GenerateCsv(CancellationToken ct)
    {
        var logs = await _context.Logs
            .Where(l => !l.IsDeleted && (l.Entity == "Student" || l.Action!.Contains("Import", StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(l => l.CreateAt)
            .AsNoTracking()
            .ToListAsync(ct);

        var records = logs.Select(l => new ImportAuditDto
        {
            Action = l.Action,
            Entity = l.Entity,
            EntityId = l.EntityId,
            UserId = l.UserId,
            Details = l.Details,
            Timestamp = l.CreateAt
        }).ToList();

        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration()))
        {
            csv.WriteRecords(records);
        }

        return memoryStream.ToArray();
    }
}
