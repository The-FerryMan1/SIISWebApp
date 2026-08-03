using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.OfficeAccounts;

public class OfficeAccountHandler(AppDbContext context) : IOfficeAccountService
{
    private readonly AppDbContext _context = context;
    private readonly PasswordHasher<OfficeAccountModel> _hasher = new();

    public async Task<ICollection<GetOfficeAccountDto>> GetAllAsync(CancellationToken ct)
    {
        var accounts = await _context.OfficeAccounts
            .AsNoTracking()
            .OrderBy(a => a.OfficeId)
            .ToListAsync(ct);

        return [.. accounts.Select(a => new GetOfficeAccountDto
        {
            Id = a.Id,
            OfficeId = a.OfficeId,
            OfficeName = OfficeEnumLabels.GetLabel(
                _context.Offices.First(o => o.Id == a.OfficeId).Name),
            Username = a.Username,
            Email = a.Email,
            CreateAt = a.CreateAt,
            UpdatedAt = a.UpdatedAt,
        })];
    }

    public async Task<GetOfficeAccountDto> GetByIdAsync(long id, CancellationToken ct)
    {
        var account = await _context.OfficeAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException("Office account not found");

        return new GetOfficeAccountDto
        {
            Id = account.Id,
            OfficeId = account.OfficeId,
            OfficeName = OfficeEnumLabels.GetLabel(
                _context.Offices.First(o => o.Id == account.OfficeId).Name),
            Username = account.Username,
            Email = account.Email,
            CreateAt = account.CreateAt,
            UpdatedAt = account.UpdatedAt,
        };
    }

    public async Task CreateAsync(CreateOfficeAccountDto dto, CancellationToken ct)
    {
        var officeExists = await _context.Offices.AnyAsync(o => o.Id == dto.OfficeId, ct);
        if (!officeExists) throw new KeyNotFoundException("Office not found");

        var account = new OfficeAccountModel
        {
            OfficeId = dto.OfficeId,
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _hasher.HashPassword(null, dto.Password),
        };

        await _context.OfficeAccounts.AddAsync(account, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(long id, UpdateOfficeAccountDto dto, CancellationToken ct)
    {
        var account = await _context.OfficeAccounts.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException("Office account not found");

        account.Username = dto.Username;
        account.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            account.PasswordHash = _hasher.HashPassword(null, dto.Password);
        }

        account.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var account = await _context.OfficeAccounts.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException("Office account not found");

        account.IsDeleted = true;
        account.DeletedAt = DateTime.Now;
        await _context.SaveChangesAsync(ct);
    }
}