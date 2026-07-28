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
using System.Globalization;
using CsvHelper.Configuration;
namespace SIISMinimalAPI.Features.Report.OjtList;

public class OjtListHandler(AppDbContext context) : IOjtListService
{
    private readonly AppDbContext _context = context;
    public async Task<byte[]> ListAllOjt(ApplicationStatusEnum status, CancellationToken ct)
    {
        var selectedStatus = status switch
        {
            ApplicationStatusEnum.Approved => ApplicationStatusEnum.Approved,
            ApplicationStatusEnum.Rejected => ApplicationStatusEnum.Rejected,
            ApplicationStatusEnum.Pending => ApplicationStatusEnum.Pending,
            _ => throw new ArgumentOutOfRangeException(nameof(status), "Unknown order status."),
        };


        var ojts = await _context.Students.Include(t => t.Application).Include(t => t.Office).Include(t => t.Internship).Where(t => t.Application.Status == selectedStatus).AsNoTracking().AsSplitQuery().ToListAsync();
        QuestPDF.Settings.License = LicenseType.Community; // or Evaluation
        var document = Document.Create(doc =>
{
    doc.Page(page =>
    {
        page.Size(PageSizes.A4.Landscape());
        page.Margin(30);

        // Header with timestamp
        page.Header().Column(col =>
        {
            col.Item().Text($"OJT Students - {selectedStatus}")
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
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text("#").SemiBold();
                header.Cell().Element(HeaderCell).Text("Student Name").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Status").SemiBold();
                header.Cell().Element(HeaderCell).Text("Office").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Hours").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Start Date").SemiBold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Created").SemiBold();
            });

            // Data rows
            int index = 1;
            foreach (var ojt in ojts)
            {
                var fullname = $"{ojt.LastName}, {ojt.FirstName} {ojt.MiddleName}".Trim();

                table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                table.Cell().Element(DataCell).Text(fullname).FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Application?.Status.ToString() ?? "-").FontSize(9);
                table.Cell().Element(DataCell).Text(ojt.Office != null ? OfficeEnumLabels.GetLabel(ojt.Office.Name) : "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Internship?.InternshipTotalHours.ToString() ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.Internship?.StartDate.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(ojt.CreateAt.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
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
        .Include(t => t.Office)
        .Include(t => t.Internship)
        .OrderBy(t => t.Application.Status == ApplicationStatusEnum.Approved)
        .AsNoTracking()
        .AsSplitQuery()
        .ToListAsync(ct);

    var records = ojts.Select(t => new OjtListDto
    {
        Name = $"{t.LastName}, {t.FirstName} {t.MiddleName}",
        Office = t.Office != null ? OfficeEnumLabels.GetLabel(t.Office.Name) : "N/A",
        StartDate = t.Internship?.StartDate,
        Status = t.Application.Status.ToString(),
        TotalHours = (int)(t.Internship?.InternshipTotalHours)
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


