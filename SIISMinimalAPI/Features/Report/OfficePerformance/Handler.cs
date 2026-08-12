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

namespace SIISMinimalAPI.Features.Report.OfficePerformance;

public class OfficePerformanceHandler(AppDbContext context) : IOfficePerformanceService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GeneratePdf(CancellationToken ct)
    {
        var offices = await _context.Offices
            .Where(o => !o.IsDeleted)
            .AsNoTracking()
            .ToListAsync(ct);

        var data = new List<OfficePerformanceDto>();
        foreach (var office in offices)
        {
            var ojts = await _context.Students
                .Include(t => t.Application)
                .Include(t => t.Placement)
                .Where(t => t.Placement != null && t.Placement!.OfficeId == office.Id && !t.IsDeleted)
                .AsNoTracking()
                .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
                .ToListAsync(ct);

            var totalOjts = ojts.Count;
            var avgAccumulated = ojts.Any() ? ojts.Average(s => s.Placement!.AccumulatedHours) : 0;
            var avgCompletion = ojts.Any() ? ojts.Average(s => s.TotalInternshipHours > 0 ? (double)s.Placement!.AccumulatedHours / s.TotalInternshipHours * 100 : 0) : 0;

            data.Add(new OfficePerformanceDto
            {
                OfficeName = office.OfficeName,
                TotalOJTs = totalOjts,
                AverageAccumulatedHours = Math.Round(avgAccumulated, 1),
                AverageCompletionRate = Math.Round(avgCompletion, 1)
            });
        }

        var ordered = data.OrderBy(d => d.OfficeName).ToList();

        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text("Office Performance Report")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Total Offices: {ordered.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(3.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2f);
                        columns.RelativeColumn(2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Office Name").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Total OJTs").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Avg Accumulated Hours").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Avg Completion Rate").Bold();

                        static IContainer HeaderCell(IContainer container) => container
                            .DefaultTextStyle(x => x.FontSize(10))
                            .Padding(0)
                            .Border(1)
                            .BorderColor(Colors.Black);
                    });

                    int index = 1;
                    foreach (var d in ordered)
                    {
                        table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).Text(d.OfficeName).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.TotalOJTs.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.AverageAccumulatedHours.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text($"{d.AverageCompletionRate:F1}%").FontSize(9);
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
        var offices = await _context.Offices
            .Where(o => !o.IsDeleted)
            .AsNoTracking()
            .ToListAsync(ct);

        var data = new List<OfficePerformanceDto>();
        foreach (var office in offices)
        {
            var ojts = await _context.Students
                .Include(t => t.Application)
                .Include(t => t.Placement)
                .Where(t => t.Placement != null && t.Placement!.OfficeId == office.Id && !t.IsDeleted)
                .AsNoTracking()
                .AsSplitQuery().OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
                .ToListAsync(ct);

            var totalOjts = ojts.Count;
            var avgAccumulated = ojts.Any() ? ojts.Average(s => s.Placement!.AccumulatedHours) : 0;
            var avgCompletion = ojts.Any() ? ojts.Average(s => s.TotalInternshipHours > 0 ? (double)s.Placement!.AccumulatedHours / s.TotalInternshipHours * 100 : 0) : 0;

            data.Add(new OfficePerformanceDto
            {
                OfficeName = office.OfficeName,
                TotalOJTs = totalOjts,
                AverageAccumulatedHours = Math.Round(avgAccumulated, 1),
                AverageCompletionRate = Math.Round(avgCompletion, 1)
            });
        }

        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration()))
        {
            csv.WriteRecords(data);
        }

        return memoryStream.ToArray();
    }
}
