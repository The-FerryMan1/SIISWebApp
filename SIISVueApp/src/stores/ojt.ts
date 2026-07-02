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
  dateOfBirth: string | Date
  createdAt: string
  updatedAt: string | null
  officeName: number
  estimatedEndDate: string
  startDate: string
}

export const useOJtStore = defineStore('ojt', () => {
  const ojts = ref<Ojt[]>([])

  const ojtInit = async () => {
    try {
      const { data } = await useAxios.get('/ojt')
      ojts.value = data
    } catch (error) {
      const err = error as AxiosError
      console.log(err.message)
    }
  }

  return {
    ojtInit,
    ojts,
  }
})
