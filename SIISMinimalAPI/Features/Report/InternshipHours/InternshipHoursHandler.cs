using System;
using System.Linq;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.InternshipHours;

public class InternshipHoursHandler(AppDbContext context) : IInternshipHoursService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GetInternshipHours(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(s => s.Internship)
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted && s.Internship != null)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(s => s.Internship.InternshipTotalHours)
            .ToListAsync(ct);

        var totalHours = students.Sum(s => s.Internship?.InternshipTotalHours ?? 0);
        var avgHours = students.Count > 0 ? Math.Round((double)totalHours / students.Count, 1) : 0;

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text("Internship Hours Summary")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Total Hours: {totalHours} | Average Hours: {avgHours} | Students: {students.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(1.3f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("#").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                        header.Cell().Element(HeaderCell).Text("Office").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Start Date").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("End Date").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Hours").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Status").Bold();

                        static IContainer HeaderCell(IContainer container) => container
                            .DefaultTextStyle(x => x.FontSize(10))
                            .Padding(0)
                            .Border(1)
                            .BorderColor(Colors.Black);
                    });

                    int index = 1;
                    foreach (var s in students)
                    {
                        var fullname = $"{s.LastName}, {s.FirstName} {s.MiddleName}".Trim();
                        var office = s.Office != null ? OfficeEnumLabels.GetLabel(s.Office.Name) : "-";
                        var internship = s.Internship!;
                        var status = s.Application?.Status.ToString() ?? "-";

                        table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).Text(fullname).FontSize(9);
                        table.Cell().Element(DataCell).Text(office).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(internship.StartDate.ToString("MM/dd/yyyy")).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(internship.EstimatedEndDate.ToString("MM/dd/yyyy")).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(internship.InternshipTotalHours.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(status).FontSize(9);

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
