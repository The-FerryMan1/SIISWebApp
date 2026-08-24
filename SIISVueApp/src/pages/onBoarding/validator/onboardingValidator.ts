import { z } from 'zod'

// ==================== ENUMS ====================
export const ApplicationStatusEnum = z.enum(['Pending', 'Approved'])
export type ApplicationStatusEnum = z.infer<typeof ApplicationStatusEnum>

export const GenderEnum = z.enum(['Male', 'Female', 'Other'])
export type GenderEnum = z.infer<typeof GenderEnum>

export const GradeLevelEnum = z.enum([
  'SeniorHighSchool',
  'College',
])
export type GradeLevelEnum = z.infer<typeof GradeLevelEnum>

export const InternshipNatureEnum = z.enum(['OnTheJobTraining', 'WorkImmersion'])
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
  id: z.number(),
  studentUUID: z.string().uuid(),
  email: z.string().email('Invalid email').max(100),
  lastName: z.string().min(1, 'Last name is required').max(50),
  firstName: z.string().min(1, 'First name is required').max(50),
  middleName: z.string().max(50).default(''),
  contactNumber: z
    .string()
    .min(1, 'Contact number is required')
    .max(20)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Invalid contact number format'),
  address: z.string().min(1, 'Address is required').max(200),
  dateOfBirth: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Invalid date format'),
  gender: z.coerce.number({ error: 'Gender is required' }).int().min(0, 'Invalid gender').max(2, 'Invalid gender'),
  gradeLevel: z.coerce
    .number({ error: 'Grade level is required' })
    .int()
    .refine((value) => [0, 1].includes(value), 'Invalid grade level'),
  schoolName: z.string().min(1, 'School name is required').max(100),
  schoolAddress: z.string().min(1, 'School address is required').max(200),
  schoolContactPerson: z.string().min(1, 'Contact person is required').max(100),
  schoolContactPersonEmail: z.string().email("Contact person's email is required").max(100),
  schoolContactPersonPhone: z
    .string()
    .min(1, "Contact person's number is required")
    .max(20)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Invalid contact number format'),
  internshipNature: z.coerce.number().int().min(0, 'Internship nature is required').max(1),
  strand: z.coerce.number().int().min(0).max(4),
  degree: z.coerce.number().int().min(0).max(11),
  totalInternshipHours: z.coerce.number().int().min(80).max(600),
  internshipStartDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Invalid date format'),
  isDeleted: z.boolean().default(false),
  createdAt: z.iso.datetime(),
  updatedAt: z.iso.datetime().nullable(),
  deletedAt: z.iso.datetime().nullable(),
  officeId: z.number().nullable(),
  fullName: z.string(),
  age: z.number(),
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
  createdAt: z.iso.datetime(),
  updatedAt: z.iso.datetime().nullable(),
  deletedAt: z.iso.datetime().nullable(),
  studentId: z.number(),
})

export const PlacementInfoSchema = z.object({
  id: z.number(),
  startDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
  estimatedEndDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
  accumulatedHours: z.number().int().min(0),
  officeId: z.number(),
  officeName: z.string(),
  studentId: z.number(),
})

export const OfficeInfoSchema = z.object({
  id: z.number(),
  officeName: z.string(),
  userId: z.string(),
  isDeleted: z.boolean(),
  createdAt: z.iso.datetime(),
  updatedAt: z.iso.datetime().nullable(),
  deletedAt: z.iso.datetime().nullable(),
})

// ==================== MAIN RESPONSE SCHEMA ====================
export const ApplicationGetByIdResponseSchema = z.object({
  application: z.object({
    id: z.number(),
    uuid: z.string().uuid(),
    status: z.number().int(),
    reason: z.string().nullable(),
    createdAt: z.iso.datetime(),
    updatedAt: z.iso.datetime(),
    studentId: z.number(),
  }),
  student: StudentInfoSchema,
  placement: PlacementInfoSchema.nullable(),
  requirements: z.array(RequirementInfoSchema),
  office: OfficeInfoSchema.nullable(),
})

// ==================== UPDATE SCHEMAS ====================
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
  gradeLevel: z.coerce
    .number({ error: 'Grade level is required' })
    .int()
    .refine((value) => [0, 1].includes(value), 'Invalid grade level'),
  schoolName: z.string().min(1, 'School name is required').max(100),
  schoolAddress: z.string().min(1, 'School address is required').max(200),
  schoolContactPerson: z.string().min(1, 'Contact person is required').max(100),
  schoolContactPersonEmail: z.email("Contact person's email is required").max(100),
  schoolContactPersonPhone: z
    .string()
    .min(11, "Contact person's contact number is required")
    .max(11)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Invalid contact number format'),
  internshipNature: z.coerce.number().int().min(0).max(1),
  strand: z.coerce.number().int().min(0).max(4),
  degree: z.coerce.number().int().min(0).max(11),
  totalInternshipHours: z.coerce
    .number({ error: 'Total hours is required' })
    .int()
    .min(80, 'Minimum 80 hours')
    .max(600, 'Maximum 600 hours'),
  internshipStartDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Invalid date format'),
})

// Main Update DTO
export const OnBoardUpdateDtoSchema = z.object({
  student: StudentUpdateDtoSchema,
})

// ==================== TYPE EXPORTS ====================
export type StudentInfo = z.infer<typeof StudentInfoSchema>
export type RequirementInfo = z.infer<typeof RequirementInfoSchema>
export type PlacementInfo = z.infer<typeof PlacementInfoSchema>
export type OfficeInfo = z.infer<typeof OfficeInfoSchema>
export type ApplicationGetByIdResponse = z.infer<typeof ApplicationGetByIdResponseSchema>

export type StudentUpdateDto = z.infer<typeof StudentUpdateDtoSchema>
export type OnBoardUpdateDto = z.infer<typeof OnBoardUpdateDtoSchema>