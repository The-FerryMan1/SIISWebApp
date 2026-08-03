<script setup lang="ts">
import { ref, onMounted, h } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { useOfficeStore, type Office } from '../../stores/office'
import { OfficeNameLabels } from '../admin/types/officeSelectValue'
import { getPaginationRowModel } from '@tanstack/vue-table'
import { useAxios } from '../../fetch/axios'
import { useDebounceFn } from '@vueuse/core'

const office = useOfficeStore()
const toast = useToast()

const accounts = ref<any[]>([])
const loading = ref(false)
const open = ref(false)
const editingId = ref<number | null>(null)

const form = ref({
  officeId: undefined as number | undefined,
  username: '',
  email: '',
  password: '',
})

const passwordRequired = ref(true)

onMounted(async () => {
  if (!office.offices) {
    await office.officeInit()
  }
  await loadAccounts()
})

async function loadAccounts() {
  loading.value = true
  try {
    const { data } = await useAxios.get('/office-accounts')
    accounts.value = data
  } catch {
    toast.add({ title: 'Failed to load office accounts', color: 'error' })
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editingId.value = null
  form.value = { officeId: undefined, username: '', email: '', password: '' }
  passwordRequired.value = true
  open.value = true
}

function openEdit(account: any) {
  editingId.value = account.id
  form.value = {
    officeId: account.officeId,
    username: account.username,
    email: account.email,
    password: '',
  }
  passwordRequired.value = false
  open.value = true
}

const debounceSave = useDebounceFn(async () => {
  try {
    loading.value = true
    if (editingId.value) {
      await useAxios.put(`/office-accounts/${editingId.value}`, {
        username: form.value.username,
        email: form.value.email,
        password: form.value.password || undefined,
      })
      toast.add({ title: 'Account updated', color: 'success' })
    } else {
      await useAxios.post('/office-accounts', {
        officeId: form.value.officeId,
        username: form.value.username,
        email: form.value.email,
        password: form.value.password,
      })
      toast.add({ title: 'Account created', color: 'success' })
    }
    open.value = false
    await loadAccounts()
  } catch (err: any) {
    const msg = err?.response?.data || err?.message || 'Operation failed'
    toast.add({ title: typeof msg === 'string' ? msg : 'Operation failed', color: 'error' })
  } finally {
    loading.value = false
  }
}, 500)

async function deleteAccount(id: number) {
  if (!confirm('Are you sure you want to delete this account?')) return
  try {
    loading.value = true
    await useAxios.delete(`/office-accounts/${id}`)
    toast.add({ title: 'Account deleted', color: 'success' })
    await loadAccounts()
  } catch {
    toast.add({ title: 'Delete failed', color: 'error' })
  } finally {
    loading.value = false
  }
}

const columns: TableColumn<any>[] = [
  { accessorKey: 'id', header: '#', cell: ({ row }) => `#${row.getValue('id')}` },
  { accessorKey: 'officeName', header: 'Office', cell: ({ row }) => row.getValue('officeName') },
  { accessorKey: 'username', header: 'Username' },
  { accessorKey: 'email', header: 'Email' },
  {
    accessorKey: 'createAt',
    header: 'Created',
    cell: ({ row }) => {
      const v = row.getValue('createAt') as string
      return v ? new Date(v).toLocaleDateString() : '-'
    },
  },
  {
    header: 'Actions',
    cell: ({ row }) =>
      h('div', { class: 'flex gap-2' }, [
        h('button', {
          class: 'px-2 py-1 text-sm bg-primary text-white rounded hover:bg-primary/80',
          onClick: () => openEdit(row.original),
        }, 'Edit'),
        h('button', {
          class: 'px-2 py-1 text-sm bg-red-500 text-white rounded hover:bg-red-600',
          onClick: () => deleteAccount(row.original.id),
        }, 'Delete'),
      ]),
  },
]

function goBack() {
  window.history.back()
}
</script>

<template>
  <UMain class="space-y-6">
    <div class="flex items-center justify-between">
      <UButton variant="ghost" color="neutral" icon="i-lucide-arrow-left" @click="goBack">
        Back
      </UButton>
    </div>

    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-4xl font-black text-primary tracking-tight">Office Accounts</h1>
        <p class="text-muted text-sm mt-1">Manage accounts for office officers</p>
      </div>
      <UButton icon="i-lucide-plus" label="Add Account" @click="openCreate" />
    </div>

    <UCard>
      <UTable
        :data="accounts"
        :columns
        :pagination-options="{ getPaginationRowModel: getPaginationRowModel() }"
        :loading
        class="w-full"
      />
    </UCard>

    <UModal v-model="open" :title="editingId ? 'Edit Account' : 'Create Account'">
      <template #body>
        <UForm @submit="debounceSave" class="space-y-4">
          <UFormField label="Office" name="officeId" :required="!editingId">
            <USelect
              v-model="form.officeId"
              :items="office.offices?.map(o => ({
                label: OfficeNameLabels[o.name as keyof typeof OfficeNameLabels] || `Office #${o.id}`,
                value: o.id,
              }))"
              placeholder="Select office"
              class="w-full"
              :disabled="!!editingId"
            />
          </UFormField>

          <UFormField label="Username" name="username" required>
            <UInput v-model="form.username" placeholder="Enter username" class="w-full" />
          </UFormField>

          <UFormField label="Email" name="email" required>
            <UInput v-model="form.email" placeholder="Enter email" type="email" class="w-full" />
          </UFormField>

          <UFormField label="Password" name="password" :required="passwordRequired">
            <UInput v-model="form.password" placeholder="Enter password" type="password" class="w-full" />
          </UFormField>
        </UForm>
      </template>

      <template #footer>
        <div class="flex justify-end gap-3">
          <UButton label="Cancel" variant="ghost" color="neutral" @click="open = false" />
          <UButton :loading="loading" label="Save" variant="solid" color="primary" @click="debounceSave" />
        </div>
      </template>
    </UModal>
  </UMain>
</template>