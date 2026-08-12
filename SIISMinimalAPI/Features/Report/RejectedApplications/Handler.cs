using System;
using System.Text;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;
using System.Globalization;

namespace SIISMinimalAPI.Features.Report.RejectedApplications;

public class RejectedApplicationsHandler(AppDbContext context) : IRejectedApplicationsService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GeneratePdf(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(t => t.Application)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Rejected && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
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
                    col.Item().Text("Rejected Applications Report")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Total Rejected: {students.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                        header.Cell().Element(HeaderCell).Text("Email").Bold();
                        header.Cell().Element(HeaderCell).Text("School").Bold();
                        header.Cell().Element(HeaderCell).Text("Reason").Bold();

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
                        table.Cell().Element(DataCell).Text(s.Email).FontSize(9);
                        table.Cell().Element(DataCell).Text(s.SchoolName).FontSize(9);
                        table.Cell().Element(DataCell).Text(s.Application!.Reason ?? "-").FontSize(9);
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

    public async Task<byte[]> GenerateCsv(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(t => t.Application)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Rejected && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .ToListAsync(ct);

        var records = students.Select(t => new RejectedApplicationsDto
        {
            Name = t.FullName,
            Email = t.Email,
            School = t.SchoolName,
            SubmissionDate = t.Application!.CreatedAt,
            Reason = t.Application.Reason ?? string.Empty
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
