import { z } from 'zod'

// ==================== ENUMS ====================
export const ApplicationStatusEnum = z.enum(['Pending', 'Approved'])
export type ApplicationStatusEnum = z.infer<typeof ApplicationStatusEnum>

export const GenderEnum = z.enum(['Male', 'Female', 'Other'])
export type GenderEnum = z.infer<typeof GenderEnum>

export const GradeLevelEnum = z.enum([
  'Grade11',
  'Grade12',
  'CollegeFirstYear',
  'CollegeSecondYear',
  'CollegeThirdYear',
  'CollegeFourthYear',
])
export type GradeLevelEnum = z.infer<typeof GradeLevelEnum>

export const InternshipNatureEnum = z.enum(['OJT', 'Apprenticeship', 'Internship', 'WorkImmersion'])
export type InternshipNatureEnum = z.infer<typeof InternshipNatureEnum>

export const StrandEnum = z.enum(['STEM', 'ABM', 'HUMSS', 'GAS', 'ICT'])
export type StrandEnum = z.infer<typeof StrandEnum>

export const DegreeEnum = z.enum([
  'BSIT',
  'BSCS',
  'BSN',
  'BSA',
  'BSBA',
  'BSEd',
  'BSCE',
  'BSEE',
  'BSME',
  'BSArch',
  'BSPharma',
  'BSPsych',
])
export type DegreeEnum = z.infer<typeof DegreeEnum>

// ==================== SCHEMAS ====================
export const StudentInfoSchema = z.object({
  email: z.string().email('Invalid email').max(100),
  lastName: z.string().min(1, 'Last name is required').max(50),
  firstName: z.string().min(1, 'First name is required').max(50),
  middleName: z.string().max(50).default(''),
  contactNumber: z
    .string()
    .min(11, 'Contact number is required')
    .max(11)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Invalid contact number format'),
  address: z.string().min(1, 'Address is required').max(200),
  dateOfBirth: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Invalid date format'),
  gender: z.coerce.number({ error: 'Gender is required' }).int().min(0).max(2, 'Invalid gender'),
  gradeLevel: z.coerce
    .number({ error: 'Grade level is required' })
    .int()
    .min(1)
    .max(12, 'Invalid grade level'),
})

export const SchoolInfoSchema = z.object({
  id: z.number().int().positive(),
  name: z.string().min(1).max(100),
  address: z.string().min(1).max(200),
  contactPerson: z.string().min(1).max(100),
  email: z.string().email(),
  contactNumber: z
    .string()
    .min(1)
    .max(20)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Invalid contact number format'),
})

export const InternshipInfoSchema = z.object({
  id: z.number().int().positive(),
  internshipNature: InternshipNatureEnum,
  strand: z.coerce
    .number()
    .int()
    .min(0)
    .max(4)
    .nullable()
    .transform((v) => (v === null ? undefined : v))
    .optional(),
  degree: z.coerce
    .number()
    .int()
    .min(0)
    .max(4)
    .nullable()
    .transform((v) => (v === null ? undefined : v))
    .optional(),
  startDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
  estimatedEndDate: z
    .string()
    .regex(/^\d{4}-\d{2}-\d{2}$/)
    .optional(),
internshipTotalHours: z.number().int().min(1).max(1000),
    accumulatedHours: z.number().int().min(0).optional(),
  })

export const RequirementInfoSchema = z.object({
  id: z.number().int().positive(),
  fileName: z.string().min(1).max(255),
  filePath: z.string().min(1).max(500),
  fileType: z
    .string()
    .min(1)
    .max(50)
    .refine((type) => {
      const allowed = ['pdf', 'doc', 'docx', 'jpg', 'jpeg', 'png']
      return allowed.includes(type.toLowerCase())
    }, 'Invalid file type'),
  isDeleted: z.boolean().default(false),
})

// ==================== MAIN RESPONSE SCHEMA ====================
export const ApplicationGetByIdResponseSchema = z.object({
  application: z.object({
    id: z.number(),
    applicationUUID: z.string().uuid(),
    status: z.number().int(), // 0 = Pending, 2 = Approved
    isDeleted: z.boolean(),
    createAt: z.iso.datetime(),
    updatedAt: z.iso.datetime().nullable(),
    deletedAt: z.iso.datetime().nullable(),
  }),
  student: StudentInfoSchema,
  school: SchoolInfoSchema,
  internship: InternshipInfoSchema,
  requirements: z.array(RequirementInfoSchema),
  office: z
    .object({
      id: z.number(),
      name: z.number().int(), // OfficeNameEnum
      department: z.string().nullable(),
      isDeleted: z.boolean(),
      createAt: z.iso.datetime().nullable(),
      updatedAt: z.iso.datetime().nullable().nullable(),
      deletedAt: z.iso.datetime().nullable().nullable(),
    })
    .nullable(),
})

export const StudentUpdateDtoSchema = z.object({
  email: z.email('Invalid email').max(100),
  lastName: z.string().min(1, 'Last name is required').max(50),
  firstName: z.string().min(1, 'First name is required').max(50),
  middleName: z.string().max(50).default(''),
  contactNumber: z
    .string()
    .min(11, 'Contact number is required')
    .max(11)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Invalid contact number format'),
  address: z.string().min(1, 'Address is required').max(200),
  dateOfBirth: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Invalid date format'),
  gender: z.coerce.number().int().min(0, 'Gender is required').max(2),
  gradeLevel: z.coerce.number().int().min(1, 'Grade level is required').max(12),
})

export const SchoolUpdateDtoSchema = z.object({
  name: z.string().min(1, 'School name is required').max(100),
  address: z.string().min(1, 'School address is required').max(200),
  contactPerson: z.string().min(1, 'Contact person is required').max(100),
  email: z.email("Contact person's email is required").max(100),
  contactNumber: z
    .string()
    .min(11, "Contact person's contact number is required")
    .max(11)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Invalid contact number format'),
})

export const InternshipUpdateDtoSchema = z
  .object({
    internshipNature: z.coerce
      .number({ error: 'Nature of internship is required' })
      .int()
      .min(0)
      .max(3),
    strand: z.coerce.number().int().min(0).max(4).optional(),
    degree: z.coerce.number().int().min(0).max(11).optional(),
    startDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Invalid date format'),
    estimatedEndDate: z.coerce.date({ error: 'End date is required' }),
    internshipTotalHours: z.coerce
      .number({ error: 'Total hours is required' })
      .int()
      .min(80, 'Minimum 80 hours')
      .max(600, 'Maximum 600 hours'),
  })
  .refine(
    (data) => {
      if (!data.startDate || !data.estimatedEndDate) return true
      return new Date(data.estimatedEndDate) > new Date(data.startDate)
    },
    {
      message: 'End date must be after start date',
      path: ['estimatedEndDate'],
    },
  )

export const RequirementsUpdateDtoSchema = z.object({
  file: z.file(),
})

export const OfficeUpdateDtoSchema = z.object({
  name: z.number().int().min(0).max(36),
})

// Main Update DTO
export const OnBoardUpdateDtoSchema = z
  .object({
    student: StudentUpdateDtoSchema,
    school: SchoolUpdateDtoSchema,
    internship: InternshipUpdateDtoSchema,
    requirements: z.array(z.instanceof(File)),
  })
  .superRefine((data, ctx) => {
    const gradeLevel = data.student.gradeLevel

    if (
      gradeLevel >= 11 &&
      gradeLevel <= 12 &&
      (data.internship.strand === null || data.internship.strand === undefined)
    ) {
      ctx.addIssue({
        code: 'custom', // ✅ Raw string literal
        message: 'Degree is required for College students',
        path: ['internship', 'strand'],
      })
    }

    if (
      gradeLevel >= 1 &&
      gradeLevel <= 4 &&
      (data.internship.degree === null || data.internship.degree === undefined)
    ) {
      ctx.addIssue({
        code: 'custom', // ✅ Raw string literal
        message: 'Strand is required for Senior high school students',
        path: ['internship', 'degree'],
      })
    }
  })

// ==================== TYPE EXPORTS ====================
export type StudentInfo = z.infer<typeof StudentInfoSchema>
export type SchoolInfo = z.infer<typeof SchoolInfoSchema>
export type InternshipInfo = z.infer<typeof InternshipInfoSchema>
export type RequirementInfo = z.infer<typeof RequirementInfoSchema>
export type ApplicationGetByIdResponse = z.infer<typeof ApplicationGetByIdResponseSchema>

export type StudentUpdateDto = z.infer<typeof StudentUpdateDtoSchema>
export type SchoolUpdateDto = z.infer<typeof SchoolUpdateDtoSchema>
export type InternshipUpdateDto = z.infer<typeof InternshipUpdateDtoSchema>
export type RequirementsUpdateDto = z.infer<typeof RequirementsUpdateDtoSchema>
export type OfficeUpdateDto = z.infer<typeof OfficeUpdateDtoSchema>
export type OnBoardUpdateDto = z.infer<typeof OnBoardUpdateDtoSchema>
