<script setup lang="ts">
import { ref, computed } from 'vue'
import { useReportStore } from '../../stores/report.ts'
import { OfficeOptions, OfficeNameEnum } from '../../shared/officeEnum.ts'
import type { SelectItem } from '@nuxt/ui'

const report = useReportStore()
const toast = useToast()

type ReportType = 
    | 'studentMasterlist'
    | 'pendingApplications'
    | 'schoolSummary'
    | 'placementUtilization'
    | 'hoursProgress'
    | 'completionSummary'
    | 'rejectedApplications'
    | 'importAudit'
    | 'officePerformance'
    | 'ojtList'
    | 'ojtPerOffice'
    | 'expiringInternships'

const reportType = ref<ReportType>('studentMasterlist')
const selectedOffice = ref<string>('')
const selectedStatus = ref<string>('')
const dateFrom = ref<string>('')
const dateTo = ref<string>('')
const days = ref<number>(30)

const loading = ref(false)

const statusOptions: SelectItem[] = [
    { label: 'All', value: '' },
    { label: 'Pending', value: '0' },
    { label: 'Approved', value: '1' },
    { label: 'Rejected', value: '2' },
]

const reportTypeOptions: SelectItem[] = [
    { label: 'Student Masterlist per Office', value: 'studentMasterlist' },
    { label: 'Pending Applications Report', value: 'pendingApplications' },
    { label: 'Application Summary by School', value: 'schoolSummary' },
    { label: 'Placement Utilization Report', value: 'placementUtilization' },
    { label: 'Hours Progress Report', value: 'hoursProgress' },
    { label: 'Completion Summary Report', value: 'completionSummary' },
    { label: 'Rejected Applications Report', value: 'rejectedApplications' },
    { label: 'Import Audit Log', value: 'importAudit' },
    { label: 'Office Performance Report', value: 'officePerformance' },
    { label: 'OJT List (All Offices)', value: 'ojtList' },
    { label: 'OJT Per Office', value: 'ojtPerOffice' },
    { label: 'Expiring Internships', value: 'expiringInternships' },
]

const isStudentMasterlist = computed(() => reportType.value === 'studentMasterlist')
const isOjtList = computed(() => reportType.value === 'ojtList')
const isOjtPerOffice = computed(() => reportType.value === 'ojtPerOffice')
const isExpiring = computed(() => reportType.value === 'expiringInternships')
const needsOffice = computed(() => isStudentMasterlist.value || isOjtPerOffice.value || isExpiring.value)

const officeNameOptions = computed<SelectItem[]>(() => {
    const keys = Object.keys(OfficeNameEnum).filter(k => isNaN(Number(k)))
    return keys.map(k => {
        const enumVal = OfficeNameEnum[k as keyof typeof OfficeNameEnum] as number
        const found = OfficeOptions.find(o => (o as any).value === enumVal)
    const label = (found as any)?.label || k
        return { label, value: k }
    })
})

const officeSelectItems = computed(() => {
    if (isOjtList.value) return [{ label: 'All Offices', value: '' }, ...OfficeOptions]
    if (isStudentMasterlist.value) return officeNameOptions.value
    return OfficeOptions
})

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

async function generatePdf() {
    if (isStudentMasterlist.value && !selectedOffice.value) {
        toast.add({ title: 'Please select an office for masterlist report', color: 'warning' })
        return
    }

    loading.value = true
    try {
        let blob: Blob | undefined
        let filename = 'report'

        switch (reportType.value) {
            case 'studentMasterlist':
                blob = await report.studentMasterlistPdf(selectedOffice.value)
                filename = 'student-masterlist'
                break
            case 'pendingApplications':
                blob = await report.pendingApplicationsPdf()
                filename = 'pending-applications'
                break
            case 'schoolSummary':
                blob = await report.schoolSummaryPdf()
                filename = 'school-summary'
                break
            case 'placementUtilization':
                blob = await report.placementUtilizationPdf()
                filename = 'placement-utilization'
                break
            case 'hoursProgress':
                blob = await report.hoursProgressPdf()
                filename = 'hours-progress'
                break
            case 'completionSummary':
                blob = await report.completionSummaryPdf()
                filename = 'completion-summary'
                break
            case 'rejectedApplications':
                blob = await report.rejectedApplicationsPdf()
                filename = 'rejected-applications'
                break
            case 'ojtList': {
                const hasFilters = selectedOffice.value || selectedStatus.value || dateFrom.value || dateTo.value
                if (hasFilters) {
                    blob = await report.pdfReportFiltered('/report/ojtList', {
                        status: selectedStatus.value ? parseInt(selectedStatus.value) : undefined,
                        office: selectedOffice.value || undefined,
                        dateFrom: dateFrom.value || undefined,
                        dateTo: dateTo.value || undefined,
                    })
                } else {
                    blob = await report.pdfReport('/report/ojtList', selectedStatus.value ? parseInt(selectedStatus.value) : undefined)
                }
                filename = 'ojt-list'
                break
            }
            case 'ojtPerOffice': {
                if (!selectedOffice.value) {
                    toast.add({ title: 'Please select an office', color: 'warning' })
                    return
                }
                const hasFilters = selectedStatus.value || dateFrom.value || dateTo.value
                if (hasFilters) {
                    blob = await report.pdfReportPerOfficeFiltered('/report/ojtPerOffice', {
                        office: parseInt(selectedOffice.value),
                        status: selectedStatus.value ? parseInt(selectedStatus.value) : undefined,
                        dateFrom: dateFrom.value || undefined,
                        dateTo: dateTo.value || undefined,
                    })
                } else {
                    blob = await report.pdfReportPerOffice('/report/ojtPerOffice', parseInt(selectedOffice.value))
                }
                filename = 'ojt-per-office'
                break
            }
            case 'expiringInternships': {
                const officeId = selectedOffice.value ? parseInt(selectedOffice.value) : undefined
                blob = await report.adminExpiringPdf(officeId, days.value)
                filename = `expiring-internships-${days.value}days`
                break
            }
            case 'officePerformance':
                blob = await report.officePerformancePdf()
                filename = 'office-performance'
                break
        }

        openPdf(blob)
    } catch {
        toast.add({ title: 'Failed to generate PDF report', color: 'error' })
    } finally {
        loading.value = false
    }
}

async function generateCsv() {
    if (isStudentMasterlist.value && !selectedOffice.value) {
        toast.add({ title: 'Please select an office for masterlist report', color: 'warning' })
        return
    }

    loading.value = true
    try {
        let blob: Blob | undefined
        let filename = 'report'

        switch (reportType.value) {
            case 'studentMasterlist':
                blob = await report.studentMasterlistCsv(selectedOffice.value)
                filename = 'student-masterlist.csv'
                break
            case 'pendingApplications':
                blob = await report.pendingApplicationsCsv()
                filename = 'pending-applications.csv'
                break
            case 'schoolSummary':
                blob = await report.schoolSummaryCsv()
                filename = 'school-summary.csv'
                break
            case 'placementUtilization':
                blob = await report.placementUtilizationCsv()
                filename = 'placement-utilization.csv'
                break
            case 'hoursProgress':
                blob = await report.hoursProgressCsv()
                filename = 'hours-progress.csv'
                break
            case 'completionSummary':
                blob = await report.completionSummaryCsv()
                filename = 'completion-summary.csv'
                break
            case 'rejectedApplications':
                blob = await report.rejectedApplicationsCsv()
                filename = 'rejected-applications.csv'
                break
            case 'importAudit':
                blob = await report.importAuditCsv()
                filename = 'import-audit.csv'
                break
            case 'ojtList': {
                const hasFilters = selectedOffice.value || selectedStatus.value || dateFrom.value || dateTo.value
                if (hasFilters) {
                    blob = await report.csvReportFiltered('/report/ojtList/csv/filtered', {
                        office: selectedOffice.value || undefined,
                        dateFrom: dateFrom.value || undefined,
                        dateTo: dateTo.value || undefined,
                    })
                } else {
                    blob = await report.csvExport('/report/ojtList/csv', selectedStatus.value ? parseInt(selectedStatus.value) : undefined)
                }
                filename = 'ojt-list.csv'
                break
            }
            case 'ojtPerOffice': {
                if (!selectedOffice.value) {
                    toast.add({ title: 'Please select an office', color: 'warning' })
                    return
                }
                blob = await report.csvExportPerOffice('/report/ojtPerOffice/csv', parseInt(selectedOffice.value))
                filename = 'ojt-per-office.csv'
                break
            }
            case 'expiringInternships':
                toast.add({ title: 'CSV export not available for Expiring Internships', color: 'warning' })
                return
            case 'officePerformance':
                blob = await report.officePerformanceCsv()
                filename = 'office-performance.csv'
                break
        }

        downloadBlob(blob, filename)
        toast.add({ title: 'CSV downloaded successfully', color: 'success' })
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
            <template #header>
                <div class="flex flex-col gap-4">
                    <UFormField label="Report Type">
                        <USelect
                            v-model="reportType"
                            :items="reportTypeOptions"
                            class="w-full md:w-96"
                        />
                    </UFormField>
                </div>
            </template>

            <div class="flex flex-wrap gap-4 items-end">
                <UFormField v-if="needsOffice" label="Office" required>
                    <USelect
                        v-model="selectedOffice"
                        :items="officeSelectItems"
                        placeholder="Select office"
                        class="w-full md:w-64"
                    />
                </UFormField>

                <UFormField v-if="isExpiring" label="Days Threshold">
                    <UInput
                        type="number"
                        v-model="days"
                        :min="1"
                        class="w-full md:w-32"
                    />
                </UFormField>

                <UFormField v-if="isOjtList || isOjtPerOffice" label="Status">
                    <USelect
                        v-model="selectedStatus"
                        :items="statusOptions"
                        placeholder="All statuses"
                        class="w-full md:w-48"
                    />
                </UFormField>

                <UFormField v-if="isOjtList || isOjtPerOffice" label="Date From">
                    <UInput
                        type="date"
                        v-model="dateFrom"
                        class="w-full md:w-48"
                    />
                </UFormField>

                <UFormField v-if="isOjtList || isOjtPerOffice" label="Date To">
                    <UInput
                        type="date"
                        v-model="dateTo"
                        class="w-full md:w-48"
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
                    v-if="reportType !== 'expiringInternships'"
                    icon="i-lucide-file-spreadsheet"
                    label="Download CSV"
                    color="secondary"
                    variant="solid"
                    :loading="loading"
                    @click="generateCsv"
                />
            </div>
        </UCard>
    </UMain>
</template>
