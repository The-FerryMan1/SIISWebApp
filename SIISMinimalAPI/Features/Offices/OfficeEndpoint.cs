using System;
using System.Reflection.Metadata;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Offices.UpdateOffice;

namespace SIISMinimalAPI.Features.Offices;

public static class OfficeEndpoint
{
    public static IEndpointRouteBuilder MapToOffice(this IEndpointRouteBuilder app)
    {   
        var group = app.MapGroup("/api/office")
        .WithTags("Office")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend")
        .RequireAuthorization();


       group.MapGet("/", [Authorize(Roles = "Admin")] async Task<IResult> (IOfficeService service, CancellationToken ct) =>
        {
            var applications =  await service.GetallOfficeAsync(ct);
            return TypedResults.Ok(applications);
        }).RequireAuthorization("Admin");

       group.MapGet("/my-office", [Authorize] async Task<IResult> (ClaimsPrincipal user, AppDbContext context, CancellationToken ct) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            var office = await context.Offices
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == userId && !o.IsDeleted, ct);

            if (office is null)
            {
                return TypedResults.NotFound("No office assigned to this account");
            }

            return TypedResults.Ok(new
            {
                id = office.Id,
                officeName = office.OfficeName,
                userId = office.UserId,
                department = office.Department
            });
        });


       group.MapPut("/{id}", async Task<IResult> ([FromRoute] long id, UpdateOfficeDto dto, IOfficeService service, CancellationToken ct) =>
       {
           try
           {
               await service.UpdateOfficeAsync(id, dto, ct);
               return TypedResults.Ok();
           }
           catch (KeyNotFoundException ex)
           {
               
               return TypedResults.BadRequest(ex.Message);
           }
           catch(Exception ex)
           {
               return TypedResults.InternalServerError(ex.Message);
           }

       });

       group.MapPut("/my-department", [Authorize] async Task<IResult> (ClaimsPrincipal user, [FromBody] UpdateOfficeDto dto, AppDbContext context, CancellationToken ct) =>
       {
           var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
           if (string.IsNullOrEmpty(userId))
           {
               return TypedResults.Unauthorized();
           }

           var office = await context.Offices.FirstOrDefaultAsync(o => o.UserId == userId && !o.IsDeleted, ct);
           if (office is null)
           {
               return TypedResults.NotFound("No office assigned to this account");
           }

           office.Department = dto.Department;
           office.UpdatedAt = DateTime.Now;
           context.Offices.Update(office);
           await context.SaveChangesAsync(ct);

           return TypedResults.Ok();
       });

        return app;
    }
}
