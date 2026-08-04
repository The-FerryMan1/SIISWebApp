import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import axios from 'axios'

interface OfficeAccount {
  id: string
  email: string
  userName: string
  roles: string[]
}

export const useOfficeAccountStore = defineStore('officeAccount', () => {
  const account = ref<OfficeAccount | null>(null)

  const isAuthenticated = () => {
    return !!localStorage.getItem('officeAuth')
  }

  const login = async (email: string, password: string) => {
    try {
      const { data } = await axios.post('/login', {
        email,
        password,
      })
      account.value = {
        id: data.userId ?? '',
        email: data.email ?? email,
        userName: data.email ?? email,
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
      await axios.post('/logout')
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
