using System;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Internal;

namespace SIISMinimalAPI.Features.RegistrationToken;

public static class RegistrationTokenEndpoint
{
    public static IEndpointRouteBuilder MapToRegistrationEndpoint(this IEndpointRouteBuilder app)
    {

         var group = app.MapGroup("/api/registrationtoken/")
        .WithTags("Registration")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");
     


        group.MapPost("/", [Authorize] async Task<IResult> (GenerateRegistrationTokenDto dto,IRegistrationTokenService services, CancellationToken ct) =>
        {
            RegistrationTokenValidator validator = new();
            await validator.ValidateAndThrowAsync(dto, cancellationToken: ct);
            try
            {
                await services.CreateRegistrationToken(dto,ct);
                return TypedResults.Created();
            }
            catch (Exception ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        }).RequireAuthorization();

        group.MapGet("/", [Authorize] async Task<IResult> (IRegistrationTokenService services, CancellationToken ct) =>
        {
            try
            {
               var result = await services.GetAllRegistrationToken(ct);
                return TypedResults.Ok(result);
            }
            catch (Exception ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        }).RequireAuthorization();

          group.MapGet("/{id}", [Authorize] async Task<IResult> (long id, IRegistrationTokenService services, CancellationToken ct) =>
        {
            try
            {
               var result = await services.GetByIdRegistrationToken(id, ct);
                return TypedResults.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        }).RequireAuthorization();

          group.MapDelete("/{id}", [Authorize] async Task<IResult> (long id, IRegistrationTokenService services, CancellationToken ct) =>
        {
            try
            {
               await services.DeleteRegistrationToken(id, ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        }).RequireAuthorization();

        
          group.MapGet("/verify/{uuid}",  async Task<IResult> (Guid uuid, IRegistrationTokenService services, CancellationToken ct) =>
        {
           
                var result = await services.VerifyRegistrationToken(uuid, ct);

            if (result)
            {
                return TypedResults.Ok();
            }
            else{
                 return TypedResults.Forbid();
            }            
        });

          group.MapPut("/extend/{id}", [Authorize] async Task<IResult> (long id, ExtendRegistrationTokenDto dto, IRegistrationTokenService services, CancellationToken ct) =>
        {
            try
            {
               await services.ExtendRegistrationToken(id, dto, ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        }).RequireAuthorization();
        return app;
    }
}
