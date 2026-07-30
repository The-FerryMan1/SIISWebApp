<script setup lang="ts">
import { ref } from 'vue';
import CsvPdfModal from '../../components/csvPdfModal.vue';
import { useReportStore } from '../../stores/report.ts';
import { OfficeNameEnum } from '../admin/types/applicationUpdateValidator.ts';
import { Labels, OfficeOptions } from '../../shared/officeEnum.ts';


const overlay = useOverlay()
const selectFileTypeModal = overlay.create(CsvPdfModal)
const report = useReportStore()

const selectedFileTypeState = ref<number | undefined>(1)
const cards = ref([
    {
        title: 'OJTs',
        description: 'List of ojt',
        icon: 'i-lucide-user',
        onClick: async () => generateReport('s')
    },
    {
        title: 'OJT Per Office',
        description: 'List of ojt per office',
        icon: 'i-lucide-building',
         onClick: async () => generateReportPerOffice()

    },
])

const generateReport = async (endpoint: string) => {
    const instance = selectFileTypeModal.open({
        title: 'Select file type',  selectPlaceholder:"Select Status", "onUpdate:modelValue": (t: number | undefined) => (selectedFileTypeState.value = t), items: [
            {
                label: 'Pending',
                value: 0
            },
            {
                label: 'Approved',
                value: 1
            },
            {
                label: 'Rejected',
                value: 2
            }
        ]
    })
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



const generateReportPerOffice = async () => {
    const instance = selectFileTypeModal.open({ title: 'Select file type', selectPlaceholder:"Select Office", "onUpdate:modelValue": (t: number | undefined) => (selectedFileTypeState.value = t), items:OfficeOptions })
    const result = await instance.result

    try {
        if (result == 'pdf') {

            const file = await report.pdfReportPerOffice('/report/ojtPerOffice', selectedFileTypeState.value)
            downloadFile(file, 'pdf', 'ojtlist')

        }
        else if (result == 'csv') {
          
        }
    }
    catch (error) {

    }
}


const downloadFile = (blob: Blob, ext?: string, filename?: string) => {
    const downloadUrl = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = downloadUrl
    a.download = `${filename}-${new Date().toLocaleDateString()}.${ext}`
    a.click()
    URL.revokeObjectURL(downloadUrl)
}
</script>

<template>

    <div>
        <h1 class="text-xl font-bold text-primary">Generate PDF/CSV</h1>
    </div>


    <UPageGrid>
        <UPageCard :spotlight="true" spotlight-color="primary" v-for="(card, index) in cards" :key="index" v-bind="card" />
    </UPageGrid>
</template>