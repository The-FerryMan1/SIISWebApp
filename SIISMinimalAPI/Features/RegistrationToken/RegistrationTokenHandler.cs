using System;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.RegistrationToken;

public class RegistrationTokenHandler(AppDbContext context) : IRegistrationTokenService
{
    private readonly AppDbContext _context = context;


    public async Task CreateRegistrationToken(GenerateRegistrationTokenDto dto, CancellationToken ct)
    {
        var registration = new Registration
        {
            Token = Guid.NewGuid().ToString(),
            ExpiryDate = dto.ExpDate
        };

        await _context.Registrations.AddAsync(registration, ct);
        await _context.SaveChangesAsync(ct);
    }


    public async Task DeleteRegistrationToken(long id, CancellationToken ct)
    {
        var registration = await _context.Registrations.FirstOrDefaultAsync(t => t.Id == id, ct)
        ?? throw new KeyNotFoundException("Registration Token not found");
        
        _context.Registrations.Remove(registration);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ExtendRegistrationToken(long id, ExtendRegistrationTokenDto dto, CancellationToken ct)
    {
        var registration = await _context.Registrations.FirstOrDefaultAsync(t => t.Id == id, ct)
        ?? throw new KeyNotFoundException("Registration Token not found");

        registration.ExpiryDate = dto.ExtendedDate;

        await _context.SaveChangesAsync(ct);
    }

    public async Task<ICollection<RegistrationTokenDto>> GetAllRegistrationToken(CancellationToken ct)
    {
        var registrations = await _context.Registrations.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

        return [.. registrations.Select(t => new RegistrationTokenDto
        {
             Id = t.Id,
             Uuid = Guid.Parse(t.Token),
             ExpDate = t.ExpiryDate,
             CreatedAt = t.CreatedAt
        })];
    }

    public async Task<RegistrationTokenDto> GetByIdRegistrationToken(long id, CancellationToken ct)
    {
        var registration = await _context.Registrations.FirstOrDefaultAsync(t => t.Id == id, ct)
        ?? throw new KeyNotFoundException("Registration Token not found");

       return new RegistrationTokenDto
       {
         Id = registration.Id,
         Uuid = Guid.Parse(registration.Token),
         ExpDate = registration.ExpiryDate,
         CreatedAt = registration.CreatedAt,  
       };
    }

    public async Task<bool> VerifyRegistrationToken(Guid guid, CancellationToken ct)
    {
        var tokenString = guid.ToString();
        var registration = await _context.Registrations.FirstOrDefaultAsync(t => t.Token == tokenString && !t.IsUsed, ct);

        if(registration is null)
        {
            return false;
        }

        if(registration.ExpiryDate < DateTime.Now)
        {
            return false;
        }

        return true;
    }
}