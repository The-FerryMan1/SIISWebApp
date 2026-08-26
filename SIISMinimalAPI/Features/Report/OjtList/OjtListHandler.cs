using System;
using System.Text;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;
using SIISMinimalAPI.Features.Shared.Utilities;
using System.Globalization;
using CsvHelper.Configuration;
namespace SIISMinimalAPI.Features.Report.OjtList;

public class OjtListHandler(AppDbContext context) : IOjtListService
{
    private readonly AppDbContext _context = context;
    public async Task<byte[]> ListAllOjt(ApplicationStatusEnum? status, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null)
            .AsNoTracking()
            .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        if (status is { } selectedStatus)
        {
            query = query.Where(t => t.Application.Status == selectedStatus);
        }

        var ojts = await query.ToListAsync(ct);
        var statusLabel = status?.ToString() ?? "All";
        QuestPDF.Settings.License = LicenseType.Community; // or Evaluation
        var document = Document.Create(doc =>
{
    doc.Page(page =>
    {
        page.Size(PageSizes.A4.Portrait());
        page.Margin(30);

        // Header with timestamp
        page.Header().Column(col =>
        {
            col.Item().Text($"OJT Students - {statusLabel}")
                .FontSize(16).Bold().AlignCenter();

            col.Item().PaddingTop(4).Text($"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
                .FontSize(9).FontColor(Colors.Grey.Darken1).AlignCenter();
        });

        // Table with borders
        page.Content().PaddingTop(15).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(2.2f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.8f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(1.2f);
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text("#").SemiBold();
                header.Cell().Element(HeaderCell).Text("Student Name").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Status").SemiBold();
                header.Cell().Element(HeaderCell).Text("Office").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Internship hours").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Start Date").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Created").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated Hours").SemiBold();
            });

            // Data rows
            int index = 1;
            foreach (var ojt in ojts)
            {
                var fullname = ojt.FullName;

                table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                table.Cell().Element(DataCell).Text(fullname).FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Application?.Status.ToString() ?? "-").FontSize(9);
                table.Cell().Element(DataCell).Text(ojt.Placement?.Office?.OfficeName ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.TotalInternshipHours.ToString() ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Placement?.StartDate.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.CreatedAt.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Placement?.AccumulatedHours.ToString() ?? "-").FontSize(9);
            }

            static IContainer HeaderCell(IContainer container) => container
                .Padding(0)
                .Border(1)
                .BorderColor(Colors.Black);

            static IContainer DataCell(IContainer container) => container
                .Padding(0)
                .Border(1)
                .BorderColor(Colors.Black);
        });

        // Footer
        page.Footer().AlignRight().PaddingTop(10).Text(text =>
        {
            text.Span("Page ").FontSize(9);
            text.CurrentPageNumber().FontSize(9);
            text.Span(" of ").FontSize(9);
            text.TotalPages().FontSize(9);
        });
    });
});
        return document.GeneratePdf();
    }

   public async Task<byte[]> OjtListCsv(CancellationToken ct)
   {
       var ojts = await _context.Students
           .Include(t => t.Application)
           .Include(t => t.Placement).ThenInclude(p => p.Office)
           .Where(t => t.Placement != null)
           .AsNoTracking()
           .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
           .ToListAsync(ct);

       var records = ojts.Select(t => new OjtListDto
       {
           Name = t.FullName,
           Office = t.Placement?.Office?.OfficeName ?? "N/A",
           StartDate = t.Placement!.StartDate,
           Status = t.Application.Status.ToString(),
           TotalHours = t.TotalInternshipHours,
           AccumulatedHours = t.Placement!.AccumulatedHours
       }).ToList();

    using var memoryStream = new MemoryStream();
    using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
    using (var csv = new CsvWriter(writer, new CsvConfiguration()))
    {
        csv.WriteRecords(records);
    }

    return memoryStream.ToArray();
}

    public async Task<byte[]> ListAllOjtFiltered(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null)
            .AsNoTracking()
            .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        query = query.ApplyFilters(filters);

        var ojts = await query.ToListAsync(ct);
        QuestPDF.Settings.License = LicenseType.Community;
        var statusLabel = filters.Status ?? "All";
        var document = Document.Create(doc =>
{
    doc.Page(page =>
    {
        page.Size(PageSizes.A4.Portrait());
        page.Margin(30);

        page.Header().Column(col =>
        {
            col.Item().Text($"OJT Students - {statusLabel} (Filtered)")
                .FontSize(16).Bold().AlignCenter();

            col.Item().PaddingTop(4).Text($"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
                .FontSize(9).FontColor(Colors.Grey.Darken1).AlignCenter();
        });

        page.Content().PaddingTop(15).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(2.2f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.8f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(1.2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text("#").SemiBold();
                header.Cell().Element(HeaderCell).Text("Student Name").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Status").SemiBold();
                header.Cell().Element(HeaderCell).Text("Office").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Internship hours").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Start Date").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Created").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated Hours").SemiBold();
            });

            int index = 1;
            foreach (var ojt in ojts)
            {
                var fullname = ojt.FullName;

                table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                table.Cell().Element(DataCell).Text(fullname).FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Application?.Status.ToString() ?? "-").FontSize(9);
                table.Cell().Element(DataCell).Text(ojt.Placement?.Office?.OfficeName ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.TotalInternshipHours.ToString() ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Placement?.StartDate.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.CreatedAt.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Placement?.AccumulatedHours.ToString() ?? "-").FontSize(9);
            }

            static IContainer HeaderCell(IContainer container) => container
                .Padding(0)
                .Border(1)
                .BorderColor(Colors.Black);

            static IContainer DataCell(IContainer container) => container
                .Padding(0)
                .Border(1)
                .BorderColor(Colors.Black);
        });

        page.Footer().AlignRight().PaddingTop(10).Text(text =>
        {
            text.Span("Page ").FontSize(9);
            text.CurrentPageNumber().FontSize(9);
            text.Span(" of ").FontSize(9);
            text.TotalPages().FontSize(9);
        });
    });
});
    return document.GeneratePdf();
}

public async Task<byte[]> OjtListCsvFiltered(CommonFilterOptions filters, CancellationToken ct)
{
    IQueryable<Student> query = _context.Students
        .Include(t => t.Application)
        .Include(t => t.Placement).ThenInclude(p => p.Office)
        .AsNoTracking()
        .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

    query = query.ApplyFilters(filters);

    var ojts = await query.ToListAsync(ct);

    var records = ojts.Select(t => new OjtListDto
    {
        Name = t.FullName,
        Office = t.Placement?.Office?.OfficeName ?? "N/A",
        StartDate = t.Placement!.StartDate,
        Status = t.Application.Status.ToString(),
        TotalHours = t.TotalInternshipHours,
        AccumulatedHours = t.Placement!.AccumulatedHours
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
