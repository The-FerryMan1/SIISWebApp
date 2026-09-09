<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSystemSettingsStore } from '../../stores/systemSettings'
import { useAxios } from '../../fetch/axios'

const settingsStore = useSystemSettingsStore()
const toast = useToast()

const form = ref({
  themeColor: '#2f5cba',
  logoFile: null as File | null,
})

const previewLogo = ref('')
const loading = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    await settingsStore.fetchSettings()
    form.value.themeColor = settingsStore.settings.themeColor || '#2f5cba'
    previewLogo.value = settingsStore.getLogoSrc()
  } finally {
    loading.value = false
  }
})

async function saveSettings() {
  loading.value = true
  try {
    const payload: any = {
      themeColor: form.value.themeColor,
    }

    if (form.value.logoFile) {
      const formData = new FormData()
      formData.append('logo', form.value.logoFile)
      const { data } = await useAxios.post('/system-settings/logo', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      payload.logoPath = data.path
      previewLogo.value = data.path
    }

    await settingsStore.updateSettings(payload)
    toast.add({ title: 'Settings saved successfully', color: 'success' })
    form.value.logoFile = null
  } catch (error) {
    toast.add({ title: 'Failed to save settings', color: 'error' })
  } finally {
    loading.value = false
  }
}

function onLogoChange(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) {
    form.value.logoFile = file
    previewLogo.value = URL.createObjectURL(file)
  }
}

const colorPresets = [
  { label: 'Blue', value: '#2f5cba' },
  { label: 'Green', value: '#15803d' },
  { label: 'Purple', value: '#7c3aed' },
  { label: 'Orange', value: '#c2410c' },
  { label: 'Red', value: '#b91c1c' },
  { label: 'Teal', value: '#0d9488' },
]
</script>

<template>
  <UMain class="space-y-6">
    <div>
      <h1 class="text-4xl font-black text-primary tracking-tight">System Settings</h1>
      <p class="text-muted text-sm mt-1">Configure logo and theme color for the system</p>
    </div>

    <UCard>
      <template #header>
        <h2 class="text-lg font-semibold">Appearance</h2>
      </template>

      <div class="space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <UFormField label="Logo">
            <div class="space-y-3">
              <UFileUpload
                v-model="form.logoFile"
                accept="image/png,image/jpeg,image/gif,image/svg+xml"
                file-icon="i-lucide-image"
                description="Upload a logo image (PNG, JPG, GIF, SVG)"
                class="w-full"
                @change="onLogoChange"
              />
              <div v-if="previewLogo" class="flex items-center gap-3">
                <img :src="previewLogo" alt="Logo preview" class="size-16 object-contain border rounded" />
                <span class="text-sm text-muted">Preview</span>
              </div>
            </div>
          </UFormField>

          <UFormField label="Theme Color">
            <div class="space-y-3">
              <UInput
                v-model="form.themeColor"
                type="color"
                class="w-full h-10"
              />
              <div class="flex flex-wrap gap-2">
                <UButton
                  v-for="preset in colorPresets"
                  :key="preset.value"
                  :color="form.themeColor === preset.value ? 'primary' : 'neutral'"
                  variant="outline"
                  size="sm"
                  @click="form.themeColor = preset.value"
                >
                  {{ preset.label }}
                </UButton>
              </div>
            </div>
          </UFormField>
        </div>

        <div class="flex justify-end">
          <UButton
            icon="i-lucide-save"
            label="Save Settings"
            color="primary"
            variant="solid"
            :loading="loading"
            @click="saveSettings"
          />
        </div>
      </div>
    </UCard>
  </UMain>
</template>
