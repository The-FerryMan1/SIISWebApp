<script setup lang="ts">
import { UseAuthStore } from '../../stores/auth'
import { computed, h, onMounted, ref, resolveComponent, useTemplateRef, watch } from 'vue'
import type { TableColumn, TableRow } from '@nuxt/ui'
import { useApplicationStore } from '../../stores/application'
import type { Applicaton } from '../../stores/application'
import { getPaginationRowModel, type Row } from '@tanstack/vue-table'
import { useRouter } from 'vue-router'
import { useDebounceFn } from '@vueuse/core'
import ConfirmationModal from '../../components/confirmationModal.vue'
import { useAxios } from '../../fetch/axios'

//states
const auth = UseAuthStore()
const application = useApplicationStore()
const UBadge = resolveComponent('UBadge')
const UChip = resolveComponent('UChip')
const UButton = resolveComponent('UButton')
const UCheckbox = resolveComponent('UCheckbox')
const router = useRouter()

const toast = useToast()
const overlay = useOverlay()
const confirmModal = overlay.create(ConfirmationModal)

onMounted(async () => {
  await application.applicationInit()
})

//table column
const columns: TableColumn<Applicaton>[] = [
  {
    accessorKey: 'fullName',
    header: 'Applicant',
    cell: ({ row }) => {
      const createdAt = row.getValue('createdAt') as string

      // ✅ Compare number with number
      const isNew = createdAt
        ? Date.now() < new Date(createdAt).getTime() + 86400000 // 24h from created
        : false

      return isNew
        ? h(UChip, { color: 'primary', size: 'sm', label: 'New' }, () => row.getValue('fullName'))
        : row.getValue('fullName')
    },
  },
  {
    accessorKey: 'degreeStrand',
    header: 'Degree/Strand',
    cell: ({ row }) => row.getValue('degreeStrand'),
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
    cell: ({ row }) => new Date(row.getValue('createdAt')).toDateString(),
  },
  {
    accessorKey: 'updatedAt',
    header: 'Updated At',
    cell: ({ row }) => {
      return row.getValue('updatedAt')
        ? new Date(row.getValue('updatedAt')).toDateString()
        : h('span', { class: 'italic' }, 'Not updated')
    },
  },
  {
    header: 'Actions',
    cell: ({ row }) => {
      const uuid = row.original.applicationUUID as string

      return h('div', { class: 'flex items-center gap-2' }, [
        h(UButton, {
          icon: 'i-lucide-eye',
          size: 'xs',
          variant: 'ghost',
          color: 'primary',
          onClick: () =>
            router.push({
              name: 'application-details',
              params: { uuid },
            }),
        }),
        h(UButton, {
          icon: 'i-lucide-pen',
          size: 'xs',
          variant: 'ghost',
          color: 'info',
          onClick: () =>
            router.push({
              name: 'application-edit',
              params: { uuid },
            }),
        }),
        h(UButton, {
          icon: 'i-lucide-trash',
          size: 'xs',
          variant: 'ghost',
          color: 'error',
          onClick: () => debounceDelete(uuid),
        }),
      ])
    },
  },
]

const debounceDelete = useDebounceFn(async (uuid: string) => {
  const instance = confirmModal.open()

  if (await instance) {
    try {
      await useAxios.delete('/application/delete/' + uuid)

      toast.add({ title: 'Delete successful', color: 'success' })

      await application.applicationInit()
    } catch (error) {
      toast.add({ title: 'Delete failed', color: 'error' })
    }
  }
}, 500)

//row actions
const rowActions = (row: Row<Applicaton>) => {
  return []
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
  {
    title: 'New',
    icon: 'i-lucide-file-plus-corner',
    text: application.applications?.filter(
      (t) => Date.now() < new Date(t.createdAt).getTime() + 86400000,
    ).length,
    color: 'bg-green-400',
  },
])

const table = useTemplateRef('table')
const globalFilter = ref('')
const pagination = ref({
  pageIndex: 0,
  pageSize: 10,
})

const statusFilter = ref(['Pending', 'Approved', 'All'])
const statusSelectedFIlter = ref('All')
const statusFilterResult = computed(() => {
  return application.applications?.filter((t) => {
    if (statusSelectedFIlter.value == 'All') {
      return t.status
    } else {
      return t.status == statusSelectedFIlter.value
    }
  })
})

watch(
  () => pagination.value.pageSize,
  (size) => {
    table.value?.tableApi?.setPageSize(size)
    pagination.value.pageIndex = 0 // reset to first page when limit changes
  },
)

const pageSize = ref(10)

watch(pageSize, (size) => {
  pagination.value.pageSize = size
  pagination.value.pageIndex = 0 // reset to page 1
})
</script>

<template>
  <div class="py-2 my-10">
    <div>
      <h2 class="text-4xl font-black text-primary">Application</h2>
      <p class="text-muted text-sm">Manage and review submitted applications</p>
    </div>
  </div>

  <div class="py-2">
    <UPageGrid
      class="mb-5"
      :ui="{ base: 'relative grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-0' }"
    >
      <UPageCard
        spotlight
        variant="outline"
        orientation="horizontal"
        reverse
        v-for="card in cards"
        :title="card.title"
      >
        <UContainer>
          <div class="flex items-center gap-10">
            <UIcon :name="card.icon" class="size-10" />
            <span class="text-3xl font-bold text-primary">{{ card.text }}</span>
          </div>
        </UContainer>
      </UPageCard>
    </UPageGrid>
  </div>

  <div class="py-2">
    <UCard>
      <template #header>
        <div class="w-full flex items-center mb-4 gap-2 md:flex-nowrap flex-wrap">
          <UInput
            v-model="globalFilter"
            class="w-full shrink-0 sm:shrink"
            placeholder="Filter..."
            icon="i-lucide-search"
          />

          <div class="ms-auto flex items-center gap-2">
            <USelect v-model="statusSelectedFIlter" :items="statusFilter" class="ms-auto" />
            <UInput
              v-model.number="pageSize"
              type="number"
              :min="1"
              class="max-w-sm"
              placeholder="Limit"
              icon="i-lucide-list-ordered"
            />
          </div>
        </div>
      </template>

      <UTable
        ref="table"
        sticky
        v-model:global-filter="globalFilter"
        v-model:pagination="pagination"
        :data="statusFilterResult ?? []"
        :columns="columns"
        class="flex-1"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
      />

      <template #footer>
        <div class="flex justify-end border-t border-default pt-4 px-4">
          <UPagination
            :page="(table?.tableApi?.getState().pagination.pageIndex || 0) + 1"
            :items-per-page="table?.tableApi?.getState().pagination.pageSize"
            :total="table?.tableApi?.getFilteredRowModel().rows.length"
            @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)"
          />
        </div>
      </template>
    </UCard>
  </div>
</template>
