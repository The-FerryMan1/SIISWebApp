<script setup lang="ts">
import { UseAuthStore } from '../../stores/auth';
import { computed, h, onMounted, ref, resolveComponent, useTemplateRef, watch } from 'vue'
import type { TableColumn, TableRow } from '@nuxt/ui'
import { useApplicationStore } from '../../stores/application';
import type { Applicaton } from '../../stores/application'
import { getPaginationRowModel, type Row } from '@tanstack/vue-table'
import { useRouter } from 'vue-router';

//states
const auth = UseAuthStore();
const application = useApplicationStore();
const UBadge = resolveComponent('UBadge')
const UChip = resolveComponent('UChip')
const UButton = resolveComponent('UButton')
const router = useRouter()

onMounted(async () => {
    await application.applicationInit();
});

//table column
const columns: TableColumn<Applicaton>[] = [
    {
        accessorKey: 'applicationUUID',
        header: '#',
        cell: ({ row }) => {
            return h('span', { class: ' text-xs ' }, `#${row.getValue('applicationUUID')}`)
        }
    },
    {
        accessorKey: 'fullName',
        header: 'Applicant',
        cell: ({ row }) => {
            const createdAt = row.getValue('createdAt') as string;

            // ✅ Compare number with number
            const isNew = createdAt
                ? Date.now() < new Date(createdAt).getTime() + 86400000 // 24h from created
                : false;

            return isNew
                ? h(UChip, { color: 'primary', size: 'sm', label: 'New' }, () => row.getValue('fullName'))
                : row.getValue('fullName');
        }
    },
    {
        accessorKey: 'status',
        header: 'Status',
        cell: ({ row }) => {
            const color = {
                Pending: 'warning' as const,
                Viewed: 'info' as const,
                Approved: 'success' as const
            }[row.getValue('status') as string]

            return h(UBadge, { class: 'capitalize', variant: 'subtle', color }, () =>
                row.getValue('status')
            )
        }
    },
    {
        accessorKey: 'createdAt',
        header: 'Created At',
        cell: ({ row }) => `${new Date(row.getValue('createdAt')).toLocaleDateString()}`
    },
    {
        accessorKey: 'updatedAt',
        header: 'Updated At',
        cell: ({ row }) => {
            return row.getValue('updatedAt') ? new Date(row.getValue('updatedAt')) : h('span', { class: 'italic' }, 'Not updated')
        }
    },
    {
        header: 'Actions',
        cell: ({ row }) => {
            return h(UButton, { icon: 'i-lucide-eye', onClick: () => router.push({ name: 'application-details', params: { uuid: row.getValue('applicationUUID') } }) })
        },

    }

]

//row actions
const rowActions = (row: Row<Applicaton>) => {
    return [

    ]
}


//cards
const cards = computed(() => [
    {
        title: "Applications",
        icon: "i-lucide-file",
        text: application.applications?.length,
        color: 'bg-info-400'

    },
    {
        title: "Approved",
        icon: "i-lucide-badge-check",
        text: application.applications?.filter(t => t.status == "Approved").length,
        color: 'bg-yellow-400'

    },
    {
        title: "Pending",
        icon: "i-lucide-ellipsis",
        text: application.applications?.filter(t => t.status == "Pending").length,
        color: 'bg-yellow-400'

    },
])


const table = useTemplateRef('table')
const globalFilter = ref('');
const pagination = ref({
    pageIndex: 0,
    pageSize: 10
})

const statusFilter = ref(['Pending', 'Viewed', 'Approved', 'All'])
const statusSelectedFIlter = ref('All')
const statusFilterResult = computed(() => {
    return application.applications?.filter(t => {



        if (statusSelectedFIlter.value == 'All') {
            return t.status
        } else {
            return t.status == statusSelectedFIlter.value
        }
    })
})

watch(() => pagination.value.pageSize, (size) => {
    table.value?.tableApi?.setPageSize(size)
    pagination.value.pageIndex = 0 // reset to first page when limit changes
})

const pageSize = ref(10)

watch(pageSize, (size) => {
    pagination.value.pageSize = size
    pagination.value.pageIndex = 0 // reset to page 1
})
</script>

<template>
    <div class="px-10 py-2 my-10">
        <div>
            <h2 class="text-4xl font-black text-primary">Application</h2>
            <p class="text-muted text-sm">Manage and review submitted applications</p>
        </div>
    </div>

    <div class="px-10 py-2">
        <UPageGrid class="mb-5" :ui="{}">
            <UPageCard spotlight variant="outline" orientation="horizontal" reverse v-for="card in cards"
                :title="card.title">
                <UContainer>
                    <div class="flex items-center gap-10">
                        <UIcon :name="card.icon" class="size-10" />
                        <span class="text-3xl font-bold text-primary ">{{ card.text }}</span>
                    </div>
                </UContainer>
            </UPageCard>
        </UPageGrid>
    </div>

    <div class="px-10 py-2">
        <UCard>

            <template #header>
                <div class="w-full flex items-center mb-4 gap-2 md:flex-nowrap flex-wrap">
                    <UInput v-model="globalFilter" class="w-full shrink-0 sm:shrink" placeholder="Filter..."
                        icon="i-lucide-search" />

                    <div class="ms-auto flex items-center gap-2">
                        <USelect v-model="statusSelectedFIlter" :items="statusFilter" class="ms-auto" />
                        <UInput v-model.number="pageSize" type="number" :min="1" class="max-w-sm" placeholder="Limit"
                            icon="i-lucide-list-ordered" />
                    </div>
                </div>


            </template>

            <UTable ref="table" sticky v-model:global-filter="globalFilter" v-model:pagination="pagination"
                :data="statusFilterResult ?? []" :columns="columns" class="flex-1" :pagination-options="{
                    getPaginationRowModel: getPaginationRowModel()
                }" />

            <template #footer>
                <UPagination :page="(table?.tableApi?.getState().pagination.pageIndex || 0) + 1"
                    :items-per-page="table?.tableApi?.getState().pagination.pageSize"
                    :total="table?.tableApi?.getFilteredRowModel().rows.length"
                    @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)" />

            </template>
        </UCard>
    </div>
</template>