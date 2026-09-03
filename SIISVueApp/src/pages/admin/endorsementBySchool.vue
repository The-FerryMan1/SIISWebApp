<script setup lang="ts">
import { computed, ref, onMounted, useTemplateRef, watch, h } from 'vue'
import { useAxios } from '../../fetch/axios'
import { useRouter } from 'vue-router'
import type { SelectItem } from '@nuxt/ui'
import { getPaginationRowModel } from '@tanstack/vue-table'
import { OfficeNameLabels } from './types/officeSelectValue'

const router = useRouter()

const toast = useToast()

const schools = ref<string[]>([])
const selectedSchool = ref<string>('')
const students = ref<any[]>([])
const selectedOffice = ref<string>('')
const loading = ref(false)
const selectedRow = ref<Record<string, boolean>>({})

function isRowSelected(uuid: string) {
  return !!(selectedRow.value && selectedRow.value[uuid])
}

function toggleRow(uuid: string) {
  if (!selectedRow.value) {
    selectedRow.value = {}
  }
  const current = selectedRow.value[uuid] || false
  selectedRow.value[uuid] = !current
  if (!selectedRow.value[uuid]) {
    delete selectedRow.value[uuid]
  }
}

function getSelectedUuids() {
  return Object.keys(selectedRow.value || {}).filter((key) => selectedRow.value![key])
}

const table = useTemplateRef('table')
const globalFilter = ref('')
const pagination = ref({
  pageIndex: 0,
  pageSize: 10,
})

const schoolOptions = computed<SelectItem[]>(() =>
  schools.value.map((s) => ({ label: s, value: s })),
)

const officeOptions = computed<SelectItem[]>(() => [
  { label: 'All offices', value: '' },
  ...Object.values(OfficeNameLabels).map((name) => ({ label: name, value: name })),
])

const filteredStudents = computed(() => {
  const q = globalFilter.value.trim().toLowerCase()
  let data = students.value
  if (q) {
    data = data.filter((s: any) =>
      [s.fullName, s.degreeStrand, s.officeName, s.status]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(q))
    )
  }
  return data
})

watch(
  () => pagination.value.pageSize,
  (size) => {
    table.value?.tableApi?.setPageSize(size)
    pagination.value.pageIndex = 0
  },
)

onMounted(async () => {
  await loadSchools()
})

async function loadSchools() {
  loading.value = true
  try {
    const { data } = await useAxios.get('application')
    const schoolSet = new Set<string>()
    data.forEach((item: any) => {
      if (item.schoolName) {
        schoolSet.add(item.schoolName)
      }
    })
    schools.value = Array.from(schoolSet).sort()
  } catch {
    toast.add({ title: 'Failed to load schools', color: 'error' })
  } finally {
    loading.value = false
  }
}

async function loadStudents() {
  if (!selectedSchool.value) return
  loading.value = true
  try {
    const { data } = await useAxios.get('application')
    students.value = data.filter((item: any) => {
      const matchSchool = item.schoolName === selectedSchool.value
      const matchOffice = !selectedOffice.value || item.officeName === selectedOffice.value
      const isApproved = item.status === 'Approved'
      return matchSchool && matchOffice && isApproved
    })
  } catch {
    toast.add({ title: 'Failed to load students', color: 'error' })
  } finally {
    loading.value = false
  }
}

async function printEndorsement() {
  if (!selectedSchool.value) {
    toast.add({ title: 'Please select a school', color: 'warning' })
    return
  }

  const selectedUuids = getSelectedUuids()
  if (!selectedUuids.length) {
    toast.add({ title: 'Please select at least one student', color: 'warning' })
    return
  }

  try {
    const { data } = await useAxios.post(
      '/endorsement',
      {
        office: selectedOffice.value || undefined,
        uuids: selectedUuids,
      },
      { responseType: 'blob' },
    )

    const blobUrl = URL.createObjectURL(data)
    const win = window.open(blobUrl, '_blank')
    win?.print()
    URL.revokeObjectURL(blobUrl)
  } catch {
    toast.add({ title: 'Failed to generate endorsement', color: 'error' })
  }
}
</script>

<template>
  <UMain class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-4xl font-black text-primary tracking-tight">Endorsement by School</h1>
        <p class="text-muted text-sm mt-1">Generate bulk endorsement for students from the same university</p>
      </div>
    </div>

    <UCard>
      <div class="flex flex-wrap gap-4 items-end">
        <UFormField label="School / University" required>
          <USelectMenu
            v-model="selectedSchool"
            :items="schoolOptions"
            placeholder="Select school"
            class="w-full md:w-80"
            value-key="value"
            @update:model-value="loadStudents"
          />
        </UFormField>

        <UFormField label="Office" v-if="selectedSchool">
          <USelectMenu
            v-model="selectedOffice"
            :items="officeOptions"
            placeholder="All offices"
            class="w-full md:w-80"
            value-key="value"
            @update:model-value="loadStudents"
          />
        </UFormField>

        <UButton
          icon="i-lucide-settings"
          label="Endorsement Settings"
          color="secondary"
          variant="outline"
          :loading="loading"
          @click="router.push({ name: 'endorsement-settings' })"
        />

        <UButton
          icon="i-lucide-printer"
          label="Print Endorsement"
          color="primary"
          variant="solid"
          :loading="loading"
          @click="printEndorsement"
        />
      </div>
    </UCard>

    <UCard v-if="students.length > 0">
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold">Students from {{ selectedSchool }}</h3>
          <div class="flex items-center gap-2">
            <UInput
              v-model="globalFilter"
              class="w-full sm:w-64"
              placeholder="Search students..."
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
        </div>
      </template>

      <UTable
        ref="table"
        sticky
        v-model:global-filter="globalFilter"
        v-model:pagination="pagination"
        :data="filteredStudents ?? []"
        :columns="[
          {
            id: 'include',
            header: 'Include',
            cell: ({ row }: { row: { original: { studentUUID: string } } }) =>
              h('input', {
                type: 'checkbox',
                checked: isRowSelected(row.original.studentUUID as string),
                onChange: () => toggleRow(row.original.studentUUID as string),
                'aria-label': 'Toggle include',
              }),
          },
          { accessorKey: 'fullName', header: 'Student Name' },
          { accessorKey: 'degreeStrand', header: 'Degree / Strand' },
          { accessorKey: 'officeName', header: 'Office' },
          { accessorKey: 'status', header: 'Status' },
        ]"
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
