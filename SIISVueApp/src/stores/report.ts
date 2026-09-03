import { defineStore } from "pinia";
import { useAxios } from "../fetch/axios";
import type { AxiosError } from "axios";


export const useReportStore = defineStore('report', () => {

     const pdfReport = async (endpoint: string, param?: number, school?: string) => {
        try {
            const { data } = await useAxios.get(endpoint, {
                params: {
                    status: param,
                    school: school,
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

    const csvExport = async(endpoint: string, param?: number, school?: string) => {
        try {
            const { data } = await useAxios.get(endpoint, {
                params: {
                    status: param,
                    school: school,
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


     const pdfReportPerOffice = async(endpoint: string, param?: number, school?: string)=>{
         try {
             const { data } = await useAxios.get(endpoint, {
                 params: {
                     office: param,
                     school: school,
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


    
     const csvExportPerOffice = async(endpoint: string, param?: number, school?: string) => {
          try {
             const { data } = await useAxios.get(endpoint, {
                 params: {
                     status: param,
                     school: school,
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

     const pdfReportFiltered = async (endpoint: string, filters: { status?: number; office?: string; school?: string; dateFrom?: string; dateTo?: string }) => {
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

     const csvReportFiltered = async (endpoint: string, filters: { office?: string; school?: string; dateFrom?: string; dateTo?: string }) => {
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

     const pdfReportPerOfficeFiltered = async (endpoint: string, filters: { office?: number; status?: number; school?: string; dateFrom?: string; dateTo?: string }) => {
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

      const officeMasterlistPdf = async (filters: { school?: string; dateFrom?: string; dateTo?: string; placementStatus?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/office/masterlist', {
                  params: filters,
                  responseType: 'blob',
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

      const officeMasterlistCsv = async (filters: { school?: string; dateFrom?: string; dateTo?: string; placementStatus?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/office/masterlist/csv', {
                  params: filters,
                  responseType: 'blob',
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

      const officeOngoingPdf = async (filters: { school?: string; dateFrom?: string; dateTo?: string; placementStatus?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/office/ongoing', {
                  params: filters,
                  responseType: 'blob',
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

      const officeOngoingCsv = async (filters: { school?: string; dateFrom?: string; dateTo?: string; placementStatus?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/office/ongoing/csv', {
                  params: filters,
                  responseType: 'blob',
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

      const officeFinishedPdf = async (filters: { school?: string; dateFrom?: string; dateTo?: string; placementStatus?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/office/finished', {
                  params: filters,
                  responseType: 'blob',
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

      const officeFinishedCsv = async (filters: { school?: string; dateFrom?: string; dateTo?: string; placementStatus?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/office/finished/csv', {
                  params: filters,
                  responseType: 'blob',
              });
              return data
          } catch (error) {
              const err = error as AxiosError
              console.log(err)
          }
      }

      const adminExpiringPdf = async (officeId?: number, days: number = 30, school?: string, dateFrom?: string, dateTo?: string) => {
           try {
               const { data } = await useAxios.get('/report/admin/expiring', {
                   params: {
                       officeId: officeId,
                       days: days,
                       school: school,
                       dateFrom: dateFrom,
                       dateTo: dateTo,
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

      const adminMasterlistPdf = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string; office?: string; placementStatus?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/masterlist/pdf', {
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

      const adminMasterlistCsv = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string; office?: string; placementStatus?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/masterlist/csv', {
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

      const adminOngoingPdf = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string; office?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/ongoing/pdf', {
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

      const adminOngoingCsv = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string; office?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/ongoing/csv', {
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

      const adminFinishedPdf = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string; office?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/finished/pdf', {
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

      const adminFinishedCsv = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string; office?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/finished/csv', {
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

      const adminRejectedPdf = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/rejected/pdf', {
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

      const adminRejectedCsv = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/rejected/csv', {
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

      const adminApprovedPdf = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string; office?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/approved/pdf', {
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

      const adminApprovedCsv = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string; office?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/approved/csv', {
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

      const adminPendingPdf = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/pending/pdf', {
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

      const adminPendingCsv = async (filters: { name?: string; school?: string; dateFrom?: string; dateTo?: string } = {}) => {
          try {
              const { data } = await useAxios.get('/report/admin/pending/csv', {
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

     const studentMasterlistPdf = async (officeName: string, school?: string) => {
          try {
              const { data } = await useAxios.get('/report/student-masterlist/pdf', {
                  params: { officeName, school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

     const studentMasterlistCsv = async (officeName: string, school?: string) => {
          try {
              const { data } = await useAxios.get('/report/student-masterlist/csv', {
                  params: { officeName, school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const pendingApplicationsPdf = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/pending-applications/pdf', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const pendingApplicationsCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/pending-applications/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const schoolSummaryPdf = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/school-summary/pdf', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const schoolSummaryCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/school-summary/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const placementUtilizationPdf = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/placement-utilization/pdf', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const placementUtilizationCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/placement-utilization/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const requirementsCompliancePdf = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/requirements-compliance/pdf', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const requirementsComplianceCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/requirements-compliance/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const hoursProgressPdf = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/hours-progress/pdf', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const hoursProgressCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/hours-progress/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const completionSummaryPdf = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/completion-summary/pdf', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const completionSummaryCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/completion-summary/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const rejectedApplicationsPdf = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/rejected-applications/pdf', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const rejectedApplicationsCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/rejected-applications/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const importAuditCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/import-audit/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const officePerformancePdf = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/office-performance/pdf', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/pdf' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

    const officePerformanceCsv = async (school?: string) => {
        try {
            const { data } = await useAxios.get('/report/office-performance/csv', {
                params: { school },
                 responseType: 'blob',
                 headers: { 'Content-Type': 'application/csv' },
             });
             return data
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
         }
     }

      const getSchools = async () => {
         try {
             const { data } = await useAxios.get('/report/admin/schools')
             return data as string[]
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
             return []
         }
     }

      const getOfficeSchools = async () => {
         try {
             const { data } = await useAxios.get('/report/office/schools')
             return data as string[]
         } catch (error) {
             const err = error as AxiosError
             console.log(err)
             return []
         }
     }

        return {
           pdfReport,
           csvExport,
           pdfReportPerOffice,
           csvExportPerOffice,
           pdfReportFiltered,
           csvReportFiltered,
           pdfReportPerOfficeFiltered,
           previewPdf,
            officeMasterlistPdf,
            officeMasterlistCsv,
            officeOngoingPdf,
            officeOngoingCsv,
            officeFinishedPdf,
            officeFinishedCsv,
           adminExpiringPdf,
           adminMasterlistPdf,
           adminMasterlistCsv,
           adminOngoingPdf,
           adminOngoingCsv,
           adminFinishedPdf,
           adminFinishedCsv,
           adminRejectedPdf,
           adminRejectedCsv,
           adminApprovedPdf,
           adminApprovedCsv,
           adminPendingPdf,
           adminPendingCsv,
           getMyOffice,
           getSchools,
           getOfficeSchools,
           studentMasterlistPdf,
           studentMasterlistCsv,
           pendingApplicationsPdf,
           pendingApplicationsCsv,
           schoolSummaryPdf,
           schoolSummaryCsv,
           placementUtilizationPdf,
           placementUtilizationCsv,
           requirementsCompliancePdf,
           requirementsComplianceCsv,
           hoursProgressPdf,
           hoursProgressCsv,
           completionSummaryPdf,
           completionSummaryCsv,
           rejectedApplicationsPdf,
           rejectedApplicationsCsv,
           importAuditCsv,
           officePerformancePdf,
           officePerformanceCsv,
        }
})
