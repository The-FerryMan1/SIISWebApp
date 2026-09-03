using System;
using System.Linq;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.OfficeReport;

public class OfficeReportHandler(AppDbContext context) : IOfficeReportService
{
    private readonly AppDbContext _context = context;

    private async Task<Office> GetOfficeAsync(string? officeName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(officeName))
        {
            throw new ArgumentException("Office is required", nameof(officeName));
        }

        var office = await _context.Offices
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OfficeName == officeName && !o.IsDeleted, ct)
            ?? await _context.Offices
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OfficeName.ToLower() == officeName.ToLower() && !o.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Office not found");

        return office;
    }

    private IQueryable<Student> BuildQuery(CommonFilterOptions filters)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = query.ApplyFilters(filters);
        return query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);
    }

    public async Task<byte[]> GenerateMasterlistPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        var office = await GetOfficeAsync(filters.Office, ct);
        var students = await BuildQuery(filters).ToListAsync(ct);

        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text($"Masterlist - {office.OfficeName}")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Total Students: {students.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(3f);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Name").Bold();
                        header.Cell().Element(HeaderCell).Text("School").Bold();
                        header.Cell().Element(HeaderCell).Text("Placement Status").Bold();

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
                table.Cell().Element(DataCell).Text(s.SchoolName).FontSize(9);
                table.Cell().Element(DataCell).Text(s.Placement?.PlacementStatus.ToString().Humanize(LetterCasing.Title) ?? "N/A").FontSize(9);
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

    public async Task<byte[]> GenerateOngoingPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        var office = await GetOfficeAsync(filters.Office, ct);
        var students = await BuildQuery(filters)
            .Where(t => t.Placement!.PlacementStatus == PlacementStatusEnum.Ongoing)
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
                    col.Item().Text($"Ongoing Internships - {office.OfficeName}")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Ongoing Students: {students.Count}")
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
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Name").Bold();
                        header.Cell().Element(HeaderCell).Text("School").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Total Internship Hours").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated Hours").Bold();

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
                        table.Cell().Element(DataCell).Text(s.SchoolName).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(s.TotalInternshipHours.ToString()).FontSize(9);
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

    public async Task<byte[]> GenerateFinishedPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        var office = await GetOfficeAsync(filters.Office, ct);
        var students = await BuildQuery(filters)
            .Where(t => t.Placement!.PlacementStatus == PlacementStatusEnum.Finished
                && t.Placement!.AccumulatedHours >= t.TotalInternshipHours)
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
                    col.Item().Text($"Finished Internships - {office.OfficeName}")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Completed Students: {students.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(3f);
                        columns.RelativeColumn(3f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Name").Bold();
                        header.Cell().Element(HeaderCell).Text("School").Bold();

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
                        table.Cell().Element(DataCell).Text(s.SchoolName).FontSize(9);
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
