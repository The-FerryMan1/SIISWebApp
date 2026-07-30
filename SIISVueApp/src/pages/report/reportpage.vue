<script setup lang="ts">
import { ref } from 'vue'
import CsvPdfModal from '../../components/csvPdfModal.vue'
import { useReportStore } from '../../stores/report.ts'
import type { SelectItem } from '@nuxt/ui'
import { OfficeOptions } from '../../shared/officeEnum.ts'

const overlay = useOverlay()
const selectFileTypeModal = overlay.create(CsvPdfModal)
const report = useReportStore()
const toast = useToast()

const loading = ref(false)

const statusOptions: SelectItem[] = [
  { label: 'Pending', value: 0 },
  { label: 'Approved', value: 1 },
  { label: 'Rejected', value: 2 },
]

interface ReportItem {
  title: string
  description: string
  icon: string
  formats: readonly ('pdf' | 'csv')[]
  action: string
  paramItems?: SelectItem[]
}

interface ReportCategory {
  title: string
  description: string
  items: ReportItem[]
}

const reportCategories = ref<ReportCategory[]>([
  {
    title: 'Student Reports',
    description: 'Student masterlist, demographics, and academic information',
    items: [
      {
        title: 'Student Masterlist',
        description: 'Complete directory with contact details and status',
        icon: 'i-lucide-users',
        formats: ['pdf', 'csv'] as const,
        action: 'students',
        paramItems: undefined,
      },
    ],
  },
  {
    title: 'Office Reports',
    description: 'Office placement statistics and OJT distribution',
    items: [
      {
        title: 'Office Statistics Summary',
        description: 'OJT count and share percentage per office',
        icon: 'i-lucide-bar-chart-3',
        formats: ['pdf'] as const,
        action: 'officesSummary',
        paramItems: undefined,
      },
    ],
  },
  {
    title: 'Application Reports',
    description: 'Application status and approval tracking',
    items: [
      {
        title: 'Pending Applications',
        description: 'Applications awaiting review and approval',
        icon: 'i-lucide-clock',
        formats: ['pdf', 'csv'] as const,
        action: 'pendingApplications',
        paramItems: undefined,
      },
      {
        title: 'OJTs by Status',
        description: 'List of OJTs filtered by approval status',
        icon: 'i-lucide-user-check',
        formats: ['pdf', 'csv'] as const,
        action: 'ojts',
        paramItems: statusOptions,
      },
      {
        title: 'OJT Per Office',
        description: 'Students assigned to a specific office',
        icon: 'i-lucide-building',
        formats: ['pdf'] as const,
        action: 'ojtsPerOffice',
        paramItems: OfficeOptions,
      },
    ],
  },
  {
    title: 'Requirements Reports',
    description: 'Document submission tracking and compliance',
    items: [
      {
        title: 'Missing Requirements',
        description: 'Approved students without submitted documents',
        icon: 'i-lucide-file-x',
        formats: ['pdf', 'csv'] as const,
        action: 'missingRequirements',
        paramItems: undefined,
      },
      {
        title: 'Requirements Checklist',
        description: 'All submitted requirements per student',
        icon: 'i-lucide-list-checks',
        formats: ['pdf', 'csv'] as const,
        action: 'requirementsChecklist',
        paramItems: undefined,
      },
    ],
  },
  {
    title: 'Internship Reports',
    description: 'Hours tracking and expiration monitoring',
    items: [
      {
        title: 'Internship Hours Summary',
        description: 'Total and average hours per student',
        icon: 'i-lucide-timer',
        formats: ['pdf'] as const,
        action: 'internshipHours',
        paramItems: undefined,
      },
      {
        title: 'Expiring Internships',
        description: 'Internships ending within the next 30 days',
        icon: 'i-lucide-alert-triangle',
        formats: ['pdf', 'csv'] as const,
        action: 'expiringInternships',
        paramItems: undefined,
      },
    ],
  },
])

const formatLabels: Record<string, string> = {
  pdf: 'PDF',
  csv: 'CSV',
}

const formatColors: Record<string, 'primary' | 'success'> = {
  pdf: 'primary',
  csv: 'success',
}

const openModal = (action: string, formats: readonly ('pdf' | 'csv')[], paramItems?: SelectItem[]) => {
  const items = paramItems ?? formats.map((f) => ({
    label: formatLabels[f],
    value: f,
  }))

  const instance = selectFileTypeModal.open({
    title: 'Export Report',
    description: 'Choose your preferred file format',
    selectPlaceholder: 'Select option',
    items,
    formats: Array.from(formats),
  })

  instance.result.then((result) => {
    if (!result || result.format === 'none') return
    handleExport(action, result.format, result.selected)
  })
}

const handleExport = async (action: string, format: string, selectedParam?: number) => {
  try {
    loading.value = true
    let blob: Blob | undefined

    switch (action) {
      case 'students':
        blob = await report.studentsReport(format)
        downloadFile(blob, format, 'students-masterlist')
        break
      case 'officesSummary':
        blob = await report.officesSummary()
        downloadFile(blob, 'pdf', 'offices-summary')
        break
      case 'pendingApplications':
        blob = await report.pendingApplications(format)
        downloadFile(blob, format, 'pending-applications')
        break
      case 'ojts':
        blob = await report.pdfReport('/report/ojtList' + (format === 'csv' ? '/csv' : ''), selectedParam ?? 1)
        downloadFile(blob, format, 'ojt-list')
        break
      case 'ojtsPerOffice':
        blob = await report.pdfReportPerOffice('/report/ojtPerOffice', selectedParam ?? 1)
        downloadFile(blob, 'pdf', 'ojt-per-office')
        break
      case 'missingRequirements':
        blob = await report.missingRequirements(format)
        downloadFile(blob, format, 'missing-requirements')
        break
      case 'requirementsChecklist':
        blob = await report.requirementsChecklist(format)
        downloadFile(blob, format, 'requirements-checklist')
        break
      case 'internshipHours':
        blob = await report.internshipHours()
        downloadFile(blob, 'pdf', 'internship-hours')
        break
      case 'expiringInternships':
        blob = await report.expiringInternships(format)
        downloadFile(blob, format, 'expiring-internships')
        break
    }

    toast.add({
      title: 'Report exported successfully',
      description: `Your ${format.toUpperCase()} report is ready.`,
      color: 'success',
    })
  } catch (error) {
    toast.add({
      title: 'Export failed',
      description: 'Something went wrong while generating the report.',
      color: 'error',
    })
  } finally {
    loading.value = false
  }
}

const downloadFile = (blob: Blob | undefined, ext: string, filename: string) => {
  if (!blob) return
  const downloadUrl = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = downloadUrl
  a.download = `${filename}-${new Date().toISOString().slice(0, 10)}.${ext}`
  a.click()
  URL.revokeObjectURL(downloadUrl)
}
</script>

<template>
  <UMain class="space-y-10">
    <div class="px-4 py-2 my-5">
      <div>
        <h2 class="text-4xl font-black text-primary">Reports</h2>
        <p class="text-muted text-sm">
          Generate and export PDF or CSV reports for students, offices, applications, and more.
        </p>
      </div>
    </div>

    <div v-for="category in reportCategories" :key="category.title" class="space-y-4">
      <div class="px-4">
        <h3 class="text-lg font-semibold text-primary">{{ category.title }}</h3>
        <p class="text-muted text-sm">{{ category.description }}</p>
      </div>

      <UPageGrid>
        <UPageCard
          v-for="item in category.items"
          :key="item.title"
          :spotlight="true"
          spotlight-color="primary"
          class="transition hover:shadow-lg"
          :title="item.title"
          :description="item.description"
          :icon="item.icon"
          :ui="{ footer: 'border-t border-default pt-4' }"
        >
          <template #footer>
            <div class="flex items-center justify-between">
              <div class="flex gap-1">
                <UBadge
                  v-for="format in item.formats"
                  :key="format"
                  :color="formatColors[format]"
                  variant="subtle"
                  size="xs"
                >
                  {{ formatLabels[format] }}
                </UBadge>
              </div>
              <UButton
                icon="i-lucide-download"
                size="xs"
                color="primary"
                variant="ghost"
                :loading="loading"
                @click="openModal(item.action, item.formats, item.paramItems)"
              >
                Export
              </UButton>
            </div>
          </template>
        </UPageCard>
      </UPageGrid>
    </div>
  </UMain>
</template>
