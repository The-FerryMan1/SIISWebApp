using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using System.Globalization;

namespace SIISMinimalAPI.Features.Report.RequirementsCompliance;

public class RequirementsComplianceHandler(AppDbContext context) : IRequirementsComplianceService
{
    private readonly AppDbContext _context = context;
    private readonly DateTime _expiredThreshold = DateTime.Now.AddYears(-1);

    public async Task<byte[]> GeneratePdf(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(t => t.Requirements)
            .Where(t => !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .ToListAsync(ct);

        var data = students.Select(s => new RequirementsComplianceDto
        {
            StudentName = s.FullName,
            Email = s.Email,
            School = s.SchoolName,
            TotalRequirements = s.Requirements?.Count ?? 0,
            MissingCount = (s.Requirements?.Count ?? 0) == 0 ? 1 : 0,
            ExpiredCount = (s.Requirements?.Count(r => r.CreatedAt <= _expiredThreshold) ?? 0)
        })
        .Where(d => d.MissingCount > 0 || d.ExpiredCount > 0)
        .OrderBy(d => d.StudentName)
        .ToList();

        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text("Requirements Compliance Report")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Students with Issues: {data.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                        header.Cell().Element(HeaderCell).Text("Email").Bold();
                        header.Cell().Element(HeaderCell).Text("School").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Missing").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Expired").Bold();

                        static IContainer HeaderCell(IContainer container) => container
                            .DefaultTextStyle(x => x.FontSize(10))
                            .Padding(0)
                            .Border(1)
                            .BorderColor(Colors.Black);
                    });

                    int index = 1;
                    foreach (var d in data)
                    {
                        table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).Text(d.StudentName).FontSize(9);
                        table.Cell().Element(DataCell).Text(d.Email).FontSize(9);
                        table.Cell().Element(DataCell).Text(d.School).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.MissingCount.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.ExpiredCount.ToString()).FontSize(9);
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
            .Include(t => t.Requirements)
            .Where(t => !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .ToListAsync(ct);

        var data = students.Select(s => new RequirementsComplianceDto
        {
            StudentName = s.FullName,
            Email = s.Email,
            School = s.SchoolName,
            TotalRequirements = s.Requirements?.Count ?? 0,
            MissingCount = (s.Requirements?.Count ?? 0) == 0 ? 1 : 0,
            ExpiredCount = (s.Requirements?.Count(r => r.CreatedAt <= _expiredThreshold) ?? 0)
        })
        .Where(d => d.MissingCount > 0 || d.ExpiredCount > 0)
        .OrderBy(d => d.StudentName)
        .ToList();

        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration()))
        {
            csv.WriteRecords(data);
        }

        return memoryStream.ToArray();
    }
}
