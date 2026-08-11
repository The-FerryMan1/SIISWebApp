// Enums (match your C# enums)
export enum ApplicationStatusEnum {
  Pending = 0,
  Approved = 2,
  // Add other statuses...
}

export enum GenderEnum {
  // 0 = ?, 1 = ?, 2 = ?
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
  // 1 = ?

  STEM = 0,
  ABM = 1,
  HUMSS = 2,
  GAS = 3,
  ICT = 4,
}

export enum DegreeEnum {
  BSIT,
  BSCS,
  BSN,
  BSA,
  BSBA,
  BSEd,
  BSCE,
  BSEE,
  BSME,
  BSArch,
  BSPharma,
  BSPsych,
}

// Types

export interface StudentInfo {
  id: number
  studentUUID: string
  email: string
  lastName: string
  firstName: string
  middleName: string
  contactNumber: string
  address: string
  dateOfBirth: string // "0001-01-01" format
  gender: GenderEnum
  gradeLevel: GradeLevelEnum
}

export interface SchoolInfo {
  id: number
  name: string
  address: string
  contactPerson: string
  email: string // Bug in data: "Mrs. Elena Garcia" should probably be an email
  contactNumber: string
}

export interface InternshipInfo {
  id: number
  internshipNature: InternshipNatureEnum
  strand: StrandEnum | null
  degree: number | null // or DegreeEnum | null
  startDate: string // "YYYY-MM-DD"
  estimatedEndDate: string | undefined
  internshipTotalHours: number
}

export interface RequirementInfo {
  id: number
  fileName: string
  filePath: string
  fileType: string
  isDeleted: boolean
}
