<script setup lang="ts">
import { computed, ref, onMounted, useTemplateRef, watch, h, resolveComponent } from 'vue'
import { useAxios } from '../../fetch/axios'
import type { TableColumn, SelectItem } from '@nuxt/ui'
import { getPaginationRowModel } from '@tanstack/vue-table'
import { OfficeNameLabels, OfficeNameEnum, OfficesArray } from '../admin/types/officeSelectValue'

const toast = useToast()

const applications = ref<any[]>([])
const selectedOffice = ref<number | undefined>(undefined)
const selectedSchool = ref<string>('')
const loading = ref(false)
const selectedRow = ref<Record<string, boolean>>({})

const UCheckBox = resolveComponent('UCheckbox')

const table = useTemplateRef('table')
const globalFilter = ref('')
const pagination = ref({
  pageIndex: 0,
  pageSize: 10,
})

const officeOptions = computed<SelectItem[]>(() => [
  { label: 'All offices', value: undefined },
  ...OfficesArray,
])

const schools = computed(() => {
  const set = new Set<string>()
  applications.value.forEach((t) => {
    if (t.schoolName) {
      set.add(t.schoolName)
    }
  })
  return Array.from(set).sort()
})

const filteredApplications = computed(() => {
  const q = globalFilter.value.trim().toLowerCase()
  let data = applications.value
  if (q) {
    data = data.filter((t: any) =>
      [t.fullName, t.schoolName, t.officeName, t.status]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(q)),
    )
  }
  if (selectedOffice.value !== undefined) {
    const officeName = OfficeNameLabels[selectedOffice.value as OfficeNameEnum]
    data = data.filter((t: any) => t.officeName === officeName)
  }
  if (selectedSchool.value) {
    data = data.filter((t: any) => t.schoolName === selectedSchool.value)
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
  await loadApplications()
})

async function loadApplications() {
  loading.value = true
  try {
    const { data } = await useAxios.get('/application')
    applications.value = data.filter((item: any) => item.status === 'Approved')
  } catch {
    toast.add({ title: 'Failed to load applications', color: 'error' })
  } finally {
    loading.value = false
  }
}

const selectedStudentUuids = computed(() => {
  return Object.keys(selectedRow.value).filter((key) => selectedRow.value[key])
})

function toggleInclude(uuid: string) {
  selectedRow.value[uuid] = !selectedRow.value[uuid]
  if (!selectedRow.value[uuid]) {
    delete selectedRow.value[uuid]
  }
}

function toggleAllFiltered() {
  const current = filteredApplications.value.map((t: any) => t.studentUUID as string)
  const allSelected = current.every((uuid) => selectedRow.value[uuid])
  if (allSelected) {
    current.forEach((uuid) => delete selectedRow.value[uuid])
  } else {
    current.forEach((uuid) => (selectedRow.value[uuid] = true))
  }
}

async function generateEndorsement() {
  if (!selectedStudentUuids.value.length) {
    toast.add({ title: 'Please select at least one student to include', color: 'warning' })
    return
  }

  const officeName = selectedOffice.value !== undefined ? OfficeNameLabels[selectedOffice.value as OfficeNameEnum] : ''

  try {
    const { data } = await useAxios.post(
      '/endorsement',
      {
        office: officeName,
        uuids: selectedStudentUuids.value,
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

const columns: TableColumn<any>[] = [
  {
    id: 'include',
    header: 'Include',
    cell: ({ row }) => {
        return h(UCheckBox)
    }
  },
  {
    accessorKey: 'fullName',
    header: 'Student Name',
  },
  {
    accessorKey: 'degreeStrand',
    header: 'Degree / Strand',
  },
  {
    accessorKey: 'schoolName',
    header: 'School',
  },
  {
    accessorKey: 'officeName',
    header: 'Office',
    cell: ({ row }) => {
      const value = row.getValue('officeName') as string | null
      return value ? h('span', {}, value) : h('span', { class: 'text-muted' }, 'N/A')
    },
  },
  {
    accessorKey: 'status',
    header: 'Status',
  },
]
</script>

<template>
  <UMain class="space-y-6">
    <div>
      <h2 class="text-4xl font-black text-primary">Endorsement</h2>
      <p class="text-muted text-sm">Select students to generate bulk endorsement letter</p>
    </div>

    <UCard>
      <div class="flex flex-wrap gap-4 items-end">
        <UFormField label="Office">
          <USelect
            v-model="selectedOffice"
            :items="officeOptions"
            placeholder="All offices"
            class="w-full md:w-80"
          />
        </UFormField>

        <UFormField label="School">
          <USelect
            v-model="selectedSchool"
            :items="schools"
            placeholder="All schools"
            class="w-full md:w-80"
          />
        </UFormField>

        <UFormField label="Search Student">
          <UInput
            v-model="globalFilter"
            class="w-full md:w-80"
            placeholder="Search by name..."
            icon="i-lucide-search"
          />
        </UFormField>

        <UButton
          icon="i-lucide-printer"
          label="Generate Endorsement"
          color="primary"
          variant="solid"
          :loading="loading"
          @click="generateEndorsement"
        />
      </div>
    </UCard>

    <UCard v-if="applications.length > 0">
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold">Approved Students</h3>
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
        v-model:row-selection="selectedRow"
        :data="filteredApplications ?? []"
        :columns="columns"
        class="w-full"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
      />

      <div v-if="!filteredApplications?.length" class="flex items-center justify-center h-32 text-muted">
        No approved students found
      </div>

      <template #footer>
        <div class="flex justify-between border-t border-default pt-4 px-4">
          <div class="text-sm text-muted">
            {{ selectedStudentUuids.length }} student(s) included for endorsement
          </div>
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
