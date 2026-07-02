import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'

// {
//     "id": 1,
//     "name": 0,
//     "currentOIC": null,
//     "createAt": "2026-06-30T09:27:19.9312183",
//     "updatedAt": null
//   },

export type Office = {
  id: number
  name: number
  currentOIC: string | null
  students: []
  createAt: string
  updatedAt: string | null
}

export const useOfficeStore = defineStore('office', () => {
  const offices = ref<Office[] | null>(null)

  const officeInit = async () => {
    try {
      const { data } = await useAxios.get('/office')
      offices.value = data
    } catch (error) {
      console.log(error)
    }
  }

  return {
    offices,
    officeInit,
  }
})
