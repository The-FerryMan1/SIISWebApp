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

namespace SIISMinimalAPI.Features.Report.RequirementsMissing;

public class MissingRequirementsHandler(AppDbContext context) : IMissingRequirementsService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GetMissingRequirements(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(s => s.Requirements)
            .Include(s => s.Internship)
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted && s.Application.Status == ApplicationStatusEnum.Approved)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);

        var missing = students.Where(s => s.Requirements == null || !s.Requirements.Any()).ToList();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text("Missing Requirements Report")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Approved Students Without Requirements: {missing.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                if (missing.Count == 0)
                {
                    page.Content().PaddingTop(50).AlignCenter().Text("All approved students have submitted requirements.")
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
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.8f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).AlignCenter().Text("#").Bold();
                            header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                            header.Cell().Element(HeaderCell).AlignCenter().Text("Grade Level").Bold();
                            header.Cell().Element(HeaderCell).Text("Office").Bold();
                            header.Cell().Element(HeaderCell).AlignCenter().Text("Applied Date").Bold();

                            static IContainer HeaderCell(IContainer container) => container
                                .DefaultTextStyle(x => x.FontSize(10))
                                .Padding(4)
                                .Border(1)
                                .BorderColor(Colors.Black);
                        });

                        int index = 1;
                        foreach (var s in missing)
                        {
                            var fullname = $"{s.LastName}, {s.FirstName} {s.MiddleName}".Trim();
                            var office = s.Office != null ? OfficeEnumLabels.GetLabel(s.Office.Name) : "-";

                            table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(9);
                            table.Cell().Element(DataCell).Text(fullname).FontSize(9);
                            table.Cell().Element(DataCell).AlignCenter().Text(s.GradeLevel.ToString().Humanize(LetterCasing.Title)).FontSize(9);
                            table.Cell().Element(DataCell).Text(office).FontSize(9);
                            table.Cell().Element(DataCell).AlignCenter().Text(s.Application.CreateAt.ToString("MM/dd/yyyy")).FontSize(9);

                            static IContainer DataCell(IContainer container) => container
                                .Padding(4)
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

    public async Task<byte[]> GetMissingRequirementsCsv(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(s => s.Requirements)
            .Include(s => s.Internship)
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted && s.Application.Status == ApplicationStatusEnum.Approved)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);

        var missing = students.Where(s => s.Requirements == null || !s.Requirements.Any()).ToList();

        var records = missing.Select(s => new MissingRequirementsDto
        {
            FullName = $"{s.LastName}, {s.FirstName} {s.MiddleName}".Trim(),
            GradeLevel = s.GradeLevel.ToString().Humanize(LetterCasing.Title),
            Office = s.Office != null ? OfficeEnumLabels.GetLabel(s.Office.Name) : "N/A",
            AppliedDate = s.Application.CreateAt
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
