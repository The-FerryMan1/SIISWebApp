import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import type { AxiosError } from 'axios'
import { useRouter } from 'vue-router'

interface User {
  userId: string
  email: string
  username: string
  isEmailVerified: boolean
}

export const UseAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const router = useRouter()

  //helper func for auth state
  const authenticate = (cred: User) => {
    localStorage.setItem('auth', 'true')
    user.value = cred
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
  const useLogin = async (credential: { username: string; password: string }) => {
    try {
      const { data } = await useAxios.post(
        '/login',
        { email: credential.username, password: credential.password },
        {
          params: {
            useCookies: true,
          },
        },
      )
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
      const { data } = await useAxios.get('user')

      const user: User = {
        userId: data.userId,
        email: data.email,
        username: data.username,
        isEmailVerified: data.isEmailVerified,
      }
      authenticate(user)
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
