<script setup lang="ts">
import { computed, h, onMounted, ref, resolveComponent, useTemplateRef, watch } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { useApplicationStore } from '../../stores/application'
import { getPaginationRowModel } from '@tanstack/vue-table'
import { useRouter } from 'vue-router'
import { useDebounceFn } from '@vueuse/core'
import ConfirmationModal from '../../components/confirmationModal.vue'
import { useAxios } from '../../fetch/axios'

const application = useApplicationStore()
const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')
const UChip = resolveComponent('UChip')
const router = useRouter()

const toast = useToast()
const overlay = useOverlay()
const confirmModal = overlay.create(ConfirmationModal)

onMounted(async () => {
  await application.applicationInit()
})

const columns: TableColumn<any>[] = [
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
        Rejected: 'error' as const,
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

const cards = computed(() => [
  {
    title: 'Applications',
    icon: 'i-lucide-file',
    text: application.applications?.length,
  },
  {
    title: 'Approved',
    icon: 'i-lucide-badge-check',
    text: application.applications?.filter((t) => t.status == 'Approved').length,
  },
  {
    title: 'Pending',
    icon: 'i-lucide-ellipsis',
    text: application.applications?.filter((t) => t.status == 'Pending').length,
  },
  {
    title: 'Rejected',
    icon: 'i-lucide-file-plus-corner',
    text: application.applications?.filter(
      (t) => t.status == "Rejected"
    ).length,
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
    pagination.value.pageIndex = 0
  },
)

const pageSize = ref(10)

watch(pageSize, (size) => {
  pagination.value.pageSize = size
  pagination.value.pageIndex = 0
})
</script>

  <template>
    <div class="py-2 my-10">
      <div>
        <h2 class="text-4xl font-black text-primary tracking-tight">Applications</h2>
        <p class="text-muted text-sm">Manage and review submitted applications</p>
      </div>
    </div>

    <div class="py-2">
      <UPageGrid class="mb-6">
        <UPageCard
          v-for="card in cards"
          :key="card.title"
          :title="card.title"
          :icon="card.icon"
          orientation="horizontal"
          color="info"
        >
          <span class="text-3xl font-bold text-primary">{{ card.text }}</span>
        </UPageCard>
      </UPageGrid>
    </div>

    <UCard>
      <template #header>
        <div class="w-full flex items-center gap-2 md:flex-nowrap flex-wrap">
          <UInput
            v-model="globalFilter"
            class="w-full shrink-0 sm:shrink"
            placeholder="Filter applications..."
            icon="i-lucide-search"
            size="md"
          />

          <div class="ms-auto flex items-center gap-2">
            <USelect v-model="statusSelectedFIlter" :items="statusFilter" class="w-auto" placeholder="Filter by status" />
            <UInput
              v-model.number="pageSize"
              type="number"
              :min="1"
              class="w-24"
              placeholder="Limit"
              icon="i-lucide-list-ordered"
              size="md"
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
  </template>
