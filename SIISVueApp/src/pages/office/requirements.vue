<script setup lang="ts">
import { ref, onMounted, h, useTemplateRef, computed, watch } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { getPaginationRowModel } from '@tanstack/vue-table'
import { useAxios } from '../../fetch/axios'

const loading = ref(false)
const requirements = ref<any[]>([])
const toast = useToast()

const table = useTemplateRef('table')
const globalFilter = ref('')
const pagination = ref({
  pageIndex: 0,
  pageSize: 10,
})

const filteredRequirements = computed(() => {
  const q = globalFilter.value.trim().toLowerCase()
  if (!q) return requirements.value
  return requirements.value.filter((r: any) =>
    [r.fileName, r.fileType, r.studentName, r.studentEmail, r.status]
      .filter(Boolean)
      .some((v) => String(v).toLowerCase().includes(q))
  )
})

watch(
  () => pagination.value.pageSize,
  (size) => {
    table.value?.tableApi?.setPageSize(size)
    pagination.value.pageIndex = 0
  },
)

const columns: TableColumn<any>[] = [
  { accessorKey: 'id', header: 'ID' },
  { accessorKey: 'fileName', header: 'File Name' },
  { accessorKey: 'fileType', header: 'Type' },
  { accessorKey: 'studentName', header: 'Student Name' },
  { accessorKey: 'studentEmail', header: 'Student Email' },
  {
    accessorKey: 'createdAt',
    header: 'Submitted',
    cell: ({ row }) => new Date(row.getValue('createdAt')).toLocaleDateString(),
  },
  {
    header: 'Actions',
    cell: ({ row }) => {
      const req = row.original
      return h('div', { class: 'flex items-center gap-2' }, [
        h('a', {
          href: `/api/application/requirements/download/${req.id}`,
          target: '_blank',
          class: 'text-primary hover:underline text-sm',
        }, 'Download'),
      ])
    },
  },
]

onMounted(async () => {
  loading.value = true
  try {
    const { data } = await useAxios.get('/requirements')
    requirements.value = data
  } catch (e: any) {
    toast.add({ title: 'Failed to load requirements', color: 'error' })
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <UMain class="py-6">
    <div class="flex items-center justify-between mb-4">
      <div>
        <h1 class="text-3xl font-black text-primary">Requirements</h1>
        <p class="text-muted text-sm">Submitted intern requirements</p>
      </div>
    </div>

    <UCard>
      <template #header>
        <div class="flex items-center gap-2 flex-wrap">
          <UInput
            v-model="globalFilter"
            class="w-full sm:w-64"
            placeholder="Search requirements..."
            icon="i-lucide-search"
          />
          <UInput
            v-model.number="pagination.pageSize"
            type="number"
            :min="1"
            class="w-full sm:w-24"
            placeholder="Limit"
            icon="i-lucide-list-ordered"
          />
        </div>
      </template>

      <UTable
        ref="table"
        sticky
        v-model:global-filter="globalFilter"
        v-model:pagination="pagination"
        :data="requirements ?? []"
        :columns
        :loading
        class="w-full"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
      />

      <div v-if="!requirements?.length && !loading" class="flex items-center justify-center h-32 text-muted">
        No requirements found
      </div>

      <template #footer>
        <div class="flex justify-end border-t border-default pt-4 px-4">
          <UPagination
            :page="(table?.tableApi?.getState().pagination.pageIndex || 0) + 1"
            :items-per-page="table?.tableApi?.getState().pagination.pageSize"
            :total="table?.tableApi?.getFilteredRowModel().rows.length"
            @update:page="(p: number) => table?.tableApi?.setPageIndex(p - 1)"
          />
        </div>
      </template>
    </UCard>
  </UMain>
</template>
