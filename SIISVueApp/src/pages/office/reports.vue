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

const reportType = ref<'masterlist' | 'ongoing' | 'finished'>('masterlist')
const loading = ref(false)
const school = ref('')
const dateFrom = ref('')
const dateTo = ref('')
const schools = ref<string[]>([])

const reportTypeOptions: SelectItem[] = [
  { label: 'Masterlist', value: 'masterlist' },
  { label: 'Ongoing', value: 'ongoing' },
  { label: 'Finished', value: 'finished' },
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
  try {
    const data = await report.getOfficeSchools()
    if (Array.isArray(data)) {
      schools.value = data
    }
  } catch {
    schools.value = []
  }
})

async function generateReport() {
  loading.value = true
  try {
    const filters = {
      school: school.value || undefined,
      dateFrom: dateFrom.value || undefined,
      dateTo: dateTo.value || undefined,
    }

    let blob: Blob | undefined
    let filename = 'report'

    switch (reportType.value) {
      case 'masterlist':
        blob = await report.officeMasterlistPdf(filters)
        filename = 'masterlist'
        break
      case 'ongoing':
        blob = await report.officeOngoingPdf(filters)
        filename = 'ongoing'
        break
      case 'finished':
        blob = await report.officeFinishedPdf(filters)
        filename = 'finished'
        break
    }

    if (blob) {
      const url = URL.createObjectURL(blob)
      const win = window.open(url, '_blank')
      win?.print()
      URL.revokeObjectURL(url)
    }
  } catch {
    toast.add({ title: 'Failed to generate report', color: 'error' })
  } finally {
    loading.value = false
  }
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
            placeholder="Select report type"
            :allow-clear="false"
            class="w-full md:w-80"
          />
        </UFormField>

        <UFormField label="School">
          <USelect
            v-model="school"
            :items="schools"
            placeholder="Filter by school"
            class="w-full md:w-64"
          />
        </UFormField>

        <UFormField label="Date From">
          <UInput
            v-model="dateFrom"
            type="date"
            class="w-full md:w-48"
          />
        </UFormField>

        <UFormField label="Date To">
          <UInput
            v-model="dateTo"
            type="date"
            class="w-full md:w-48"
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
  </UMain>
</template>
