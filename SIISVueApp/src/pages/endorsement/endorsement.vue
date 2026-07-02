<script setup lang="ts">
import z from 'zod';
import { OfficeNameEnum, OfficeNameLabels, OfficesArray } from '../admin/types/officeSelectValue';
import { computed, h, ref, resolveComponent, useTemplateRef, watch } from 'vue';

import { storeToRefs } from 'pinia';
import { useOJtStore, type Ojt } from '../../stores/ojt';
import type { TableColumn } from '@nuxt/ui';

type Payload = {
    office: OfficeNameEnum,
    uuids: string[]
}


const ojt = useOJtStore();     
const {ojts} = storeToRefs(ojt)                                                                                                                                                                 
const officeSchema = z.object({
    office: z.enum(OfficeNameEnum, { error: 'Office is required' }),
})
type OfficeSchema = z.infer<typeof officeSchema>

const UCheckbox = resolveComponent('UCheckbox')
const selectedOffice = ref<Partial<OfficeSchema>>({
    office: undefined,
})
const selectedRow = ref()
const table = useTemplateRef('table')
const payload = ref<Partial<Payload>>({
    office: undefined,
    uuids: undefined
})


watch(selectedOffice, async()=>{
    try {
        await ojt.ojtInit()
    } catch (error) {
        
    }
},{immediate: true})


const officeFilter = computed(()=> ojts.value.filter((t)=> t.officeName == selectedOffice.value.office))
const genderLabel = (g: number) => ['Male', 'Female', 'Other'][g] ?? 'Unknown'
const iconGender = (g: number | null) => {
  if (g === null || g === undefined) return 'i-lucide-help-circle'
  return ['i-lucide-mars', 'i-lucide-venus', 'i-lucide-circle-small'][g] ?? 'i-lucide-help-circle'
}
const UChip = resolveComponent('UChip')
const UIcon = resolveComponent('UIcon')
const columns: TableColumn<Ojt>[] = [
     {
    id: 'select',
    header: ({ table }) =>
      h(UCheckbox, {
        modelValue: table.getIsSomePageRowsSelected()
          ? 'indeterminate'
          : table.getIsAllPageRowsSelected(),
        'onUpdate:modelValue': (value: boolean | 'indeterminate') =>
          table.toggleAllPageRowsSelected(!!value),
        'aria-label': 'Select all'
      }),
    cell: ({ row }) =>
      h(UCheckbox, {
        modelValue: row.getIsSelected(),
        'onUpdate:modelValue': (value: boolean | 'indeterminate') => row.toggleSelected(!!value),
        'aria-label': 'Select row'
      })
  },
  {
    accessorKey: 'ojtUUID',
    header: '#',
    cell: ({ row }) => {
      return h('span', { class: ' text-xs ' }, `#${row.getValue('ojtUUID')}`)
    },
  },
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
]
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
const selectedShit = computed(()=>table.value?.tableApi.getSelectedRowModel().rows.map(t => t.getValue('ojtUUID') as string))


const generate = async()=>{
    payload.value = {
        office: selectedOffice.value.office,
        uuids: selectedShit.value
    }

    console.log(payload.value)
}

</script>

<template>
    
    <UMain class="space-y-5">
        <div class="px-10 py-2 my-10">
            <div>
                <h2 class="text-4xl font-black text-primary">Endorsement</h2>
                <p class="text-muted text-sm">Multiple/Bulk endorsement letter generation</p>
            </div>
        </div>
        <UPageCard>
            <template #header class="w-full">
                <UFormField name="selected.office" label="Select Office" required class="w-full">
                    <USelect icon="i-lucide-building" v-model="selectedOffice.office" placeholder="Select office"
                        :items="OfficesArray" class="w-full" />


                        <UButton @click="generate" label="Generate selected rows"/>
                </UFormField>
            </template>
            <UTable ref="table" v-model:row-selection="selectedRow"  :data="officeFilter ??[]" :columns />
        </UPageCard>
       <div class="px-4 py-3.5 border-t border-accented text-sm text-muted">
      {{ table?.tableApi?.getFilteredSelectedRowModel().rows.length || 0 }} of
      {{ table?.tableApi?.getFilteredRowModel().rows.length || 0 }} row(s) selected.
    </div>
    </UMain>
</template>