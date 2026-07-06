using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Endorsement.Bulk;
using SIISMinimalAPI.Features.Endorsement.Create;

namespace SIISMinimalAPI.Features.Endorsement;

public class EndorsementHandler(AppDbContext context) : IEndorsementService
{
    private readonly AppDbContext _context = context;

    public async Task<Document?> GenerateEndorsement(Guid uuid, CancellationToken ct)
    {
        var stud = await _context.Students
            .Include(t => t.School)
            .Include(t => t.Internship)
            .Include(t => t.Application)
            .Include(t => t.Office)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>t.Application.ApplicationUUID == uuid, ct)
            ?? throw new KeyNotFoundException("Application not found");

        // Guard against null Office
        if (stud.Office == null)
            throw new InvalidOperationException("Student has no office assigned.");

        QuestPDF.Settings.License = LicenseType.Community; // or Evaluation
        var basePath = Directory.GetCurrentDirectory(); // or Directory.GetCurrentDirectory()
        var imagePath = Path.Combine(basePath, "Features", "Endorsement", "Shared", "logo.png");
        var docs = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);

                page.Header().Column(c =>
                {
                    // Header
                    c.Item().AlignCenter().PaddingBottom(5).Width(50).Height(50).Image(imagePath);
                    c.Item().AlignCenter().Text("Republic of the Philippines").FontSize(12);

                    c.Item().AlignCenter().Text("Province of Cavite").FontSize(12);

                    c.Item().AlignCenter().Text("OFFICE OF THE PROVINCIAL GOVERNOR")
                        .FontSize(14).Bold();

                    c.Item().AlignCenter().Text("Trece Martires City").FontSize(12);
                });
                
                page.Content().Column(content =>
                {
                    content.Item().PaddingVertical(20);
                    // Date
                    content.Item().AlignLeft().Text(DateTime.Now.ToString("MMMM dd, yyyy")).FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Recipient block
                    content.Item().AlignLeft().Column(recipient =>
                    {
                        recipient.Item().Text(stud.Office.CurrentOIC ?? "The Officer in Charge").Bold().FontSize(12);
                        recipient.Item().Text($"OIC, {stud.Office.Name}").FontSize(12);
                        recipient.Item().Text("Trece Martires City").FontSize(12);
                    });

                    content.Item().PaddingVertical(5);

                    // Salutation
                    content.Item().AlignLeft().Text($"Dear {stud.Office.CurrentOIC ?? "Sir/Madam"}").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Greetings
                    content.Item().AlignLeft().Text("Greetings").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Body
                    content.Item().AlignLeft().Text(text =>
                    {
                        text.Span("Respectfully endorsing the following student of the ").FontSize(12);
                        text.Span(stud.School?.Name ?? "the university").Bold().FontSize(12); // Use School.Name not Address
                        text.Span($", to conduct his/her on-the-job training ({stud.Internship?.InternshipTotalHours ?? 486} hours) in your office:").FontSize(12);
                    });

                    content.Item().PaddingVertical(5);

                    // Student name

                
                    content.Item().Text($"1. {stud.LastName}, {stud.FirstName} {stud.MiddleName}")
                        .FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Attachment note
                    content.Item().AlignLeft().Text("Attached is the resume of the student for your reference.").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Thank you
                    content.Item().AlignLeft().Text("Thank you very much.").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Closing
                    content.Item().AlignLeft().Text("Very truly yours,").FontSize(12);

                    content.Item().PaddingVertical(5);

                    // Staff name from DTO or default
                    content.Item().AlignLeft().Text(stud.Office.CurrentOIC ?? "Staff Name").FontSize(12).Bold();
                     content.Item().AlignLeft().Text("Executive Assistant IV").FontSize(12).SemiBold();

                    // Footer
                    // content.Item().PaddingTop(0).AlignCenter()
                    //     .Text("New Provincial Government Center, Trece Martires City, Cavite")
                    //     .FontSize(10).Italic();
                });
            });
        });

        return docs;
    }

    public Task<Document?> MultiOjtEndorsement(EndorsementBulkDto dto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}