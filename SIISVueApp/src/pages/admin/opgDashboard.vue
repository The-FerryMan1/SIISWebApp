<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import { computed, onMounted, ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart, LineChart, PieChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import { UseAuthStore } from '../../stores/auth'
import { useOfficeStore } from '../../stores/office'
import { useOJtStore } from '../../stores/ojt'
import { useApplicationStore } from '../../stores/application'

use([CanvasRenderer, BarChart, LineChart, PieChart, GridComponent, TooltipComponent, LegendComponent])

const auth = UseAuthStore()
const officeStore = useOfficeStore()
const ojtStore = useOJtStore()
const applicationStore = useApplicationStore()

const loading = ref(true)

const offices = computed(() => officeStore.offices ?? [])
const ojts = computed(() => ojtStore.ojts ?? [])
const applications = computed(() => applicationStore.applications ?? [])

const totalOffices = computed(() => offices.value.length)
const totalOjts = computed(() => ojts.value.length)
const totalApplications = computed(() => applications.value.length)
const pendingApplications = computed(() => applications.value.filter((a) => a.status === 'Pending').length)
const approvedApplications = computed(() => applications.value.filter((a) => a.status === 'Approved').length)
const rejectedApplications = computed(() => applications.value.filter((a) => a.status === 'Rejected').length)

const placementUtilization = computed(() => {
  return offices.value.map((office) => ({
    name: office.officeName,
    assigned: (office as any).students?.length ?? 0,
    capacity: (office as any).students?.length ?? 0,
  }))
})

const placementOption = computed(() => ({
  tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
  grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true },
  xAxis: { type: 'category', data: placementUtilization.value.map((i) => i.name), axisLabel: { fontSize: 10, rotate: 25, interval: 0 } },
  yAxis: { type: 'value', minInterval: 1 },
  series: [{
    type: 'bar',
    data: placementUtilization.value.map((i) => i.assigned),
    itemStyle: { color: '#6366f1' },
  }],
}))

const completionTrend = computed(() => {
  const counts = new Map<string, number>()
  applications.value.forEach((app) => {
    if (app.status === 'Approved') {
      const key = new Date(app.updatedAt || app.createdAt).toISOString().slice(0, 10)
      counts.set(key, (counts.get(key) ?? 0) + 1)
    }
  })
  return Array.from(counts.entries())
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([date, count]) => ({ date, count }))
})

const completionOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true },
  xAxis: { type: 'category', data: completionTrend.value.map((i) => new Date(i.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })), axisLabel: { rotate: 20 } },
  yAxis: { type: 'value', minInterval: 1 },
  series: [{
    type: 'line',
    smooth: true,
    data: completionTrend.value.map((i) => i.count),
    itemStyle: { color: '#10b981' },
    lineStyle: { width: 3 },
    areaStyle: { color: 'rgba(16, 185, 129, 0.12)' },
  }],
}))

const statusOption = computed(() => ({
  tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
  legend: { bottom: 0 },
  series: [{
    type: 'pie',
    radius: '70%',
    data: [
      { value: pendingApplications.value, name: 'Pending', itemStyle: { color: '#f59e0b' } },
      { value: approvedApplications.value, name: 'Approved', itemStyle: { color: '#10b981' } },
      { value: rejectedApplications.value, name: 'Rejected', itemStyle: { color: '#ef4444' } },
    ],
    label: { formatter: '{b}: {c} ({d}%)' },
  }],
}))

type OfficeRow = { office: string; ojts: number; pending: number; approved: number }

const officeTableColumns: TableColumn<OfficeRow>[] = [
  { accessorKey: 'office', header: 'Office' },
  { accessorKey: 'ojts', header: 'Total OJTs' },
  { accessorKey: 'pending', header: 'Pending' },
  { accessorKey: 'approved', header: 'Approved' },
]

const officeTableData = computed<OfficeRow[]>(() => {
  return offices.value.map((office) => {
    const officeName = office.officeName
    const officeOjts = ojts.value.filter((o) => o.officeName === officeName)
    const pending = applications.value.filter((a) => a.status === 'Pending' && officeOjts.some((o) => o.officeName === officeName)).length
    const approved = applications.value.filter((a) => a.status === 'Approved' && officeOjts.some((o) => o.officeName === officeName)).length
    return { office: officeName, ojts: officeOjts.length, pending, approved }
  })
})

onMounted(async () => {
  loading.value = true
  try {
    await Promise.all([
      officeStore.officeInit(),
      ojtStore.ojtInit(),
      applicationStore.applicationInit(),
    ])
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="space-y-6">
    <div class="px-1 py-2 my-10">
      <div>
        <h2 class="text-4xl font-black text-primary">OPG Dashboard</h2>
        <p class="text-muted text-sm">Oversight view across all provincial offices and OJT placements.</p>
      </div>
    </div>

    <template v-if="loading">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <USkeleton v-for="i in 4" :key="i" class="h-24 w-full" />
      </div>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6">
        <USkeleton class="h-80 w-full" />
        <USkeleton class="h-80 w-full" />
      </div>
    </template>

    <template v-else>
      <UPageGrid>
        <UPageCard title="Total Offices" icon="i-lucide-building" orientation="horizontal">
          <h2 class="text-3xl font-bold text-primary">{{ totalOffices }}</h2>
        </UPageCard>
        <UPageCard title="Total OJTs" icon="i-lucide-users" orientation="horizontal">
          <h2 class="text-3xl font-bold text-primary">{{ totalOjts }}</h2>
        </UPageCard>
        <UPageCard title="Pending Applications" icon="i-lucide-clock" orientation="horizontal">
          <h2 class="text-3xl font-bold text-amber-500">{{ pendingApplications }}</h2>
        </UPageCard>
        <UPageCard title="Approved Applications" icon="i-lucide-check-circle" orientation="horizontal">
          <h2 class="text-3xl font-bold text-green-600">{{ approvedApplications }}</h2>
        </UPageCard>
      </UPageGrid>

      <div class="grid gap-6 xl:grid-cols-2">
        <UCard title="Placement per Office">
          <VChart v-if="placementUtilization.length" :option="placementOption" autoresize style="height: 320px; width: 100%" />
          <div v-else class="flex items-center justify-center h-64 text-muted">No data available</div>
        </UCard>

        <UCard title="Approval Trend">
          <VChart v-if="completionTrend.length" :option="completionOption" autoresize style="height: 320px; width: 100%" />
          <div v-else class="flex items-center justify-center h-64 text-muted">No data available</div>
        </UCard>
      </div>

      <div class="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
        <UCard title="Application Status by Office">
          <UTable v-if="officeTableData.length" :data="officeTableData" :columns="officeTableColumns" />
          <div v-else class="flex items-center justify-center h-48 text-muted">No data available</div>
        </UCard>

        <UCard title="Overall Status">
          <VChart v-if="totalApplications" :option="statusOption" autoresize style="height: 320px; width: 100%" />
          <div v-else class="flex items-center justify-center h-64 text-muted">No data available</div>
        </UCard>
      </div>
    </template>
  </div>
</template>
