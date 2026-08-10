using System;
using System.Linq;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Report.AdminReport;

public class AdminReportHandler(AppDbContext context) : IAdminReportService
{
    private readonly AppDbContext _context = context;

    private IQueryable<Student> BaseQuery(long? officeId = null)
    {
        var query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        if (officeId.HasValue)
        {
            query = query.Where(t => t.Placement!.OfficeId == officeId.Value);
        }

        return query;
    }

    public async Task<byte[]> GenerateExpiringInternshipsPdf(long? officeId, int days, CancellationToken ct)
    {
        var threshold = DateOnly.FromDateTime(DateTime.Now.AddDays(days));

        var students = await BaseQuery(officeId)
            .Where(t => t.Placement!.EstimatedEndDate <= threshold)
            .OrderBy(t => t.Placement!.EstimatedEndDate)
            .ToListAsync(ct);

        var officeName = officeId.HasValue
            ? await _context.Offices
                .Where(o => o.Id == officeId.Value && !o.IsDeleted)
                .Select(o => o.OfficeName)
                .FirstOrDefaultAsync(ct)
            : "All Offices";

        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text($"Expiring Internships - {officeName}")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Expiring within {days} days: {students.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                        header.Cell().Element(HeaderCell).Text("Office").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Status").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Start Date").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("End Date").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated").Bold();

                        static IContainer HeaderCell(IContainer container) => container
                            .DefaultTextStyle(x => x.FontSize(10))
                            .Padding(0)
                            .Border(1)
                            .BorderColor(Colors.Black);
                    });

                    int index = 1;
                    foreach (var s in students)
                    {
                        table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).Text(s.FullName).FontSize(9);
                        table.Cell().Element(DataCell).Text(s.Placement!.Office!.OfficeName).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(s.Application?.Status.ToString() ?? "N/A").FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(s.Placement!.StartDate.ToString("MM/dd/yyyy")).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(s.Placement!.EstimatedEndDate.ToString("MM/dd/yyyy")).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(s.Placement!.AccumulatedHours.ToString()).FontSize(9);
                    }

                    static IContainer DataCell(IContainer container) => container
                        .Padding(0)
                        .Border(1)
                        .BorderColor(Colors.Black);
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
