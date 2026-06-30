<script setup lang="ts">
import { useOfficeStore, type Office } from '../../stores/office';
import type { FormSubmitEvent, TableColumn } from '@nuxt/ui';
import { OfficeNameEnum, OfficeNameLabels } from '../admin/types/officeSelectValue';
import { getPaginationRowModel } from '@tanstack/vue-table';
import OjtCountChart from './partials/ojtCountChart.vue';
import OfficeUpdateModal from '../../components/officeUpdateModal.vue';
import { resolveComponent, onMounted, h, computed, ref } from 'vue';
import z from 'zod';


const office = useOfficeStore()
const UButton = resolveComponent('UButton')
const overlay = useOverlay()
const overlayModal = overlay.create(OfficeUpdateModal, {destroyOnClose: true})




const table = ref()
const pagination = ref({ pageIndex: 0, pageSize: 5 })
const globalFilter = ref('')
const totalOffices = computed(() => office.offices?.length ?? 0)

onMounted(async () => {
    if (!office.offices) {
        await office.officeInit()
    }
})

const view = (id: number) => {
   overlayModal.open()

}

const columns: TableColumn<Office>[] = [
    {
        accessorKey: 'id',
        header: '#',
        cell: ({ row }) => `#${row.getValue('id')}`
    },
    {
        accessorKey: 'name',
        header: 'Office name',
        cell: ({ row }) => {
            const officeValue = row.getValue('name') as OfficeNameEnum
            return OfficeNameLabels[officeValue] ?? 'Unknown'
        }
    },
    {
        accessorKey: 'currentOIC',
        header: 'Current officer-in-charge',
        cell: ({ row }) => row.getValue('currentOIC') || 'Officer in Charge not assigned'
    },
    {
        accessorKey: 'students',
        header: 'OJT count',
        cell: ({ row }) => {
            const count = row.getValue('students') as [] ?? []
            return count.length > 0 ? count.length : h('span', { class: 'text-muted italic' }, 'No OJT')
        }
    },
    {
        accessorKey: 'createAt',
        header: 'Created At',
        cell: ({ row }) => {
            const value = row.getValue('createAt') as Date
            return value ? new Date(value).toDateString() : '-'
        }
    },
    {
        accessorKey: 'updatedAt',
        header: 'Updated At',
        cell: ({ row }) => row.getValue('updatedAt') || 'Not yet updated'
    },
    {
        header: 'Actions',
        cell: ({ row }) => h(UButton, {
            icon: 'i-lucide-pen',
            color: 'neutral',
            variant: 'ghost',
            onClick: () => view(row.original.id)
        })
    }
]





const close = () =>{

}



</script>

<template>
    <UMain class="space-y-10">


        <div class="px-4 py-2 my-5">
            <div>
                <h2 class="text-4xl font-black text-primary">Offices</h2>
                <p class="text-muted text-sm">Manage current officer-in-charge</p>
            </div>
        </div>

        <UPageGrid>
            <UPageCard title="Total Offices" icon="i-lucide-building" orientation="horizontal">
                <h1 class="text-4xl text-primary font-bold">{{ totalOffices }}</h1>
            </UPageCard>
        </UPageGrid>

        <UCard>
            <template #header>
                <UInput v-model="globalFilter" placeholder="search..." icon="i-lucide-search" />
            </template>

            <UTable ref="table" sticky class="w-full max-h-96 flex-1" :pagination-options="{
                getPaginationRowModel: getPaginationRowModel()
            }" v-model:pagination="pagination" v-model:global-filter="globalFilter" :data="office.offices ?? []"
                :columns />

            <template #footer>
                <div class="flex justify-end border-t border-default pt-4 px-4">
                    <UPagination :page="(table?.tableApi?.getState().pagination.pageIndex || 0) + 1"
                        :items-per-page="table?.tableApi?.getState().pagination.pageSize"
                        :total="table?.tableApi?.getFilteredRowModel().rows.length"
                        @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)" />
                </div>
            </template>
        </UCard>

        <OjtCountChart />

    </UMain>
</template>