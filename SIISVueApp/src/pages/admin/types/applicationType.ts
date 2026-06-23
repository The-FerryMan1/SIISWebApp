import type { OfficeNameEnum } from "./officeSelectValue";

// Enums (match your C# enums)
export enum ApplicationStatusEnum {
  Pending = 0,
  Approved = 2
  // Add other statuses...
}

export enum GenderEnum {
  // 0 = ?, 1 = ?, 2 = ?
  Male = 0,
  Female = 1,
  Other = 2,
}

export enum GradeLevelEnum {
  // 0 = ?, 1 = ?, 2 = ?
  Grade11 = 1,
  Grade12 = 2,
}

export enum InternshipNatureEnum {
  // 1 = ?
  WorkImmersion = 1,
}

export enum StrandEnum {
  // 1 = ?
  STEM = 1,
  ABM = 2,
  HUMSS = 3,
  // etc.
}


// Types
export interface ApplicationInfo {
  id: number;
  applicationUUID: string;
  status: ApplicationStatusEnum;
  isDeleted: boolean;
  createAt: string; // or Date if you parse it
  updatedAt: string | null;
  deletedAt: string | null;
}

export interface StudentInfo {
  id: number;
  studentUUID: string;
  email: string;
  lastName: string;
  firstName: string;
  middleName: string;
  contactNumber: string;
  address: string;
  dateOfBirth: string; // "0001-01-01" format
  gender: GenderEnum;
  gradeLevel: GradeLevelEnum;
  isDeleted: boolean;
  createAt: string;
  updatedAt: string | null;
  deletedAt: string | null;
  officeId: number | null;
}

export interface SchoolInfo {
  id: number;
  name: string;
  address: string;
  contactPerson: string;
  email: string; // Bug in data: "Mrs. Elena Garcia" should probably be an email
  contactNumber: string;
  isDeleted: boolean;
  createAt: string;
  updatedAt: string | null;
  deletedAt: string | null;
}

export interface InternshipInfo {
  id: number;
  internshipNature: InternshipNatureEnum;
  strand: StrandEnum | null;
  degree: number | null; // or DegreeEnum | null
  startDate: string; // "YYYY-MM-DD"
  estimatedEndDate: string;
  internshipTotalHours: number;
  isDeleted: boolean;
  createAt: string;
  updatedAt: string | null;
  deletedAt: string | null;
}

export interface RequirementInfo {
  id: number;
  fileName: string;
  filePath: string;
  fileType: string;
  isDeleted: boolean;
  createAt: string;
  updatedAt: string | null;
  deletedAt: string | null;
}

export interface OfficeInfo {
  id: number;
  name: OfficeNameEnum;
  currentOIC: string | null;
  isDeleted: boolean;
  createAt: string;
  updatedAt: string | null;
  deletedAt: string | null;
}

// Main response type
export interface ApplicationGetByIdResponse {
  application: ApplicationInfo;
  student: StudentInfo;
  school: SchoolInfo;
  internship: InternshipInfo;
  requirements: RequirementInfo[];
  office: OfficeInfo | null;
}