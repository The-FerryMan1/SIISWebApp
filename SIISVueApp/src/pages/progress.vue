<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
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

onMounted(async () => {
  const uuid = route.params.uuid
  if (!uuid || typeof uuid !== 'string') {
    toast.add({ title: 'Invalid student ID', color: 'error' })
    router.back()
    return
  }

  try {
    const { data } = await useAxios.get(`/progress/${uuid}`)
    progress.value = data
  } catch {
    error.value = true
    toast.add({ title: 'Failed to load progress', color: 'error' })
  } finally {
    loading.value = false
  }
})

const goBack = () => {
  router.back()
}
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
          <UPageCard title="Progress" icon="i-lucide-bar-chart-2" variant="outline">
            <p class="text-3xl font-bold" :class="progressColor">{{ progress.progressPercent }}%</p>
          </UPageCard>
        </div>

        <div class="mt-6">
          <h4 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">Progress Bar</h4>
          <div class="w-full h-4 bg-gray-200 rounded-full overflow-hidden">
            <div
              class="h-full rounded-full transition-all duration-500"
              :class="progressColor"
              :style="{ width: `${Math.min(progress.progressPercent, 100)}%` }"
            />
          </div>
        </div>

        <div class="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
          <UPageCard title="Training Hours This Week" icon="i-lucide-calendar" variant="outline">
            <p class="text-2xl font-bold text-blue-600">{{ progress.trainingHoursForWeek }}</p>
          </UPageCard>
          <UPageCard title="Total Training Hours Rendered" icon="i-lucide-activity" variant="outline">
            <p class="text-2xl font-bold text-purple-600">{{ progress.trainingHoursRendered }}</p>
          </UPageCard>
        </div>

        <template #footer>
          <div class="flex items-center justify-between text-sm text-gray-500">
            <span>Office: {{ progress.office }}</span>
            <span>Status: {{ progress.placementStatus }}</span>
          </div>
        </template>
      </UCard>
    </div>
  </UMain>
</template>
