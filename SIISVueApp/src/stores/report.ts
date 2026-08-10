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

     const pdfReportFiltered = async (endpoint: string, filters: { status?: number; office?: string; dateFrom?: string; dateTo?: string }) => {
          try {
              const { data } = await useAxios.get(endpoint, {
                  params: filters,
                  responseType: 'blob',
                  headers: { 'Content-Type': 'application/pdf' },
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

     const csvReportFiltered = async (endpoint: string, filters: { office?: string; dateFrom?: string; dateTo?: string }) => {
          try {
              const { data } = await useAxios.get(endpoint, {
                  params: filters,
                  responseType: 'blob',
                  headers: { 'Content-Type': 'application/csv' },
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

     const pdfReportPerOfficeFiltered = async (endpoint: string, filters: { office?: number; status?: number; dateFrom?: string; dateTo?: string }) => {
          try {
              const { data } = await useAxios.get(endpoint, {
                  params: filters,
                  responseType: 'blob',
                  headers: { 'Content-Type': 'application/pdf' },
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

     const previewPdf = async (endpoint: string, params?: Record<string, any>) => {
          try {
              const { data } = await useAxios.get(endpoint, {
                  params,
                  responseType: 'blob',
                  headers: { 'Content-Type': 'application/pdf' },
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
     }

     const officeMasterlistPdf = async (officeId: number) => {
         try {
             const { data } = await useAxios.get('/report/office/masterlist', {
                 params: { officeId },
                 responseType: 'blob',
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

     const officeExpiringPdf = async (officeId: number) => {
         try {
             const { data } = await useAxios.get('/report/office/expiring', {
                 params: { officeId },
                 responseType: 'blob',
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

     const officeFinishedPdf = async (officeId: number) => {
         try {
             const { data } = await useAxios.get('/report/office/finished', {
                 params: { officeId },
                 responseType: 'blob',
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

      const adminExpiringPdf = async (officeId?: number, days: number = 30) => {
          try {
              const { data } = await useAxios.get('/report/admin/expiring', {
                  params: {
                      officeId: officeId,
                      days: days,
                  },
                  responseType: 'blob',
                  headers: { 'Content-Type': 'application/pdf' },
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

      const getMyOffice = async () => {
         try {
             const { data } = await useAxios.get('office/my-office')
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
             return null
         }
     }

      return {
         pdfReport,
         csvExport,
         pdfReportPerOffice,
         pdfReportFiltered,
         csvReportFiltered,
         pdfReportPerOfficeFiltered,
         previewPdf,
         officeMasterlistPdf,
         officeExpiringPdf,
         officeFinishedPdf,
         adminExpiringPdf,
         getMyOffice,
      }
})