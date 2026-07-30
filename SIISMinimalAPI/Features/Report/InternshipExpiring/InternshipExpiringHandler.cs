using System;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.InternshipExpiring;

public class InternshipExpiringHandler(AppDbContext context) : IInternshipExpiringService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GetExpiringInternships(CancellationToken ct, int daysThreshold = 30)
    {
        var thresholdDate = DateOnly.FromDateTime(DateTime.Now.AddDays(daysThreshold));

        var students = await _context.Students
            .Include(s => s.Internship)
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted 
                && s.Internship != null 
                && s.Internship.EstimatedEndDate <= thresholdDate
                && s.Application.Status == ApplicationStatusEnum.Approved)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(s => s.Internship.EstimatedEndDate)
            .ToListAsync(ct);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text($"Expiring Internships (Next {daysThreshold} Days)")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Students Expiring: {students.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                if (students.Count == 0)
                {
                    page.Content().PaddingTop(50).AlignCenter().Text($"No internships expiring within the next {daysThreshold} days.")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                }
                else
                {
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.8f);
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).AlignCenter().Text("#").Bold();
                            header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                            header.Cell().Element(HeaderCell).Text("Office").Bold();
                            header.Cell().Element(HeaderCell).AlignCenter().Text("End Date").Bold();
                            header.Cell().Element(HeaderCell).AlignCenter().Text("Days Left").Bold();
                            header.Cell().Element(HeaderCell).AlignCenter().Text("Hours").Bold();

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
                            var daysLeft = internship.EstimatedEndDate.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber;

                            table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                            table.Cell().Element(DataCell).Text(fullname).FontSize(9);
                            table.Cell().Element(DataCell).Text(office).FontSize(9);
                            table.Cell().Element(DataCell).AlignCenter().Text(internship.EstimatedEndDate.ToString("MM/dd/yyyy")).FontSize(9);
                             table.Cell().Element(DataCell).AlignCenter().Text(daysLeft.ToString()).FontSize(9);
                            table.Cell().Element(DataCell).AlignCenter().Text(internship.InternshipTotalHours.ToString()).FontSize(9);

                            static IContainer DataCell(IContainer container) => container
                                .Padding(0)
                                .Border(1)
                                .BorderColor(Colors.Black);
                        }
                    });
                }

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

    public async Task<byte[]> GetExpiringInternshipsCsv(CancellationToken ct, int daysThreshold = 30)
    {
        var thresholdDate = DateOnly.FromDateTime(DateTime.Now.AddDays(daysThreshold));

        var students = await _context.Students
            .Include(s => s.Internship)
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted 
                && s.Internship != null 
                && s.Internship.EstimatedEndDate <= thresholdDate
                && s.Application.Status == ApplicationStatusEnum.Approved)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(s => s.Internship.EstimatedEndDate)
            .ToListAsync(ct);

        var records = students.Select(s => new InternshipExpiringDto
        {
            FullName = $"{s.LastName}, {s.FirstName} {s.MiddleName}".Trim(),
            Office = s.Office != null ? OfficeEnumLabels.GetLabel(s.Office.Name) : "N/A",
            EstimatedEndDate = s.Internship!.EstimatedEndDate,
            DaysLeft = s.Internship.EstimatedEndDate.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber,
            TotalHours = s.Internship.InternshipTotalHours
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
