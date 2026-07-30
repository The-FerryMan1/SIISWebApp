using System;
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

namespace SIISMinimalAPI.Features.Report.ApplicationsPending;

public class PendingApplicationsHandler(AppDbContext context) : IPendingApplicationsService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GetPendingApplications(CancellationToken ct)
    {
        var applications = await _context.Applications
            .Include(a => a.Student)
            .ThenInclude(s => s.Internship)
            .Include(a => a.Student.Office)
            .Where(a => a.Status == ApplicationStatusEnum.Pending && !a.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(a => a.CreateAt)
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
                    col.Item().Text("Pending Applications")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Total Pending: {applications.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("#").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Grade Level").Bold();
                        header.Cell().Element(HeaderCell).Text("Office").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Internship Type").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Applied Date").Bold();

                        static IContainer HeaderCell(IContainer container) => container
                            .DefaultTextStyle(x => x.FontSize(10))
                            .Padding(0)
                            .Border(1)
                            .BorderColor(Colors.Black);
                    });

                    int index = 1;
                    foreach (var app in applications)
                    {
                        var fullname = $"{app.Student.LastName}, {app.Student.FirstName} {app.Student.MiddleName}".Trim();
                        var office = app.Student.Office != null ? OfficeEnumLabels.GetLabel(app.Student.Office.Name) : "-";
                        var internship = app.Student.Internship;
                        var gradeLevel = app.Student.GradeLevel.ToString().Humanize(LetterCasing.Title);
                        var internshipType = internship != null ? internship.InternshipNature.ToString().Humanize(LetterCasing.Title) : "-";

                        table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).Text(fullname).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(gradeLevel).FontSize(9);
                        table.Cell().Element(DataCell).Text(office).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(internshipType).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(app.CreateAt.ToString("MM/dd/yyyy")).FontSize(9);

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

    public async Task<byte[]> GetPendingApplicationsCsv(CancellationToken ct)
    {
        var applications = await _context.Applications
            .Include(a => a.Student)
            .ThenInclude(s => s.Internship)
            .Include(a => a.Student.Office)
            .Where(a => a.Status == ApplicationStatusEnum.Pending && !a.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(a => a.CreateAt)
            .ToListAsync(ct);

        var records = applications.Select(a => new PendingApplicationsDto
        {
            FullName = $"{a.Student.LastName}, {a.Student.FirstName} {a.Student.MiddleName}".Trim(),
            GradeLevel = a.Student.GradeLevel.ToString().Humanize(LetterCasing.Title),
            Office = a.Student.Office != null ? OfficeEnumLabels.GetLabel(a.Student.Office.Name) : "N/A",
            InternshipType = a.Student.Internship != null ? a.Student.Internship.InternshipNature.ToString().Humanize(LetterCasing.Title) : "N/A",
            AppliedDate = a.CreateAt
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
