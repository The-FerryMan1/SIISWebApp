using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SIISMinimalAPI.Features.Endorsement;

public static class EndorsementSettingsEndpoint
{
    public static IEndpointRouteBuilder MapToEndorsementSettings(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/endorsement-settings")
            .WithTags("EndorsementSettings")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/", GetEndorsementSettings)
            .WithName("GetEndorsementSettings")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPut("/", UpdateEndorsementSettings)
            .WithName("UpdateEndorsementSettings")
            .WithOpenApi()
            .RequireAuthorization();

        return app;
    }

    [Authorize(Roles = "Admin")]
    private static IResult GetEndorsementSettings(IOptions<EndorsementSettings> settings)
    {
        return TypedResults.Ok(new { settings = settings.Value });
    }

    [Authorize(Roles = "Admin")]
    private static IResult UpdateEndorsementSettings(EndorsementSettingsUpdateRequest request, IConfiguration configuration)
    {
        try
        {
            // Update in-memory configuration
            var section = configuration.GetSection("EndorsementSettings");
            
            // Update Header
            if (request.Header != null)
            {
                if (request.Header.Country != null)
                    section["Header:Country"] = request.Header.Country;
                if (request.Header.Province != null)
                    section["Header:Province"] = request.Header.Province;
                if (request.Header.OfficeTitle != null)
                    section["Header:OfficeTitle"] = request.Header.OfficeTitle;
                if (request.Header.City != null)
                    section["Header:City"] = request.Header.City;
                if (request.Header.LogoPath != null)
                    section["Header:LogoPath"] = request.Header.LogoPath;
            }

            // Update Footer
            if (request.Footer != null)
            {
                if (request.Footer.SigningOfficerTitle != null)
                    section["Footer:SigningOfficerTitle"] = request.Footer.SigningOfficerTitle;
                if (request.Footer.Closing != null)
                    section["Footer:Closing"] = request.Footer.Closing;
                if (request.Footer.FooterAddress != null)
                    section["Footer:FooterAddress"] = request.Footer.FooterAddress;
            }

            // Update Body
            if (request.Body != null)
            {
                if (request.Body.Salutation != null)
                    section["Body:Salutation"] = request.Body.Salutation;
                if (request.Body.Greeting != null)
                    section["Body:Greeting"] = request.Body.Greeting;
                if (request.Body.IntroTemplate != null)
                    section["Body:IntroTemplate"] = request.Body.IntroTemplate;
                if (request.Body.AttachmentNote != null)
                    section["Body:AttachmentNote"] = request.Body.AttachmentNote;
                if (request.Body.ThankYou != null)
                    section["Body:ThankYou"] = request.Body.ThankYou;
            }

            // Update MaxStudentsPerPage
            if (request.MaxStudentsPerPage.HasValue)
            {
                section["MaxStudentsPerPage"] = request.MaxStudentsPerPage.ToString()!;
            }

            return TypedResults.Ok(new { message = "Settings updated successfully" });
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(title: "Failed to update settings", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

public class EndorsementSettingsUpdateRequest
{
    public EndorsementHeaderUpdateRequest? Header { get; set; }
    public EndorsementBodyUpdateRequest? Body { get; set; }
    public EndorsementFooterUpdateRequest? Footer { get; set; }
    public int? MaxStudentsPerPage { get; set; }
}

public class EndorsementHeaderUpdateRequest
{
    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? OfficeTitle { get; set; }
    public string? City { get; set; }
    public string? LogoPath { get; set; }
}

public class EndorsementFooterUpdateRequest
{
    public string? SigningOfficerTitle { get; set; }
    public string? Closing { get; set; }
    public string? FooterAddress { get; set; }
}

public class EndorsementBodyUpdateRequest
{
    public string? Salutation { get; set; }
    public string? Greeting { get; set; }
    public string? IntroTemplate { get; set; }
    public string? AttachmentNote { get; set; }
    public string? ThankYou { get; set; }
}
