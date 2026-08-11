import { z } from 'zod'
import {
  DegreeEnum,
  GenderEnum,
  GradeLevelEnum,
  InternshipNatureEnum,
  StrandEnum,
} from './applicationType'

// Enums
export const Gender = z.enum(GenderEnum)
export const GradeLevel = z.enum(GradeLevelEnum)
export const InternshipNature = z.enum(InternshipNatureEnum)
export const Strand = z.enum(StrandEnum)
export const Degree = z.enum(DegreeEnum)

// Flat Student Update DTO (new structure - no nested School/Internship)
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
  GradeLevel: z.union([z.literal(0), z.literal(1)]),
  SchoolName: z.string().min(1).max(100),
  SchoolAddress: z.string().min(1).max(200),
  SchoolContactPerson: z.string().min(1).max(100),
  SchoolContactPersonEmail: z.string().email().max(100),
  SchoolContactPersonPhone: z
    .string()
    .min(1)
    .max(20)
    .regex(/^[\d\s\+\-\(\)]+$/, 'Contact number contains invalid characters'),
  InternshipNature: z.number().int().min(0).max(1),
  Strand: z.number().int().min(0).max(4),
  Degree: z.number().int().min(0).max(11),
  TotalInternshipHours: z.number().int().min(1).max(1000),
})

// Main OnBoard Update DTO
export const OnBoardUpdateDtoSchema = z.object({
  Student: StudentUpdateDtoSchema,
})

// Type inference
export type StudentUpdateDto = z.infer<typeof StudentUpdateDtoSchema>
export type OnBoardUpdateDto = z.infer<typeof OnBoardUpdateDtoSchema>