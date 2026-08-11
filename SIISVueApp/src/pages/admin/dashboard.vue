<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import { computed, onMounted } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart, LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent } from 'echarts/components'
import { UseAuthStore } from '../../stores/auth'
import { useOfficeStore } from '../../stores/office'
import { useOJtStore } from '../../stores/ojt'
import { useApplicationStore } from '../../stores/application'
import OjtCountChart from '../office/partials/ojtCountChart.vue'
import OjtPieChart from '../ojt/graph/ojtPieChart.vue'

use([CanvasRenderer, BarChart, LineChart, GridComponent, TooltipComponent])

const auth = UseAuthStore()
const officeStore = useOfficeStore()
const ojtStore = useOJtStore()
const applicationStore = useApplicationStore()

const offices = computed(() => officeStore.offices ?? [])
const ojts = computed(() => ojtStore.ojts ?? [])
const applications = computed(() => applicationStore.applications ?? [])

const totalOffices = computed(() => offices.value.length)
const totalOjts = computed(() => ojts.value.length)
const maleCount = computed(() => ojts.value.filter((ojt) => ojt.gender === 0).length)
const femaleCount = computed(() => ojts.value.filter((ojt) => ojt.gender === 1).length)
const pendingApplications = computed(
  () => applications.value.filter((app) => app.status === 'Pending').length,
)
const approvedApplications = computed(
  () => applications.value.filter((app) => app.status === 'Approved').length,
)

const officeChartData = computed(() => {
  return offices.value.map((office) => ({
    name: office.officeName,
    value: (office as any).students?.length ?? 0,
  }))
})

const officeChartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    axisPointer: { type: 'shadow' },
  },
  grid: { left: '6%', right: '4%', bottom: '20%', containLabel: true },
  xAxis: {
    type: 'category',
    data: officeChartData.value.map((item) => item.name),
    axisLabel: {
      fontSize: 10,
      interval: 0,
      rotate: 25,
    },
  },
  yAxis: {
    type: 'value',
    minInterval: 1,
  },
  series: [
    {
      type: 'bar',
      data: officeChartData.value.map((item) => item.value),
      itemStyle: {
        color: '#8b5cf6',
      },
    },
  ],
}))

const applicationTrendData = computed(() => {
  const counts = new Map<string, number>()

  applications.value.forEach((application) => {
    const createdAt = application.createdAt ? new Date(application.createdAt) : null

    if (!createdAt || Number.isNaN(createdAt.getTime())) return

    const key = createdAt.toISOString().slice(0, 10)
    counts.set(key, (counts.get(key) ?? 0) + 1)
  })

  return Array.from(counts.entries())
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([date, count]) => ({ date, count }))
})

const applicationTrendOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    formatter: (params: Array<{ value: number; name: string }>) => {
      const point = params[0]
      if (!point) return 'No data'
      return `${point.name}: ${point.value} submissions`
    },
  },
  grid: { left: '6%', right: '4%', bottom: '15%', containLabel: true },
  xAxis: {
    type: 'category',
    data: applicationTrendData.value.map((item) =>
      new Date(item.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
    ),
    axisLabel: {
      interval: 0,
      rotate: 20,
    },
  },
  yAxis: {
    type: 'value',
    minInterval: 1,
  },
  series: [
    {
      type: 'line',
      smooth: true,
      data: applicationTrendData.value.map((item) => item.count),
      itemStyle: { color: '#2563eb' },
      lineStyle: { width: 3 },
      areaStyle: { color: 'rgba(37, 99, 235, 0.12)' },
    },
  ],
}))

const applicationChartOption = computed(() => ({
  tooltip: {
    trigger: 'item',
    formatter: '{b}: {c}',
  },
  legend: {
    bottom: 0,
  },
  series: [
    {
      type: 'pie',
      radius: '70%',
      data: [
        { value: pendingApplications.value, name: 'Pending', itemStyle: { color: '#f59e0b' } },
        { value: approvedApplications.value, name: 'Approved', itemStyle: { color: '#10b981' } },
      ],
      label: {
        formatter: '{b}: {c}',
      },
    },
  ],
}))

type RecentOjtRow = {
  name: string
  office: string
  gender: string
}

const tableColumns: TableColumn<RecentOjtRow>[] = [
  { accessorKey: 'name', header: 'Name' },
  { accessorKey: 'office', header: 'Office' },
  { accessorKey: 'gender', header: 'Gender' },
]

  const recentOjts = computed<RecentOjtRow[]>(() =>
  ojts.value.slice(0, 8).map((ojt) => ({
    name: `${ojt.firstName} ${ojt.lastName}`,
    office: ojt.officeName,
    gender: ojt.gender === 0 ? 'Male' : ojt.gender === 1 ? 'Female' : 'Others',
  })),
)

onMounted(async () => {
  await Promise.all([
    officeStore.officeInit(),
    ojtStore.ojtInit(),
    applicationStore.applicationInit(),
  ])
})
</script>

<template>
  <div class="space-y-6">
    <div class="px-1 py-2 my-10">
      <div>
        <h2 class="text-4xl font-black text-primary">Dashboard</h2>
        <p class="text-muted text-sm">
          Overview of office placements, participant demographics, and recent activity.
        </p>
      </div>
    </div>

    <UPageGrid>
      <UPageCard title="Total Offices"  icon="i-lucide-building" orientation="horizontal">
        <h2 class="text-3xl font-bold text-primary">{{ totalOffices }}</h2>
      </UPageCard>
      <UPageCard title="Total OJTs" icon="i-lucide-users" orientation="horizontal">
        <h2 class="text-3xl font-bold text-primary">{{ totalOjts }}</h2>
      </UPageCard>
      <UPageCard title="Male Participants" icon="i-lucide-user" orientation="horizontal">
        <h2 class="text-3xl font-bold text-primary">{{ maleCount }}</h2>
      </UPageCard>
      <UPageCard title="Female Participants" icon="i-lucide-user-check" orientation="horizontal">
        <h2 class="text-3xl font-bold text-primary">{{ femaleCount }}</h2>
      </UPageCard>
      <UPageCard title="Pending Applications" icon="i-lucide-clock-3" orientation="horizontal">
        <h2 class="text-3xl font-bold text-amber-500">{{ pendingApplications }}</h2>
      </UPageCard>
      <UPageCard
        title="Approved Applications"
        icon="i-lucide-circle-check"
        orientation="horizontal"
      >
        <h2 class="text-3xl font-bold text-emerald-500">{{ approvedApplications }}</h2>
      </UPageCard>
    </UPageGrid>

    <div class="grid gap-6 xl:grid-cols-2">
      <OjtCountChart />
      <OjtPieChart />
    </div>

    <div class="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
      <UCard title="Application status">
        <VChart :option="applicationChartOption" autoresize style="height: 320px; width: 100%" />
      </UCard>

      <UCard title="Application submissions trend">
        <VChart :option="applicationTrendOption" autoresize style="height: 320px; width: 100%" />
      </UCard>
    </div>

    <UCard title="Recent OJT participants">
      <UTable :data="recentOjts" :columns="tableColumns" />
    </UCard>
  </div>
</template>
