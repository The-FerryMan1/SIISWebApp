using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
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
using SIISMinimalAPI.Features.Shared.Utilities;
using System.Globalization;

namespace SIISMinimalAPI.Features.Report.AdminReport;

public class AdminReportHandler(AppDbContext context) : IAdminReportService
{
    private readonly AppDbContext _context = context;

    private IQueryable<Student> ApplyCommonStudentFilters(IQueryable<Student> query, CommonFilterOptions filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var name = filters.Name.Trim();
            query = query.Where(t =>
                (t.FirstName != null && t.FirstName.Contains(name)) ||
                (t.LastName != null && t.LastName.Contains(name)));
        }

        if (!string.IsNullOrWhiteSpace(filters.School))
        {
            var school = filters.School.Trim();
            query = query.Where(t => t.SchoolName != null && t.SchoolName.Contains(school));
        }

        if (!string.IsNullOrWhiteSpace(filters.Office))
        {
            var office = filters.Office.Trim();
            query = query.Where(t => t.Placement != null && t.Placement!.Office != null && t.Placement.Office.OfficeName == office);
        }

        return query;
    }

    private IQueryable<Student> ApplyApplicationDateFilter(IQueryable<Student> query, CommonFilterOptions filters)
    {
        if (filters.DateFrom.HasValue)
        {
            query = query.Where(t => t.Application != null && t.Application.CreatedAt >= filters.DateFrom.Value);
        }

        if (filters.DateTo.HasValue)
        {
            query = query.Where(t => t.Application != null && t.Application.CreatedAt <= filters.DateTo.Value);
        }

        return query;
    }

    private IQueryable<Student> ApplyPlacementDateFilter(IQueryable<Student> query, CommonFilterOptions filters)
    {
        if (filters.DateFrom.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(filters.DateFrom.Value);
            query = query.Where(t => t.Placement != null && t.Placement.StartDate >= dateOnly);
        }

        if (filters.DateTo.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(filters.DateTo.Value);
            query = query.Where(t => t.Placement != null && t.Placement.StartDate <= dateOnly);
        }

        return query;
    }

    private byte[] GeneratePdfDocument(string title, int totalCount, Action<TableDescriptor> buildTable)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text(title)
                        .FontSize(20).Bold().AlignCenter();

                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                    col.Item().PaddingTop(3).Text($"Total Count: {totalCount}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                });

                page.Content().Table(table =>
                {
                    buildTable(table);
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

    private byte[] GenerateCsv<T>(IEnumerable<T> records)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration()))
        {
            csv.WriteRecords(records);
        }

        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateExpiringInternshipsPdf(long? officeId, int days, string? school, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var threshold = DateOnly.FromDateTime(DateTime.Now.AddDays(days));

        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        if (officeId.HasValue)
        {
            query = query.Where(t => t.Placement!.OfficeId == officeId.Value);
        }

        query = query.Where(t => t.Placement!.EstimatedEndDate <= threshold);

        if (!string.IsNullOrWhiteSpace(school))
        {
            var schoolFilter = school.Trim();
            query = query.Where(t => t.SchoolName != null && t.SchoolName.Contains(schoolFilter));
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= dateTo.Value);
        }

        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var officeName = officeId.HasValue
            ? await _context.Offices
                .Where(o => o.Id == officeId.Value && !o.IsDeleted)
                .Select(o => o.OfficeName)
                .FirstOrDefaultAsync(ct)
            : "All Offices";

        return GeneratePdfDocument($"Expiring Internships - {officeName}", students.Count, table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                header.Cell().Element(HeaderCell).Text("Office").Bold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Status").Bold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Start Date").Bold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("End Date").Bold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Accumulated").Bold();

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
                table.Cell().Element(DataCell).Text(s.Placement?.Office?.OfficeName ?? "N/A").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(s.Placement?.PlacementStatus.ToString().Humanize(LetterCasing.Title) ?? "N/A").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(s.Placement?.StartDate.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(s.Placement?.EstimatedEndDate.ToString("MM/dd/yyyy") ?? "-").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text(s.Placement?.AccumulatedHours.ToString() ?? "-").FontSize(9);
            }

            static IContainer DataCell(IContainer container) => container
                .Padding(0)
                .Border(1)
                .BorderColor(Colors.Black);
        });
    }

    public async Task<byte[]> GenerateMasterlistPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Approved && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyApplicationDateFilter(query, filters);
        
        // Apply placement status filter if specified
        if (!string.IsNullOrWhiteSpace(filters.PlacementStatus) && Enum.TryParse<PlacementStatusEnum>(filters.PlacementStatus, true, out var placementStatus))
        {
            query = query.Where(t => t.Placement != null && t.Placement.PlacementStatus == placementStatus);
        }
        
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        return GeneratePdfDocument("Masterlist - Approved Applications", students.Count, table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                header.Cell().Element(HeaderCell).Text("Name").Bold();
                header.Cell().Element(HeaderCell).AlignCenter().Text("Placement Status").Bold();
                header.Cell().Element(HeaderCell).Text("Office Assigned").Bold();

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
                table.Cell().Element(DataCell).AlignCenter().Text(s.Placement?.PlacementStatus.ToString().Humanize(LetterCasing.Title) ?? "N/A").FontSize(9);
                table.Cell().Element(DataCell).Text(s.Placement?.Office?.OfficeName ?? "N/A").FontSize(9);
            }

            static IContainer DataCell(IContainer container) => container
                .Padding(0)
                .Border(1)
                .BorderColor(Colors.Black);
        });
    }

    public async Task<byte[]> GenerateMasterlistCsv(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Approved && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyApplicationDateFilter(query, filters);
        
        // Apply placement status filter if specified
        if (!string.IsNullOrWhiteSpace(filters.PlacementStatus) && Enum.TryParse<PlacementStatusEnum>(filters.PlacementStatus, true, out var placementStatus))
        {
            query = query.Where(t => t.Placement != null && t.Placement.PlacementStatus == placementStatus);
        }
        
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var records = students.Select(t => new AdminReportMasterlistDto
        {
            Name = t.FullName,
            PlacementStatus = t.Placement?.PlacementStatus.ToString().Humanize(LetterCasing.Title),
            OfficeAssigned = t.Placement?.Office?.OfficeName
        }).ToList();

        return GenerateCsv(records);
    }

    public async Task<byte[]> GenerateOngoingPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && t.Placement.PlacementStatus == PlacementStatusEnum.Ongoing && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyPlacementDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        return GeneratePdfDocument("Ongoing List", students.Count, table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
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
    }

    public async Task<byte[]> GenerateOngoingCsv(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && t.Placement.PlacementStatus == PlacementStatusEnum.Ongoing && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyPlacementDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var records = students.Select(t => new AdminReportOngoingDto
        {
            Name = t.FullName,
            School = t.SchoolName,
            TotalInternshipHours = t.TotalInternshipHours,
            AccumulatedHours = t.Placement!.AccumulatedHours
        }).ToList();

        return GenerateCsv(records);
    }

    public async Task<byte[]> GenerateFinishedPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && t.Placement.PlacementStatus == PlacementStatusEnum.Finished && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyPlacementDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        return GeneratePdfDocument("Finished List", students.Count, table =>
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
    }

    public async Task<byte[]> GenerateFinishedCsv(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && t.Placement.PlacementStatus == PlacementStatusEnum.Finished && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyPlacementDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var records = students.Select(t => new AdminReportFinishedDto
        {
            Name = t.FullName,
            School = t.SchoolName
        }).ToList();

        return GenerateCsv(records);
    }

    public async Task<byte[]> GenerateRejectedPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Rejected && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyApplicationDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        return GeneratePdfDocument("Rejected Applications", students.Count, table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                header.Cell().Element(HeaderCell).Text("Name").Bold();
                header.Cell().Element(HeaderCell).Text("School").Bold();
                header.Cell().Element(HeaderCell).Text("Reason / Remarks").Bold();

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
                table.Cell().Element(DataCell).Text(s.Application!.Reason ?? "-").FontSize(9);
            }

            static IContainer DataCell(IContainer container) => container
                .Padding(0)
                .Border(1)
                .BorderColor(Colors.Black);
        });
    }

    public async Task<byte[]> GenerateRejectedCsv(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Rejected && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyApplicationDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var records = students.Select(t => new AdminReportRejectedDto
        {
            Name = t.FullName,
            School = t.SchoolName,
            Reason = t.Application!.Reason ?? string.Empty
        }).ToList();

        return GenerateCsv(records);
    }

    public async Task<byte[]> GenerateApprovedPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Approved && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyApplicationDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        return GeneratePdfDocument("Approved Applications", students.Count, table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                header.Cell().Element(HeaderCell).Text("Name").Bold();
                header.Cell().Element(HeaderCell).Text("School").Bold();
                header.Cell().Element(HeaderCell).Text("Office Assigned").Bold();

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
                table.Cell().Element(DataCell).Text(s.Placement?.Office?.OfficeName ?? "N/A").FontSize(9);
            }

            static IContainer DataCell(IContainer container) => container
                .Padding(0)
                .Border(1)
                .BorderColor(Colors.Black);
        });
    }

    public async Task<byte[]> GenerateApprovedCsv(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Approved && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyApplicationDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var records = students.Select(t => new AdminReportApprovedDto
        {
            Name = t.FullName,
            School = t.SchoolName,
            OfficeAssigned = t.Placement?.Office?.OfficeName
        }).ToList();

        return GenerateCsv(records);
    }

    public async Task<byte[]> GeneratePendingPdf(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Pending && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyApplicationDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        return GeneratePdfDocument("Pending Applications", students.Count, table =>
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
    }

    public async Task<byte[]> GeneratePendingCsv(CommonFilterOptions filters, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students
            .Include(t => t.Application)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Pending && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery();

        query = ApplyCommonStudentFilters(query, filters);
        query = ApplyApplicationDateFilter(query, filters);
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var students = await query.ToListAsync(ct);

        var records = students.Select(t => new AdminReportPendingDto
        {
            Name = t.FullName,
            School = t.SchoolName
        }).ToList();

        return GenerateCsv(records);
    }
}
