<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useOfficeAccountStore } from '../../stores/officeAuth'
import { useReportStore } from '../../stores/report.ts'
import type { SelectItem } from '@nuxt/ui'
import { useRouter } from 'vue-router'
import { useAxios } from '../../fetch/axios'

const officeAuth = useOfficeAccountStore()
const report = useReportStore()
const router = useRouter()
const toast = useToast()

const reportType = ref<'masterlist' | 'expiring' | 'finished'>('masterlist')
const loading = ref(false)
const previewOpen = ref(false)
const previewUrl = ref<string | null>(null)
const previewFileName = ref<string>('')

const reportTypeOptions: SelectItem[] = [
  { label: 'Masterlist', value: 'masterlist' },
  { label: 'Expiring Internships', value: 'expiring' },
  { label: 'Finished Internships', value: 'finished' },
]

const myOfficeId = ref<number | null>(null)

async function loadMyOffice() {
  try {
    const { data } = await useAxios.get('office/my-office')
    myOfficeId.value = data.id
  } catch {
    myOfficeId.value = null
  }
}

onMounted(async () => {
  await loadMyOffice()
})

async function generateReport() {
  if (!myOfficeId.value) {
    toast.add({ title: 'No office assigned', color: 'error' })
    return
  }

  loading.value = true
  try {
    let blob: Blob | undefined
    let filename = 'report'

    switch (reportType.value) {
      case 'masterlist':
        blob = await report.officeMasterlistPdf(myOfficeId.value)
        filename = 'masterlist'
        break
      case 'expiring':
        blob = await report.officeExpiringPdf(myOfficeId.value)
        filename = 'expiring'
        break
      case 'finished':
        blob = await report.officeFinishedPdf(myOfficeId.value)
        filename = 'finished'
        break
    }

    if (blob) {
      previewFileName.value = `${filename}_${new Date().toISOString().split('T')[0]}.pdf`
      const url = URL.createObjectURL(blob)
      previewUrl.value = url
      previewOpen.value = true
    }
  } catch {
    toast.add({ title: 'Failed to generate report', color: 'error' })
  } finally {
    loading.value = false
  }
}

function downloadPreview() {
  if (!previewUrl.value) return
  const a = document.createElement('a')
  a.href = previewUrl.value
  a.download = previewFileName.value
  a.click()
}

function printPreview() {
  if (!previewUrl.value) return
  const win = window.open(previewUrl.value, '_blank')
  win?.print()
}

function closePreview() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value)
  }
  previewUrl.value = null
  previewOpen.value = false
}

function logout() {
  officeAuth.logout()
  router.push({ name: 'office-login' })
}
</script>

<template>
  <UMain class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-4xl font-black text-primary tracking-tight">Reports</h1>
        <p class="text-muted text-sm mt-1">Generate reports for your office</p>
      </div>
      <UButton icon="i-lucide-log-out" label="Logout" variant="outline" color="error" @click="logout" />
    </div>

    <UCard>
      <div class="flex flex-wrap gap-4 items-end">
        <UFormField label="Report Type">
          <USelect
            v-model="reportType"
            :items="reportTypeOptions"
            class="w-full md:w-80"
          />
        </UFormField>

        <UButton
          icon="i-lucide-file-text"
          label="Generate Report"
          color="primary"
          variant="solid"
          :loading="loading"
          @click="generateReport"
        />
      </div>
    </UCard>

    <UModal v-model:open="previewOpen" title="Report Preview">
      <template #body>
        <div class="w-full h-[70vh] border rounded-lg overflow-hidden bg-gray-50">
          <embed
            v-if="previewUrl"
            :src="previewUrl"
            type="application/pdf"
            class="w-full h-full"
          />
        </div>
      </template>

      <template #footer>
        <div class="flex justify-between w-full">
          <UButton label="Close" variant="ghost" color="neutral" @click="closePreview" />
          <div class="flex gap-2">
            <UButton icon="i-lucide-download" label="Download" variant="solid" color="primary" @click="downloadPreview" />
            <UButton icon="i-lucide-printer" label="Print" variant="solid" color="info" @click="printPreview" />
          </div>
        </div>
      </template>
    </UModal>
  </UMain>
</template>
