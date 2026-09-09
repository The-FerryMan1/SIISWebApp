<script setup lang="ts">
import { ref, onMounted, h } from 'vue'
import { useReportStore } from '../../stores/report'

interface WeeklyHistoryDto {
  id: number
  officeName: string
  weekStartDate: string
  weekEndDate: string
  reportPeriod: string
  totalStudents: number
  totalHoursThisWeek: number
  generatedAt: string
}

const report = useReportStore()
const loading = ref(false)
const history = ref<WeeklyHistoryDto[]>([])

onMounted(async () => {
  loading.value = true
  try {
    history.value = await report.getWeeklyHistory() || []
  } finally {
    loading.value = false
  }
})

function downloadWeeklyReport(item: WeeklyHistoryDto) {
  const filters = {
    dateFrom: item.weekStartDate,
    dateTo: item.weekEndDate,
  }
  report.officeWeeklyPdf(filters).then((blob) => {
    if (blob) {
      const url = URL.createObjectURL(blob)
      const win = window.open(url, '_blank')
      win?.print()
      URL.revokeObjectURL(url)
    }
  })
}
</script>

<template>
  <UMain class="space-y-6">
    <div>
      <h1 class="text-4xl font-black text-primary tracking-tight">Weekly Report History</h1>
      <p class="text-muted text-sm mt-1">View and download past weekly reports</p>
    </div>

    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-semibold">Weekly History</h2>
          <UBadge v-if="history.length" color="primary" variant="subtle">
            {{ history.length }} reports
          </UBadge>
        </div>
      </template>

      <div v-if="loading" class="flex justify-center py-8">
        <USpinner size="lg" />
      </div>

      <div v-else-if="history.length === 0" class="text-center py-12 text-muted">
        <UIcon name="i-lucide-calendar-clock" class="text-4xl mb-2" />
        <p>No weekly reports yet</p>
        <p class="text-sm mt-1">Weekly reports will appear here after they are generated</p>
      </div>

      <UTable v-else
        :data="history"
        :columns="[
          { accessorKey: 'reportPeriod', header: 'Week Period' },
          { accessorKey: 'officeName', header: 'Office' },
          { accessorKey: 'totalStudents', header: 'Students' },
          { accessorKey: 'totalHoursThisWeek', header: 'Hours This Week' },
          { accessorKey: 'generatedAt', header: 'Generated', cell: ({ row }) => new Date(row.original.generatedAt).toLocaleString() },
          { id: 'action', header: '', cell: ({ row }) => h('UButton', { icon: 'i-lucide-download', size: 'sm', variant: 'ghost', label: 'Download', onClick: () => downloadWeeklyReport(row.original) }) }
        ]"
        class="w-full"
      />
    </UCard>
  </UMain>
</template>
