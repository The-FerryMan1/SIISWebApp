using System;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.OfficesSummary;

public class OfficesSummaryHandler(AppDbContext context) : IOfficesSummaryService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GetOfficesSummary(CancellationToken ct)
    {
        var offices = await _context.Offices
            .Include(o => o.Students)
            .Where(o => !o.IsDeleted)
            .AsNoTracking()
            .ToListAsync(ct);

        var totalStudents = offices.Sum(o => o.Students.Count(s => !s.IsDeleted));

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text("Office Statistics Summary")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Total OJTs: {totalStudents} | Total Offices: {offices.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(3f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("#").Bold();
                        header.Cell().Element(HeaderCell).Text("Office").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("OIC").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Students").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Share %").Bold();

                        static IContainer HeaderCell(IContainer container) => container
                            .DefaultTextStyle(x => x.FontSize(10))
                            .Padding(0)
                            .Border(1)
                            .BorderColor(Colors.Black);
                    });

                    int index = 1;
                    foreach (var office in offices.OrderByDescending(o => o.Students.Count(s => !s.IsDeleted)))
                    {
                        var studentCount = office.Students.Count(s => !s.IsDeleted);
                        var share = totalStudents > 0 ? Math.Round((double)studentCount / totalStudents * 100, 1) : 0;

                        table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).Text(OfficeEnumLabels.GetLabel(office.Name)).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(office.CurrentOIC ?? "-").FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(studentCount.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text($"{share}%").FontSize(9);

                        static IContainer DataCell(IContainer container) => container
                            .Padding(0)
                            .Border(1)
                            .BorderColor(Colors.Black);
                    }
                });

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
}
