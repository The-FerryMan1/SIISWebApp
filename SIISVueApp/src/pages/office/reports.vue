<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
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

interface OfficeReportFilters {
  school: string
  dateFrom: string
  dateTo: string
  placementStatus: string
}

const officeFilterStore: Record<'masterlist' | 'ongoing' | 'finished', OfficeReportFilters> = {
  masterlist: { school: '', dateFrom: '', dateTo: '', placementStatus: '' },
  ongoing: { school: '', dateFrom: '', dateTo: '', placementStatus: '' },
  finished: { school: '', dateFrom: '', dateTo: '', placementStatus: '' },
}

const school = ref(officeFilterStore[reportType.value].school)
const dateFrom = ref(officeFilterStore[reportType.value].dateFrom)
const dateTo = ref(officeFilterStore[reportType.value].dateTo)
const placementStatus = ref(officeFilterStore[reportType.value].placementStatus)
const schools = ref<string[]>([])

const placementStatusOptions: SelectItem[] = [
  { label: 'All Status', value: '' },
  { label: 'Ongoing', value: 'Ongoing' },
  { label: 'Finished', value: 'Finished' },
]

const reportTypeOptions: SelectItem[] = [
  { label: 'Masterlist', value: 'masterlist' },
  { label: 'Ongoing', value: 'ongoing' },
  { label: 'Finished', value: 'finished' },
]

const myOfficeId = ref<number | null>(null)

watch(reportType, (newType) => {
  school.value = officeFilterStore[newType].school
  dateFrom.value = officeFilterStore[newType].dateFrom
  dateTo.value = officeFilterStore[newType].dateTo
  placementStatus.value = officeFilterStore[newType].placementStatus
})

watch([school, dateFrom, dateTo, placementStatus], ([sch, from, to, status]) => {
  officeFilterStore[reportType.value] = {
    school: sch,
    dateFrom: from,
    dateTo: to,
    placementStatus: status,
  }
})

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

function clearFilters() {
  const empty: OfficeReportFilters = { school: '', dateFrom: '', dateTo: '', placementStatus: '' }
  school.value = ''
  dateFrom.value = ''
  dateTo.value = ''
  placementStatus.value = ''
  officeFilterStore[reportType.value] = empty
}

async function generateReport() {
  loading.value = true
  try {
    const filters = {
      school: school.value || undefined,
      dateFrom: dateFrom.value || undefined,
      dateTo: dateTo.value || undefined,
      placementStatus: placementStatus.value || undefined,
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

        <UFormField label="Placement Status">
          <USelect
            v-model="placementStatus"
            :items="placementStatusOptions"
            placeholder="Filter by status"
            :allow-clear="false"
            class="w-full md:w-64"
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

      <UButton
        icon="i-lucide-x"
        label="Clear Filter"
        color="neutral"
        variant="outline"
        :loading="loading"
        @click="clearFilters"
      />
    </div>
  </UCard>
  </UMain>
</template>
