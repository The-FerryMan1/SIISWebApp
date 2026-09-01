<script setup lang="ts">
import { useOfficeStore, type Office } from '../../stores/office'
import type { TableColumn } from '@nuxt/ui'
import { getPaginationRowModel } from '@tanstack/vue-table'
import OjtCountChart from './partials/ojtCountChart.vue'
import { onMounted, computed, ref, h } from 'vue'
import { useAxios } from '../../fetch/axios'
import { abbreviateEmail } from '../../utils/validators'

const office = useOfficeStore()
const toast = useToast()

const loading = ref<boolean>(false)
const pagination = ref({ pageIndex: 0, pageSize: 5 })
const globalFilter = ref('')
const totalOffices = computed(() => office.offices?.length ?? 0)

const editDialogOpen = ref(false)
const editingOffice = ref<Office | null>(null)
const editForm = ref({ officeName: '', department: '' })

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
    accessorKey: 'userEmail',
    header: 'Officer Account',
    cell: ({ row }) => abbreviateEmail(row.getValue('userEmail')) || 'Not assigned',
  },
  {
    accessorKey: 'department',
    header: 'Department',
    cell: ({ row }) => row.getValue('department') || 'Not set',
  },
  {
    accessorKey: 'createdAt',
    header: 'Created At',
    cell: ({ row }) => {
      const value = row.getValue('createdAt') as string
      return value ? new Date(value).toLocaleDateString() : '-'
    },
  },
  {
    accessorKey: 'updatedAt',
    header: 'Updated At',
    cell: ({ row }) => {
      const value = row.getValue('updatedAt') as string | null
      return value ? new Date(value).toLocaleDateString() : 'Not yet updated'
    },
  },
  {
    header: 'Actions',
    cell: ({ row }) =>
      h('UButton', {
        icon: 'i-lucide-pen',
        color: 'primary',
        variant: 'ghost',
        onClick: () => openEdit(row.original),
      }),
  },
]

const openEdit = (officeItem: Office) => {
  editingOffice.value = officeItem
  editForm.value = {
    officeName: officeItem.officeName,
    department: officeItem.department || '',
  }
  editDialogOpen.value = true
}

const saveEdit = async () => {
  if (!editingOffice.value) return
  try {
    loading.value = true
    await useAxios.put('/office/' + editingOffice.value.id, {
      officeName: editForm.value.officeName,
      department: editForm.value.department,
    })
    await office.officeInit()
    editDialogOpen.value = false
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

      <div v-if="!office.offices?.length" class="flex items-center justify-center h-32 text-muted">
        No offices found
      </div>

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

    <UModal v-model:open="editDialogOpen" title="Edit Office">
      <template #body>
        <div class="space-y-4">
          <UFormField label="Office Name">
            <UInput v-model="editForm.officeName" />
          </UFormField>
          <UFormField label="Department">
            <UInput v-model="editForm.department" />
          </UFormField>
        </div>
      </template>

      <template #footer>
        <div class="flex justify-end gap-3">
          <UButton label="Cancel" variant="ghost" color="neutral" @click="editDialogOpen = false" />
          <UButton label="Save" color="primary" :loading="loading" @click="saveEdit" />
        </div>
      </template>
    </UModal>

    <OjtCountChart />
  </UMain>
</template>
