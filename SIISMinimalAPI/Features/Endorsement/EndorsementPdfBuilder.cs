using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Endorsement;

public class EndorsementPdfBuilder
{
    private readonly EndorsementSettings _settings;
    private readonly User _currentUser;
    private readonly string _basePath;

    public EndorsementPdfBuilder(EndorsementSettings settings, User currentUser, string basePath)
    {
        _settings = settings;
        _currentUser = currentUser;
        _basePath = basePath;
    }

    public Document BuildEndorsement(string recipientDepartment, string recipientName, string schoolName, List<Student> students)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Check if we need pagination
        if (students.Count <= _settings.MaxStudentsPerPage)
        {
            return BuildSinglePage(recipientDepartment, recipientName, schoolName, students, null);
        }

        // Create multi-page document with pagination
        return BuildMultiPage(recipientDepartment, recipientName, schoolName, students);
    }

    private Document BuildSinglePage(string recipientDepartment, string recipientName, string schoolName, List<Student> students, int? pageNumber)
    {
        var isSingleStudent = students.Count == 1;
        var logoPath = ResolveLogoPath();
        var hasLogo = !string.IsNullOrEmpty(logoPath) && File.Exists(logoPath);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);

                page.Header().Column(c => BuildHeader(c, hasLogo, logoPath));

                page.Content().Column(content =>
                {
                    BuildContent(content, recipientDepartment, recipientName, schoolName, students, isSingleStudent);
                    BuildStudentList(content, students);
                    BuildFooter(content);
                });

                if (pageNumber.HasValue)
                {
                    page.Footer().AlignCenter().Text($"Page {pageNumber}").FontSize(10).Italic().FontColor(Colors.Grey.Darken2);
                }
            });
        });

        return document;
    }

    private Document BuildMultiPage(string recipientDepartment, string recipientName, string schoolName, List<Student> students)
    {
        var isSingleStudent = false;
        var logoPath = ResolveLogoPath();
        var hasLogo = !string.IsNullOrEmpty(logoPath) && File.Exists(logoPath);
        var pages = (int)Math.Ceiling((double)students.Count / _settings.MaxStudentsPerPage);

        var document = Document.Create(container =>
        {
            for (int pageIdx = 0; pageIdx < pages; pageIdx++)
            {
                var pageStudents = students
                    .Skip(pageIdx * _settings.MaxStudentsPerPage)
                    .Take(_settings.MaxStudentsPerPage)
                    .ToList();

                var isFirstPage = pageIdx == 0;
                var isLastPage = pageIdx == pages - 1;

                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);

                    page.Header().Column(c => BuildHeader(c, hasLogo, logoPath));

                    page.Content().Column(content =>
                    {
                        if (isFirstPage)
                        {
                            BuildContent(content, recipientDepartment, recipientName, schoolName, pageStudents, isSingleStudent);
                        }
                        else
                        {
                            content.Item().PaddingVertical(10);
                            content.Item().AlignLeft().Text($"Continuation - Page {pageIdx + 1}").FontSize(11).Bold().Italic();
                            content.Item().PaddingVertical(5);
                            content.Item().AlignLeft().Text("Students:").FontSize(11).Bold();
                            content.Item().PaddingVertical(5);
                        }

                        BuildStudentList(content, pageStudents, isFirstPage ? 1 : pageStudents.First().GetHashCode() % 100 + 1);

                        if (isLastPage)
                        {
                            BuildFooter(content);
                        }
                    });

                    page.Footer().AlignCenter().Text($"Page {pageIdx + 1} of {pages}").FontSize(10).Italic().FontColor(Colors.Grey.Darken2);
                });
            }
        });

        return document;
    }

    private void BuildHeader(ColumnDescriptor c, bool hasLogo, string? logoPath)
    {
        if (hasLogo && !string.IsNullOrEmpty(logoPath))
        {
            c.Item().AlignCenter().PaddingBottom(5).Width(50).Height(50).Image(logoPath);
        }

        c.Item().AlignCenter().Text(_settings.Header.Country).FontSize(12);
        c.Item().AlignCenter().Text(_settings.Header.Province).FontSize(12);
        c.Item().AlignCenter().Text(_settings.Header.OfficeTitle).FontSize(14).Bold();
        c.Item().AlignCenter().Text(_settings.Header.City).FontSize(12);
    }

    private string? ResolveLogoPath()
    {
        var configuredPath = _settings.Header.LogoPath;
        if (string.IsNullOrEmpty(configuredPath))
        {
            return Path.Combine(_basePath, "Features", "Endorsement", "Shared", "logo.png");
        }

        if (Path.IsPathFullyQualified(configuredPath))
        {
            return configuredPath;
        }

        if (configuredPath.StartsWith("/") || configuredPath.StartsWith("\\"))
        {
            return Path.Combine(_basePath, "wwwroot", configuredPath.TrimStart('/', '\\'));
        }

        return Path.Combine(_basePath, configuredPath);
    }

    private void BuildContent(ColumnDescriptor content, string recipientDepartment, string recipientName, string schoolName, List<Student> students, bool isSingleStudent)
    {
        content.Item().PaddingVertical(20);
        content.Item().AlignLeft().Text(DateTime.Now.ToString("MMMM dd, yyyy")).FontSize(12);
        content.Item().PaddingVertical(5);

        // Recipient block
        content.Item().AlignLeft().Column(recipient =>
        {
            if (!string.IsNullOrEmpty(recipientDepartment))
            {
                recipient.Item().Text($"{recipientDepartment}").Bold().FontSize(12);
            }
            recipient.Item().Text($"{recipientName}").FontSize(12);
            recipient.Item().Text(_settings.Header.City).FontSize(12);
        });

        content.Item().PaddingVertical(5);
        content.Item().AlignLeft().Text(_settings.Body.Salutation).FontSize(12);
        content.Item().PaddingVertical(5);
        content.Item().AlignLeft().Text(_settings.Body.Greeting).FontSize(12);
        content.Item().PaddingVertical(5);

        var studentWord = isSingleStudent ? "student" : "students";
        var hoursText = isSingleStudent ? $" ({students.First().TotalInternshipHours} hours)" : string.Empty;
        var intro = _settings.Body.IntroTemplate
            .Replace("{students}", studentWord, StringComparison.OrdinalIgnoreCase)
            .Replace("{school}", schoolName, StringComparison.OrdinalIgnoreCase)
            .Replace("{hours}", hoursText, StringComparison.OrdinalIgnoreCase);

        content.Item().AlignLeft().Text(text =>
        {
            text.Span(intro).FontSize(12);
        });

        content.Item().PaddingVertical(10);
    }

    private void BuildStudentList(ColumnDescriptor content, List<Student> students, int startIndex = 1)
    {
        int index = startIndex;
        foreach (var student in students)
        {
            var hours = string.IsNullOrEmpty(student.TotalInternshipHours.ToString()) ? "" : $" - {student.TotalInternshipHours} hours";
            content.Item().Text($"{index}. {student.FullName}{hours}").FontSize(12);
            index++;
        }
    }

    private void BuildFooter(ColumnDescriptor content)
    {
        content.Item().PaddingVertical(5);
        content.Item().AlignLeft().Text(_settings.Body.AttachmentNote).FontSize(12);
        content.Item().PaddingVertical(5);
        content.Item().AlignLeft().Text(_settings.Body.ThankYou).FontSize(12);
        content.Item().PaddingVertical(5);
        content.Item().AlignLeft().Text(_settings.Footer.Closing).FontSize(12);
        content.Item().PaddingVertical(5);

        var staffName = $"{_currentUser.FirstName} {_currentUser.LastName}".Trim();
        content.Item().AlignLeft().Text(staffName).FontSize(12).Bold();
        content.Item().AlignLeft().Text(_settings.Footer.SigningOfficerTitle).FontSize(12).SemiBold();

        if (!string.IsNullOrEmpty(_settings.Footer.FooterAddress))
        {
            content.Item().PaddingTop(10).AlignCenter()
                .Text(_settings.Footer.FooterAddress)
                .FontSize(10).Italic();
        }
    }
}
