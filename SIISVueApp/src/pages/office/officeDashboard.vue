<script setup lang="ts">
import { ref, onMounted, h, computed } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { useOfficeAccountStore } from '../../stores/officeAuth'
import { useAxios } from '../../fetch/axios'
import { useRouter } from 'vue-router'
import { getPaginationRowModel } from '@tanstack/vue-table'

const officeAuth = useOfficeAccountStore()
const router = useRouter()
const toast = useToast()

const loading = ref(false)
const dashboard = ref<any>(null)
const searchQuery = ref('')
const editOpen = ref(false)
const editingStudent = ref<any>(null)
const editForm = ref({ startDate: '', estimatedEndDate: '', accumulatedHours: 0 })
const departmentOpen = ref(false)
const departmentForm = ref('')
const profileOpen = ref(false)
const profileForm = ref({ lastName: '', firstName: '', middleName: '', username: '', email: '' })

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

const filteredStudents = computed(() => {
  if (!dashboard.value?.students?.length) return []
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return dashboard.value.students
  return dashboard.value.students.filter((s: any) =>
    [s.fullName, s.email, s.school, s.status, s.officeName].some((v) =>
      String(v ?? '').toLowerCase().includes(q)
    )
  )
})

onMounted(async () => {
  if (!officeAuth.isAuthenticated()) {
    router.push({ name: 'office-login' })
    return
  }
  await loadDashboard()
})

async function loadDashboard() {
  loading.value = true
  try {
    const userId = officeAuth.account?.id
    if (!userId) {
      toast.add({ title: 'No account session found', color: 'error' })
      return
    }
    const { data: myOffice } = await useAxios.get('office/my-office')
    const { data } = await useAxios.get(`/office-dashboard/${myOffice.id}`)
    dashboard.value = data
  } catch {
    toast.add({ title: 'Failed to load dashboard', color: 'error' })
  } finally {
    loading.value = false
  }
}

function goToStudent(uuid: string) {
  router.push({ name: 'ojt-details', params: { uuid } })
}

function openEditDates(student: any) {
  editingStudent.value = student
  const maxHours = student.totalHours ?? 0
  editForm.value = {
    startDate: student.startDate || '',
    estimatedEndDate: student.estimatedEndDate || '',
    accumulatedHours: Math.min(student.accumulatedHours ?? 0, maxHours),
  }
  editOpen.value = true
}

async function saveDates() {
  if (!editingStudent.value) return
  try {
    const maxHours = editingStudent.value.totalHours ?? 0
    const cappedHours = Math.min(editForm.value.accumulatedHours, maxHours)
    
    await useAxios.put(`/office-dashboard/placement/${editingStudent.value.studentUuid}`, {
      startDate: editForm.value.startDate,
      estimatedEndDate: editForm.value.estimatedEndDate,
      accumulatedHours: cappedHours,
    })
    toast.add({ title: 'Dates updated successfully', color: 'success' })
    editOpen.value = false
    await loadDashboard()
  } catch {
    toast.add({ title: 'Failed to update dates', color: 'error' })
  }
}

function openEditDepartment() {
  departmentForm.value = dashboard.value?.department || ''
  departmentOpen.value = true
}

async function saveDepartment() {
  try {
    await useAxios.put('/office/my-department', {
      department: departmentForm.value
    })
    toast.add({ title: 'Department updated successfully', color: 'success' })
    departmentOpen.value = false
    await loadDashboard()
  } catch {
    toast.add({ title: 'Failed to update department', color: 'error' })
  }
}

function openEditProfile() {
  profileForm.value = {
    lastName: officeAuth.account?.lastName || '',
    firstName: officeAuth.account?.firstName || '',
    middleName: officeAuth.account?.middleName || '',
    username: officeAuth.account?.userName || '',
    email: officeAuth.account?.email || '',
  }
  profileOpen.value = true
}

async function saveProfile() {
  try {
    await useAxios.put('/user', {
      lastName: profileForm.value.lastName,
      firstName: profileForm.value.firstName,
      middleName: profileForm.value.middleName,
      username: profileForm.value.username,
      email: profileForm.value.email,
    })
    toast.add({ title: 'Profile updated successfully', color: 'success' })
    profileOpen.value = false
    await loadDashboard()
  } catch {
    toast.add({ title: 'Failed to update profile', color: 'error' })
  }
}

async function refreshAccount() {
  try {
    const { data } = await useAxios.get('user')
    officeAuth.account = {
      id: officeAuth.account!.id,
      email: data.email,
      userName: data.username,
      firstName: data.firstName,
      lastName: data.lastName,
      middleName: data.middleName,
      roles: officeAuth.account!.roles,
    }
    localStorage.setItem('officeAccount', JSON.stringify(officeAuth.account))
  } catch {
    // ignore
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
        <h1 class="text-4xl font-black text-primary tracking-tight">Office Dashboard</h1>
        <p class="text-muted text-sm mt-1">Welcome, {{ officeAuth.account?.firstName }} {{ officeAuth.account?.lastName }}</p>
      </div>
      <UButton icon="i-lucide-log-out" label="Logout" variant="outline" color="error" @click="logout" />
    </div>

    <template v-if="dashboard">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <UPageCard title="Total OJTs" icon="i-lucide-users" variant="outline">
          <p class="text-3xl font-bold text-primary">{{ dashboard.totalStudents }}</p>
        </UPageCard>
        <UPageCard title="Approved" icon="i-lucide-check-circle" variant="outline">
          <p class="text-3xl font-bold text-green-600">{{ dashboard.approvedCount }}</p>
        </UPageCard>
        <UPageCard title="Pending" icon="i-lucide-clock" variant="outline">
          <p class="text-3xl font-bold text-yellow-600">{{ dashboard.pendingCount }}</p>
        </UPageCard>
        <UPageCard title="Rejected" icon="i-lucide-x-circle" variant="outline">
          <p class="text-3xl font-bold text-red-600">{{ dashboard.rejectedCount }}</p>
        </UPageCard>
      </div>

      <UPageCard title="Office Information" icon="i-lucide-building" variant="outline">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <p class="text-sm font-medium text-muted">Office</p>
            <p class="text-base font-semibold">{{ dashboard.officeName }}</p>
          </div>
          <div>
            <p class="text-sm font-medium text-muted">Department</p>
            <div class="flex items-center gap-2">
              <p class="text-base">{{ dashboard.department || 'Not assigned' }}</p>
              <UButton icon="i-lucide-pen" size="xs" variant="ghost" color="primary" @click="openEditDepartment" />
            </div>
          </div>
          <div>
            <p class="text-sm font-medium text-muted">Account</p>
            <div class="flex items-center gap-2">
              <p class="text-base">{{ officeAuth.account?.email }}</p>
              <UButton icon="i-lucide-pen" size="xs" variant="ghost" color="primary" @click="openEditProfile" />
            </div>
          </div>
        </div>
      </UPageCard>

      <UCard>
        <template #header>
          <div class="flex items-center justify-between gap-4">
            <h3 class="text-lg font-semibold">Assigned OJTs</h3>
            <UInput
              v-model="searchQuery"
              icon="i-lucide-search"
              placeholder="Search students..."
              class="w-full md:w-64"
            />
          </div>
        </template>

        <UTable
          :data="filteredStudents"
          :columns
          :pagination-options="{ getPaginationRowModel: getPaginationRowModel() }"
          :loading
          class="w-full"
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
      </UCard>

      <UModal v-model:open="profileOpen" title="Edit Profile">
        <template #body>
          <UForm class="space-y-4">
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <UFormField label="Last Name" required>
                <UInput v-model="profileForm.lastName" placeholder="Enter last name" class="w-full" />
              </UFormField>
              <UFormField label="First Name" required>
                <UInput v-model="profileForm.firstName" placeholder="Enter first name" class="w-full" />
              </UFormField>
              <UFormField label="Middle Name" required>
                <UInput v-model="profileForm.middleName" placeholder="Enter middle name" class="w-full" />
              </UFormField>
            </div>
            <UFormField label="Username" required>
              <UInput v-model="profileForm.username" placeholder="Enter username" class="w-full" />
            </UFormField>
            <UFormField label="Email" required>
              <UInput v-model="profileForm.email" placeholder="Enter email" class="w-full" disabled />
            </UFormField>
          </UForm>
        </template>
        <template #footer>
          <div class="flex justify-end gap-3">
            <UButton label="Cancel" variant="ghost" color="neutral" @click="profileOpen = false" />
            <UButton label="Save" variant="solid" color="primary" @click="saveProfile" />
          </div>
        </template>
      </UModal>

      <UModal v-model:open="departmentOpen" title="Edit Department">
        <template #body>
          <UFormField label="Department" required>
            <UInput v-model="departmentForm" placeholder="Enter department name" class="w-full" />
          </UFormField>
        </template>
        <template #footer>
          <div class="flex justify-end gap-3">
            <UButton label="Cancel" variant="ghost" color="neutral" @click="departmentOpen = false" />
            <UButton label="Save" variant="solid" color="primary" @click="saveDepartment" />
          </div>
        </template>
      </UModal>

      <UModal v-model:open="editOpen" title="Edit Internship Dates">
        <template #body>
          <UForm class="space-y-4">
            <UFormField label="Start Date">
              <UInput type="date" v-model="editForm.startDate" class="w-full" />
            </UFormField>
            <UFormField label="Estimated End Date">
              <UInput type="date" v-model="editForm.estimatedEndDate" class="w-full" />
            </UFormField>
            <UFormField label="Accumulated Hours">
              <UInput 
                type="number" 
                v-model="editForm.accumulatedHours" 
                :max="editingStudent?.totalHours ?? 0"
                min="0" 
                class="w-full" 
              />
              <p v-if="editingStudent" class="text-xs text-muted mt-1">
                Maximum: {{ editingStudent.totalHours }} hours
              </p>
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
    </template>

    <template v-else-if="!loading">
      <UAlert icon="i-lucide-info" title="No data available" description="There are no students assigned to your office yet." />
    </template>
  </UMain>
</template>
