import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'

export interface SystemSettings {
  logoPath?: string
  themeColor?: string
}

export const useSystemSettingsStore = defineStore('systemSettings', () => {
  const settings = ref<SystemSettings>({})
  const loading = ref(false)

  const fetchSettings = async () => {
    loading.value = true
    try {
      const { data } = await useAxios.get('/system-settings')
      settings.value = data
      applyThemeColor(data.themeColor)
    } catch (error) {
      console.log(error)
    } finally {
      loading.value = false
    }
  }

  const updateSettings = async (payload: { logoPath?: string; themeColor?: string }) => {
    loading.value = true
    try {
      const { data } = await useAxios.put('/system-settings', payload)
      settings.value = data
      if (payload.themeColor) {
        applyThemeColor(payload.themeColor)
      }
      if (payload.logoPath) {
        applyLogoPath(payload.logoPath)
      }
      return data
    } catch (error) {
      console.log(error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const applyThemeColor = (color?: string) => {
    if (!color) return
    const root = document.documentElement
    root.style.setProperty('--color-primary-400', color)
    root.style.setProperty('--color-primary-500', color)
    root.style.setProperty('--color-primary-600', color)
  }

  const applyLogoPath = (path?: string) => {
    if (!path) return
    const logoImg = document.querySelector('img[data-system-logo]')
    if (logoImg) {
      logoImg.setAttribute('src', path)
    }
  }

  const getLogoSrc = () => {
    return settings.value.logoPath || '/assets/img/logo.png'
  }

  return {
    settings,
    loading,
    fetchSettings,
    updateSettings,
    applyThemeColor,
    applyLogoPath,
    getLogoSrc,
  }
})
