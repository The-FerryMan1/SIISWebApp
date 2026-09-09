<script setup lang="ts">
import { useInboxStore } from '../../stores/inbox'
import { onMounted, ref, h, resolveComponent } from 'vue'
import { useRouter } from 'vue-router'
import type { InboxItem } from '../../stores/inbox'
import { useAxios } from '../../fetch/axios'

const router = useRouter()
const inbox = useInboxStore()
const loading = ref(false)
const UBadge = resolveComponent('UBadge')
const UButton = resolveComponent('UButton')

onMounted(async () => {
  loading.value = true
  try {
    await inbox.fetchInbox()
  } finally {
    loading.value = false
  }
})

function navigateTo(item: InboxItem) {
  if (item.actionUrl) {
    router.push(item.actionUrl)
  }
}

const priorityColor = (priority: string) => {
  switch (priority) {
    case 'High': return 'error'
    case 'Medium': return 'warning'
    case 'Low': return 'info'
    default: return 'neutral'
  }
}

const typeLabel = (type: string) => {
  switch (type) {
    case 'PendingApplication': return 'Pending Application'
    case 'ExpiringInternship': return 'Expiring Internship'
    default: return type
  }
}
</script>

<template>
  <UMain class="space-y-6">
    <div>
      <h1 class="text-4xl font-black text-primary tracking-tight">Inbox</h1>
      <p class="text-muted text-sm mt-1">Pending tasks and notifications</p>
    </div>

    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-semibold">Your Inbox</h2>
          <UBadge v-if="inbox.items.length" color="error" variant="solid">
            {{ inbox.items.length }} pending
          </UBadge>
        </div>
      </template>

      <div v-if="loading" class="flex justify-center py-8">
        <USpinner size="lg" />
      </div>

      <div v-else-if="inbox.items.length === 0" class="text-center py-12 text-muted">
        <UIcon name="i-lucide-inbox" class="text-4xl mb-2" />
        <p>No pending tasks</p>
      </div>

      <UTable v-else
        :data="inbox.items"
        :columns="[
          { accessorKey: 'type', header: 'Type', cell: ({ row }) => typeLabel(row.original.type) },
          { accessorKey: 'title', header: 'Title' },
          { accessorKey: 'studentName', header: 'Student' },
          { accessorKey: 'schoolName', header: 'School' },
          { accessorKey: 'officeName', header: 'Office' },
          { accessorKey: 'createdAt', header: 'Date', cell: ({ row }) => new Date(row.original.createdAt).toLocaleDateString() },
          { accessorKey: 'priority', header: 'Priority', cell: ({ row }) => h(UBadge, { color: priorityColor(row.original.priority), variant: 'subtle', size: 'sm' }, () => row.original.priority) },
          { id: 'action', header: '', cell: ({ row }) => h('UButton', { icon: 'i-lucide-arrow-right', size: 'sm', variant: 'ghost', onClick: () => navigateTo(row.original) }) }
        ]"
        class="w-full"
      />
    </UCard>
  </UMain>
</template>
