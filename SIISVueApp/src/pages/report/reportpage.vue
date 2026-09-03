<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useReportStore } from '../../stores/report.ts'
import { OfficeOptions } from '../../shared/officeEnum.ts'
import type { SelectItem } from '@nuxt/ui'

const report = useReportStore()
const toast = useToast()

type ReportType = 
    | 'masterlist'
    | 'ongoing'
    | 'finished'
    | 'rejected'
    | 'approved'
    | 'pending'

const reportType = ref<ReportType>('masterlist')

interface ReportFilters {
  selectedOffice: string
  school: string
  dateFrom: string
  dateTo: string
  placementStatus: string
}

const filterStore: Record<ReportType, ReportFilters> = {
  masterlist: { selectedOffice: '', school: '', dateFrom: '', dateTo: '', placementStatus: '' },
  ongoing: { selectedOffice: '', school: '', dateFrom: '', dateTo: '', placementStatus: '' },
  finished: { selectedOffice: '', school: '', dateFrom: '', dateTo: '', placementStatus: '' },
  rejected: { selectedOffice: '', school: '', dateFrom: '', dateTo: '', placementStatus: '' },
  approved: { selectedOffice: '', school: '', dateFrom: '', dateTo: '', placementStatus: '' },
  pending: { selectedOffice: '', school: '', dateFrom: '', dateTo: '', placementStatus: '' },
}

const selectedOffice = ref(filterStore[reportType.value].selectedOffice)
const dateFrom = ref(filterStore[reportType.value].dateFrom)
const dateTo = ref(filterStore[reportType.value].dateTo)
const school = ref(filterStore[reportType.value].school)
const placementStatus = ref(filterStore[reportType.value].placementStatus)
const schools = ref<string[]>([])
const loading = ref(false)

const reportTypeOptions: SelectItem[] = [
    { label: 'Masterlist', value: 'masterlist' },
    { label: 'Ongoing List', value: 'ongoing' },
    { label: 'Finished List', value: 'finished' },
    { label: 'Rejected Applications', value: 'rejected' },
    { label: 'Approved Applications', value: 'approved' },
    { label: 'Pending Applications', value: 'pending' },
]

const isApplicationReport = computed(() => reportType.value === 'rejected' || reportType.value === 'approved' || reportType.value === 'pending')
const isInternReport = computed(() => reportType.value === 'masterlist' || reportType.value === 'ongoing' || reportType.value === 'finished')
const needsOffice = computed(() => reportType.value === 'masterlist' || reportType.value === 'approved')
const needsDate = computed(() => true)
const needsStatus = computed(() => false)
const needsPlacementStatus = computed(() => reportType.value === 'masterlist')

const officeSelectItems = computed(() => {
    return [{ label: 'All Offices', value: '' }, ...OfficeOptions.map(o => ({
        label: (o as { label?: string }).label ?? '',
        value: (o as { label?: string }).label ?? ''
    }))]
})

const schoolSelectItems = computed(() => {
    return [{ label: 'All Schools', value: '' }, ...schools.value.map((s) => ({ label: s, value: s }))]
})

watch(reportType, (newType) => {
  selectedOffice.value = filterStore[newType].selectedOffice
  school.value = filterStore[newType].school
  dateFrom.value = filterStore[newType].dateFrom
  dateTo.value = filterStore[newType].dateTo
  placementStatus.value = filterStore[newType].placementStatus
})

watch([selectedOffice, school, dateFrom, dateTo, placementStatus], ([office, sch, from, to, status]) => {
  filterStore[reportType.value] = {
    selectedOffice: office,
    school: sch,
    dateFrom: from,
    dateTo: to,
    placementStatus: status,
  }
})

onMounted(async () => {
    try {
      const data = await report.getSchools()
      if (Array.isArray(data)) {
        schools.value = data
      }
    } catch {
      schools.value = []
    }
})

function clearFilters() {
  const empty: ReportFilters = { selectedOffice: '', school: '', dateFrom: '', dateTo: '', placementStatus: '' }
  selectedOffice.value = ''
  school.value = ''
  dateFrom.value = ''
  dateTo.value = ''
  placementStatus.value = ''
  filterStore[reportType.value] = empty
}

function openPdf(blob: Blob | undefined) {
    if (blob) {
        const url = URL.createObjectURL(blob)
        const win = window.open(url, '_blank')
        win?.print()
        setTimeout(() => URL.revokeObjectURL(url), 1000)
    }
}

function downloadBlob(blob: Blob | undefined, filename: string) {
    if (!blob) return
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = filename
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
}

function getFilters() {
    const baseFilters = {
        school: school.value || undefined,
        dateFrom: dateFrom.value || undefined,
        dateTo: dateTo.value || undefined,
        office: selectedOffice.value || undefined,
    }
    
    // Add placement status filter only for masterlist reports
    if (reportType.value === 'masterlist') {
        return {
            ...baseFilters,
            placementStatus: placementStatus.value || undefined,
        }
    }
    
    return baseFilters
}

async function generatePdf() {
    loading.value = true
    try {
        let blob: Blob | undefined
        let filename = 'report'
        const filters = getFilters()

        switch (reportType.value) {
            case 'masterlist':
                blob = await report.adminMasterlistPdf(filters)
                filename = 'masterlist'
                break
            case 'ongoing':
                blob = await report.adminOngoingPdf(filters)
                filename = 'ongoing'
                break
            case 'finished':
                blob = await report.adminFinishedPdf(filters)
                filename = 'finished'
                break
            case 'rejected':
                blob = await report.adminRejectedPdf(filters)
                filename = 'rejected-applications'
                break
            case 'approved':
                blob = await report.adminApprovedPdf(filters)
                filename = 'approved-applications'
                break
            case 'pending':
                blob = await report.adminPendingPdf(filters)
                filename = 'pending-applications'
                break
        }

         if (blob) {
            openPdf(blob)
        } else {
            toast.add({ title: 'No report data generated. Please check your filters.', color: 'warning' })
        }
    } catch {
        toast.add({ title: 'Failed to generate PDF report', color: 'error' })
    } finally {
        loading.value = false
    }
}

async function generateCsv() {
    loading.value = true
    try {
        let blob: Blob | undefined
        let filename = 'report'
        const filters = getFilters()

        switch (reportType.value) {
            case 'masterlist':
                blob = await report.adminMasterlistCsv(filters)
                filename = 'masterlist.csv'
                break
            case 'ongoing':
                blob = await report.adminOngoingCsv(filters)
                filename = 'ongoing.csv'
                break
            case 'finished':
                blob = await report.adminFinishedCsv(filters)
                filename = 'finished.csv'
                break
            case 'rejected':
                blob = await report.adminRejectedCsv(filters)
                filename = 'rejected-applications.csv'
                break
            case 'approved':
                blob = await report.adminApprovedCsv(filters)
                filename = 'approved-applications.csv'
                break
            case 'pending':
                blob = await report.adminPendingCsv(filters)
                filename = 'pending-applications.csv'
                break
        }

        if (blob) {
            downloadBlob(blob, filename)
            toast.add({ title: 'CSV downloaded successfully', color: 'success' })
        } else {
            toast.add({ title: 'No report data generated. Please check your filters.', color: 'warning' })
        }
    } catch {
        toast.add({ title: 'Failed to generate CSV report', color: 'error' })
    } finally {
        loading.value = false
    }
}
</script>

<template>
    <UMain class="space-y-6">
        <div>
            <h1 class="text-4xl font-black text-primary tracking-tight">Reports</h1>
            <p class="text-muted text-sm mt-1">Generate and preview OJT reports</p>
        </div>

        <UCard>
            <div class="flex flex-wrap gap-4 items-end">
                <UFormField label="Report Type">
                    <USelectMenu
                        v-model="reportType"
                        :items="reportTypeOptions"
                        value-key="value"
                        class="w-full md:w-96"
                    />
                </UFormField>

                <UFormField v-if="needsOffice" label="Office" required>
                    <USelectMenu
                        v-model="selectedOffice"
                        :items="officeSelectItems"
                        placeholder="Select office"
                        value-key="value"
                        class="w-full md:w-64"
                    />
                </UFormField>

                <UFormField label="School">
                    <USelectMenu
                        v-model="school"
                        :items="schoolSelectItems"
                        placeholder="Filter by school"
                        class="w-full md:w-64"
                        value-key="value"
                    />
                </UFormField>

                <UFormField label="Date From">
                    <UInput
                        type="date"
                        v-model="dateFrom"
                        class="w-full md:w-48"
                    />
                </UFormField>

                <UFormField label="Date To">
                    <UInput
                        type="date"
                        v-model="dateTo"
                        class="w-full md:w-48"
                    />
                </UFormField>

                <UFormField v-if="needsPlacementStatus" label="Placement Status">
                    <USelectMenu
                        v-model="placementStatus"
                        :items="[
                            { label: 'All Status', value: '' },
                            { label: 'Ongoing', value: 'Ongoing' },
                            { label: 'Finished', value: 'Finished' }
                        ]"
                        placeholder="Select status"
                        class="w-full md:w-48"
                        value-key="value"
                    />
                </UFormField>

                <UButton
                    icon="i-lucide-file-text"
                    label="Download PDF"
                    color="primary"
                    variant="solid"
                    :loading="loading"
                    @click="generatePdf"
                />

                <UButton
                    icon="i-lucide-file-spreadsheet"
                    label="Download CSV"
                    color="secondary"
                    variant="solid"
                    :loading="loading"
                    @click="generateCsv"
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
