
using FluentValidation;


namespace SIISMinimalAPI.Features.OnBoarding;

public static class OnBoardingEndpoint
{
    public static IEndpointRouteBuilder MapOnBoardingEnpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/onboading")
        .WithTags("OnBoarding")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");

        group.MapPost("/", async Task<IResult> (OnBoardingDto dto, CancellationToken ct, IOnBoadringService service) =>
        {

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
         .AllowAnonymous();

        group.MapPut("/details/{uuid}", async Task<IResult> (Guid uuid, OnBoardUpdateDto dto, CancellationToken ct, IOnBoadringService service) =>
     {

         StudentUpdateDtoValidator studentvalidator = new();
         SchoolUpdateDtoValidator schoolValidator = new();
         InternshipUpdateDtoValidator internshipValidator = new();
         RequirementsUpdateDtoValidator requirementsValidator = new();
         OfficeUpdateDtoValidator officeValidator = new();
         OnBoardUpdateDtoValidator validationRules = new(studentvalidator, schoolValidator, internshipValidator,requirementsValidator, officeValidator);




         var validationResult = await validationRules.ValidateAsync(dto, ct);

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
            //  await validationRules.ValidateAndThrowAsync(dto);
             await service.UpdatedOnBoarding(uuid, dto, ct);
             return Results.Ok();
         }
         catch (Exception ex)
         {
             return Results.BadRequest(ex.Message);
         }
        ;
     }
     )
      .WithName("UpdateOnBoarding")
      .ProducesValidationProblem()
      .RequireAuthorization();

        return app;
    }
}
