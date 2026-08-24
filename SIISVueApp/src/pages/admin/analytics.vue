<script setup lang="ts">
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
const ojts = computed(() => ojtStore.ojts ?? [])
const applications = computed(() => applicationStore.applications ?? [])

const schoolData = computed(() => {
  const counts = new Map<string, number>()
  ojts.value.forEach((ojt) => {
    const school = ojt.universitySchool || 'Unknown'
    counts.set(school, (counts.get(school) ?? 0) + 1)
  })
  return Array.from(counts.entries())
    .sort(([, a], [, b]) => b - a)
    .slice(0, 10)
    .map(([name, value]) => ({ name, value }))
})

const schoolOption = computed(() => ({
  tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
  grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true },
  xAxis: { type: 'category', data: schoolData.value.map((i) => i.name), axisLabel: { fontSize: 10, rotate: 25, interval: 0 } },
  yAxis: { type: 'value', minInterval: 1 },
  series: [{
    type: 'bar',
    data: schoolData.value.map((i) => i.value),
    itemStyle: { color: '#8b5cf6' },
  }],
}))

const strandData = computed(() => {
  const counts = new Map<string, number>()
  ojts.value.forEach((ojt) => {
    const strand = (ojt as any).strand || (ojt as any).degree || 'Unknown'
    counts.set(strand, (counts.get(strand) ?? 0) + 1)
  })
  return Array.from(counts.entries()).map(([name, value]) => ({ name, value }))
})

const strandOption = computed(() => ({
  tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
  legend: { bottom: 0, type: 'scroll' },
  series: [{
    type: 'pie',
    radius: ['40%', '70%'],
    data: strandData.value.map((item, idx) => ({
      ...item,
      itemStyle: { color: ['#6366f1', '#ec4899', '#10b981', '#f59e0b', '#3b82f6', '#ef4444', '#8b5cf6', '#14b8a6'][idx % 8] },
    })),
    label: { formatter: '{b}: {d}%' },
  }],
}))

const genderDistribution = computed(() => {
  const counts: Record<number, number> = {}
  ojts.value.forEach((ojt) => {
    counts[ojt.gender] = (counts[ojt.gender] || 0) + 1
  })
  return Object.entries(counts).map(([gender, count]) => ({
    name: gender === '0' ? 'Male' : gender === '1' ? 'Female' : 'Others',
    value: count,
    itemStyle: { color: ['#6366f1', '#ec4899', '#10b981'][Number(gender) % 3] },
  }))
})

const genderOption = computed(() => ({
  tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
  legend: { bottom: 0 },
  series: [{
    type: 'pie',
    radius: '70%',
    data: genderDistribution.value,
    label: { formatter: '{b}: {d}%' },
  }],
}))

const monthlyTrend = computed(() => {
  const counts = new Map<string, number>()
  applications.value.forEach((app) => {
    const key = new Date(app.createdAt).toISOString().slice(0, 7)
    counts.set(key, (counts.get(key) ?? 0) + 1)
  })
  return Array.from(counts.entries())
    .sort(([a], [b]) => a.localeCompare(b))
    .slice(-12)
    .map(([month, count]) => ({ month: new Date(month + '-01').toLocaleDateString('en-US', { month: 'short', year: 'numeric' }), count }))
})

const monthlyOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true },
  xAxis: { type: 'category', data: monthlyTrend.value.map((i) => i.month), axisLabel: { rotate: 20 } },
  yAxis: { type: 'value', minInterval: 1 },
  series: [{
    type: 'line',
    smooth: true,
    data: monthlyTrend.value.map((i) => i.count),
    itemStyle: { color: '#3b82f6' },
    lineStyle: { width: 3 },
    areaStyle: { color: 'rgba(59, 130, 246, 0.12)' },
  }],
}))

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
        <h2 class="text-4xl font-black text-primary">Analytics</h2>
        <p class="text-muted text-sm">Detailed insights into OJT placements, applications, and demographics.</p>
      </div>
    </div>

    <template v-if="loading">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <USkeleton v-for="i in 4" :key="i" class="h-80 w-full" />
      </div>
    </template>

    <template v-else>
      <div class="grid gap-6 xl:grid-cols-2">
        <UCard title="Top Schools by OJT Count">
          <VChart v-if="schoolData.length" :option="schoolOption" autoresize style="height: 320px; width: 100%" />
          <div v-else class="flex items-center justify-center h-64 text-muted">No data available</div>
        </UCard>

        <UCard title="Strand / Degree Distribution">
          <VChart v-if="strandData.length" :option="strandOption" autoresize style="height: 320px; width: 100%" />
          <div v-else class="flex items-center justify-center h-64 text-muted">No data available</div>
        </UCard>
      </div>

      <div class="grid gap-6 xl:grid-cols-2">
        <UCard title="Gender Distribution">
          <VChart v-if="genderDistribution.length" :option="genderOption" autoresize style="height: 320px; width: 100%" />
          <div v-else class="flex items-center justify-center h-64 text-muted">No data available</div>
        </UCard>

        <UCard title="Monthly Application Trend">
          <VChart v-if="monthlyTrend.length" :option="monthlyOption" autoresize style="height: 320px; width: 100%" />
          <div v-else class="flex items-center justify-center h-64 text-muted">No data available</div>
        </UCard>
      </div>
    </template>
  </div>
</template>
