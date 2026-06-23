using System;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using SIISMinimalAPI.Features.Endorsement.Create;

namespace SIISMinimalAPI.Features.Endorsement;

public static class EndorsementEnpoint
{
    public static IEndpointRouteBuilder MapToEndorsement(this IEndpointRouteBuilder app)
    {
         var group = app.MapGroup("/endorsement")
        .WithTags("Endorsement")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");

        group.MapGet("/{uuid}",  async Task<IResult>([FromRoute]Guid uuid, CancellationToken ct, IEndorsementService service) =>
        {   
            try
            {
                var result = await service.GenerateEndorsement(uuid, ct);
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
        return app;
    }
}
