<script setup lang="ts">
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, DataZoomComponent } from 'echarts/components'
import { graphic } from 'echarts/core'
import { computed, ref } from 'vue'
import { useOfficeStore } from '../../../stores/office'
import { storeToRefs } from 'pinia'
import { OfficeNameLabels, type OfficeNameEnum } from '../../admin/types/officeSelectValue'

use([CanvasRenderer, BarChart, GridComponent, TooltipComponent, DataZoomComponent])

const office = useOfficeStore()
const { offices } = storeToRefs(office)

const values = computed(() => offices.value?.map((t) => t.students.length) ?? [])
const officeName = computed(() => {
  return (
    offices.value?.map((t) => {
      const name = t.name as OfficeNameEnum
      return OfficeNameLabels[name] ?? 'Unknown'
    }) ?? []
  )
})

const option = ref({
  tooltip: {
    trigger: 'axis',
    axisPointer: { type: 'shadow' },
    valueFormatter: (value: number) => Math.round(value).toString(),
  },
  grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true }, // Increased bottom for labels
  xAxis: {
    type: 'category',
    data: officeName,
    axisLabel: {
      fontSize: 10,
      rotate: 45,
      interval: 0,
    },
  },
  yAxis: {
    type: 'value',
    minInterval: 1,
    axisLabel: {
      formatter: '{value}',
    },
  },
  dataZoom: [
    {
      type: 'slider',
      xAxisIndex: 0,
      start: 0,
      end: 30,
    },
  ],
  series: [
    {
      type: 'bar',
      data: values,
      itemStyle: {
        color: new graphic.LinearGradient(0, 0, 0, 1, [
          { offset: 0, color: '#818cf8' }, // indigo-400
          { offset: 0.5, color: '#6366f1' }, // indigo-500
          { offset: 1, color: '#312e81' }, // indigo-900
        ]),
      },
    },
  ],
})
</script>

<template>
  <UCard title="OJT count per office">
    <VChart :option="option" autoresize style="height: 400px; width: 100%" />
  </UCard>
</template>
