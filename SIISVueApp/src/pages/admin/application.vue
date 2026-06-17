<script setup lang="ts">
import { UseAuthStore } from '../../stores/auth';
import { computed, h, onMounted, ref, resolveComponent, useTemplateRef } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { useApplicationStore } from '../../stores/application';
import type { Applicaton } from '../../stores/application'
const auth = UseAuthStore();
const application = useApplicationStore();
const UBadge = resolveComponent('UBadge')
const UChip = resolveComponent('UChip')

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

]

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
  pageSize: 5
})
</script>

<template>

    <div class="px-10 py-2 my-10">
        <div>
            <h2 class="text-2xl font-black">Application</h2>
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
                <div class="w-full flex items-center">
                    <UInput v-model="globalFilter" class="max-w-sm w-full" placeholder="Filter..."
                        icon="i-lucide-search" />
                    <UInput v-model="pagination.pageSize" class="ms-auto max-w-sm w-1/15" label="Limit" placeholder="limit"
                        icon="i-lucide-list-ordered" />
                </div>

            </template>

            <UTable ref="table" sticky v-model:global-filter="globalFilter" :data="application.applications ?? []"
                :columns="columns" class="flex-1" />

            <template #footer>
                <div v-if="application.applications">
                    <UPagination v-if="application.applications?.length > 10" :page="(table?.tableApi?.getState().pagination.pageIndex || 0) + 1"
                        :items-per-page="table?.tableApi?.getState().pagination.pageSize"
                        :total="table?.tableApi?.getFilteredRowModel().rows.length"
                        @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)" />
                </div>

            </template>
        </UCard>
    </div>

</template>