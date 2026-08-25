import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useAxios } from '../fetch/axios'
import type { AxiosError } from 'axios'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { useOfficeAccountStore } from './officeAuth'

interface User {
  userId: string
  email: string
  username: string
  lastName: string
  firstName: string
  middleName: string
  isEmailVerified: boolean
  roles: string[]
}

export const UseAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const router = useRouter()

  const role = computed(() => user.value?.roles?.[0] ?? 'Admin')

  const isAdmin = computed(() => user.value?.roles?.includes('Admin') ?? false)
  const isOPG = computed(() => user.value?.roles?.includes('OPG') ?? false)
  const isOfficer = computed(() => user.value?.roles?.includes('Officer') ?? false)

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
  const useLogin = async (credential: { identifier: string; password: string }) => {
    try {
      const { data } = await axios.post(
        '/login',
        { email: credential.identifier, password: credential.password },
        {
          withCredentials: true,
          params: {
            useCookies: true,
          },
        },
      )

      const roles = data.roles ?? []

      if (roles.includes('Admin') || roles.includes('OPG')) {
        await useVerify()
        return { role: 'admin' as const }
      }

      if (roles.includes('Officer')) {
        const officeAuth = useOfficeAccountStore()
        officeAuth.setAccount(data)
        localStorage.setItem('officeAuth', 'true')
        localStorage.setItem('officeAccount', JSON.stringify({
          id: data.userId ?? '',
          email: data.email ?? credential.identifier,
          userName: data.email ?? credential.identifier,
          firstName: data.firstName ?? '',
          lastName: data.lastName ?? '',
          middleName: data.middleName ?? '',
          roles: data.roles ?? [],
        }))
        return { role: 'office' as const }
      }

      throw new Error('Unknown role')
    } catch (error) {
      unauthenticate()
      const officeAuth = useOfficeAccountStore()
      officeAuth.logout()
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
        lastName: data.lastName,
        firstName: data.firstName,
        middleName: data.middleName,
        isEmailVerified: data.isEmailVerified,
        roles: data.roles ?? [],
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
    role,
    isAdmin,
    isOPG,
    isOfficer,
    useVerify,
    authInit,
    unauthenticate,
  }
})
