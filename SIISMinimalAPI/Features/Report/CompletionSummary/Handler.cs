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
using SIISMinimalAPI.Features.Shared.Models;
using SIISMinimalAPI.Features.Shared.Utilities;
using System.Globalization;

namespace SIISMinimalAPI.Features.Report.CompletionSummary;

public class CompletionSummaryHandler(AppDbContext context) : ICompletionSummaryService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GeneratePdf(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && t.Placement!.AccumulatedHours >= t.TotalInternshipHours && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = query.ApplyFilters(filters).OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var data = students.Select(s => new CompletionSummaryDto
        {
            StudentName = s.FullName,
            Office = s.Placement!.Office!.OfficeName,
            TotalHours = s.TotalInternshipHours,
            AccumulatedHours = s.Placement.AccumulatedHours,
            CompletionDate = s.Placement.EstimatedEndDate.ToString("MM/dd/yyyy")
        })
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
                    col.Item().Text("Completion Summary Report")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Completed Internships: {data.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                        header.Cell().Element(HeaderCell).Text("Office").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Total Hours").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Completion Date").Bold();

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
                        table.Cell().Element(DataCell).Text(d.Office).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.TotalHours.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.AccumulatedHours.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.CompletionDate).FontSize(9);
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

    public async Task<byte[]> GenerateCsv(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && t.Placement!.AccumulatedHours >= t.TotalInternshipHours && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = query.ApplyFilters(filters).OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var data = students.Select(s => new CompletionSummaryDto
        {
            StudentName = s.FullName,
            Office = s.Placement!.Office!.OfficeName,
            TotalHours = s.TotalInternshipHours,
            AccumulatedHours = s.Placement.AccumulatedHours,
            CompletionDate = s.Placement.EstimatedEndDate.ToString("MM/dd/yyyy")
        })
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
