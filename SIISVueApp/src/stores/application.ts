import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import type { Axios, AxiosError } from 'axios'

export type Applicaton = {
  id: number
  uuid: string
  fullName: string
  degreeStrand: string
  status: string
  createdAt: Date
  updatedAt: Date | null
}

export const useApplicationStore = defineStore('applicaton', () => {
  const applications = ref<Applicaton[] | null>([])
  const applicationError = ref()
  const applicationInit = async () => {
    await getAllAsync()
  }

  const getAllAsync = async () => {
    try {
      const { data } = await useAxios.get('/application')
      applications.value = data.map((item: any) => ({
        ...item,
        uuid: item.applicationUUID ?? item.uuid,
      }))
    } catch (error) {
      console.log(error)
    }
  }

  const rejectApplication = async(uuid: string, reason?: string)=>{
    try {
      await useAxios.put('/application/details/reject/' + uuid, { reason })
    } catch (error) {
      const err = error as AxiosError
      applicationError.value = err
      console.log(error)
    }
  }

  return {
    applications,
    applicationInit,
    getAllAsync,
    rejectApplication
  }
})
