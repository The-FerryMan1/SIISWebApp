namespace SIISMinimalAPI.Features.Endorsement;

public class EndorsementSettings
{
    /// <summary>
    /// Header section of the endorsement letter
    /// </summary>
    public EndorsementHeader Header { get; set; } = new();

    /// <summary>
    /// Body section of the endorsement letter
    /// </summary>
    public EndorsementBody Body { get; set; } = new();

    /// <summary>
    /// Footer/Closing section of the endorsement letter
    /// </summary>
    public EndorsementFooter Footer { get; set; } = new();

    /// <summary>
    /// Maximum students per page before pagination
    /// </summary>
    public int MaxStudentsPerPage { get; set; } = 10;
}

public class EndorsementHeader
{
    /// <summary>
    /// Country name
    /// </summary>
    public string Country { get; set; } = "Republic of the Philippines";

    /// <summary>
    /// Province name
    /// </summary>
    public string Province { get; set; } = "Province of Cavite";

    /// <summary>
    /// Office name/title
    /// </summary>
    public string OfficeTitle { get; set; } = "OFFICE OF THE PROVINCIAL GOVERNOR";

    /// <summary>
    /// City/Location name
    /// </summary>
    public string City { get; set; } = "Trece Martires City";

    /// <summary>
    /// Path to logo image (relative to wwwroot or absolute)
    /// </summary>
    public string? LogoPath { get; set; }
}

public class EndorsementFooter
{
    /// <summary>
    /// Job title of the signing officer
    /// </summary>
    public string SigningOfficerTitle { get; set; } = "Executive Assistant IV";

    /// <summary>
    /// Closing salutation
    /// </summary>
    public string Closing { get; set; } = "Very truly yours,";

    /// <summary>
    /// Address for footer
    /// </summary>
    public string? FooterAddress { get; set; }
}

public class EndorsementBody
{
    /// <summary>
    /// Salutation line
    /// </summary>
    public string Salutation { get; set; } = "Dear Sir/Madam,";

    /// <summary>
    /// Greeting line
    /// </summary>
    public string Greeting { get; set; } = "Greetings,";

    /// <summary>
    /// Intro sentence before student list. Use {school} for school name and {hours} for hours.
    /// </summary>
    public string IntroTemplate { get; set; } = "Respectfully endorsing the following {students} of the {school}, to conduct their on-the-job training{hours} in your office:";

    /// <summary>
    /// Attachment note
    /// </summary>
    public string AttachmentNote { get; set; } = "Attached are the resume(s) of the student(s) for your reference.";

    /// <summary>
    /// Thank you line
    /// </summary>
    public string ThankYou { get; set; } = "Thank you very much.";
}

public class EndorsementDto
{
    public EndorsementSettings Settings { get; set; } = new();
}
