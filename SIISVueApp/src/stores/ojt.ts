import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import type { AxiosError } from 'axios'

export interface Ojt {
  ojtUUID: string
  lastName: string
  firstName: string
  middleName: string
  gender: number
  universitySchool: string
  dateOfBirth: string | Date
  createdAt: string
  updatedAt: string | null
  officeName: string
  estimatedEndDate: string
  startDate: string
}

export interface OjtDetails {
  studentUUID: string
  email: string
  lastName: string
  firstName: string
  middleName: string
  contactNumber: string
  address: string
  office: number
  dateOfBirth: string
  gender: number
  gradeLevel: number
}

export const useOJtStore = defineStore('ojt', () => {
  const ojts = ref<Ojt[]>([])
  const ojtDetails = ref<OjtDetails | null>(null)

  const ojtInit = async () => {
    try {
      const { data } = await useAxios.get('/ojt')
      ojts.value = data
    } catch (error) {
      const err = error as AxiosError
      console.log(err.message)
    }
  }

  const ojtDetailsInit = async (uuid: string) => {
    try {
      const { data } = await useAxios.get('ojt/' + uuid)
      ojtDetails.value = data
    } catch (error) {
      const err = error as AxiosError
      console.log(err.message)
    }
  }

  const deleteRequest = async (uuid: string) =>{
    try {
      await useAxios.delete('ojt/' + uuid)
    } catch (error) {
      const err = error as AxiosError
      console.log(err.message)
    }
  }

  return {
    ojtInit,
    ojts,
    ojtDetailsInit,
    ojtDetails,
    deleteRequest
  }
})
