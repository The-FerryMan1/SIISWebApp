<script setup lang="ts">
import { ref, onMounted, h, useTemplateRef, computed, watch } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { getPaginationRowModel } from '@tanstack/vue-table'
import { useOfficeAccountStore } from '../../stores/officeAuth'
import { useAxios } from '../../fetch/axios'
import { useRouter } from 'vue-router'

const officeAuth = useOfficeAccountStore()
const router = useRouter()
const toast = useToast()

const loading = ref(false)
const students = ref<any[]>([])
const editOpen = ref(false)
const editingStudent = ref<any>(null)
const editForm = ref({ startDate: '', estimatedEndDate: '' })

const table = useTemplateRef('table')
const globalFilter = ref('')
const pagination = ref({
  pageIndex: 0,
  pageSize: 10,
})

const filteredStudents = computed(() => {
  const q = globalFilter.value.trim().toLowerCase()
  if (!q) return students.value
  return students.value.filter((s: any) =>
    [s.fullName, s.email, s.school, s.status, s.officeName]
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
  { accessorKey: 'fullName', header: 'Student Name' },
  { accessorKey: 'status', header: 'Status', cell: ({ row }) => {
    const status = row.getValue('status') as string
    const color = status === 'Approved' ? 'text-green-600 font-semibold' : status === 'Pending' ? 'text-yellow-600 font-semibold' : 'text-red-600 font-semibold'
    return h('span', { class: color }, status)
  }},
  { accessorKey: 'school', header: 'School' },
  { accessorKey: 'startDate', header: 'Start Date', cell: ({ row }) => {
    const v = row.getValue('startDate') as string
    return v ? new Date(v).toLocaleDateString() : '-'
  }},
  { accessorKey: 'estimatedEndDate', header: 'Est. End Date', cell: ({ row }) => {
    const v = row.getValue('estimatedEndDate') as string
    return v ? new Date(v).toLocaleDateString() : '-'
  }},
  { accessorKey: 'totalHours', header: 'Total Hours' },
  { accessorKey: 'accumulatedHours', header: 'Accumulated' },
  { accessorKey: 'hoursProgress', header: 'Progress', cell: ({ row }) => {
    const progress = row.getValue('hoursProgress') as number
    const color = progress >= 100 ? 'bg-green-500' : progress >= 50 ? 'bg-yellow-500' : 'bg-red-500'
    return h('div', { class: 'flex items-center gap-2' }, [
      h('div', { class: 'w-24 h-2 bg-gray-200 rounded-full overflow-hidden' }, [
        h('div', { class: `h-full ${color} rounded-full transition-all`, style: { width: `${Math.min(progress, 100)}%` } })
      ]),
      h('span', { class: 'text-xs text-muted' }, `${progress}%`)
    ])
  }},
  {
    header: 'Actions',
    cell: ({ row }) => h('div', { class: 'flex gap-1' }, [
      h('button', {
        class: 'p-1 hover:text-primary',
        title: 'Edit dates',
        onClick: () => openEditDates(row.original),
      }, '📅'),
    ]),
  },
]

onMounted(async () => {
  if (!officeAuth.isAuthenticated()) {
    router.push({ name: 'office-login' })
    return
  }
  await loadStudents()
})

async function loadStudents() {
  loading.value = true
  try {
    const userId = officeAuth.account?.id
    if (!userId) {
      toast.add({ title: 'No account session found', color: 'error' })
      return
    }
    const { data: myOffice } = await useAxios.get('office/my-office')
    const { data } = await useAxios.get(`/office-dashboard/${myOffice.id}`)
    students.value = data.students || []
  } catch {
    toast.add({ title: 'Failed to load interns', color: 'error' })
  } finally {
    loading.value = false
  }
}

function goToStudent(uuid: string) {
  router.push({ name: 'ojt-details', params: { uuid } })
}

function openEditDates(student: any) {
  editingStudent.value = student
  editForm.value = {
    startDate: student.startDate || '',
    estimatedEndDate: student.estimatedEndDate || '',
  }
  editOpen.value = true
}

async function saveDates() {
  if (!editingStudent.value) return
  try {
    await useAxios.put(`/office-dashboard/internship/${editingStudent.value.studentUuid}`, {
      startDate: editForm.value.startDate,
      estimatedEndDate: editForm.value.estimatedEndDate,
    })
    toast.add({ title: 'Dates updated successfully', color: 'success' })
    editOpen.value = false
    await loadStudents()
  } catch {
    toast.add({ title: 'Failed to update dates', color: 'error' })
  }
}

function logout() {
  officeAuth.logout()
  router.push({ name: 'office-login' })
}
</script>

<template>
  <UMain class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-4xl font-black text-primary tracking-tight">Interns</h1>
        <p class="text-muted text-sm mt-1">List of interns assigned to your office</p>
      </div>
      <UButton icon="i-lucide-log-out" label="Logout" variant="outline" color="error" @click="logout" />
    </div>

    <UCard>
      <template #header>
        <div class="flex items-center gap-2 flex-wrap">
          <UInput
            v-model="globalFilter"
            class="w-full sm:w-64"
            placeholder="Search interns..."
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
        :data="filteredStudents ?? []"
        :columns
        :loading
        class="w-full"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
        @row-click="(row: any) => goToStudent(row.original.studentUuid)"
      >
        <template #actions-cell="{ row }">
          <UButton
            icon="i-lucide-calendar"
            size="xs"
            variant="ghost"
            color="primary"
            @click.stop="openEditDates(row.original)"
          />
        </template>
      </UTable>

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

    <UModal v-model:open="editOpen" title="Edit Internship Dates">
      <template #body>
        <UForm class="space-y-4">
          <UFormField label="Start Date">
            <UInput type="date" v-model="editForm.startDate" class="w-full" />
          </UFormField>
          <UFormField label="Estimated End Date">
            <UInput type="date" v-model="editForm.estimatedEndDate" class="w-full" />
          </UFormField>
        </UForm>
      </template>

      <template #footer>
        <div class="flex justify-end gap-3">
          <UButton label="Cancel" variant="ghost" color="neutral" @click="editOpen = false" />
          <UButton label="Save" variant="solid" color="primary" @click="saveDates" />
        </div>
      </template>
    </UModal>
  </UMain>
</template>
