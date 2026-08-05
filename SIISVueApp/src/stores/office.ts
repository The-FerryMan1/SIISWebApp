import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'

export type Office = {
  id: number
  officeName: string
  userId: string
  userEmail: string | null
  department: string | null
  createdAt: string
  updatedAt: string | null
}

export const useOfficeStore = defineStore('office', () => {
  const offices = ref<Office[] | null>(null)

  const officeInit = async () => {
    try {
      const { data } = await useAxios.get('office')
      offices.value = data.map((item: any) => ({
        id: item.id,
        officeName: item.name,
        userId: item.userId,
        userEmail: item.userEmail,
        department: item.department,
        createdAt: item.createAt,
        updatedAt: item.updatedAt,
      }))
    } catch (error) {
      console.log(error)
    }
  }

  return {
    offices,
    officeInit,
  }
})
