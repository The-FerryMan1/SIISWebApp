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



const values = computed(() => offices.value?.map(t => t.students.length))
const officeName = computed(() => {
    return offices.value?.map(t => {
        const name = t.name as OfficeNameEnum
        return OfficeNameLabels[name].toString()
    })
})

const option = ref({
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'value' },
    yAxis: {
        type: 'category',
        data: officeName,
        axisLabel: { fontSize: 10 }
    },
    dataZoom: [
        { type: 'slider', yAxisIndex: 0, start: 0, end: 30 }
    ],
    series: [{
        type: 'bar',
        data: values,
        itemStyle: {
            color: new graphic.LinearGradient(0, 0, 1, 0, [
                { offset: 0, color: '#83bff6' },
                { offset: 0.5, color: '#188df0' },
                { offset: 1, color: '#188df0' }
            ])
        }
    }]
})
</script>

<template>

    <UCard title="OJT count per office">
        <VChart :option="option" autoresize style="height: 400px; width: 100%;" />
    </UCard>

</template>