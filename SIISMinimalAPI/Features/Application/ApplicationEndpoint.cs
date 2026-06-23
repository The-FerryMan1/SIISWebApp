using System;
using System.Reflection.Metadata.Ecma335;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Application.AssignAndApprove;

namespace SIISMinimalAPI.Features.Application;

public static class  ApplicationEndpoint
{
    public static IEndpointRouteBuilder MapToApplication(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/application")
        .WithTags("Application")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");

        group.MapGet("/", async Task<IResult> (IApplicationService service, CancellationToken ct) =>
        {
            var applications =  await service.GetAllAsync(ct);
           return TypedResults.Ok(applications);
        });

        group.MapGet("/{uuid}", async Task<IResult> (Guid uuid, IApplicationService service, CancellationToken ct) =>
        {   

           

            try
            {
                var result = await service.GetByIdAsync(uuid, ct);
                return TypedResults.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                
                return TypedResults.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return TypedResults.InternalServerError(ex.Message);
            }
        });

        group.MapPost("/details/{uuid}", async Task<IResult> (Guid uuid, RequestDto requestDto,  IApplicationService service, CancellationToken ct) =>
        {

            Validator validationRules = new();
            ValidationResult valiResult = validationRules.Validate(requestDto);

              if (!valiResult.IsValid)
            {
           
                var errors = valiResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                return TypedResults.ValidationProblem(errors);
            }


            try
            {
                await service.AssignAndApprove(uuid, requestDto, ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                
                return TypedResults.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return TypedResults.InternalServerError(ex.Message);
            }
        });

        group.MapPut("/trash/{uuid}", async Task<IResult>([FromRoute]Guid uuid, IApplicationService service , CancellationToken ct) =>
        {
            try
            {
                await service.Trash(uuid, ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                
                 return TypedResults.NotFound(ex.Message);
            }
        });

        return app;
    }
}
