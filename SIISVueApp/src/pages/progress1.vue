<script setup lang="ts">
import { onMounted, ref, computed, h } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAxios } from '../fetch/axios'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const loading = ref(true)
const error = ref(false)
const progress = ref<{
  studentUuid: string
  studentName: string
  office: string
  totalHours: number
  accumulatedHours: number
  remainingHours: number
  trainingHoursRendered: number
  trainingHoursForWeek: number
  progressPercent: number
  placementStatus: string
} | null>(null)

interface WeeklyReport {
  id: number
  weekStartDate: string
  weekEndDate: string
  totalHours: number
  createdAt: string
  dailyReports: DailyReport[]
}

interface DailyReport {
  id: number
  date: string
  activities: string
  hours: number
  inCharge?: string
  remarks?: string
  incidentReport?: string
}

const weeklyReports = ref<WeeklyReport[]>([])
const weeklyLoading = ref(false)

const weeklyReportModal = ref(false)
const savingWeeklyReport = ref(false)
const dailyEntries = ref<{
  date: string
  activities: string
  hours: number
  inCharge: string
  remarks: string
  incidentReport: string
}[]>([
  { date: '', activities: '', hours: 0, inCharge: '', remarks: '', incidentReport: '' }
])
const weekStart = ref('')
const weekEnd = ref('')

const totalWeeklyHours = computed(() => 
  dailyEntries.value.reduce((sum, entry) => sum + (entry.hours || 0), 0)
)

const progressColor = computed(() => {
  if (!progress.value) return 'bg-gray-500'
  const p = progress.value.progressPercent
  if (p >= 100) return 'bg-green-500'
  if (p >= 50) return 'bg-yellow-500'
  return 'bg-red-500'
})

const statusColor = computed(() => {
  if (!progress.value) return 'neutral'
  return progress.value.placementStatus === 'Finished' ? 'success' : 'warning'
})

async function fetchProgress() {
  const uuid = route.params.uuid
  if (!uuid || typeof uuid !== 'string') {
    toast.add({ title: 'Invalid student ID', color: 'error' })
    router.back()
    return
  }

  try {
    const { data } = await useAxios.get(`/progress/${uuid}`)
    progress.value = data
  } catch (error: any) {
    error.value = true
    const msg = error.response?.data?.title || error.response?.data?.message || 'Failed to load progress'
    if (msg.includes('Placement not found') || msg.includes('placement')) {
      toast.add({ title: 'Student not assigned to an office yet. Admin must assign & approve first.', color: 'warning' })
    } else {
      toast.add({ title: msg, color: 'error' })
    }
  } finally {
    loading.value = false
  }
}

async function fetchWeeklyReports() {
  const uuid = route.params.uuid
  if (!uuid || typeof uuid !== 'string') return
  
  weeklyLoading.value = true
  try {
    const { data } = await useAxios.get(`/weekly-report/student/${uuid}`)
    weeklyReports.value = data
  } catch {
    weeklyReports.value = []
  } finally {
    weeklyLoading.value = false
  }
}

function openWeeklyReportModal() {
  if (!progress.value) return
  
  const today = new Date()
  const dayOfWeek = today.getDay()
  const startOfWeek = new Date(today)
  startOfWeek.setDate(today.getDate() - dayOfWeek)
  const endOfWeek = new Date(startOfWeek)
  endOfWeek.setDate(startOfWeek.getDate() + 6)
  
   weekStart.value = startOfWeek.toISOString().split('T')[0] ?? ''
   weekEnd.value = endOfWeek.toISOString().split('T')[0] ?? ''
  
  dailyEntries.value = [
    { date: weekStart.value, activities: '', hours: 0, inCharge: '', remarks: '', incidentReport: '' }
  ]
  
  weeklyReportModal.value = true
}

function closeWeeklyReportModal() {
  weeklyReportModal.value = false
  savingWeeklyReport.value = false
}

function addDailyEntry() {
   const newDate = dailyEntries.value.length > 0
    ? new Date(dailyEntries.value[dailyEntries.value.length - 1]!.date)
    : new Date(weekStart.value)
  newDate.setDate(newDate.getDate() + 1)
  
  if (newDate <= new Date(weekEnd.value)) {
    dailyEntries.value.push({
       date: newDate.toISOString().split('T')[0] ?? '',
      activities: '',
      hours: 0,
      inCharge: '',
      remarks: '',
      incidentReport: ''
    })
  } else {
    toast.add({ title: 'Cannot add more days beyond week end', color: 'warning' })
  }
}

function removeDailyEntry(index: number) {
  if (dailyEntries.value.length > 1) {
    dailyEntries.value.splice(index, 1)
  }
}

function formatDate(dateStr: string) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

function formatDateShort(dateStr: string) {
  if (!dateStr) return ''
  const date = new Date(dateStr + 'T00:00:00')
  return date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' })
}

const goBack = () => {
  router.back()
}

async function submitWeeklyReport() {
  if (!progress.value) return
  
  const validEntries = dailyEntries.value.filter(e => e.activities.trim() && e.hours > 0)
  if (validEntries.length === 0) {
    toast.add({ title: 'Please add at least one daily entry with activities and hours', color: 'warning' })
    return
  }

  savingWeeklyReport.value = true
  
  try {
    const uuid = route.params.uuid as string
    const request = {
      StudentUuid: uuid,
      WeekStartDate: weekStart.value,
      DailyReports: validEntries.map(entry => ({
        Date: entry.date,
        Activities: entry.activities,
        Hours: entry.hours,
        InCharge: entry.inCharge || undefined,
        Remarks: entry.remarks || undefined,
        IncidentReport: entry.incidentReport || undefined
      }))
    }

    await useAxios.post('weekly-report', request)
    toast.add({ title: 'Weekly report submitted successfully!', color: 'success' })
    closeWeeklyReportModal()
    await fetchWeeklyReports()
  } catch (error: any) {
    const msg = error.response?.data?.title || error.message || 'Failed to submit weekly report'
    toast.add({ title: msg, color: 'error' })
  } finally {
    savingWeeklyReport.value = false
  }
}

const viewWeeklyReport = (report: WeeklyReport) => {
  console.log('View report:', report)
}

onMounted(async () => {
  await fetchProgress()
  await fetchWeeklyReports()
})
</script>

<template>
  <UMain class="py-8">
    <UButton @click="goBack" variant="ghost" class="mb-4" icon="i-lucide-arrow-left" label="Back" />

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-4">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-gray-400" />
      <p class="text-gray-500">Loading progress...</p>
    </div>

    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 gap-4">
      <UIcon name="i-lucide-alert-circle" class="w-12 h-12 text-red-500" />
      <p class="text-gray-500">Failed to load progress data.</p>
      <UButton @click="goBack" label="Go Back" color="primary" />
    </div>

    <div v-else-if="progress" class="space-y-6">
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-2xl font-bold text-primary">OJT Progress</h2>
              <p class="text-sm text-gray-500 mt-1">{{ progress.studentName }}</p>
            </div>
            <UBadge :color="statusColor" variant="soft" size="lg">
              {{ progress.placementStatus }}
            </UBadge>
          </div>
        </template>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <UPageCard title="Total Hours" icon="i-lucide-clock" variant="outline">
            <p class="text-3xl font-bold text-primary">{{ progress.totalHours }}</p>
          </UPageCard>
          <UPageCard title="Accumulated Hours" icon="i-lucide-check-circle" variant="outline">
            <p class="text-3xl font-bold text-green-600">{{ progress.accumulatedHours }}</p>
          </UPageCard>
          <UPageCard title="Remaining Hours" icon="i-lucide-hourglass" variant="outline">
            <p class="text-3xl font-bold text-orange-600">{{ progress.remainingHours }}</p>
          </UPageCard>
          <UP
