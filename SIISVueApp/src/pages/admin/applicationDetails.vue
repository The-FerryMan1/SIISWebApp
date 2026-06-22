<script setup lang="ts">
import { watch, ref, computed, h, resolveComponent } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useClipboard } from '@vueuse/core'
import type { TableColumn, BreadcrumbItem  } from '@nuxt/ui'
import { ApplicationStatusEnum, type ApplicationGetByIdResponse } from './types/applicationType'
import { useAxios } from '../../fetch/axios'
import { UseAuthStore } from '../../stores/auth'
import { title } from 'process'
import { describe } from 'zod/v4/core'

const auth = UseAuthStore()
const route = useRoute()
const router = useRouter()
const toast = useToast()
const { copy } = useClipboard()

const details = ref<ApplicationGetByIdResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const overlay = useOverlay()
const modalComp = resolveComponent('UModal')
const modal = overlay.create(h(modalComp))

const isPending = computed(() => details.value?.application?.status === ApplicationStatusEnum.Pending)

// --- Data Fetching ---
watch(() => route.params.uuid, async (value) => {
  if (!value) return
  loading.value = true
  error.value = null
  try {
    const { data } = await useAxios.get('/application/' + route.params.uuid)
    details.value = data
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to load application details'
  } finally {
    loading.value = false
  }
}, { immediate: true })

// --- Actions ---
const copyId = (uuid: string) => {
  copy(uuid)
  toast.add({ title: 'Clipboard', description: 'Application ID copied', color: 'success' })
}

const goBack = () => router.back()

// --- Helpers ---
const statusColor = (status: ApplicationStatusEnum) => {
  switch (status) {
    case ApplicationStatusEnum.Pending: return 'warning'
    case ApplicationStatusEnum.Approved: return 'success'
    default: return 'neutral'
  }
}

const statusLabel = (status: ApplicationStatusEnum) => ApplicationStatusEnum[status] ?? 'Unknown'

const genderLabel = (g: number) => ['Male', 'Female', 'Other'][g] ?? 'Unknown'
const gradeLabel = (g: number) => ['', 'Grade 11', 'Grade 12'][g] ?? 'Unknown'
const strandLabel = (s: number | null) => {
  const map: Record<number, string> = { 1: 'STEM', 2: 'ABM', 3: 'HUMSS' }
  return s ? (map[s] ?? 'Unknown') : 'N/A'
}
const natureLabel = (n: number) => ({ 1: 'Work Immersion' })[n] ?? 'Unknown'

// --- Requirements Table Columns ---
const requirementColumns: TableColumn<any>[] = [
  { accessorKey: 'fileName', header: 'File Name' },
  { accessorKey: 'fileType', header: 'Type' },
  {
    accessorKey: 'filePath',
    header: 'Action',
    cell: ({ row }) => {
      const path = row.getValue('filePath') as string
      return h(resolveComponent('UButton'), {
        size: 'xs',
        variant: 'ghost',
        color: 'primary',
        icon: 'i-lucide-download',
        label: 'Download',
        to: path,
        target: '_blank'
      })
    }
  }
]

const items = computed<BreadcrumbItem[]>(() => [
  {
    label: 'Application',
    icon: 'i-lucide-book-open',
    to: '/application'
  },
  {
    label: 'Application details',
    icon: 'i-lucide-box',
    to: '/application/details/'+ route.params.uuid
  },
])

//assign office & approve
const openModal = () =>{
  modal.open(
    {
      title: 'Assign office & Approved',
      description: route.params.uuid
    }
  )
}

</script>

<template>
  
    <!-- Header -->
    <div class="flex items-center justify-between">
      <UButton variant="ghost" color="neutral" icon="i-lucide-arrow-left" @click="goBack">
        Back
      </UButton>
    </div>

    <UBreadcrumb :items="items" />

    <!-- Loading State -->
    <template v-if="loading">
      <div class="space-y-4">
        <USkeleton class="h-8 w-1/3" />
        <USkeleton class="h-4 w-1/4" />
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6">
          <USkeleton class="h-64 w-full" />
          <USkeleton class="h-64 w-full" />
        </div>
      </div>
    </template>

    <!-- Error State -->
    <UAlert v-else-if="error" color="error" icon="i-lucide-circle-x" :title="error" />

    <!-- Content -->
    <template v-else-if="details">
      <!-- Title + Status -->
      <div class="flex flex-col sm:flex-row sm:items-center gap-3">
        <div>
          <h1 class="text-2xl font-black text-highlighted">Application Details</h1>
          <div class="flex items-center gap-2 mt-1">
            <small class="text-muted">UUID: {{ details.application.applicationUUID }}</small>
            <UButton
              size="xs"
              variant="ghost"
              color="neutral"
              icon="i-lucide-copy"
              @click="copyId(details.application.applicationUUID)"
              aria-label="Copy UUID"
            />
          </div>
        </div>
        <UBadge
          class="sm:ml-auto capitalize"
          :label="statusLabel(details.application.status)"
          :color="statusColor(details.application.status)"
          variant="subtle"
          size="md"
        />
      </div>

      <USeparator class="my-4" />

      <!-- Tabs -->
      <UTabs
        :items="[
          { label: 'Student', icon: 'i-lucide-user', slot: 'student' },
          { label: 'School', icon: 'i-lucide-school', slot: 'school' },
          { label: 'Internship', icon: 'i-lucide-briefcase', slot: 'internship' },
          { label: 'Requirements', icon: 'i-lucide-file-text', slot: 'requirements' },
          { label: 'Office', icon: 'i-lucide-building', slot: 'office' }
        ]"
        variant="pill"
        class="w-full"
      >
        <!-- Student Tab -->
        <template #student>
          <UPageCard title="Student Information" icon="i-lucide-user-round" variant="outline" class="mt-4">
            <UForm disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Last Name">
                <UInput v-model="details.student.lastName" class="w-full" />
              </UFormField>
              <UFormField label="First Name">
                <UInput v-model="details.student.firstName" class="w-full" />
              </UFormField>
              <UFormField label="Middle Name">
                <UInput v-model="details.student.middleName" class="w-full" />
              </UFormField>
              <UFormField label="Email">
                <UInput v-model="details.student.email" class="w-full" />
              </UFormField>
              <UFormField label="Contact Number">
                <UInput v-model="details.student.contactNumber" class="w-full" />
              </UFormField>
              <UFormField label="Address">
                <UInput v-model="details.student.address" class="w-full" />
              </UFormField>
              <UFormField label="Date of Birth">
                <UInput v-model="details.student.dateOfBirth" class="w-full" />
              </UFormField>
              <UFormField label="Gender">
                <UInput :model-value="genderLabel(details.student.gender)" class="w-full" />
              </UFormField>
              <UFormField label="Grade Level">
                <UInput :model-value="gradeLabel(details.student.gradeLevel)" class="w-full" />
              </UFormField>
            </UForm>
          </UPageCard>
        </template>

        <!-- School Tab -->
        <template #school>
          <UPageCard title="School Information" icon="i-lucide-school" variant="outline" class="mt-4">
            <UForm disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="School Name">
                <UInput v-model="details.school.name" class="w-full" />
              </UFormField>
              <UFormField label="Address">
                <UInput v-model="details.school.address" class="w-full" />
              </UFormField>
              <UFormField label="Contact Person">
                <UInput v-model="details.school.contactPerson" class="w-full" />
              </UFormField>
              <UFormField label="Email">
                <UInput v-model="details.school.email" class="w-full" />
              </UFormField>
              <UFormField label="Contact Number">
                <UInput v-model="details.school.contactNumber" class="w-full" />
              </UFormField>
            </UForm>
          </UPageCard>
        </template>

        <!-- Internship Tab -->
        <template #internship>
          <UPageCard title="Internship Information" icon="i-lucide-briefcase" variant="outline" class="mt-4">
            <UForm disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Nature">
                <UInput :model-value="natureLabel(details.internship.internshipNature)" class="w-full" />
              </UFormField>
              <UFormField label="Strand">
                <UInput :model-value="strandLabel(details.internship.strand)" class="w-full" />
              </UFormField>
              <UFormField label="Degree">
                <UInput :model-value="details.internship.degree?.toString() ?? 'N/A'" class="w-full" />
              </UFormField>
              <UFormField label="Start Date">
                <UInput v-model="details.internship.startDate" class="w-full" />
              </UFormField>
              <UFormField label="Estimated End Date">
                <UInput v-model="details.internship.estimatedEndDate" class="w-full" />
              </UFormField>
              <UFormField label="Total Hours">
                <UInput v-model="details.internship.internshipTotalHours" class="w-full" />
              </UFormField>
            </UForm>
          </UPageCard>
        </template>

        <!-- Requirements Tab -->
        <template #requirements>
          <UPageCard title="Submitted Requirements" icon="i-lucide-file-text" variant="outline" class="mt-4">
            <UTable
              v-if="details.requirements?.length"
              :data="details.requirements"
              :columns="requirementColumns"
              class="w-full"
            />
            <UAlert v-else color="neutral" icon="i-lucide-inbox" title="No requirements submitted yet." />
          </UPageCard>
        </template>

        <!-- Office Tab -->
        <template #office>
          <UPageCard
            :title="details.office ? 'Assigned Office' : 'No Office Assigned'"
            :icon="details.office ? 'i-lucide-building' : 'i-lucide-building-x'"
            variant="outline"
            class="mt-4"
          >
            <UForm v-if="details.office" disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Office Name">
                <UInput :model-value="details.office.name" class="w-full" />
              </UFormField>
              <UFormField label="Current OIC">
                <UInput :model-value="details.office.currentOIC ?? 'N/A'" class="w-full" />
              </UFormField>
            </UForm>
            <p v-else class="text-muted text-sm">This application has not been assigned to an office yet.</p>
          </UPageCard>
        </template>
      </UTabs>

     
      <div  class="flex justify-end gap-3 pt-4">


        <UModal v-if="isPending" title="Assign office & approve" :description="route.params.uuid as string">
            <UButton label=" Assign office & approve"color="success" icon="i-lucide-check" variant="subtle" />

            <template>
                <UForm>
                    <UFormField>
                        
                    </UFormField>
                </UForm>
            </template>
        </UModal>

      
         <UButton v-if="!isPending" color="info" icon="i-lucide-file-text">
          Endoresment letter
        </UButton>
        <UButton v-if="isPending" color="error" variant="outline" icon="i-lucide-x">
          Reject
        </UButton>
        <UButton v-if="!isPending" color="error" variant="outline" icon="i-lucide-trash">
          Delete
        </UButton>
       
      </div>
    </template>
</template>