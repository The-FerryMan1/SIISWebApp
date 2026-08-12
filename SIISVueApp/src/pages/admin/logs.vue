<script setup lang="ts">
import { ref, onMounted, h, useTemplateRef, computed, watch } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { getPaginationRowModel } from '@tanstack/vue-table'
import { useAxios } from '../../fetch/axios'

const loading = ref(false)
const logs = ref<any[]>([])
const toast = useToast()

const table = useTemplateRef('table')
const globalFilter = ref('')
const pagination = ref({
  pageIndex: 0,
  pageSize: 10,
})

const filteredLogs = computed(() => {
  const q = globalFilter.value.trim().toLowerCase()
  if (!q) return logs.value
  return logs.value.filter((l: any) =>
    [l.action, l.entity, l.entityId, l.userId, l.details]
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
  { accessorKey: 'action', header: 'Action' },
  { accessorKey: 'entity', header: 'Entity' },
  { accessorKey: 'entityId', header: 'Entity ID' },
  { accessorKey: 'userId', header: 'User ID' },
  { accessorKey: 'details', header: 'Details' },
  {
    accessorKey: 'createdAt',
    header: 'Timestamp',
    cell: ({ row }) => new Date(row.getValue('createdAt')).toLocaleString(),
  },
]

onMounted(async () => {
  loading.value = true
  try {
    const { data } = await useAxios.get('/logs')
    logs.value = data
  } catch (e: any) {
    console.error('Failed to load logs', e)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <UMain class="py-6">
    <div class="flex items-center justify-between mb-4">
      <div>
        <h1 class="text-3xl font-black text-primary">Audit Logs</h1>
        <p class="text-muted text-sm">System activity and changes</p>
      </div>
    </div>

    <UCard>
      <template #header>
        <div class="flex items-center gap-2 flex-wrap">
          <UInput
            v-model="globalFilter"
            class="w-full sm:w-64"
            placeholder="Search logs..."
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
        :data="filteredLogs ?? []"
        :columns
        :loading
        class="w-full"
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
            @update:page="(p: number) => table?.tableApi?.setPageIndex(p - 1)"
          />
        </div>
      </template>
    </UCard>
  </UMain>
</template>
