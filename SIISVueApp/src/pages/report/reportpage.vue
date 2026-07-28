<script setup lang="ts">
import { ref } from 'vue';
import CsvPdfModal from '../../components/csvPdfModal.vue';
import { useReportStore } from '../../stores/report.ts';


const overlay = useOverlay()
const selectFileTypeModal = overlay.create(CsvPdfModal)
const report = useReportStore()

const selectedFileTypeState = ref<number|undefined>(1)
const cards = ref([
    {
        title: 'OJTs',
        description: 'Nuxt UI integrates with Nuxt Icon to access over 200,000+ icons from Iconify.',
        icon: 'i-lucide-user',
        onClick: async () => generateReport('s')
    },
    {
        title: 'OJT Per Office',
        description: 'Nuxt UI integrates with Nuxt Fonts to provide plug-and-play font optimization.',
        icon: 'i-lucide-building',
        to: '/docs/getting-started/integrations/fonts'
    },
])

const generateReport = async (endpoint: string) => {
    const instance = selectFileTypeModal.open({ title: 'Select file type', "onUpdate:modelValue":(t:number|undefined)=>(selectedFileTypeState.value = t)})
    const result = await instance.result

    try {
        if (result == 'csv') {

             const file = await report.pdfReport('/report/ojtList/csv', selectedFileTypeState.value)
            downloadFile(file, 'csv', 'ojtlist')

        }
        else if (result == 'pdf') {
            const file = await report.pdfReport('/report/ojtList', selectedFileTypeState.value)
            downloadFile(file, 'pdf', 'ojtlist')
        }
    }
    catch (error) {

    }

}


const downloadFile = (blob: Blob, ext?:string, filename?:string)=>{
    const downloadUrl = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = downloadUrl
    a.download = `${filename}-${new Date().toLocaleDateString()}.${ext}`
  a.click()
  URL.revokeObjectURL(downloadUrl)
}
</script>

<template>
    <UPageGrid>
        <UPageCard v-for="(card, index) in cards" :key="index" v-bind="card" />
    </UPageGrid>
</template>