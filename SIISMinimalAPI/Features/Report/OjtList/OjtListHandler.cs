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

         // Header
         page.Header().PaddingBottom(15).Column(col =>
         {
             col.Item().Text($"OJT Students - {selectedStatus}")
                 .FontSize(20).Bold().AlignCenter();

             col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                 .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
         });

         // Content
         page.Content().Table(table =>
         {
             // Column definitions — 7 columns to match 7 headers
             table.ColumnsDefinition(columns =>
             {
                 columns.ConstantColumn(35);     // No
                 columns.RelativeColumn(2.5f);   // Student Name
                 columns.RelativeColumn(1.4f);   // Status
                 columns.RelativeColumn(2f);     // Office
                 columns.RelativeColumn(1.1f);   // Total Hrs
                 columns.RelativeColumn(1.3f);   // Start Date
                 columns.RelativeColumn(1.3f);   // Created At
             });

             // Header row
             table.Header(header =>
             {
                 header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                 header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                 header.Cell().Element(HeaderCell).AlignCenter().Text("Status").Bold();
                 header.Cell().Element(HeaderCell).Text("Office").Bold();
                 header.Cell().Element(HeaderCell).AlignCenter().Text("Total Hrs").Bold();
                 header.Cell().Element(HeaderCell).AlignCenter().Text("Start Date").Bold();
                 header.Cell().Element(HeaderCell).AlignCenter().Text("Created At").Bold();

                 static IContainer HeaderCell(IContainer container) => container
                     .DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.White))
                     .PaddingVertical(8)
                     .PaddingHorizontal(5)
                     .Background(Colors.Blue.Medium)
                     .BorderBottom(2)
                     .BorderColor(Colors.Blue.Darken2);
             });

             // Data rows
             int index = 1;
             foreach (var ojt in ojts)
             {
                 var isEven = index % 2 == 0;
                 var fullname = $"{ojt.LastName}, {ojt.FirstName} {ojt.MiddleName}".Trim();
                 var status = ojt.Application?.Status;

                 table.Cell().Element(c => DataCell(c, isEven)).AlignCenter()
                     .Text(index++.ToString()).FontSize(9);

                 table.Cell().Element(c => DataCell(c, isEven))
                     .Text(fullname).FontSize(9);

                 table.Cell().Element(c => DataCell(c, isEven)).AlignCenter()
                     .Text(status?.ToString() ?? "N/A")
                     .FontSize(9)
                     .FontColor(GetStatusColor(status));

                 table.Cell().Element(c => DataCell(c, isEven))
                     .Text(ojt.Office != null ? OfficeEnumLabels.GetLabel(ojt.Office.Name) : "N/A")
                     .FontSize(9);

                 table.Cell().Element(c => DataCell(c, isEven)).AlignCenter()
                     .Text(ojt.Internship?.InternshipTotalHours.ToString() ?? "-").FontSize(9);

                 table.Cell().Element(c => DataCell(c, isEven)).AlignCenter()
                     .Text(ojt.Internship?.StartDate.ToString("MM/dd/yyyy") ?? "-").FontSize(9);

                 table.Cell().Element(c => DataCell(c, isEven)).AlignCenter()
                     .Text(ojt.CreateAt.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
             }

             static IContainer DataCell(IContainer container, bool isEven) => container
                 .PaddingVertical(6)
                 .PaddingHorizontal(5)
                 .Background(isEven ? Colors.Grey.Lighten4 : Colors.White)
                 .BorderBottom(1)
                 .BorderColor(Colors.Grey.Lighten2);

             static string GetStatusColor(ApplicationStatusEnum? status) => status switch
             {
                 ApplicationStatusEnum.Approved => Colors.Green.Darken2,
                 ApplicationStatusEnum.Pending => Colors.Orange.Darken2,
                 ApplicationStatusEnum.Rejected => Colors.Red.Darken2,
                 _ => Colors.Black
             };
         });

         // Footer
         page.Footer().AlignCenter().PaddingTop(10).Text(text =>
         {
             text.Span("Page ").FontSize(9).FontColor(Colors.Grey.Darken1);
             text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
             text.Span(" of ").FontSize(9).FontColor(Colors.Grey.Darken1);
             text.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
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


