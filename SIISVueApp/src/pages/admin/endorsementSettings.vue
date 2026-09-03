<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAxios } from '../../fetch/axios'
import EndorsementPreview from './endorsementPreview.vue'

const toast = useToast()
const loading = ref(false)
const saving = ref(false)

interface EndorsementSettings {
  header: {
    country: string
    province: string
    officeTitle: string
    city: string
    logoPath?: string
  }
  body: {
    salutation: string
    greeting: string
    introTemplate: string
    attachmentNote: string
    thankYou: string
  }
  footer: {
    signingOfficerTitle: string
    closing: string
    footerAddress?: string
  }
  maxStudentsPerPage: number
}

const settings = ref<EndorsementSettings>({
  header: {
    country: '',
    province: '',
    officeTitle: '',
    city: '',
    logoPath: undefined,
  },
  body: {
    salutation: '',
    greeting: '',
    introTemplate: '',
    attachmentNote: '',
    thankYou: '',
  },
  footer: {
    signingOfficerTitle: '',
    closing: '',
    footerAddress: undefined,
  },
  maxStudentsPerPage: 10,
})

const sampleStudents = [
  { fullName: 'Juan Dela Cruz', totalInternshipHours: 100 },
  { fullName: 'Maria Santos', totalInternshipHours: 120 },
]

onMounted(async () => {
  await loadSettings()
})

async function loadSettings() {
  loading.value = true
  try {
    const { data } = await useAxios.get('/endorsement-settings')
    if (data.settings) {
      settings.value = data.settings
    }
  } catch (error) {
    toast.add({ title: 'Failed to load settings', color: 'error' })
  } finally {
    loading.value = false
  }
}

async function saveSettings() {
  saving.value = true
  try {
    const payload = {
      header: settings.value.header,
      body: settings.value.body,
      footer: settings.value.footer,
      maxStudentsPerPage: settings.value.maxStudentsPerPage,
    }
    await useAxios.put('/endorsement-settings', payload)
    toast.add({ title: 'Settings saved successfully', color: 'success' })
  } catch (error) {
    toast.add({ title: 'Failed to save settings', color: 'error' })
  } finally {
    saving.value = false
  }
}

async function resetSettings() {
  await loadSettings()
}
</script>

<template>
  <UMain class="space-y-6">
    <div>
      <h1 class="text-4xl font-black text-primary tracking-tight">Endorsement Settings</h1>
      <p class="text-muted text-sm mt-1">Configure endorsement letter templates</p>
    </div>

    <div v-if="!loading" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <UCard class="space-y-6">
        <h2 class="text-2xl font-bold text-primary">Settings</h2>

        <div class="space-y-4">
          <h3 class="text-lg font-semibold text-primary">Header Settings</h3>

          <UFormField label="Country">
            <UInput v-model="settings.header.country" placeholder="e.g., Republic of the Philippines" class="w-full" />
          </UFormField>

          <UFormField label="Province">
            <UInput v-model="settings.header.province" placeholder="e.g., Province of Cavite" class="w-full" />
          </UFormField>

          <UFormField label="Office Title">
            <UInput v-model="settings.header.officeTitle" placeholder="e.g., OFFICE OF THE PROVINCIAL GOVERNOR" class="w-full" />
          </UFormField>

          <UFormField label="City">
            <UInput v-model="settings.header.city" placeholder="e.g., Trece Martires City" class="w-full" />
          </UFormField>

          <UFormField label="Logo Path (Optional)">
            <UInput v-model="settings.header.logoPath" placeholder="/assets/logo.png" class="w-full" />
            <img v-if="settings.header.logoPath" :src="settings.header.logoPath" alt="Logo preview" class="mt-2 max-h-16 object-contain" />
          </UFormField>
        </div>

        <UDivider />

        <div class="space-y-4">
          <h3 class="text-lg font-semibold text-primary">Body Settings</h3>

          <UFormField label="Salutation">
            <UInput v-model="settings.body.salutation" placeholder="e.g., Dear Sir/Madam," class="w-full" />
          </UFormField>

          <UFormField label="Greeting">
            <UInput v-model="settings.body.greeting" placeholder="e.g., Greetings," class="w-full" />
          </UFormField>

          <UFormField label="Intro Template">
            <UTextarea v-model="settings.body.introTemplate" placeholder="e.g., Respectfully endorsing the following {students} of the {school}, to conduct their on-the-job training{hours} in your office:" :rows="2" class="w-full" />
            <p class="text-xs text-muted mt-1">Use {students}, {school}, and {hours} as placeholders</p>
          </UFormField>

          <UFormField label="Attachment Note">
            <UInput v-model="settings.body.attachmentNote" placeholder="e.g., Attached are the development letter(s) of the student(s) for your reference." class="w-full" />
          </UFormField>

          <UFormField label="Thank You">
            <UInput v-model="settings.body.thankYou" placeholder="e.g., Thank you very much." class="w-full" />
          </UFormField>
        </div>

        <UDivider />

        <div class="space-y-4">
          <h3 class="text-lg font-semibold text-primary">Footer Settings</h3>

          <UFormField label="Signing Officer Title">
            <UInput v-model="settings.footer.signingOfficerTitle" placeholder="e.g., Executive Assistant IV" class="w-full" />
          </UFormField>

          <UFormField label="Closing Salutation">
            <UInput v-model="settings.footer.closing" placeholder="e.g., Very truly yours," class="w-full" />
          </UFormField>

          <UFormField label="Footer Address (Optional)">
            <UTextarea
              v-model="settings.footer.footerAddress"
              placeholder="Address to display at footer"
              :rows="3"
              class="w-full"
            />
          </UFormField>
        </div>

        <UDivider />

        <div class="space-y-4">
          <h3 class="text-lg font-semibold text-primary">Pagination Settings</h3>

          <UFormField label="Max Students Per Page">
            <UInput
              v-model.number="settings.maxStudentsPerPage"
              type="number"
              placeholder="10"
              :min="1"
              :max="50"
              class="w-full"
            />
            <p class="text-xs text-muted mt-1">When bulk endorsing, pages will be created automatically after this many students</p>
          </UFormField>
        </div>

        <div class="flex gap-3 pt-4">
          <UButton
            color="primary"
            label="Save Settings"
            :loading="saving"
            @click="saveSettings"
          />
          <UButton
            color="primary"
            variant="outline"
            label="Reset"
            :loading="saving"
            @click="resetSettings"
          />
        </div>

        <UAlert
          icon="i-lucide-info"
          title="Information"
          description="Changes are applied immediately to new endorsement letters. Existing settings are loaded from appsettings.json on application restart."
          color="warning"
        />
      </UCard>

      <div class="lg:sticky lg:top-6 h-fit">
        <EndorsementPreview :settings="settings" :sample-students="sampleStudents" />
      </div>
    </div>

    <UCard v-else>
      <div class="flex justify-center items-center py-8">
        <UIcon name="i-lucide-loader-2" class="animate-spin mr-2" />
        Loading settings...
      </div>
    </UCard>
  </UMain>
</template>
