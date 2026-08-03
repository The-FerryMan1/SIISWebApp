using System;
using Microsoft.AspNetCore.Authorization;
using SIISMinimalAPI.Features.OfficeAccounts;

namespace SIISMinimalAPI.Features.OfficeAccounts;

public static class OfficeAccountEndpoint
{
    public static IEndpointRouteBuilder MapToOfficeAccount(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/office-accounts")
            .WithTags("OfficeAccounts")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapGet("/", [Authorize(Roles = "Admin")] async Task<IResult>(
            IOfficeAccountService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(ct);
            return TypedResults.Ok(result);
        }).RequireAuthorization("Admin");

        group.MapGet("/{id:long}", [Authorize(Roles = "Admin")] async Task<IResult>(
            long id, IOfficeAccountService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.GetByIdAsync(id, ct);
                return TypedResults.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        }).RequireAuthorization("Admin");

        group.MapPost("/", [Authorize(Roles = "Admin")] async Task<IResult>(
            CreateOfficeAccountDto dto, IOfficeAccountService service, CancellationToken ct) =>
        {
            try
            {
                await service.CreateAsync(dto, ct);
                return TypedResults.Created();
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:long}", [Authorize(Roles = "Admin")] async Task<IResult>(
            long id, UpdateOfficeAccountDto dto, IOfficeAccountService service, CancellationToken ct) =>
        {
            try
            {
                await service.UpdateAsync(id, dto, ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        }).RequireAuthorization("Admin");

        group.MapDelete("/{id:long}", [Authorize(Roles = "Admin")] async Task<IResult>(
            long id, IOfficeAccountService service, CancellationToken ct) =>
        {
            try
            {
                await service.DeleteAsync(id, ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        }).RequireAuthorization("Admin");

        return app;
    }
}