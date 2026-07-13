
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;


namespace SIISMinimalAPI.Features.OnBoarding;

public static class OnBoardingEndpoint
{
    public static IEndpointRouteBuilder MapOnBoardingEnpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/onboading")
        .WithTags("OnBoarding")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");

        group.MapPost("/", async Task<IResult> ([FromForm] OnBoardingDto dto, CancellationToken ct, IOnBoadringService service) =>
        {
            if (dto.Student is null || dto.School is null || dto.Internship is null)
            {
                return Results.BadRequest("Student, school, and internship details are required.");
            }

            StudentRegDtoValidator studentvalidator = new();
            SchoolRegDtoValidator schoolValidator = new();
            InternshipRegDtoValidator internshipValidator = new();
            RequirementsRegDtoValidator requirementsValidator = new();
            OnBoardingDtoValidator validationRules = new(studentvalidator, schoolValidator, internshipValidator, requirementsValidator);



            // ✅ Use ValidateAsync
            var validationResult = await validationRules.ValidateAsync(dto, ct);

            if (!validationResult.IsValid)
            {
                // ✅ Return proper ValidationProblem, not just log
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                return TypedResults.ValidationProblem(errors);
            }

            try
            {
                await validationRules.ValidateAndThrowAsync(dto);
                await service.CreateOnBoarding(dto, ct);
                return Results.Created();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
           ;
        }
        )
         .WithName("CreateOnBoarding")
         .ProducesValidationProblem()
         .DisableAntiforgery()
         .AllowAnonymous();

        group.MapPut("/details/{uuid}", async Task<IResult> (
    Guid uuid,
    [FromForm] OnBoardUpdateDto dto,
    CancellationToken ct,
    IOnBoadringService service) =>
{
    var validator = new OnBoardUpdateDtoValidator();
    var validationResult = await validator.ValidateAsync(dto, ct);

    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return TypedResults.ValidationProblem(errors);
    }

    try
    {
        await service.UpdatedOnBoarding(uuid, dto, ct);
        return Results.Ok();
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound("Application not found");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
})
.WithName("UpdateOnBoarding")
.ProducesValidationProblem()
 .DisableAntiforgery()
.RequireAuthorization();

        return app;
    }
}
