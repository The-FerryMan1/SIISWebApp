<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAxios } from '../../fetch/axios'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const axios = useAxios

const uuid = route.params.uuid as string
const loading = ref(true)
const saving = ref(false)
const progressData = ref<{
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

interface DailyEntry {
  date: string
  activities: string
  hours: number
  inCharge: string
  remarks: string
  incidentReport: string
}

const dailyEntries = ref<DailyEntry[]>([
  { date: '', activities: '', hours: 0, inCharge: '', remarks: '', incidentReport: '' }
])

const weekStart = ref('')
const weekEnd = ref('')

const totalWeeklyHours = computed(() =>
  dailyEntries.value.reduce((sum, entry) => sum + (entry.hours || 0), 0)
)

onMounted(async () => {
  if (!uuid) {
    console.error('No UUID in route params:', route.params)
    toast.add({ title: 'Invalid student ID', color: 'error' })
    router.back()
    return
  }

  try {
    // Try progress endpoint first (requires placement)
    try {
      const { data } = await axios.get(`/progress/${uuid}`)
      progressData.value = data
    } catch (progressError: any) {
      console.error('Progress endpoint error:', progressError.response?.data || progressError.message)
      // If no placement, fetch student directly
      if (typeof progressError.response?.data === 'string' &&
        progressError.response.data.includes('Placement not found') ||
        progressError.response?.data?.message?.includes('Placement not found') ||
        progressError.response?.data?.title?.includes('Placement not found')) {
        const { data: student } = await axios.get(`/progress/student/${uuid}`)
        progressData.value = {
          studentUuid: student.studentUUID,
          studentName: student.fullName,
          office: 'Not assigned',
          totalHours: student.totalInternshipHours,
          accumulatedHours: 0,
          remainingHours: student.totalInternshipHours,
          trainingHoursRendered: 0,
          trainingHoursForWeek: 0,
          progressPercent: 0,
          placementStatus: 'Pending'
        }
      } else {
        throw progressError
      }
    }

    const today = new Date()
    const dayOfWeek = today.getDay()
    const startOfWeek = new Date(today)
    startOfWeek.setDate(today.getDate() - dayOfWeek)
    const endOfWeek = new Date(startOfWeek)
    endOfWeek.setDate(startOfWeek.getDate() + 6)

    weekStart.value = startOfWeek.toISOString().split('T')[0] ?? ''
    weekEnd.value = endOfWeek.toISOString().split('T')[0] ?? ''

    if (dailyEntries.value[0]) {
      dailyEntries.value[0].date = weekStart.value
    }
  } catch (error: any) {
    console.error('Form load error:', error.response?.data || error.message)
    const msg = error.response?.data?.title || error.response?.data?.message || 'Failed to load progress data'
    if (msg.includes('Placement not found') || msg.includes('placement')) {
      toast.add({ title: 'Student has no placement yet. Admin must assign & approve first.', color: 'warning' })
    } else {
      toast.add({ title: msg, color: 'error' })
    }
    router.back()
  } finally {
    loading.value = false
  }
})

function addDailyEntry() {
  const lastEntry = dailyEntries.value[dailyEntries.value.length - 1]
  const newDate = lastEntry
    ? new Date(lastEntry.date)
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
  const date = new Date(dateStr + 'T00:00:00')
  return date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' })
}

async function submitWeeklyReport() {
  if (!progressData.value) return

  const validEntries = dailyEntries.value.filter(e => e.activities.trim() && e.hours > 0)
  if (validEntries.length === 0) {
    toast.add({ title: 'Please add at least one daily entry with activities and hours', color: 'warning' })
    return
  }

  saving.value = true

  try {
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

    await axios.post('/weekly-report', request)
    toast.add({ title: 'Weekly report submitted successfully!', color: 'success' })
    router.back()
  } catch (error: any) {
    const msg = error.response?.data?.title || error.message || 'Failed to submit weekly report'
    toast.add({ title: msg, color: 'error' })
  } finally {
    saving.value = false
  }
}

function goBack() {
  router.back()
}
</script>

<template>
  <UMain class="py-8">
    <UButton @click="goBack" variant="ghost" class="mb-4" icon="i-lucide-arrow-left" label="Back to Progress" />

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-4">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-gray-400" />
      <p class="text-gray-500">Loading...</p>
    </div>

    <div v-else-if="progressData" class="space-y-6">
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-2xl font-bold text-primary">Create Weekly Report</h2>
              <p class="text-sm text-gray-500 mt-1">{{ progressData.studentName }} • {{ progressData.office }}</p>
            </div>
          </div>
        </template>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <UPageCard title="Week Period" icon="i-lucide-calendar" variant="outline">
            <div class="flex items-center gap-4 text-sm">
              <span>{{ formatDate(weekStart) }}</span>
              <UIcon name="i-lucide-arrow-right" class="text-gray-400" />
              <span>{{ formatDate(weekEnd) }}</span>
            </div>
          </UPageCard>
          <UPageCard title="Total Hours This Week" icon="i-lucide-clock" variant="outline">
            <p class="text-3xl font-bold text-blue-600">{{ totalWeeklyHours }}</p>
          </UPageCard>
          <UPageCard title="Remaining Hours" icon="i-lucide-hourglass" variant="outline">
            <p class="text-3xl font-bold text-orange-600">{{ progressData.remainingHours }}</p>
          </UPageCard>
        </div>

        <div class="border-t pt-6">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-lg font-semibold">Daily Entries</h3>
              <UButton @click="addDailyEntry" icon="i-lucide-plus" label="Add Day" size="sm" />
            </div>

            <div v-for="(entry, index) in dailyEntries" :key="index" class="space-y-4 p-4 bg-gray-50 rounded-lg border">
              <div class="flex items-center justify-between mb-2">
                <h4 class="font-medium">{{ formatDate(entry.date) || `Day ${index + 1}` }}</h4>
                <UButton v-if="dailyEntries.length > 1" @click="removeDailyEntry(index)" variant="ghost" size="sm"
                  color="error" icon="i-lucide-trash-2" />
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium mb-1">Date</label>
                  <UInput v-model="entry.date" type="date" />
                </div>
                <div>
                  <label class="block text-sm font-medium mb-1">Hours</label>
                  <UInput v-model.number="entry.hours" type="number" min="0" />
                </div>
                <div class="md:col-span-2">
                  <label class="block text-sm font-medium mb-1">Activities</label>
                  <UInput v-model="entry.activities" placeholder="Description of daily activities" />
                </div>
                <div class="md:col-span-2">
                  <label class="block text-sm font-medium mb-1">In Charge</label>
                  <UInput v-model="entry.inCharge" placeholder="Person in charge" />
                </div>
                <div class="md:col-span-2">
                  <label class="block text-sm font-medium mb-1">Remarks</label>
                  <UInput v-model="entry.remarks" placeholder="Additional remarks" />
                </div>
                <div class="md:col-span-2">
                  <label class="block text-sm font-medium mb-1">Incident Report</label>
                  <UInput v-model="entry.incidentReport" placeholder="Incident report (if any)" />
                </div>
              </div>
            </div>
          </div>

          <div class="flex justify-end gap-4 pt-6 border-t">
            <UButton @click="goBack" variant="ghost" label="Cancel" />
            <UButton @click="submitWeeklyReport" :loading="saving" :disabled="saving" label="Submit Weekly Report" color="primary" />
          </div>
        </UCard>
    </div>

  </UMain>

</template>