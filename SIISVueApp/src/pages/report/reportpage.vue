<script setup lang="ts">
import { ref, computed } from 'vue'
import { useReportStore } from '../../stores/report.ts'
import { OfficeNameLabels } from '../admin/types/officeSelectValue'
import { OfficeOptions } from '../../shared/officeEnum.ts'
import type { SelectItem } from '@nuxt/ui'

const report = useReportStore()
const toast = useToast()

const reportType = ref<'ojtList' | 'ojtPerOffice' | 'expiringInternships'>('ojtList')
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
    { label: 'OJT List (All Offices)', value: 'ojtList' },
    { label: 'OJT Per Office', value: 'ojtPerOffice' },
    { label: 'Expiring Internships', value: 'expiringInternships' },
]

const isOjtList = computed(() => reportType.value === 'ojtList')
const isExpiring = computed(() => reportType.value === 'expiringInternships')
const officeSelectItems = computed(() => {
    if (isOjtList.value || isExpiring.value) return [{ label: 'All Offices', value: '' }, ...OfficeOptions]
    return OfficeOptions
})

async function generatePreview() {
    if (reportType.value === 'ojtPerOffice' && !selectedOffice.value) {
        toast.add({ title: 'Please select an office for per-office report', color: 'warning' })
        return
    }

    loading.value = true
    try {
        let blob: Blob | undefined
        let filename = 'report'

        if (reportType.value === 'expiringInternships') {
            const officeId = selectedOffice.value ? parseInt(selectedOffice.value) : undefined
            blob = await report.adminExpiringPdf(officeId, days.value)
            filename = `expiring-internships-${days.value}days`
        } else if (isOjtList.value) {
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
        } else {
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
        }

        if (blob) {
            const url = URL.createObjectURL(blob)
            const win = window.open(url, '_blank')
            win?.print()
            setTimeout(() => URL.revokeObjectURL(url), 1000)
        }
    } catch {
        toast.add({ title: 'Failed to generate report', color: 'error' })
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
                <UFormField v-if="isOjtList || isExpiring" label="Office">
                    <USelect
                        v-model="selectedOffice"
                        :items="officeSelectItems"
                        placeholder="All offices"
                        class="w-full md:w-64"
                    />
                </UFormField>

                <UFormField v-if="!isOjtList && !isExpiring" label="Office" required>
                    <USelect
                        v-model="selectedOffice"
                        :items="OfficeOptions"
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

                <UFormField v-if="!isExpiring" label="Status">
                    <USelect
                        v-model="selectedStatus"
                        :items="statusOptions"
                        placeholder="All statuses"
                        class="w-full md:w-48"
                    />
                </UFormField>

                <UFormField v-if="!isExpiring" label="Date From">
                    <UInput
                        type="date"
                        v-model="dateFrom"
                        class="w-full md:w-48"
                    />
                </UFormField>

                <UFormField v-if="!isExpiring" label="Date To">
                    <UInput
                        type="date"
                        v-model="dateTo"
                        class="w-full md:w-48"
                    />
                </UFormField>

                <UButton
                    icon="i-lucide-file-text"
                    label="Generate Report"
                    color="primary"
                    variant="solid"
                    :loading="loading"
                    @click="generatePreview"
                />
            </div>
        </UCard>
    </UMain>
</template>
