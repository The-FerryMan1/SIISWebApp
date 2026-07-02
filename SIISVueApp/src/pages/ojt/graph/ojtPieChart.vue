<script setup lang="ts">
import { computed } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { PieChart } from 'echarts/charts'
import { TooltipComponent, LegendComponent } from 'echarts/components'
import { storeToRefs } from 'pinia'
import { useOJtStore } from '../../../stores/ojt'

use([CanvasRenderer, PieChart, TooltipComponent, LegendComponent])

const ojtStore = useOJtStore()
const { ojts } = storeToRefs(ojtStore)

const genderLabels: Record<number, string> = {
  0: 'Male',
  1: 'Female',
  2: 'Others',
}

const genderColors = ['#6366f1', '#ec4899', '#10b981'] // indigo, pink, emerald

const genderData = computed(() => {
  if (!ojts.value?.length) return []

  // Count genders
  const counts: Record<number, number> = {}
  ojts.value.forEach((ojt) => {
    counts[ojt.gender] = (counts[ojt.gender] || 0) + 1
  })

  // Format for ECharts
  return Object.entries(counts).map(([gender, count]) => ({
    name: genderLabels[Number(gender)] || 'Unknown',
    value: count,
    itemStyle: { color: genderColors[Number(gender)] || '#9ca3af' },
  }))
})

const totalCount = computed(() => ojts.value?.length || 0)

const option = computed(() => ({
  tooltip: {
    trigger: 'item',
    formatter: '{b}: {c} ({d}%)',
  },
  legend: {
    orient: 'vertical',
    left: 'left',
    top: 'center',
  },
  series: [
    {
      name: 'Gender Distribution',
      type: 'pie',
      radius: '60%', // Full pie
      center: ['50%', '50%'],
      data: genderData.value,
      emphasis: {
        itemStyle: {
          shadowBlur: 10,
          shadowOffsetX: 0,
          shadowColor: 'rgba(0, 0, 0, 0.5)',
        },
      },
      label: {
        formatter: '{b}: {c} ({d}%)',
      },
    },
  ],
  // Optional: Add title with total
  title: {
    text: `Total: ${totalCount.value}`,
    left: 'center',
    top: 'center',
    textStyle: {
      fontSize: 16,
      fontWeight: 'bold',
      color: '#374151',
    },
  },
}))
</script>

<template>
  <UCard title="Gender Distribution">
    <div v-if="!ojts?.length" class="flex items-center justify-center h-75 text-muted">
      No data available
    </div>
    <VChart v-else :option="option" autoresize style="height: 350px; width: 100%" />
  </UCard>
</template>
