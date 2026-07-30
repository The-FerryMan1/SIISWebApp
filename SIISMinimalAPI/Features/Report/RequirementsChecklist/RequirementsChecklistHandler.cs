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
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Report.RequirementsChecklist;

public class RequirementsChecklistHandler(AppDbContext context) : IRequirementsChecklistService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GetRequirementsChecklist(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(s => s.Requirements)
            .Include(s => s.Internship)
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted && s.Application.Status == ApplicationStatusEnum.Approved)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(s => s.LastName)
            .ToListAsync(ct);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text("Requirements Checklist")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Approved Students with Requirements: {students.Count}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                foreach (var student in students)
                {
                    var fullname = $"{student.LastName}, {student.FirstName} {student.MiddleName}".Trim();
                    var office = student.Office != null ? OfficeEnumLabels.GetLabel(student.Office.Name) : "-";
                    var requirements = student.Requirements?.OrderBy(r => r.CreateAt).ToList() ?? new List<RequirementModel>();

                    page.Content().PaddingBottom(20).Column(col =>
                    {
                        col.Item().Text(fullname)
                            .FontSize(12).Bold();

                        col.Item().Text($"Office: {office} | Status: {student.Application?.Status}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);

                        if (requirements.Any())
                        {
                            col.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3f);
                                    columns.RelativeColumn(2f);
                                    columns.RelativeColumn(2f);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("File Name").Bold();
                                    header.Cell().Element(HeaderCell).AlignCenter().Text("Type").Bold();
                                    header.Cell().Element(HeaderCell).AlignCenter().Text("Submitted").Bold();

                                    static IContainer HeaderCell(IContainer container) => container
                                        .DefaultTextStyle(x => x.FontSize(9))
                                        .Padding(0)
                                        .Border(1)
                                        .BorderColor(Colors.Black);
                                });

                                foreach (var req in requirements)
                                {
                                    table.Cell().Element(DataCell).Text(req.FileName).FontSize(9);
                                    table.Cell().Element(DataCell).AlignCenter().Text(req.FileType ?? "-").FontSize(9);
                                    table.Cell().Element(DataCell).AlignCenter().Text(req.CreateAt.ToString("MM/dd/yyyy")).FontSize(9);

                                    static IContainer DataCell(IContainer container) => container
                                        .Padding(0)
                                        .Border(1)
                                        .BorderColor(Colors.Black);
                                }
                            });
                        }
                        else
                        {
                            col.Item().PaddingTop(5).Text("No requirements submitted.")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
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

    public async Task<byte[]> GetRequirementsChecklistCsv(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(s => s.Requirements)
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted && s.Application.Status == ApplicationStatusEnum.Approved)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(s => s.LastName)
            .ToListAsync(ct);

        var records = new List<RequirementsChecklistDto>();

        foreach (var s in students)
        {
            var fullname = $"{s.LastName}, {s.FirstName} {s.MiddleName}".Trim();
            var office = s.Office != null ? OfficeEnumLabels.GetLabel(s.Office.Name) : "N/A";
            var requirements = s.Requirements?.OrderBy(r => r.CreateAt).ToList() ?? new List<RequirementModel>();

            if (requirements.Any())
            {
                foreach (var req in requirements)
                {
                    records.Add(new RequirementsChecklistDto
                    {
                        StudentName = fullname,
                        Office = office,
                        FileName = req.FileName,
                        FileType = req.FileType ?? "-",
                        SubmittedAt = req.CreateAt
                    });
                }
            }
            else
            {
                records.Add(new RequirementsChecklistDto
                {
                    StudentName = fullname,
                    Office = office,
                    FileName = "N/A",
                    FileType = "N/A",
                    SubmittedAt = DateTime.MinValue
                });
            }
        }

        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration()))
        {
            csv.WriteRecords(records);
        }

        return memoryStream.ToArray();
    }
}
