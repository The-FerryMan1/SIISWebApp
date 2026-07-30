<script setup lang="ts">
import type { SelectItem } from '@nuxt/ui'
import { ref, computed } from 'vue'
import CsvPdfModal from '../../components/csvPdfModal.vue'
import { useReportStore } from '../../stores/report.ts'
import { OfficeOptions } from '../../shared/officeEnum.ts'

const overlay = useOverlay()
const selectFileTypeModal = overlay.create(CsvPdfModal)
const report = useReportStore()
const toast = useToast()

const loading = ref(false)
const activeCategory = ref<string>('all')

const statusOptions: SelectItem[] = [
  { label: 'Pending', value: 0 },
  { label: 'Approved', value: 1 },
  { label: 'Rejected', value: 2 },
]

interface ReportItem {
  id: string
  title: string
  description: string
  icon: string
  color: 'primary' | 'success' | 'warning' | 'error' | 'info' | 'neutral'
  formats: readonly ('pdf' | 'csv')[]
  action: string
  paramItems?: SelectItem[]
  category: string
}

const reportItems = ref<ReportItem[]>([
  {
    id: 'students',
    title: 'Student Masterlist',
    description: 'Complete directory with contact details, grade level, and application status for all students.',
    icon: 'i-lucide-users',
    color: 'primary',
    formats: ['pdf', 'csv'] as const,
    action: 'students',
    category: 'student',
  },
  {
    id: 'offices-summary',
    title: 'Office Statistics',
    description: 'OJT count and percentage distribution across all provincial offices.',
    icon: 'i-lucide-bar-chart-3',
    color: 'info',
    formats: ['pdf'] as const,
    action: 'officesSummary',
    category: 'office',
  },
  {
    id: 'pending-applications',
    title: 'Pending Applications',
    description: 'Applications currently awaiting review and approval by the administration.',
    icon: 'i-lucide-clock',
    color: 'warning',
    formats: ['pdf', 'csv'] as const,
    action: 'pendingApplications',
    category: 'application',
  },
  {
    id: 'ojts-by-status',
    title: 'OJTs by Status',
    description: 'Filter OJT list by approval status: Pending, Approved, or Rejected.',
    icon: 'i-lucide-user-check',
    color: 'success',
    formats: ['pdf', 'csv'] as const,
    action: 'ojts',
    paramItems: statusOptions,
    category: 'application',
  },
  {
    id: 'ojts-per-office',
    title: 'OJT Per Office',
    description: 'Students assigned to a specific office with full internship details.',
    icon: 'i-lucide-building',
    color: 'primary',
    formats: ['pdf'] as const,
    action: 'ojtsPerOffice',
    paramItems: OfficeOptions,
    category: 'office',
  },
  {
    id: 'missing-requirements',
    title: 'Missing Requirements',
    description: 'Approved students who have not yet submitted their required documents.',
    icon: 'i-lucide-file-x',
    color: 'error',
    formats: ['pdf', 'csv'] as const,
    action: 'missingRequirements',
    category: 'requirements',
  },
  {
    id: 'requirements-checklist',
    title: 'Requirements Checklist',
    description: 'All submitted requirements per student with file names and submission dates.',
    icon: 'i-lucide-list-checks',
    color: 'info',
    formats: ['pdf', 'csv'] as const,
    action: 'requirementsChecklist',
    category: 'requirements',
  },
  {
    id: 'internship-hours',
    title: 'Internship Hours',
    description: 'Total and average internship hours per student with office assignment.',
    icon: 'i-lucide-timer',
    color: 'primary',
    formats: ['pdf'] as const,
    action: 'internshipHours',
    category: 'internship',
  },
  {
    id: 'expiring-internships',
    title: 'Expiring Internships',
    description: 'Internships ending within the next 30 days for proactive follow-up.',
    icon: 'i-lucide-alert-triangle',
    color: 'warning',
    formats: ['pdf', 'csv'] as const,
    action: 'expiringInternships',
    category: 'internship',
  },
])

const categories = computed(() => {
  const cats = new Map<string, { label: string; icon: string; value: string }>()
  cats.set('all', { label: 'All Reports', icon: 'i-lucide-layout-grid', value: 'all' })
  reportItems.value.forEach((item) => {
    if (!cats.has(item.category)) {
      const labels: Record<string, { label: string; icon: string; value: string }> = {
        student: { label: 'Students', icon: 'i-lucide-users', value: 'student' },
        office: { label: 'Offices', icon: 'i-lucide-building', value: 'office' },
        application: { label: 'Applications', icon: 'i-lucide-file-text', value: 'application' },
        requirements: { label: 'Requirements', icon: 'i-lucide-folder', value: 'requirements' },
        internship: { label: 'Internships', icon: 'i-lucide-timer', value: 'internship' },
      }
      cats.set(item.category, labels[item.category] ?? { label: item.category, icon: 'i-lucide-folder', value: item.category })
    }
  })
  return Array.from(cats.values())
})

const filteredReports = computed(() => {
  if (activeCategory.value === 'all') return reportItems.value
  return reportItems.value.filter((item) => item.category === activeCategory.value)
})

const formatLabel = (format: string) => format.toUpperCase()
const formatColor = (format: string): 'primary' | 'success' => format === 'pdf' ? 'primary' : 'success'

const openModal = (action: string, formats: readonly ('pdf' | 'csv')[], paramItems?: SelectItem[]) => {
  const instance = selectFileTypeModal.open({
    title: 'Export Report',
    description: 'Choose your preferred file format',
    selectPlaceholder: 'Select option',
    items: paramItems,
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
    <UMain class="space-y-8">
      <div class="px-4 py-2">
        <h2 class="text-4xl font-black text-primary tracking-tight">Reports</h2>
        <p class="text-muted text-sm mt-1">
          Generate and export PDF or CSV reports for students, offices, applications, and more.
        </p>
      </div>

      <div class="px-4">
        <USelectMenu
          v-model="activeCategory"
          :items="categories as SelectItem[]"
          value-attribute="value"
          label-attribute="label"
          placeholder="Filter reports..."
          class="w-full sm:w-64"
          size="md"
        />
      </div>

      <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <UCard
          v-for="item in filteredReports"
          :key="item.id"
          class="transition hover:shadow-lg hover:border-primary/50"
          variant="outline"
        >
          <div class="flex items-start gap-4">
            <div
              class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl"
              :class="`bg-${item.color}/10 text-${item.color}`"
            >
              <UIcon :name="item.icon" class="h-6 w-6" />
            </div>

            <div class="flex-1 min-w-0">
              <h3 class="font-semibold text-primary truncate">{{ item.title }}</h3>
              <p class="text-sm text-muted mt-1 line-clamp-2">{{ item.description }}</p>

              <div class="flex items-center justify-between mt-4">
                <div class="flex gap-1.5">
                  <UBadge
                    v-for="format in item.formats"
                    :key="format"
                    :color="formatColor(format)"
                    variant="subtle"
                    size="xs"
                  >
                    {{ formatLabel(format) }}
                  </UBadge>
                </div>

                <UButton
                  icon="i-lucide-download"
                  size="xs"
                  color="primary"
                  variant="ghost"
                  :loading="loading"
                  @click="openModal(item.action, item.formats, item.paramItems)"
                />
              </div>
            </div>
          </div>
        </UCard>
      </div>
    </UMain>
  </template>
