using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Report.InternshipHours;

namespace SIISMinimalAPI.Features.Report.InternshipHours;

public static class InternshipHoursEndpoint
{
    public static IEndpointRouteBuilder MapToInternshipHours(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/internship/hours")
            .WithTags("InternshipHours")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/", [Authorize] async Task<IResult>(CancellationToken ct, IInternshipHoursService service) =>
        {
            try
            {
                var result = await service.GetInternshipHours(ct);
                return TypedResults.File(result, "application/pdf", "internship-hours.pdf");
            }
            catch (System.Exception ex)
            {
                return TypedResults.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
