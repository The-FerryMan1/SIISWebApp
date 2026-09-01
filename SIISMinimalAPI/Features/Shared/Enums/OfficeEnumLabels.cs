using System;

namespace SIISMinimalAPI.Features.Shared.Enums;

public static class OfficeEnumLabels
{
    public static readonly Dictionary<OfficeNameEnum, string> Labels = new()
    {
        [OfficeNameEnum.OfficeOfTheProvincialGovernor] = "Office of the Provincial Governor",
        [OfficeNameEnum.OfficeOfTheProvincialViceGovernor] = "Office of the Provincial Vice Governor",
        [OfficeNameEnum.OfficeOfTheProvincialAdministrator] = "Office of the Provincial Administrator",
        [OfficeNameEnum.OpgRoadSafetyDivision] = "OPG Road Safety Division",
        [OfficeNameEnum.BidsAndAwardsCommitteeB] = "Bids and Awards Committee B",
        [OfficeNameEnum.BidsAndAwardsCommitteeA] = "Bids and Awards Committee A",
        [OfficeNameEnum.CaviteProvincialJail] = "Cavite Provincial Jail",
        [OfficeNameEnum.OpgOfficeOfTheProvincialYouthDevelopmentOfficer] = "OPG Office of the Provincial Youth Development Officer",
        [OfficeNameEnum.OfficeOfTheProvincialHealthOfficer] = "Office of the Provincial Health Officer",
        [OfficeNameEnum.LocalEconomicDevelopmentAndInvestmentPromotionsOffice] = "Local Economic Development and Investment Promotions Office",
        [OfficeNameEnum.CaviteCenterForMentalHealth] = "Cavite Center for Mental Health",
        [OfficeNameEnum.CaviteQualityManagementOffice] = "Cavite Quality Management Office",
        [OfficeNameEnum.OfficeOfTheSangguniangPanlalawigan] = "Office of the Sangguniang Panlalawigan",
        [OfficeNameEnum.OpgOfficeOfTheProvincialInternalAuditServices] = "OPG Office of the Provincial Internal Audit Services",
        [OfficeNameEnum.ProvincialInformationAndCommunicationsTechnologyOffice] = "Provincial Information and Communications Technology Office",
        [OfficeNameEnum.OfficeOfTheProvincialEnvironmentAndNaturalResourcesOfficer] = "Office of the Provincial Environment and Natural Resources Officer",
        [OfficeNameEnum.OfficeOfTheProvincialDisasterRiskReductionAndManagementOfficer] = "Office of the Provincial Disaster Risk Reduction and Management Officer",
        [OfficeNameEnum.PgCaviteOfficeOfPublicSafety] = "PG Cavite Office of Public Safety",
        [OfficeNameEnum.OfficeOfTheProvincialEngineer] = "Office of the Provincial Engineer",
        [OfficeNameEnum.OfficeOfTheProvincialVeterinarian] = "Office of the Provincial Veterinarian",
        [OfficeNameEnum.OfficeOfTheProvincialSocialWelfareAndDevelopmentOfficer] = "Office of the Provincial Social Welfare and Development Officer",
        [OfficeNameEnum.OfficeOfTheProvincialAgriculturist] = "Office of the Provincial Agriculturist",
        [OfficeNameEnum.OfficeOfTheProvincialPopulationOfficer] = "Office of the Provincial Population Officer",
        [OfficeNameEnum.OfficeOfTheProvincialAssessor] = "Office of the Provincial Assessor",
        [OfficeNameEnum.OfficeOfTheProvincialTreasurer] = "Office of the Provincial Treasurer",
        [OfficeNameEnum.OfficeOfTheProvincialAccountant] = "Office of the Provincial Accountant",
        [OfficeNameEnum.OfficeOfTheProvincialBudgetOfficer] = "Office of the Provincial Budget Officer",
        [OfficeNameEnum.OfficeOfTheProvincialGeneralServicesOfficer] = "Office of the Provincial General Services Officer",
        [OfficeNameEnum.OfficeOfTheProvincialLegalOfficer] = "Office of the Provincial Legal Officer",
        [OfficeNameEnum.OpgOfficeOfTheProvincialPersonsWithDisabilityAffairsOfficer] = "OPG Office of the Provincial Persons with Disability Affairs Officer",
        [OfficeNameEnum.OfficeOfTheProvincialPlanningAndDevelopmentCoordinator] = "Office of the Provincial Planning and Development Coordinator",
        [OfficeNameEnum.OfficeOfTheProvincialInformationOfficer] = "Office of the Provincial Information Officer",
        [OfficeNameEnum.OfficeOfTheProvincialTourismOfficer] = "Office of the Provincial Tourism Officer",
        [OfficeNameEnum.OfficeOfTheProvincialCooperativesDevelopmentOfficer] = "Office of the Provincial Cooperatives Development Officer",
        [OfficeNameEnum.OfficeOfTheProvincialPublicEmploymentServiceManager] = "Office of the Provincial Public Employment Service Manager",
        [OfficeNameEnum.ProvincialHousingAndDevelopmentManagementOffice] = "Provincial Housing and Development Management Office",
        [OfficeNameEnum.OfficeOfTheProvincialHumanResourceManagementOfficer] = "Office of the Provincial Human Resource Management Officer"
    };

    public static string GetLabel(OfficeNameEnum office) =>
        Labels.TryGetValue(office, out var label) ? label : office.ToString();

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "of", "the", "and", "or", "for", "in", "on", "at", "to", "a", "an"
    };

    public static string GetAbbreviation(string officeName)
    {
        if (string.IsNullOrWhiteSpace(officeName))
        {
            return string.Empty;
        }

        var words = officeName.Split(new[] {' ', '-', ','}, StringSplitOptions.RemoveEmptyEntries);
        var abbrev = string.Concat(words
            .Where(w => !StopWords.Contains(w.Trim()))
            .Select(w => char.ToLowerInvariant(w.Trim()[0])));

        return abbrev;
    }
}
