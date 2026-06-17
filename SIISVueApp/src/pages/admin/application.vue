<script setup lang="ts">
import { UseAuthStore } from '../../stores/auth'
import { computed, h, onMounted, ref, resolveComponent, useTemplateRef } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { useApplicationStore } from '../../stores/application'
import type { Applicaton } from '../../stores/application'
import type { Row } from '@tanstack/vue-table'
import { useToast } from '@nuxt/ui/runtime/composables/useToast.js'
import { useClipboard } from '@vueuse/core'

//states
const auth = UseAuthStore()
const application = useApplicationStore()
const toast = useToast()
const { copy } = useClipboard()

//table states
const table = useTemplateRef('table')
const globalFilter = ref('')
const statusFilter = ref(['Pending', 'Approved', 'Viewed'])
const statusFilterSeleted = ref('Pending')
const pagination = ref({
  pageIndex: 0,
  pageSize: 5,
})

const statusFilterResult = computed(()=> application.applications?.filter(t => t.status == statusFilterSeleted.value))



//components
const UBadge = resolveComponent('UBadge')
const UChip = resolveComponent('UChip')
const UDropdownMenu = resolveComponent('UDropdownMenu')
const UButton = resolveComponent('UButton')

onMounted(async () => {
  if (application.applications?.length) return
  await application.applicationInit()
})

//table column
const columns: TableColumn<Applicaton>[] = [
  {
    accessorKey: 'applicationUUID',
    header: '#',
    cell: ({ row }) => {
      return h('span', { class: ' text-xs ' }, `#${row.getValue('applicationUUID')}`)
    },
  },
  {
    accessorKey: 'fullName',
    header: 'Applicant',
    cell: ({ row }) => {
      const createdAt = row.getValue('createdAt') as string
      const isNew = createdAt
        ? Date.now() < new Date(createdAt).getTime() + 86400000 // 24h from created
        : false

      return isNew
        ? h(UChip, { color: 'primary', size: 'sm', label: 'New' }, () => row.getValue('fullName'))
        : row.getValue('fullName')
    },
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => {
      const color = {
        Pending: 'warning' as const,
        Viewed: 'info' as const,
        Approved: 'success' as const,
      }[row.getValue('status') as string]

      return h(UBadge, { class: 'capitalize', variant: 'subtle', color }, () =>
        row.getValue('status'),
      )
    },


  },

  {
    accessorKey: 'createdAt',
    header: 'Created At',
    cell: ({ row }) => `${new Date(row.getValue('createdAt')).toLocaleDateString()}`,
  },
  {
    accessorKey: 'updatedAt',
    header: 'Updated At',
    cell: ({ row }) => {
      return row.getValue('updatedAt')
        ? new Date(row.getValue('updatedAt'))
        : h('span', { class: 'italic' }, 'Not updated')
    },
  },
  {
    id: 'actions',
    meta: {
      class: {
        td: 'text-right'
      }
    },
    cell: ({ row }) => {
      return h(
        UDropdownMenu,
        {
          content: {
            align: 'end'
          },
          items: getRowItems(row),
          'aria-label': 'Actions dropdown'
        },
        () =>
          h(UButton, {
            icon: 'i-lucide-ellipsis-vertical',
            color: 'neutral',
            variant: 'ghost',
            'aria-label': 'Actions dropdown'
          })
      )
    }
  }
]

function getRowItems(row: Row<Applicaton>) {
  return [
    {
      type: 'label',
      label: 'Actions'
    },
    {
      label: 'Copy application ID',
      onSelect() {
        copy(`${row.original.applicationUUID}`)

        toast.add({
          title: 'Application unique ID copied to clipboard!',
          color: 'success',
          icon: 'i-lucide-circle-check'
        })
      }
    },
    {
      type: 'separator'
    },
    {
      label: 'View Application',
    },
  ]
}

//cards
const cards = computed(() => [
  {
    title: 'Applications',
    icon: 'i-lucide-file',
    text: application.applications?.length,
    color: 'bg-info-400',
  },
  {
    title: 'Approved',
    icon: 'i-lucide-badge-check',
    text: application.applications?.filter((t) => t.status == 'Approved').length,
    color: 'bg-yellow-400',
  },
  {
    title: 'Pending',
    icon: 'i-lucide-ellipsis',
    text: application.applications?.filter((t) => t.status == 'Pending').length,
    color: 'bg-yellow-400',
  },
])


</script>

<template>
  <div class="px-10 py-2 my-10">
    <div>
      <h2 class="text-3xl font-black mb-1 text-primary">Application</h2>
      <p class="text-muted text-sm">Manage and review submitted applications</p>
    </div>
  </div>

  <div class="px-10 py-2">
    <UPageGrid class="mb-2~" :ui="{}">
      <UPageCard spotlight variant="outline" orientation="horizontal" reverse v-for="card in cards" :title="card.title">
        <UContainer>
          <div class="flex items-center gap-10">
            <UIcon :name="card.icon" class="size-10" />
            <span class="text-3xl font-bold text-primary">{{ card.text }}</span>
          </div>
        </UContainer>
      </UPageCard>
    </UPageGrid>
  </div>

  <div class="px-10 py-2">
    <UCard>
      <template #header>
        <div class="w-full flex items-center">
          <UInput v-model="globalFilter" class="max-w-sm w-full" placeholder="Search..." icon="i-lucide-search" />

          <div class="ms-auto flex items-center gap-2" >
              <USelect icon="i-lucide-funnel" v-model="statusFilterSeleted" :items="statusFilter" />
             <UInput class="max-w-sm w-20" v-model="pagination.pageSize" label="Limit" placeholder="limit"
            icon="i-lucide-list-ordered" />
          </div>
         
        </div>
      </template>

      <UTable  ref="table" sticky v-model:global-filter="globalFilter" :data="statusFilterResult?? []"
        :columns="columns" class="flex-1" />

      <template #footer>
        <div v-if="application.applications" class="flex items-center">
          <UBadge>
            <span class="text-xs">Total item: {{ application.applications?.length }}</span>
          </UBadge>

          <UPagination class="ms-auto" v-if="application.applications?.length > 1"
            :page="(table?.tableApi?.getState().pagination.pageIndex || 0) + 1"
            :items-per-page="table?.tableApi?.getState().pagination.pageSize"
            :total="table?.tableApi?.getFilteredRowModel().rows.length"
            @update:page="(p: number) => table?.tableApi?.setPageIndex(p - 1)" />
        </div>
      </template>
    </UCard>
  </div>
</template>
