using System;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Offices.GetAllOffices;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.OjtPerOffice;

public class OjtPerOfficehandler(AppDbContext context) : IOjtPerOfficeService
{
    private readonly AppDbContext _context = context;
    public async Task<byte[]> ListAllOjtPerOffice(OfficeNameEnum office, CancellationToken ct)
    {
        // No need for switch - just use the enum directly
        var ojtOffice = await _context.Students
            .Include(t => t.Application)
            .Include(t => t.Office)
            .Include(t => t.Internship)
            .Where(t => t.Office.Name == office) // Direct enum comparison
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
        QuestPDF.Settings.License = LicenseType.Community; // or Evaluation
        var document = Document.Create(doc =>
    {
        doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);

            // Header
            page.Header().PaddingBottom(15).Column(col =>
            {
                col.Item().Text($"OJT Students - {OfficeEnumLabels.GetLabel(office)}")
                    .FontSize(20).Bold().AlignCenter();

                col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                    .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();

                col.Item().PaddingTop(3).Text($"Total Students: {ojtOffice.Count}")
                    .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
            });

            // Content
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(2.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f);
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).AlignCenter().Text("No").Bold();
                    header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Status").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Grade level").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Strand").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Degree").Bold();
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Internship hours").Bold();

                    static IContainer HeaderCell(IContainer container) => container
                        .DefaultTextStyle(x => x.FontSize(10))
                        .Padding(0)
                        .Border(1)
                        .BorderColor(Colors.Black);
                });

                // Data rows
                int index = 1;
                foreach (var ojt in ojtOffice)
                {
                    var fullname = $"{ojt.LastName}, {ojt.FirstName} {ojt.MiddleName}".Trim();
                    var status = ojt.Application?.Status;
                    var totalHours = ojt.Internship?.InternshipTotalHours ?? 0;
                    var gradeLevel = ojt.GradeLevel.ToString().Humanize(LetterCasing.Title);
                    var degree = ojt.Internship?.Degree?.ToString().Humanize(LetterCasing.Title) ?? "N/A";
                    var strand =  ojt.Internship?.Strand?.ToString().Humanize(LetterCasing.Title) ?? "N/A";

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(index++.ToString()).FontSize(9);

                    table.Cell().Element(DataCell)
                        .Text(fullname).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(status?.ToString() ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(gradeLevel.ToString() ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(strand ?? "N/A").FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                       .Text(degree).FontSize(9);

                    table.Cell().Element(DataCell).AlignCenter()
                        .Text(totalHours > 0 ? totalHours.ToString() : "-").FontSize(9);
                }

                static IContainer DataCell(IContainer container) => container
                    .Padding(0)
                    .Border(1)
                    .BorderColor(Colors.Black);
            });

            // Footer
            page.Footer().AlignCenter().PaddingTop(10).Text(text =>
            {
                text.Span("Page ").FontSize(9).FontColor(Colors.Grey.Darken1);
                text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                text.Span(" of ").FontSize(9).FontColor(Colors.Grey.Darken1);
                text.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    });

        return document.GeneratePdf(); // Returns byte[]
    }


    private static OfficeNameEnum GetOfficeSwitch(OfficeNameEnum office)
    {
        var selectedOffice = office switch
        {
            OfficeNameEnum.OfficeOfTheProvincialGovernor
                => OfficeNameEnum.OfficeOfTheProvincialGovernor,

            OfficeNameEnum.OfficeOfTheProvincialViceGovernor
                => OfficeNameEnum.OfficeOfTheProvincialViceGovernor,

            OfficeNameEnum.OfficeOfTheProvincialAdministrator
                => OfficeNameEnum.OfficeOfTheProvincialAdministrator,

            OfficeNameEnum.OpgRoadSafetyDivision
                => OfficeNameEnum.OpgRoadSafetyDivision,

            OfficeNameEnum.BidsAndAwardsCommitteeB
                => OfficeNameEnum.BidsAndAwardsCommitteeB,

            OfficeNameEnum.BidsAndAwardsCommitteeA
                => OfficeNameEnum.BidsAndAwardsCommitteeA,

            OfficeNameEnum.CaviteProvincialJail
                => OfficeNameEnum.CaviteProvincialJail,

            OfficeNameEnum.OpgOfficeOfTheProvincialYouthDevelopmentOfficer
                => OfficeNameEnum.OpgOfficeOfTheProvincialYouthDevelopmentOfficer,

            OfficeNameEnum.OfficeOfTheProvincialHealthOfficer
                => OfficeNameEnum.OfficeOfTheProvincialHealthOfficer,

            OfficeNameEnum.LocalEconomicDevelopmentAndInvestmentPromotionsOffice
                => OfficeNameEnum.LocalEconomicDevelopmentAndInvestmentPromotionsOffice,

            OfficeNameEnum.CaviteCenterForMentalHealth
                => OfficeNameEnum.CaviteCenterForMentalHealth,

            OfficeNameEnum.CaviteQualityManagementOffice
                => OfficeNameEnum.CaviteQualityManagementOffice,

            OfficeNameEnum.OfficeOfTheSangguniangPanlalawigan
                => OfficeNameEnum.OfficeOfTheSangguniangPanlalawigan,

            OfficeNameEnum.OpgOfficeOfTheProvincialInternalAuditServices
                => OfficeNameEnum.OpgOfficeOfTheProvincialInternalAuditServices,

            OfficeNameEnum.ProvincialInformationAndCommunicationsTechnologyOffice
                => OfficeNameEnum.ProvincialInformationAndCommunicationsTechnologyOffice,

            OfficeNameEnum.OfficeOfTheProvincialEnvironmentAndNaturalResourcesOfficer
                => OfficeNameEnum.OfficeOfTheProvincialEnvironmentAndNaturalResourcesOfficer,

            OfficeNameEnum.OfficeOfTheProvincialDisasterRiskReductionAndManagementOfficer
                => OfficeNameEnum.OfficeOfTheProvincialDisasterRiskReductionAndManagementOfficer,

            OfficeNameEnum.PgCaviteOfficeOfPublicSafety
                => OfficeNameEnum.PgCaviteOfficeOfPublicSafety,

            OfficeNameEnum.OfficeOfTheProvincialEngineer
                => OfficeNameEnum.OfficeOfTheProvincialEngineer,

            OfficeNameEnum.OfficeOfTheProvincialVeterinarian
                => OfficeNameEnum.OfficeOfTheProvincialVeterinarian,

            OfficeNameEnum.OfficeOfTheProvincialSocialWelfareAndDevelopmentOfficer
                => OfficeNameEnum.OfficeOfTheProvincialSocialWelfareAndDevelopmentOfficer,

            OfficeNameEnum.OfficeOfTheProvincialAgriculturist
                => OfficeNameEnum.OfficeOfTheProvincialAgriculturist,

            OfficeNameEnum.OfficeOfTheProvincialPopulationOfficer
                => OfficeNameEnum.OfficeOfTheProvincialPopulationOfficer,

            OfficeNameEnum.OfficeOfTheProvincialAssessor
                => OfficeNameEnum.OfficeOfTheProvincialAssessor,

            OfficeNameEnum.OfficeOfTheProvincialTreasurer
                => OfficeNameEnum.OfficeOfTheProvincialTreasurer,

            OfficeNameEnum.OfficeOfTheProvincialAccountant
                => OfficeNameEnum.OfficeOfTheProvincialAccountant,

            OfficeNameEnum.OfficeOfTheProvincialBudgetOfficer
                => OfficeNameEnum.OfficeOfTheProvincialBudgetOfficer,

            OfficeNameEnum.OfficeOfTheProvincialGeneralServicesOfficer
                => OfficeNameEnum.OfficeOfTheProvincialGeneralServicesOfficer,

            OfficeNameEnum.OfficeOfTheProvincialLegalOfficer
                => OfficeNameEnum.OfficeOfTheProvincialLegalOfficer,

            OfficeNameEnum.OpgOfficeOfTheProvincialPersonsWithDisabilityAffairsOfficer
                => OfficeNameEnum.OpgOfficeOfTheProvincialPersonsWithDisabilityAffairsOfficer,

            OfficeNameEnum.OfficeOfTheProvincialPlanningAndDevelopmentCoordinator
                => OfficeNameEnum.OfficeOfTheProvincialPlanningAndDevelopmentCoordinator,

            OfficeNameEnum.OfficeOfTheProvincialInformationOfficer
                => OfficeNameEnum.OfficeOfTheProvincialInformationOfficer,

            OfficeNameEnum.OfficeOfTheProvincialTourismOfficer
                => OfficeNameEnum.OfficeOfTheProvincialTourismOfficer,

            OfficeNameEnum.OfficeOfTheProvincialCooperativesDevelopmentOfficer
                => OfficeNameEnum.OfficeOfTheProvincialCooperativesDevelopmentOfficer,

            OfficeNameEnum.OfficeOfTheProvincialPublicEmploymentServiceManager
                => OfficeNameEnum.OfficeOfTheProvincialPublicEmploymentServiceManager,

            OfficeNameEnum.ProvincialHousingAndDevelopmentManagementOffice
                => OfficeNameEnum.ProvincialHousingAndDevelopmentManagementOffice,

            OfficeNameEnum.OfficeOfTheProvincialHumanResourceManagementOfficer
                => OfficeNameEnum.OfficeOfTheProvincialHumanResourceManagementOfficer,

            _ => office
        };


        return selectedOffice;
    }
}
