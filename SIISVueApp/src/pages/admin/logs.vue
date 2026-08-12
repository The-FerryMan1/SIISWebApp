<script setup lang="ts">
import { ref, onMounted, h } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { useAxios } from '../../fetch/axios'

const loading = ref(false)
const logs = ref<any[]>([])

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
      <UTable
        :data="logs"
        :columns
        :loading
        class="w-full"
      />
    </UCard>
  </UMain>
</template>
