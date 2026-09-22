using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Report.WeeklyReport;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;
using SIISMinimalAPI.Features.Shared.Utilities;
using System.Globalization;
using ProgressModel = SIISMinimalAPI.Features.Shared.Models.Progress;

namespace SIISMinimalAPI.Features.Report.WeeklyReport;

public class WeeklyReportHandler(AppDbContext context) : IWeeklyReportService
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

    private IQueryable<Student> BuildStudentQuery(CommonFilterOptions filters)
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

    public async Task<byte[]> GenerateWeeklyPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        var office = await GetOfficeAsync(filters.Office, ct);
        var students = await BuildStudentQuery(filters).ToListAsync(ct);

        var weekStart = filters.DateFrom ?? DateTime.Now.AddDays(-7);
        var weekEnd = filters.DateTo ?? DateTime.Now;

        var data = new List<WeeklyStudentDto>();

foreach (var s in students)
            {
                var placement = s.Placement!;
                var weekProgresses = placement.Progress != null && placement.Progress.CreatedAt >= weekStart && placement.Progress.CreatedAt <= weekEnd
                    ? new List<ProgressModel> { placement.Progress }
                    : new List<ProgressModel>();

                var hoursThisWeek = weekProgresses.Sum(p => p.TrainingHoursForWeek);
                var latestProgress = placement.Progress;

            var progressPercent = 0.0;
            if (s.TotalInternshipHours > 0 && placement.AccumulatedHours > 0)
            {
                progressPercent = Math.Round((double)placement.AccumulatedHours / s.TotalInternshipHours * 100, 1);
            }

            data.Add(new WeeklyStudentDto
            {
                FullName = s.FullName,
                School = s.SchoolName,
                PlacementStatus = placement.PlacementStatus.ToString().Humanize(LetterCasing.Title),
                TotalHours = s.TotalInternshipHours,
                AccumulatedHours = placement.AccumulatedHours,
                HoursThisWeek = hoursThisWeek,
                ProgressPercent = progressPercent
            });
        }

        var reportPeriod = $"{weekStart:MMM dd} - {weekEnd:MMM dd, yyyy}";

        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text($"Weekly Report - {office.OfficeName}")
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Period: {reportPeriod}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Total Students: {data.Count}")
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
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                        header.Cell().Element(HeaderCell).Text("School").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Hours This Week").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Total Hours").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated").Bold();
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Progress").Bold();

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
                        table.Cell().Element(DataCell).Text(d.FullName).FontSize(9);
                        table.Cell().Element(DataCell).Text(d.School).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.HoursThisWeek.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.TotalHours.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text(d.AccumulatedHours.ToString()).FontSize(9);
                        table.Cell().Element(DataCell).AlignCenter().Text($"{d.ProgressPercent}%").FontSize(9);
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

    public async Task<byte[]> GenerateWeeklyCsv(CommonFilterOptions filters, CancellationToken ct)
    {
        var office = await GetOfficeAsync(filters.Office, ct);
        var students = await BuildStudentQuery(filters).ToListAsync(ct);

        var weekStart = filters.DateFrom ?? DateTime.Now.AddDays(-7);
        var weekEnd = filters.DateTo ?? DateTime.Now;

        var data = new List<WeeklyStudentDto>();

foreach (var s in students)
            {
                var placement = s.Placement!;
                var weekProgresses = placement.Progress != null && placement.Progress.CreatedAt >= weekStart && placement.Progress.CreatedAt <= weekEnd
                    ? new List<ProgressModel> { placement.Progress }
                    : new List<ProgressModel>();

                var hoursThisWeek = weekProgresses.Sum(p => p.TrainingHoursForWeek);

            var progressPercent = 0.0;
            if (s.TotalInternshipHours > 0 && placement.AccumulatedHours > 0)
            {
                progressPercent = Math.Round((double)placement.AccumulatedHours / s.TotalInternshipHours * 100, 1);
            }

            data.Add(new WeeklyStudentDto
            {
                FullName = s.FullName,
                School = s.SchoolName,
                PlacementStatus = placement.PlacementStatus.ToString().Humanize(LetterCasing.Title),
                TotalHours = s.TotalInternshipHours,
                AccumulatedHours = placement.AccumulatedHours,
                HoursThisWeek = hoursThisWeek,
                ProgressPercent = progressPercent
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

    public async Task<List<WeeklyHistoryDto>> GetWeeklyHistory(string? officeName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(officeName))
        {
            return new List<WeeklyHistoryDto>();
        }

        var office = await _context.Offices
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OfficeName == officeName && !o.IsDeleted, ct);

        if (office is null)
        {
            return new List<WeeklyHistoryDto>();
        }

        var students = await _context.Students
            .Include(t => t.Placement).ThenInclude(p => p.Progress)
            .Where(t => t.Placement != null && t.Placement!.OfficeId == office.Id && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);

        var history = new List<WeeklyHistoryDto>();
        var weekGroups = students
            .Where(s => s.Placement?.Progress != null)
            .Select(s => s.Placement!.Progress!)
            .GroupBy(p => new
            {
                Year = p.CreatedAt.Year,
                Week = System.Globalization.ISOWeek.GetWeekOfYear(p.CreatedAt)
            })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Week)
            .ToList();

        foreach (var group in weekGroups)
        {
            var monday = ISOWeek.ToDateTime(group.Key.Year, group.Key.Week, DayOfWeek.Monday);
            var sunday = monday.AddDays(6);

            var studentCount = group.Select(p => p.Placement!.StudentId).Distinct().Count();
            var totalHours = group.Sum(p => p.TrainingHoursForWeek);

            history.Add(new WeeklyHistoryDto
            {
                OfficeName = office.OfficeName,
                WeekStartDate = monday,
                WeekEndDate = sunday,
                ReportPeriod = $"{monday:MMM dd} - {sunday:MMM dd, yyyy}",
                TotalStudents = studentCount,
                TotalHoursThisWeek = totalHours,
                GeneratedAt = group.Max(p => p.CreatedAt)
            });
        }

        return history;
    }
}
