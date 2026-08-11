using System;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.OjtPerOffice;

public class OjtPerOfficehandler(AppDbContext context) : IOjtPerOfficeService
{
    private readonly AppDbContext _context = context;
    public async Task<byte[]> ListAllOjtPerOffice(string office, CancellationToken ct)
    {
        var ojtOffice = await _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && t.Placement!.Office!.OfficeName == office)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
        QuestPDF.Settings.License = LicenseType.Community; // or Evaluation
        var document = Document.Create(doc =>
    {
        doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);

            // Header
            page.Header().PaddingBottom(15).Column(col =>
            {
                col.Item().Text($"OJT Students - {office}")
                    .FontSize(20).Bold().AlignCenter();

                col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                    .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                col.Item().PaddingTop(3).Text($"Total Students: {ojtOffice.Count}")
                    .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
            });

            // Content
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(2.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f);
columns.RelativeColumn(1.5f);   // Started Date
                columns.RelativeColumn(1.5f);   // Estimated End Date
                columns.RelativeColumn(1.2f);   // Accumulated Hours
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                    header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Status").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Grade level").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Strand").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Degree").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Internship hours").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Started Date").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Estimated End Date").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated Hours").Bold();

                    static IContainer HeaderCell(IContainer container) => container
                        .DefaultTextStyle(x => x.FontSize(10))
                        .Padding(0)
                        .Border(1)
                        .BorderColor(Colors.Black);
                });

                // Data rows
                int index = 1;
                foreach (var ojt in ojtOffice)
                {
                    var fullname = ojt.FullName;
                    var status = ojt.Application?.Status;
                    var totalHours = ojt.TotalInternshipHours;
                    var gradeLevel = ojt.GradeLevel.ToString().Humanize(LetterCasing.Title);
                    var degree = ojt.Degree.ToString().Humanize(LetterCasing.Title);
                    var strand =  ojt.Strand.ToString().Humanize(LetterCasing.Title) ?? "N/A";
                    var startedDate = ojt.Placement?.StartDate.ToString("MM/dd/yyyy") ?? "-";
                    var estimatedDate = ojt.Placement?.EstimatedEndDate.ToString("MM/dd/yyyy") ?? "-";
                    var accumulatedHours = ojt.Placement?.AccumulatedHours.ToString() ?? "-";

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(index++.ToString()).FontSize(9);

                    table.Cell().Element(DataCell)
                        .Text(fullname).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(status?.ToString() ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(gradeLevel.ToString() ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(strand ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(degree).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(totalHours > 0 ? totalHours.ToString() : "-").FontSize(9);
                    
                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(startedDate).FontSize(9);

                   table.Cell().Element(DataCell).AlignCenter()
                        .Text(estimatedDate).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(accumulatedHours).FontSize(9);
                }

                static IContainer DataCell(IContainer container) => container
                    .Padding(0)
                    .Border(1)
                    .BorderColor(Colors.Black);
            });

            // Footer
            page.Footer().AlignCenter().PaddingTop(10).Text(text =>
            {
                text.Span("Page ").FontSize(9).FontColor(Colors.Grey.Darken1);
                text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                text.Span(" of ").FontSize(9).FontColor(Colors.Grey.Darken1);
                text.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    });

        return document.GeneratePdf(); // Returns byte[]
    }

    public async Task<byte[]> ListAllOjtPerOfficeFiltered(string? office, ApplicationStatusEnum? status, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null)
            .AsNoTracking()
            .AsSplitQuery();

        if (!string.IsNullOrEmpty(office))
        {
            query = query.Where(t => t.Placement != null && t.Placement!.Office!.OfficeName == office);
        }

        if (status is { } selectedStatus)
        {
            query = query.Where(t => t.Application.Status == selectedStatus);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(t => t.Placement != null && t.Placement!.StartDate >= DateOnly.FromDateTime(dateFrom.Value));
        }

        if (dateTo.HasValue)
        {
            query = query.Where(t => t.Placement != null && t.Placement!.StartDate <= DateOnly.FromDateTime(dateTo.Value));
        }

        var ojtOffice = await query.ToListAsync(ct);
        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(doc =>
    {
        doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);

            page.Header().PaddingBottom(15).Column(col =>
            {
                var officeLabel = office != null ? office : "All Offices";
                col.Item().Text($"OJT Students - {officeLabel} (Filtered)")
                    .FontSize(20).Bold().AlignCenter();

                col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                    .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                col.Item().PaddingTop(3).Text($"Total Students: {ojtOffice.Count}")
                    .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
            });

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(2.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                    header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Status").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Grade level").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Strand").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Degree").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Internship hours").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Started Date").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Estimated End Date").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated Hours").Bold();

                    static IContainer HeaderCell(IContainer container) => container
                        .DefaultTextStyle(x => x.FontSize(10))
                        .Padding(0)
                        .Border(1)
                        .BorderColor(Colors.Black);
                });

                int index = 1;
                foreach (var ojt in ojtOffice)
                {
                    var fullname = ojt.FullName;
                    var status = ojt.Application?.Status;
                    var totalHours = ojt.TotalInternshipHours;
                    var accumulatedHours = ojt.Placement?.AccumulatedHours.ToString() ?? "-";
                    var gradeLevel = ojt.GradeLevel.ToString().Humanize(LetterCasing.Title);
                    var degree = ojt.Degree.ToString().Humanize(LetterCasing.Title);
                    var strand =  ojt.Strand.ToString().Humanize(LetterCasing.Title) ?? "N/A";
                    var startedDate = ojt.Placement?.StartDate.ToString("MM/dd/yyyy") ?? "-";
                    var estimatedDate = ojt.Placement?.EstimatedEndDate.ToString("MM/dd/yyyy") ?? "-";

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(index++.ToString()).FontSize(9);

                    table.Cell().Element(DataCell)
                        .Text(fullname).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(status?.ToString() ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(gradeLevel.ToString() ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(strand ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(degree).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(totalHours > 0 ? totalHours.ToString() : "-").FontSize(9);
                    
                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(startedDate).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(estimatedDate).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(accumulatedHours).FontSize(9);
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
