<script setup lang="ts">
import { ref, onMounted, h } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { useAxios } from '../../fetch/axios'

const loading = ref(false)
const requirements = ref<any[]>([])
const toast = useToast()

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
      <UTable
        :data="requirements"
        :columns
        :loading
        class="w-full"
      />
    </UCard>
  </UMain>
</template>
