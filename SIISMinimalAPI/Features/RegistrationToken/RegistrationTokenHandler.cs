using System;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.RegistrationToken;

public class RegistrationTokenHandler(AppDbContext context) : IRegistrationTokenService
{
    private readonly AppDbContext _context = context;


    //registration token generation
    public async Task  CreateRegistrationToken(GenerateRegistrationTokenDto dto, CancellationToken ct)
    {
        var registrationToken = new RegistrationTokenModel
        {
            ExpDate = dto.ExpDate
        };

        await _context.AddAsync(registrationToken, ct);
        await _context.SaveChangesAsync(ct);
    }


    //delete registration token
    public async Task DeleteRegistrationToken(long id, CancellationToken ct)
    {
        var registrationToken = await _context.RegistrationTokens.FirstOrDefaultAsync(t => t.Id == id, ct)
        ?? throw new KeyNotFoundException("Registration Token not found");
        

        _context.Remove(registrationToken);
        await _context.SaveChangesAsync(ct);
    }

    //extend registration token
    public async Task ExtendRegistrationToken(long id, ExtendRegistrationTokenDto dot, CancellationToken ct)
    {
        var registrationToken = await _context.RegistrationTokens.FirstOrDefaultAsync(t => t.Id == id)
        ?? throw new KeyNotFoundException("Registration Token not found");

        registrationToken.ExpDate = dot.ExtendedDate;

        await _context.SaveChangesAsync(ct);
    }

    //get all registration token
    public async Task<ICollection<RegistrationTokenDto>>? GetAllRegistrationToken(CancellationToken ct)
    {
        var registrationTokens = await _context.RegistrationTokens.OrderByDescending(t => t.CreateAt).ToListAsync(ct);

        return [.. registrationTokens.Select(t => new RegistrationTokenDto
        {
             Id = t.Id,
             Uuid = t.Token,
             ExpDate = t.ExpDate,
             CreatedAt = t.CreateAt
        })];
    }

    // get one registration by id
    public async Task<RegistrationTokenDto>? GetByIdRegistrationToken(long id, CancellationToken ct)
    {
        var registrationToken = await _context.RegistrationTokens.FirstOrDefaultAsync(t => t.Id == id)
        ?? throw new KeyNotFoundException("Registration Token not found");

       return new RegistrationTokenDto
       {
         Id = registrationToken.Id,
         Uuid = registrationToken.Token,
         ExpDate = registrationToken.ExpDate,
         CreatedAt = registrationToken.CreateAt,  
       };
    }

    //verify token
    public async Task<bool> VerifyRegistrationToken(Guid guid, CancellationToken ct)
    {
        var token = await _context.RegistrationTokens.FirstOrDefaultAsync(t => t.Token == guid, ct);

        if(token is null)
        {
            return false;
        }

        if(token.ExpDate < DateTime.Now)
        {
            return false;
        }

        return true;
    }
}
