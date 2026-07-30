import { defineStore } from "pinia";
import { useAxios } from "../fetch/axios";
import type { AxiosError } from "axios";


export const useReportStore = defineStore('report', () => {

    const pdfReport = async (endpoint: string, param?: number) => {
        try {
            const { data } = await useAxios.get(endpoint, {
                params: {
                    status: param
                },
                responseType: 'blob',
                headers: {
                    'Content-Type': 'application/pdf'
                },
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    const csvExport = async(endpoint: string, param?: number) => {
         try {
            const { data } = await useAxios.get(endpoint, {
                params: {
                    status: param
                },
                responseType: 'blob',
                headers: {
                    'Content-Type': 'application/csv'
                },
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }


    const pdfReportPerOffice = async(endpoint: string, param?: number)=>{
         try {
            const { data } = await useAxios.get(endpoint, {
                params: {
                    office: param
                },
                responseType: 'blob',
                headers: {
                    'Content-Type': 'application/pdf'
                },
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    const pendingApplications = async (format?: string) => {
        const endpoint = format === 'csv' ? '/report/applications/pending/csv' : '/report/applications/pending'
        try {
            const { data } = await useAxios.get(endpoint, {
                responseType: 'blob',
                headers: {
                    'Content-Type': format === 'csv' ? 'text/csv' : 'application/pdf'
                }
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    const missingRequirements = async (format?: string) => {
        const endpoint = format === 'csv' ? '/report/requirements/missing/csv' : '/report/requirements/missing'
        try {
            const { data } = await useAxios.get(endpoint, {
                responseType: 'blob',
                headers: {
                    'Content-Type': format === 'csv' ? 'text/csv' : 'application/pdf'
                }
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    const officesSummary = async () => {
        try {
            const { data } = await useAxios.get('/report/offices/summary', {
                responseType: 'blob',
                headers: { 'Content-Type': 'application/pdf' }
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    const studentsReport = async (format?: string) => {
        const endpoint = format === 'csv' ? '/report/students/csv' : '/report/students'
        try {
            const { data } = await useAxios.get(endpoint, {
                responseType: 'blob',
                headers: {
                    'Content-Type': format === 'csv' ? 'text/csv' : 'application/pdf'
                }
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    const internshipHours = async () => {
        try {
            const { data } = await useAxios.get('/report/internship/hours', {
                responseType: 'blob',
                headers: { 'Content-Type': 'application/pdf' }
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    const requirementsChecklist = async (format?: string) => {
        const endpoint = format === 'csv' ? '/report/requirements/checklist/csv' : '/report/requirements/checklist'
        try {
            const { data } = await useAxios.get(endpoint, {
                responseType: 'blob',
                headers: {
                    'Content-Type': format === 'csv' ? 'text/csv' : 'application/pdf'
                }
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    const expiringInternships = async (format?: string) => {
        const endpoint = format === 'csv' ? '/report/internship/expiring/csv' : '/report/internship/expiring'
        try {
            const { data } = await useAxios.get(endpoint, {
                responseType: 'blob',
                headers: {
                    'Content-Type': format === 'csv' ? 'text/csv' : 'application/pdf'
                }
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    
    
    const csvExportPerOffice = async(endpoint: string, param?: number) => {
         try {
            const { data } = await useAxios.get(endpoint, {
                params: {
                    status: param
                },
                responseType: 'blob',
                headers: {
                    'Content-Type': 'application/csv'
                },
            });
            return data
        } catch (error) {
            const err = error as AxiosError
            console.log(err)
        }
    }

    return {
        pdfReport,
        csvExport,
        pdfReportPerOffice,
        pendingApplications,
        missingRequirements,
        officesSummary,
        studentsReport,
        internshipHours,
        requirementsChecklist,
        expiringInternships
    }
})