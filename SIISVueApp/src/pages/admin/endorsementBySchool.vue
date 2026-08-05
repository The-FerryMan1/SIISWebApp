<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useAxios } from '../../fetch/axios'
import type { SelectItem } from '@nuxt/ui'
import { OfficeNameLabels, type OfficeNameEnum } from './types/officeSelectValue'

const toast = useToast()

const schools = ref<string[]>([])
const selectedSchool = ref<string>('')
const students = ref<any[]>([])
const selectedOffice = ref<string>('')
const loading = ref(false)

const schoolOptions = computed<SelectItem[]>(() =>
  schools.value.map((s) => ({ label: s, value: s })),
)

const officeOptions = computed<SelectItem[]>(() => [
  { label: 'All offices', value: '' },
  ...Object.values(OfficeNameLabels).map((name) => ({ label: name, value: name })),
])

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

  try {
    const url = `/endorsement/school/${encodeURIComponent(selectedSchool.value)}${selectedOffice.value ? '?office=' + encodeURIComponent(selectedOffice.value) : ''}`
    const { data } = await useAxios.get(url, {
      responseType: 'blob',
    })

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
          <USelect
            v-model="selectedSchool"
            :items="schoolOptions"
            placeholder="Select school"
            class="w-full md:w-80"
            @update:model-value="loadStudents"
          />
        </UFormField>

        <UFormField label="Office">
          <USelect
            v-model="selectedOffice"
            :items="officeOptions"
            placeholder="All offices"
            class="w-full md:w-80"
            @update:model-value="loadStudents"
          />
        </UFormField>

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
        <h3 class="text-lg font-semibold">Students from {{ selectedSchool }}</h3>
      </template>

      <UTable :data="students" :columns="[
        { accessorKey: 'fullName', header: 'Student Name' },
        { accessorKey: 'degreeStrand', header: 'Degree / Strand' },
        { accessorKey: 'officeName', header: 'Office' },
        { accessorKey: 'status', header: 'Status' },
      ]" class="w-full" />
    </UCard>
  </UMain>
</template>
