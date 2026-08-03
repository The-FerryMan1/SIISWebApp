import { z } from 'zod'
import {
  DegreeEnum,
  GenderEnum,
  GradeLevelEnum,
  InternshipNatureEnum,
  StrandEnum,
} from './applicationType'

// Enums
export const Genden = z.enum(GenderEnum)
export const GradeLevel = z.enum(GradeLevelEnum)
export const InternshipNature = z.enum(InternshipNatureEnum)
export const Strand = z.enum(StrandEnum)
export const Degree = z.enum(DegreeEnum)
export const OfficeNameEnum = z.enum([
  'OfficeOfTheProvincialGovernor',
  'OfficeOfTheProvincialViceGovernor',
  'OfficeOfTheProvincialAdministrator',
  'OpgRoadSafetyDivision',
  'BidsAndAwardsCommitteeB',
  'BidsAndAwardsCommitteeA',
  'CaviteProvincialJail',
  'OpgOfficeOfTheProvincialYouthDevelopmentOfficer',
  'OfficeOfTheProvincialHealthOfficer',
  'LocalEconomicDevelopmentAndInvestmentPromotionsOffice',
  'CaviteCenterForMentalHealth',
  'CaviteQualityManagementOffice',
  'OfficeOfTheSangguniangPanlalawigan',
  'OpgOfficeOfTheProvincialInternalAuditServices',
  'ProvincialInformationAndCommunicationsTechnologyOffice',
  'OfficeOfTheProvincialEnvironmentAndNaturalResourcesOfficer',
  'OfficeOfTheProvincialDisasterRiskReductionAndManagementOfficer',
  'PgCaviteOfficeOfPublicSafety',
  'OfficeOfTheProvincialEngineer',
  'OfficeOfTheProvincialVeterinarian',
  'OfficeOfTheProvincialSocialWelfareAndDevelopmentOfficer',
  'OfficeOfTheProvincialAgriculturist',
  'OfficeOfTheProvincialPopulationOfficer',
  'OfficeOfTheProvincialAssessor',
  'OfficeOfTheProvincialTreasurer',
  'OfficeOfTheProvincialAccountant',
  'OfficeOfTheProvincialBudgetOfficer',
  'OfficeOfTheProvincialGeneralServicesOfficer',
  'OfficeOfTheProvincialLegalOfficer',
  'OpgOfficeOfTheProvincialPersonsWithDisabilityAffairsOfficer',
  'OfficeOfTheProvincialPlanningAndDevelopmentCoordinator',
  'OfficeOfTheProvincialInformationOfficer',
  'OfficeOfTheProvincialTourismOfficer',
  'OfficeOfTheProvincialCooperativesDevelopmentOfficer',
  'OfficeOfTheProvincialPublicEmploymentServiceManager',
  'ProvincialHousingAndDevelopmentManagementOffice',
  'OfficeOfTheProvincialHumanResourceManagementOfficer',
])

// Student Update DTO
export const StudentUpdateDtoSchema = z.object({
  Email: z.email().max(100),
  LastName: z.string().min(1).max(50),
  FirstName: z.string().min(1).max(50),
  MiddleName: z.string().max(50).default(''),
  ContactNumber: z
    .string()
    .min(1)
    .max(20)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Contact number contains invalid characters'),
  Address: z.string().min(1).max(200),
  DateOfBirth: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Invalid date format (YYYY-MM-DD)'),
  Gender: z.number().int().min(0).max(2),
  GradeLevel: z.number().int().min(1).max(2),
})

// School Update DTO
export const SchoolUpdateDtoSchema = z.object({
  Name: z.string().min(1).max(100),
  Address: z.string().min(1).max(200),
  ContactPerson: z.string().min(1).max(100),
  Email: z.string().email().max(100),
  ContactNumber: z
    .string()
    .min(1)
    .max(20)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Contact number contains invalid characters'),
})

// Internship Update DTO
export const InternshipUpdateDtoSchema = z
  .object({
    InternshipNature: z.number().int().min(1).max(2),
    Strand: z.number().int().min(1).max(4).nullable().optional(),
    Degree: z.number().int().min(0).max(11).nullable().optional(),
    StartDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
    EstimatedEndDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
    InternshipTotalHours: z.number().int().min(1).max(1000),
    AccumulatedHours: z.number().int().min(0).optional(),
  })
  .refine(
    (data) => {
      const start = new Date(data.StartDate)
      const end = new Date(data.EstimatedEndDate)
      return end > start
    },
    {
      message: 'End date must be after start date',
      path: ['EstimatedEndDate'],
    },
  )
  .refine(
    (data) => {
      const start = new Date(data.StartDate)
      const today = new Date()
      today.setHours(0, 0, 0, 0)
      return start >= today
    },
    {
      message: 'Start date cannot be in the past',
      path: ['StartDate'],
    },
  )

// Requirements Update DTO
export const RequirementsUpdateDtoSchema = z.object({
  FileName: z.string().min(1).max(255),
  FilePath: z
    .string()
    .min(1)
    .max(500)
    .refine((path) => !path.includes('..') && !path.includes('<'), {
      message: 'Invalid file path',
    }),
  FileType: z
    .string()
    .min(1)
    .max(50)
    .refine((type) => {
      const allowed = ['pdf', 'doc', 'docx', 'jpg', 'jpeg', 'png']
      return allowed.includes(type.toLowerCase())
    }, 'File type must be pdf, doc, docx, jpg, jpeg, or png'),
})

// Office Update DTO
export const OfficeUpdateDtoSchema = z.object({
  Name: z.number().int().min(0).max(36),
})

// Main OnBoard Update DTO
export const OnBoardUpdateDtoSchema = z.object({
  Student: StudentUpdateDtoSchema,
  School: SchoolUpdateDtoSchema,
  Internship: InternshipUpdateDtoSchema,
  Requirements: z.array(RequirementsUpdateDtoSchema),
  Office: OfficeUpdateDtoSchema,
})

// Type inference
export type StudentUpdateDto = z.infer<typeof StudentUpdateDtoSchema>
export type SchoolUpdateDto = z.infer<typeof SchoolUpdateDtoSchema>
export type InternshipUpdateDto = z.infer<typeof InternshipUpdateDtoSchema>
export type RequirementsUpdateDto = z.infer<typeof RequirementsUpdateDtoSchema>
export type OfficeUpdateDto = z.infer<typeof OfficeUpdateDtoSchema>
export type OnBoardUpdateDto = z.infer<typeof OnBoardUpdateDtoSchema>
