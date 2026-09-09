import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import { UseAuthStore } from './auth'
import { useOfficeAccountStore } from './officeAuth'

export interface InboxItem {
  id: string
  type: string
  title: string
  description: string
  studentName: string
  schoolName: string
  officeName?: string
  createdAt: string
  actionUrl: string
  priority: string
}

export const useInboxStore = defineStore('inbox', () => {
  const items = ref<InboxItem[]>([])
  const unreadCount = ref(0)
  const loading = ref(false)

  const fetchInbox = async () => {
    loading.value = true
    try {
      const auth = UseAuthStore()
      const officeAuth = useOfficeAccountStore()
      let endpoint = '/inbox/admin'
      if (!auth.isAdmin && officeAuth.isAuthenticated()) {
        endpoint = '/inbox/office'
      } else if (!auth.isAdmin) {
        return
      }
      const { data } = await useAxios.get(endpoint)
      items.value = data
    } catch (error) {
      console.log(error)
    } finally {
      loading.value = false
    }
  }

  const fetchUnreadCount = async () => {
    try {
      const { data } = await useAxios.get('/inbox/count')
      unreadCount.value = data.count
    } catch (error) {
      console.log(error)
    }
  }

  return { items, unreadCount, loading, fetchInbox, fetchUnreadCount }
})
