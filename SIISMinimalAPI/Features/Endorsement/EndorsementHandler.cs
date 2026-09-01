using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Endorsement.Bulk;
using SIISMinimalAPI.Features.Endorsement.Create;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Endorsement;

public class EndorsementHandler(AppDbContext context, UserManager<User> userManager, IOptions<EndorsementSettings> endorsementSettings, IWebHostEnvironment env) : IEndorsementService
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly EndorsementSettings _settings = endorsementSettings.Value;
    private readonly string _basePath = env.ContentRootPath;
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

        var officeName = stud.Placement?.Office?.OfficeName ?? string.Empty;
        var department = !string.IsNullOrEmpty(officeName) ? stud.Placement!.Office!.Department ?? string.Empty : string.Empty;

        var builder = new EndorsementPdfBuilder(_settings, user, _basePath);
        return builder.BuildEndorsement(department, officeName, stud.SchoolName ?? "their university", new List<Student> { stud });
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
        var schoolName = students.First().SchoolName ?? "their university";

        var builder = new EndorsementPdfBuilder(_settings, user, _basePath);
        return builder.BuildEndorsement(department, officeName, schoolName, students);
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

        var officeName = students.First().Placement?.Office?.OfficeName ?? string.Empty;
        var department = !string.IsNullOrEmpty(officeName) ? students.First().Placement!.Office!.Department ?? string.Empty : string.Empty;

        var builder = new EndorsementPdfBuilder(_settings, user, _basePath);
        return builder.BuildEndorsement(department, officeName, schoolName, students);
    }
}
