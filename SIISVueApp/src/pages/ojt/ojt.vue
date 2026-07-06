<script setup lang="ts">
import { UseAuthStore } from '../../stores/auth'
import { computed, h, onMounted, ref, resolveComponent, useTemplateRef, watch } from 'vue'
import type { TableColumn, TableRow } from '@nuxt/ui'
import { useApplicationStore } from '../../stores/application'
import type { Applicaton } from '../../stores/application'
import { getPaginationRowModel, type Row } from '@tanstack/vue-table'
import { useRouter } from 'vue-router'
import { type Ojt, useOJtStore } from '../../stores/ojt'
import { OfficeNameLabels, type OfficeNameEnum } from '../admin/types/officeSelectValue'
import { GenderEnum } from '../onBoarding/validator/onboardingValidator'
import OjtPieChart from './graph/ojtPieChart.vue'

//states
const auth = UseAuthStore()
const application = useApplicationStore()
const ojt = useOJtStore()
const router = useRouter()

const UBadge = resolveComponent('UBadge')
const UChip = resolveComponent('UChip')
const UButton = resolveComponent('UButton')
const UDropdownMenu = resolveComponent('UDropdownMenu')
const UIcon = resolveComponent('UIcon')
const genderLabel = (g: number) => ['Male', 'Female', 'Other'][g] ?? 'Unknown'
const iconGender = (g: number | null) => {
  if (g === null || g === undefined) return 'i-lucide-help-circle'
  return ['i-lucide-mars', 'i-lucide-venus', 'i-lucide-circle-small'][g] ?? 'i-lucide-help-circle'
}

onMounted(async () => {
  await ojt.ojtInit()
})

//table column
const columns: TableColumn<Ojt>[] = [
  {
    accessorKey: 'lastName',
    header: 'Lastname',
    cell: ({ row }) => {
      const createdAt = row.getValue('createdAt') as string

      const isNew = createdAt
        ? Date.now() < new Date(createdAt).getTime() + 86400000 // 24h from created
        : false

      return isNew
        ? h(UChip, { color: 'primary', size: 'sm', label: 'New' }, () => row.getValue('lastName'))
        : row.getValue('lastName')
    },
  },
  {
    accessorKey: 'firstName',
    header: 'Firstname',
    cell: ({ row }) => row.getValue('firstName'),
  },
  {
    accessorKey: 'middleName',
    header: 'Middlename',
    cell: ({ row }) => {
      const value = row.getValue('middleName') as string | null

      return value ? h('span', {}, value) : h('span', { class: 'text-muted' }, 'N/A')
    },
  },
  {
    accessorKey: 'gender',
    header: 'Gender',
    cell: ({ row }) => {
      const value = row.getValue('gender') as number | null

      if (value === null || value === undefined) {
        return h('span', { class: 'text-muted italic' }, 'N/A')
      }

      return h('div', { class: 'flex items-center gap-2' }, [
        h(UIcon, { name: iconGender(value), class: 'size-4' }),
        h('span', genderLabel(value)),
      ])
    },
  },
  {
    accessorKey: 'dateOfBirth',
    header: 'Date of birth',
    cell: ({ row }) => {
      return new Date(row.getValue('dateOfBirth')).toDateString().slice(3)
    },
  },
  {
    accessorKey: 'dateOfBirth',
    header: 'Age',
    cell: ({ row }) => {
      return getAge(row.getValue('dateOfBirth'))
    },
  },
  {
    accessorKey: 'officeName',
    header: 'Office',
    cell: ({ row }) => {
      const value = row.getValue('officeName') as OfficeNameEnum | null
      if (value == null || value == undefined) {
        return h('span', { class: 'text-muted' }, 'N/A')
      } else {
        return h('span', {}, OfficeNameLabels[value])
      }
    },
  },
  {
    header: 'Start date - Estimated end ',
    cell: ({ row }) =>
      `${new Date(row.original.startDate).toDateString()} - ${new Date(row.original.estimatedEndDate).toDateString()}`,
  },
  {
    accessorKey: 'createdAt',
    header: 'Created At',
    cell: ({ row }) => new Date(row.getValue('createdAt')).toDateString(),
  },
  {
    accessorKey: 'updatedAt',
    header: 'Updated At',
    cell: ({ row }) => {
      return row.getValue('updatedAt')
        ? new Date(row.getValue('updatedAt')).toDateString()
        : h('span', { class: 'italic' }, 'Not updated')
    },
  },
    {
    header: 'Actions',
    cell: ({ row }) => {
      const uuid = row.original.ojtUUID as string;

      return h('div', { class: 'flex items-center gap-2' }, [
        h(UButton, {
          icon: 'i-lucide-eye',
          size: 'xs',
          variant: 'ghost',
          color: 'primary',
          onClick: () => router.push({
            name: 'application-details',
            params: { uuid },
          }),
        }),
        h(UButton, {
          icon: 'i-lucide-pen',
          size: 'xs',
          variant: 'ghost',
          color: 'info',
          onClick: () => router.push({
            name: 'application-edit',
            params: { uuid },
          }),
        }),
        h(UButton, {
          icon: 'i-lucide-trash',
          size: 'xs',
          variant: 'ghost',
          color: 'error',
          onClick: () => console.log(uuid),
        }),
      ]);
    },
  },
]

//row actions
function getRowItems(row: Row<Ojt>) {
  return [
    {
      type: 'label',
      label: 'Actions',
    },
    {
      label: 'View details',
      onSelect() {
          
      },
    },
    {
      type: 'separator',
    },
    {
      label: 'Change office',
    },
  ]
}

const getAge = (birthDate: string | Date): number => {
  const today = new Date()
  const birth = new Date(birthDate)

  let age = today.getFullYear() - birth.getFullYear()
  const monthDiff = today.getMonth() - birth.getMonth()

  // Adjust if birthday hasn't occurred yet this year
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
    age--
  }

  return age
}

//cards
const cards = computed(() => [
  {
    title: 'Total OJT',
    icon: 'i-lucide-file',
    text: ojt.ojts?.length,
    color: 'bg-info-400',
  },
])

const table = useTemplateRef('table')
const globalFilter = ref('')
const pagination = ref({
  pageIndex: 0,
  pageSize: 10,
})

const genderFilter = ref(['Male', 'Female', 'Other', 'All'])
const genderSelectedFIlter = ref('All')
const genderFilterResult = computed(() => {
  return ojt.ojts.filter((t) => {
    if (genderSelectedFIlter.value === 'All') {
      return true // Show all
    }
    return genderLabel(t.gender) === genderSelectedFIlter.value
  })
})

watch(
  () => pagination.value.pageSize,
  (size) => {
    table.value?.tableApi?.setPageSize(size)
    pagination.value.pageIndex = 0 // reset to first page when limit changes
  },
)

const pageSize = ref(10)

watch(pageSize, (size) => {
  pagination.value.pageSize = size
  pagination.value.pageIndex = 0 // reset to page 1
})
</script>

<template>
  <UMain>
    <div class="px-10 py-2 my-10">
      <div>
        <h2 class="text-4xl font-black text-primary">OJT</h2>
        <p class="text-muted text-sm"></p>
      </div>
    </div>

    <div class="px-10 py-2">
      <UPageGrid
        class="mb-5"
        :ui="{ base: 'relative grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-0' }"
      >
        <UPageCard
          spotlight
          variant="outline"
          orientation="horizontal"
          reverse
          v-for="card in cards"
          :title="card.title"
        >
          <UContainer>
            <div class="flex items-center gap-10">
              <UIcon :name="card.icon" class="size-10" />
              <span class="text-3xl font-bold text-primary">{{ card.text }}</span>
            </div>
          </UContainer>
        </UPageCard>
      </UPageGrid>
    </div>

    <div class="px-10 py-2">
      <UCard class="mb-5">
        <template #header>
          <div class="w-full flex items-center mb-4 gap-2 md:flex-nowrap flex-wrap">
            <UInput
              v-model="globalFilter"
              class="w-full shrink-0 sm:shrink"
              placeholder="Filter..."
              icon="i-lucide-search"
            />

            <div class="ms-auto flex items-center gap-2">
              <USelect v-model="genderSelectedFIlter" :items="genderFilter" class="ms-auto" />
              <UInput
                v-model.number="pageSize"
                type="number"
                :min="1"
                class="max-w-sm"
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
          :data="genderFilterResult ?? []"
          :columns="columns"
          class="flex-1"
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
              @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)"
            />
          </div>
        </template>
      </UCard>

      <OjtPieChart />
    </div>
  </UMain>
</template>
