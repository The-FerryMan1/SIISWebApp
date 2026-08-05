import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import axios from 'axios'

interface OfficeAccount {
  id: string
  email: string
  userName: string
  firstName: string
  lastName: string
  middleName: string
  roles: string[]
}

export const useOfficeAccountStore = defineStore('officeAccount', () => {
  const account = ref<OfficeAccount | null>(null)

  const isAuthenticated = () => {
    return !!localStorage.getItem('officeAuth')
  }

  const login = async (email: string, password: string) => {
    try {
      const { data } = await axios.post(
        '/login',
        { email, password },
        {
          params: {
            useCookies: true,
          },
        },
      )
      account.value = {
        id: data.userId ?? '',
        email: data.email ?? email,
        userName: data.email ?? email,
        firstName: data.firstName ?? '',
        lastName: data.lastName ?? '',
        middleName: data.middleName ?? '',
        roles: data.roles ?? [],
      }
      localStorage.setItem('officeAuth', 'true')
      localStorage.setItem('officeAccount', JSON.stringify(account.value))
      return data
    } catch (error) {
      logout()
      throw error
    }
  }

  const logout = async () => {
    try {
      await useAxios.post('/auth/logout')
    } catch {
      // ignore logout errors
    }
    account.value = null
    localStorage.removeItem('officeAuth')
    localStorage.removeItem('officeAccount')
  }

  const init = () => {
    const stored = localStorage.getItem('officeAccount')
    if (stored) {
      try {
        account.value = JSON.parse(stored)
      } catch {
        account.value = null
      }
    }
  }

  return {
    account,
    login,
    logout,
    isAuthenticated,
    init,
  }
})
