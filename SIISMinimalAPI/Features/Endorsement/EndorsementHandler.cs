using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Endorsement.Bulk;
using SIISMinimalAPI.Features.Endorsement.Create;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Endorsement;

public class EndorsementHandler(AppDbContext context, UserManager<User> userManager) : IEndorsementService
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    public async Task<Document?> GenerateEndorsement(Guid uuid, string currentUserId, CancellationToken ct)
    {

        var user = await _userManager.FindByIdAsync(currentUserId)
        ?? throw new KeyNotFoundException("No user found");


        var stud = await _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
            ?? throw new KeyNotFoundException("Application not found");

        QuestPDF.Settings.License = LicenseType.Community; // or Evaluation
        var basePath = Directory.GetCurrentDirectory();
        var imagePath = Path.Combine(basePath, "Features", "Endorsement", "Shared", "logo.png");
        var hasLogo = File.Exists(imagePath);
        var officeName = stud.Placement?.Office?.OfficeName ?? string.Empty;
        var department = !string.IsNullOrEmpty(officeName) ? stud.Placement!.Office!.Department ?? string.Empty : string.Empty;
        var docs = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);

                page.Header().Column(c =>
                {
                    if (hasLogo)
                    {
                        c.Item().AlignCenter().PaddingBottom(5).Width(50).Height(50).Image(imagePath);
                    }
                    c.Item().AlignCenter().Text("Republic of the Philippines").FontSize(12);

                    c.Item().AlignCenter().Text("Province of Cavite").FontSize(12);

                    c.Item().AlignCenter().Text("OFFICE OF THE PROVINCIAL GOVERNOR")
                        .FontSize(14).Bold();

                    c.Item().AlignCenter().Text("Trece Martires City").FontSize(12);
                });

                page.Content().Column(content =>
                {
                    content.Item().PaddingVertical(20);
                    // Date
                    content.Item().AlignLeft().Text(DateTime.Now.ToString("MMMM dd, yyyy")).FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Recipient block
                    content.Item().AlignLeft().Column(recipient =>
                    {
                        if (!string.IsNullOrEmpty(department))
                        {
                            recipient.Item().Text($"{department}").Bold().FontSize(12);
                        }
                        recipient.Item().Text($"{officeName}").FontSize(12);
                        recipient.Item().Text("Trece Martires City").FontSize(12);
                    });

                    content.Item().PaddingVertical(5);

                    // Salutation
                    content.Item().AlignLeft().Text($"Dear Sir/Madam").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Greetings
                    content.Item().AlignLeft().Text("Greetings").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Body
                    content.Item().AlignLeft().Text(text =>
                    {
                        text.Span("Respectfully endorsing the following student of the ").FontSize(12);
                        text.Span(stud.SchoolName ?? "their university").Bold().FontSize(12);
                        text.Span($", to conduct their on-the-job training ({stud.TotalInternshipHours} hours) in your office:").FontSize(12);
                    });

                    content.Item().PaddingVertical(10);

                    // Student name


                    content.Item().Text($"1. {stud.FullName}")
                        .FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Attachment note
                    content.Item().AlignLeft().Text("Attached is the resume of the student for your reference.").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Thank you
                    content.Item().AlignLeft().Text("Thank you very much.").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Closing
                    content.Item().AlignLeft().Text("Very truly yours,").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Staff name from User account
                    var staffName = $"{user.FirstName} {user.LastName}".Trim();
                    content.Item().AlignLeft().Text(staffName).FontSize(12).Bold();
                    content.Item().AlignLeft().Text("Executive Assistant IV").FontSize(12).SemiBold();

                    // Footer
                    // content.Item().PaddingTop(0).AlignCenter()
                    //     .Text("New Provincial Government Center, Trece Martires City, Cavite")
                    //     .FontSize(10).Italic();
                });
            });
        });

        return docs;
    }

    public async Task<Document?> MultiOjtEndorsement(EndorsementBulkDto dto, string currentUserId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(currentUserId)
            ?? throw new KeyNotFoundException("No user found");

        var students = await _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .AsSplitQuery()
            .AsNoTracking()
            .Where(t => dto.UUIDS.Contains(t.StudentUUID) && !t.IsDeleted && t.Application.Status == Shared.Enums.ApplicationStatusEnum.Approved)
            .ToListAsync(ct);

        if (!students.Any())
            throw new KeyNotFoundException("No approved students found for the selected IDs");

        var officeName = dto.Office ?? students.First().Placement?.Office?.OfficeName ?? string.Empty;
        var department = students.First().Placement?.Office?.Department ?? string.Empty;

        QuestPDF.Settings.License = LicenseType.Community;
        var basePath = Directory.GetCurrentDirectory();
        var imagePath = Path.Combine(basePath, "Features", "Endorsement", "Shared", "logo.png");
        var hasLogo = File.Exists(imagePath);

        var docs = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);

                page.Header().Column(c =>
                {
                    if (hasLogo)
                    {
                        c.Item().AlignCenter().PaddingBottom(5).Width(50).Height(50).Image(imagePath);
                    }
                    c.Item().AlignCenter().Text("Republic of the Philippines").FontSize(12);
                    c.Item().AlignCenter().Text("Province of Cavite").FontSize(12);
                    c.Item().AlignCenter().Text("OFFICE OF THE PROVINCIAL GOVERNOR")
                        .FontSize(14).Bold();
                    c.Item().AlignCenter().Text("Trece Martires City").FontSize(12);
                });

                page.Content().Column(content =>
                {
                    content.Item().PaddingVertical(20);
                    content.Item().AlignLeft().Text(DateTime.Now.ToString("MMMM dd, yyyy")).FontSize(12);
                    content.Item().PaddingVertical(5);

                    content.Item().AlignLeft().Column(recipient =>
                    {
                        if (!string.IsNullOrEmpty(department))
                        {
                            recipient.Item().Text($"{department}").Bold().FontSize(12);
                        }
                        recipient.Item().Text($"{officeName}").FontSize(12);
                        recipient.Item().Text("Trece Martires City").FontSize(12);
                    });

                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text($"Dear Sir/Madam").FontSize(12);
                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text("Greetings").FontSize(12);
                    content.Item().PaddingVertical(5);

                    content.Item().AlignLeft().Text(text =>
                    {
                        text.Span("Respectfully endorsing the following students of the ").FontSize(12);
                        text.Span(students.First().SchoolName ?? "their university").Bold().FontSize(12);
                        text.Span($", to conduct their on-the-job training in your office:").FontSize(12);
                    });

                    content.Item().PaddingVertical(10);

                    int index = 1;
                    foreach (var stud in students)
                    {
                        content.Item().Text($"{index}. {stud.FullName} - {stud.TotalInternshipHours} hours")
                            .FontSize(12);
                        index++;
                    }

                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text("Attached are the resumes of the students for your reference.").FontSize(12);
                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text("Thank you very much.").FontSize(12);
                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text("Very truly yours,").FontSize(12);
                    content.Item().PaddingVertical(5);
                    var staffName = $"{user.FirstName} {user.LastName}".Trim();
                    content.Item().AlignLeft().Text(staffName).FontSize(12).Bold();
                    content.Item().AlignLeft().Text("Executive Assistant IV").FontSize(12).SemiBold();
                });
            });
        });

        return docs;
    }

    public async Task<Document?> GenerateEndorsementBySchool(string schoolName, string? office, string currentUserId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(currentUserId)
            ?? throw new KeyNotFoundException("No user found");

        var query = _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .AsSplitQuery()
            .AsNoTracking()
            .Where(t => t.SchoolName == schoolName && !t.IsDeleted && t.Application.Status == Shared.Enums.ApplicationStatusEnum.Approved);

        if (!string.IsNullOrEmpty(office))
        {
            query = query.Where(t => t.Placement!.Office!.OfficeName == office);
        }

        var students = await query.ToListAsync(ct);

        if (!students.Any())
            throw new KeyNotFoundException("No approved students found for this school");

        QuestPDF.Settings.License = LicenseType.Community;
        var basePath = Directory.GetCurrentDirectory();
        var imagePath = Path.Combine(basePath, "Features", "Endorsement", "Shared", "logo.png");
        var hasLogo = File.Exists(imagePath);
         var officeName = students.First().Placement?.Office?.OfficeName ?? string.Empty;
         var department = !string.IsNullOrEmpty(officeName) ? students.First().Placement!.Office!.Department ?? string.Empty : string.Empty;

        var docs = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);

                page.Header().Column(c =>
                {
                    if (hasLogo)
                    {
                        c.Item().AlignCenter().PaddingBottom(5).Width(50).Height(50).Image(imagePath);
                    }
                    c.Item().AlignCenter().Text("Republic of the Philippines").FontSize(12);
                    c.Item().AlignCenter().Text("Province of Cavite").FontSize(12);
                    c.Item().AlignCenter().Text("OFFICE OF THE PROVINCIAL GOVERNOR")
                        .FontSize(14).Bold();
                    c.Item().AlignCenter().Text("Trece Martires City").FontSize(12);
                });

                page.Content().Column(content =>
                {
                    content.Item().PaddingVertical(20);
                    content.Item().AlignLeft().Text(DateTime.Now.ToString("MMMM dd, yyyy")).FontSize(12);
                    content.Item().PaddingVertical(5);

                    content.Item().AlignLeft().Column(recipient =>
                    {
                        if (!string.IsNullOrEmpty(department))
                        {
                            recipient.Item().Text($"{department}").Bold().FontSize(12);
                        }
                        recipient.Item().Text($"{officeName}").FontSize(12);
                        recipient.Item().Text("Trece Martires City").FontSize(12);
                    });

                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text($"Dear Sir/Madam").FontSize(12);
                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text("Greetings").FontSize(12);
                    content.Item().PaddingVertical(5);

                    content.Item().AlignLeft().Text(text =>
                    {
                        text.Span("Respectfully endorsing the following students of the ").FontSize(12);
                        text.Span(schoolName).Bold().FontSize(12);
                        text.Span($", to conduct their on-the-job training in your office:").FontSize(12);
                    });

                    content.Item().PaddingVertical(10);

                    int index = 1;
                    foreach (var stud in students)
                    {
                        content.Item().Text($"{index}. {stud.FullName} - {stud.TotalInternshipHours} hours")
                            .FontSize(12);
                        index++;
                    }

                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text("Attached are the resumes of the students for your reference.").FontSize(12);
                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text("Thank you very much.").FontSize(12);
                    content.Item().PaddingVertical(5);
                    content.Item().AlignLeft().Text("Very truly yours,").FontSize(12);
                    content.Item().PaddingVertical(5);
                    var staffName = $"{user.FirstName} {user.LastName}".Trim();
                    content.Item().AlignLeft().Text(staffName).FontSize(12).Bold();
                    content.Item().AlignLeft().Text("Executive Assistant IV").FontSize(12).SemiBold();
                });
            });
        });

        return docs;
    }
}
