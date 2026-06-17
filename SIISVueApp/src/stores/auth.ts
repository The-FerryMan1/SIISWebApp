import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import type { AxiosError } from 'axios'

export const UseAuthStore = defineStore('auth', () => {
  const user = ref<string | null>(null)

  //helper func for auth state
  const authenticate = (email: string) => {
    localStorage.setItem('auth', 'true')
    user.value = email
  }
  // helper func to unauthenticate a user
  const unauthenticate = () => {
    localStorage.removeItem('auth')
    user.value = null
  }

  const authInit = () => {
    return !!localStorage.getItem('auth')
  }

  //login
  const useLogin = async (credential: { email: string; password: string }) => {
    try {
      const { data } = await useAxios.post('/login', credential, {
        params: {
          useCookies: true,
        },
      })
      await useVerify()
    } catch (error) {
      unauthenticate()
      const errorMessage = error as AxiosError
      throw new Error(errorMessage.message)
    }
  }

  //verify
  const useVerify = async () => {
    try {
      const { data } = await useAxios.get('/manage/info')
      authenticate(data.email)
    } catch (error) {
      unauthenticate()
      const errorMessage = error as AxiosError
      throw new Error(errorMessage.message)
    }
  }

  return {
    useLogin,
    user,
    useVerify,
    authInit,
  }
})
