<script setup lang="ts">
import { useOfficeStore, type Office } from '../../stores/office'
import type { FormSubmitEvent, TableColumn } from '@nuxt/ui'
import { OfficeNameEnum, OfficeNameLabels } from '../admin/types/officeSelectValue'
import { getPaginationRowModel } from '@tanstack/vue-table'
import OjtCountChart from './partials/ojtCountChart.vue'
import OfficeUpdateModal from '../../components/officeUpdateModal.vue'
import { resolveComponent, onMounted, h, computed, ref } from 'vue'
import z from 'zod'
import { useBattery, useDebounce, useDebouncedRefHistory, useDebounceFn } from '@vueuse/core'
import { useAxios } from '../../fetch/axios.ts'

const office = useOfficeStore()
const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')
const overlay = useOverlay()
const overlayModal = overlay.create(OfficeUpdateModal)

const table = ref()
const pagination = ref({ pageIndex: 0, pageSize: 5 })
const globalFilter = ref('')
const totalOffices = computed(() => office.offices?.length ?? 0)
const loading = ref<boolean>(false)
const toast = useToast()

const schema = z.object({
  department: z.string().min(1),
})
type Schema = z.infer<typeof schema>
const officeOIC = ref<Partial<Schema>>({
  department: undefined,
})

onMounted(async () => {
  if (!office.offices) {
    await office.officeInit()
  }
})

const view = async (id: number) => {
  const current = office.offices?.find((o) => o.id === id)

  const result = await overlayModal.open({
    title: 'Edit Office',
    officeId: current?.id,
    department: current?.department,
    loading: loading.value,
  })

if (result) {
     await debounceSubmit(result.id, result.department, result.honorific)
   }
}

const debounceSubmit = useDebounceFn(async (id: number, department: string, honorific: string) => {
  try {
    loading.value = true
    await useAxios.put('/office/' + id, { department: department, honorific: honorific })
    await office.officeInit()
    toast.add({ title: 'Office Updated Successfully', color: 'primary' })
  } catch (error) {
    console.log(error)
    toast.add({ title: 'Office update failed', color: 'error' })
  } finally {
    loading.value = false
  }
}, 500)

const columns: TableColumn<Office>[] = [
  {
    accessorKey: 'id',
    header: '#',
    cell: ({ row }) => `#${row.getValue('id')}`,
  },
  {
    accessorKey: 'name',
    header: 'Office name',
    cell: ({ row }) => {
      const officeValue = row.getValue('name') as OfficeNameEnum
      return OfficeNameLabels[officeValue] ?? 'Unknown'
    },
  },
   {
    accessorKey: 'honorific',
    header: 'Honorific',
    cell: ({ row }) => row.getValue('honorific') || 'None',
  },
  {
    accessorKey: 'department',
    header: 'Department',
    cell: ({ row }) => row.getValue('department') || 'No department assigned',
  },
  {
    accessorKey: 'students',
    header: 'OJT count',
    cell: ({ row }) => {
      const count = (row.getValue('students') as []) ?? []
      return count.length > 0
        ? h(UBadge, {}, count.length)
        : h('span', { class: 'text-muted italic' }, 'No OJT')
    },
  },
  {
    accessorKey: 'createAt',
    header: 'Created At',
    cell: ({ row }) => {
      const value = row.getValue('createAt') as Date
      return value ? new Date(value).toDateString() : '-'
    },
  },
  {
    accessorKey: 'updatedAt',
    header: 'Updated At',
    cell: ({ row }) => row.getValue('updatedAt') || 'Not yet updated',
  },
  {
    header: 'Actions',
    cell: ({ row }) =>
      h(UButton, {
        icon: 'i-lucide-pen',
        color: 'primary',
        variant: 'ghost',
        onClick: () => view(row.original.id),
      }),
  },
]

const close = () => {}
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
        <p class="text-muted text-sm">Manage department information</p>
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
          <!-- ✅ Uses computed refs, not inline tableApi calls -->
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
