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

namespace SIISMinimalAPI.Features.Report.Students;

public class StudentsHandler(AppDbContext context) : IStudentsService
{
    private readonly AppDbContext _context = context;

    public async Task<byte[]> GetStudentsPdf(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(s => s.School)
            .Include(s => s.Internship)
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted)
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
                    col.Item().Text("Student Masterlist")
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
                        columns.ConstantColumn(25);
                        columns.RelativeColumn(1.8f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("#").Bold();
                        header.Cell().Element(HeaderCell).Text("Full Name").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Gender").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Grade").Bold();
                        header.Cell().Element(HeaderCell).Text("Email").Bold();
                        header.Cell().Element(HeaderCell).Text("Contact").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Office").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Status").Bold();

                        static IContainer HeaderCell(IContainer container) => container
                            .DefaultTextStyle(x => x.FontSize(9))
                            .Padding(3)
                            .Border(1)
                            .BorderColor(Colors.Black);
                    });

                    int index = 1;
                    foreach (var s in students)
                    {
                        var fullname = $"{s.LastName}, {s.FirstName} {s.MiddleName}".Trim();
                        var office = s.Office != null ? OfficeEnumLabels.GetLabel(s.Office.Name) : "-";
                        var status = s.Application?.Status.ToString() ?? "-";

                        table.Cell().Element(DataCell).AlignCenter().Text(index++.ToString()).FontSize(8);
                        table.Cell().Element(DataCell).Text(fullname).FontSize(8);
                        table.Cell().Element(DataCell).AlignCenter().Text(s.Gender.ToString()).FontSize(8);
                        table.Cell().Element(DataCell).AlignCenter().Text(s.GradeLevel.ToString().Humanize(LetterCasing.Title)).FontSize(8);
                        table.Cell().Element(DataCell).Text(s.Email).FontSize(8);
                        table.Cell().Element(DataCell).Text(s.ContactNumber).FontSize(8);
                        table.Cell().Element(DataCell).Text(office).FontSize(8);
                        table.Cell().Element(DataCell).AlignCenter().Text(status).FontSize(8);

                        static IContainer DataCell(IContainer container) => container
                            .Padding(3)
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

    public async Task<byte[]> GetStudentsCsv(CancellationToken ct)
    {
        var students = await _context.Students
            .Include(s => s.Office)
            .Include(s => s.Application)
            .Where(s => !s.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(s => s.LastName)
            .ToListAsync(ct);

        var records = students.Select(s => new StudentsDto
        {
            FullName = $"{s.LastName}, {s.FirstName} {s.MiddleName}".Trim(),
            Gender = s.Gender.ToString(),
            GradeLevel = s.GradeLevel.ToString(),
            Email = s.Email,
            ContactNumber = s.ContactNumber,
            Address = s.Address,
            Office = s.Office != null ? OfficeEnumLabels.GetLabel(s.Office.Name) : "N/A",
            Status = s.Application?.Status.ToString() ?? "N/A",
            DateOfBirth = s.DateOfBirth.ToString("yyyy-MM-dd")
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
