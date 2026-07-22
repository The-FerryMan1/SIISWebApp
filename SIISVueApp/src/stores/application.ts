import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import type { Axios, AxiosError } from 'axios'

export type Applicaton = {
  id: number
  applicationUUID: string
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
      applications.value = data
    } catch (error) {
      console.log(error)
    }
  }

  const rejectApplication = async(uuid: string)=>{
    try {
      await useAxios.put('/application/details/reject/' + uuid)
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
