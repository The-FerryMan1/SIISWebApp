import type { OfficeNameEnum } from './officeSelectValue'

// Enums (match your C# enums)
export enum ApplicationStatusEnum {
  Pending = 0,
  Approved = 1,
  Rejected = 2
}

export enum GenderEnum {
  Male = 0,
  Female = 1,
  Other = 2,
}

export enum GradeLevelEnum {
  SeniorHighSchool = 0,
  College = 1,
}

export enum InternshipNatureEnum {
  OnTheJobTraining = 0,
  WorkImmersion = 1,
}

export enum StrandEnum {
  STEM = 0,
  ABM = 1,
  HUMSS = 2,
  GAS = 3,
  ICT = 4,
}

export enum DegreeEnum {
  BSIT = 0,
  BSCS = 1,
  BSN = 2,
  BSA = 3,
  BSBA = 4,
  BSEd = 5,
  BSCE = 6,
  BSEE = 7,
  BSME = 8,
  BSArch = 9,
  BSPharma = 10,
  BSPsych = 11,
}

// Types
export interface ApplicationInfo {
  id: number
  uuid: string
  status: ApplicationStatusEnum
  reason?: string
  createdAt: string
  updatedAt: string
  studentId: number
}

export interface StudentInfo {
  id: number
  studentUUID: string
  email: string
  lastName: string
  firstName: string
  middleName: string
  contactNumber: string
  address: string
  dateOfBirth: string
  gender: GenderEnum
  gradeLevel: GradeLevelEnum
  schoolName: string
  schoolAddress: string
  schoolContactPerson: string
  schoolContactPersonEmail: string
  schoolContactPersonPhone: string
  internshipNature: InternshipNatureEnum
  strand: StrandEnum
  degree: DegreeEnum
  totalInternshipHours: number
  isDeleted: boolean
  createdAt: string
  updatedAt: string | null
  deletedAt: string | null
  officeId: number | null
  fullName: string
  age: number
}

export interface PlacementInfo {
  id: number
  startDate: string
  estimatedEndDate: string
  accumulatedHours: number
  officeId: number
  officeName: string
  studentId: number
}

export interface RequirementInfo {
  id: number
  fileName: string
  filePath: string
  fileType: string
  isDeleted: boolean
  createdAt: string
  updatedAt: string | null
  deletedAt: string | null
  studentId: number
}

export interface OfficeInfo {
  id: number
  officeName: string
  userId: string
  isDeleted: boolean
  createdAt: string
  updatedAt: string | null
  deletedAt: string | null
}

// Main response type
export interface ApplicationGetByIdResponse {
  application: ApplicationInfo
  student: StudentInfo
  school: SchoolInfo
  internship: InternshipInfo
  placement: PlacementInfo | null
  requirements: RequirementInfo[]
  office: OfficeInfo | null
}

export interface SchoolInfo {
  name: string
  address: string
  contactPerson: string
  email: string
  contactNumber: string
}

export interface InternshipInfo {
  internshipNature: InternshipNatureEnum
  strand: StrandEnum
  degree: DegreeEnum
  internshipTotalHours: number
}