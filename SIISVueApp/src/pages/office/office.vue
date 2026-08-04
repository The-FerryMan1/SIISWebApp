<script setup lang="ts">
import { useOfficeStore, type Office } from '../../stores/office'
import type { TableColumn } from '@nuxt/ui'
import { getPaginationRowModel } from '@tanstack/vue-table'
import OjtCountChart from './partials/ojtCountChart.vue'
import { resolveComponent, onMounted, h, computed, ref } from 'vue'
import { useAxios } from '../../fetch/axios'

const office = useOfficeStore()
const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')
const toast = useToast()

const loading = ref<boolean>(false)
const table = ref()
const pagination = ref({ pageIndex: 0, pageSize: 5 })
const globalFilter = ref('')
const totalOffices = computed(() => office.offices?.length ?? 0)

onMounted(async () => {
  if (!office.offices) {
    await office.officeInit()
  }
})

const columns: TableColumn<Office>[] = [
  {
    accessorKey: 'id',
    header: '#',
    cell: ({ row }) => `#${row.getValue('id')}`,
  },
  {
    accessorKey: 'officeName',
    header: 'Office name',
    cell: ({ row }) => row.getValue('officeName'),
  },
  {
    accessorKey: 'userId',
    header: 'Officer Account',
    cell: ({ row }) => row.getValue('userEmail') || 'Not assigned',
  },
  {
    accessorKey: 'createdAt',
    header: 'Created At',
    cell: ({ row }) => {
      const value = row.getValue('createdAt') as string
      return value ? new Date(value).toDateString() : '-'
    },
  },
  {
    accessorKey: 'updatedAt',
    header: 'Updated At',
    cell: ({ row }) => {
      const value = row.getValue('updatedAt') as string | null
      return value ? new Date(value).toDateString() : 'Not yet updated'
    },
  },
  {
    header: 'Actions',
    cell: ({ row }) =>
      h(UButton, {
        icon: 'i-lucide-pen',
        color: 'primary',
        variant: 'ghost',
        onClick: () => editOffice(row.original),
      }),
  },
]

const editOffice = async (officeItem: Office) => {
  const newName = prompt('Edit office name:', officeItem.officeName)
  if (!newName || newName === officeItem.officeName) return
  try {
    loading.value = true
    await useAxios.put('/office/' + officeItem.id, { officeName: newName })
    await office.officeInit()
    toast.add({ title: 'Office updated successfully', color: 'success' })
  } catch {
    toast.add({ title: 'Office update failed', color: 'error' })
  } finally {
    loading.value = false
  }
}

const pageIndex = computed(() => pagination.value.pageIndex)
const pageSize = computed(() => pagination.value.pageSize)

const totalRows = computed(() => {
  const data = office.offices ?? []
  if (!globalFilter.value) return data.length
  const f = globalFilter.value.toLowerCase()
  return data.filter((item) =>
    Object.values(item as object).some((v) => String(v).toLowerCase().includes(f)),
  ).length
})

const setPage = (p: number) => {
  pagination.value = { ...pagination.value, pageIndex: p - 1 }
}
</script>

<template>
  <UMain class="space-y-10">
    <div class="px-4 py-2 my-5">
      <div>
        <h2 class="text-4xl font-black text-primary">Offices</h2>
        <p class="text-muted text-sm">Manage office information</p>
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

      <UTable
        ref="table"
        sticky
        class="w-full max-h-96 flex-1"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
        v-model:pagination="pagination"
        v-model:global-filter="globalFilter"
        :data="office.offices ?? []"
        :columns
      />

      <template #footer>
        <div class="flex justify-end border-t border-default pt-4 px-4">
          <UPagination
            :page="pageIndex + 1"
            :items-per-page="pageSize"
            :total="totalRows"
            @update:page="setPage"
          />
        </div>
      </template>
    </UCard>

    <OjtCountChart />
  </UMain>
</template>
