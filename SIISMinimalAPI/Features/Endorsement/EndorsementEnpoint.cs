using System;
using System.Reflection.Metadata;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Features.Endorsement.Bulk;
using SIISMinimalAPI.Features.Endorsement.Create;

namespace SIISMinimalAPI.Features.Endorsement;

public static class EndorsementEnpoint
{
    public static IEndpointRouteBuilder MapToEndorsement(this IEndpointRouteBuilder app)
    {
         var group = app.MapGroup("/api/endorsement")
        .WithTags("Endorsement")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");

        group.MapGet("/{uuid}", [Authorize]  async Task<IResult>(Guid uuid, ClaimsPrincipal user,  CancellationToken ct, IEndorsementService service) =>
        {   
            

            try
            {

                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await service.GenerateEndorsement(uuid,userId, ct);
                return TypedResults.File(result.GeneratePdf(), "application/pdf", $"endorsement-{DateTime.Now}-{uuid}");
            }
            catch (KeyNotFoundException ex)
            {
                
                return TypedResults.NotFound(ex.Message);
            }catch(Exception ex)
            {
                return TypedResults.InternalServerError(ex.Message);
            }
        });

        group.MapGet("/school/{schoolName}", [Authorize] async Task<IResult>(string schoolName, [FromQuery] string? office, ClaimsPrincipal user, CancellationToken ct, IEndorsementService service) =>
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await service.GenerateEndorsementBySchool(schoolName, office, userId!, ct);
                return TypedResults.File(result.GeneratePdf(), "application/pdf", $"endorsement-{schoolName}-{DateTime.Now:yyyyMMdd}.pdf");
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

        group.MapPost("/", [Authorize] async Task<IResult>([FromBody] EndorsementBulkDto dto, ClaimsPrincipal user, CancellationToken ct, IEndorsementService service) =>
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await service.MultiOjtEndorsement(dto, userId!, ct);
                return TypedResults.File(result!.GeneratePdf(), "application/pdf", $"endorsement-bulk-{DateTime.Now:yyyyMMdd}.pdf");
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

        return app;
    }
}
