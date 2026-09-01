<script setup lang="ts">
import { watch, ref, computed, h, resolveComponent, useTemplateRef, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useClipboard, useDebounceFn } from '@vueuse/core'
import type { TableColumn, BreadcrumbItem, FormSubmitEvent } from '@nuxt/ui'
import { ApplicationStatusEnum, type ApplicationGetByIdResponse } from './types/applicationType'
import { useAxios } from '../../fetch/axios'
import { OfficeNameLabels, OfficesArray, OfficeNameEnum } from './types/officeSelectValue'
import z from 'zod'
import { validateDateRange, validateAccumulatedHours, isNonEmpty } from '../../utils/validators'
import ConfirmationModal from '../../components/confirmationModal.vue'
import { useApplicationStore } from '../../stores/application.ts'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const overlay = useOverlay()
const UModal = resolveComponent('UModal')
const modalOverlay = overlay.create(h(UModal))
const cModal = overlay.create(ConfirmationModal)
const { copy } = useClipboard()
const formRef = useTemplateRef('form')
const modalRef = useTemplateRef('modal')
const details = ref<ApplicationGetByIdResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const isDisabled = ref<boolean>(true)
const isPending = computed(
  () => details.value?.application?.status === ApplicationStatusEnum.Pending,
)
const application = useApplicationStore()
const isPrinting = ref(false)

// --- Data Fetching ---
watch(
  () => route.params.uuid,
  async (value) => {
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
  },
  { immediate: true },
)

// --- Actions ---
const copyId = (uuid: string) => {
  copy(uuid)
  toast.add({ title: 'Clipboard', description: 'Application ID copied', color: 'success' })
}

const goBack = () => router.back()

// --- Helpers ---
const statusColor = (status: ApplicationStatusEnum) => {
  switch (status) {
    case ApplicationStatusEnum.Rejected:
      return 'error'
    case ApplicationStatusEnum.Approved:
      return 'success'
    default:
      return 'neutral'
  }
}

const statusLabel = (status: ApplicationStatusEnum) => ApplicationStatusEnum[status] ?? 'Unknown'

const genderLabel = (g: number) => ['Male', 'Female', 'Other'][g] ?? 'Unknown'
const gradeLabel = (g: number) => {
  const grades: Record<number, string> = {
    0: 'Senior High School',
    1: 'College',
  }
  return grades[g] ?? 'Unknown'
}

const strandLabel = (s: number | null | undefined) => {
  const map: Record<number, string> = { 0: 'STEM', 1: 'ABM', 2: 'HUMSS', 3: 'GAS', 4: 'ICT' }

  return s !== null && s !== undefined ? (map[s] ?? 'Unknown') : 'N/A'
}

const degreeLabel = (s: number | null | undefined) => {
  const map: Record<number, string> = {
    0: 'BSIT',
    1: 'BSCS',
    2: 'BSN',
    3: 'BSA',
    4: 'BSBA',
    5: 'BSEd',
    6: 'BSCE',
    7: 'BSEE',
    8: 'BSME',
    9: 'BSArch',
    10: 'BSPharma',
    11: 'BSPsych',
  }

  return s !== null && s !== undefined ? (map[s] ?? 'Unknown') : 'N/A'
}

const natureLabel = (n: number) =>
  ({ 0: 'On-the-Job-Training', 1: 'Work Immersion' })[n] ?? 'Unknown'

const requirementTypeLabel = (t: number) =>
  ({ 0: 'MOA', 1: 'Resume', 2: 'Other' })[t] ?? 'Unknown'

// --- Requirements Table Columns ---
const requirementColumns: TableColumn<any>[] = [
  { accessorKey: 'fileName', header: 'File Name' },
  { accessorKey: 'fileType', header: 'File Type' },
  {
    accessorKey: 'requirementType',
    header: 'Requirement Type',
    cell: ({ row }) => requirementTypeLabel(row.original.requirementType),
  },
  {
    accessorKey: 'filePath',
    header: 'Action',
    cell: ({ row }) => {
       const id = row.original.id
       const type = requirementTypeLabel(row.original.requirementType)
       return h(resolveComponent('UButton'), {
         size: 'xs',
         variant: 'ghost',
         color: 'primary',
         icon: 'i-lucide-printer',
         label: `Print Preview`,
         onClick: () => printPreview(id),
       })
     },
   },
]

const items = computed<BreadcrumbItem[]>(() => [
  {
    label: 'Application',
    icon: 'i-lucide-book-open',
    to: '/application',
  },
  {
    label: 'Application details',
    icon: 'i-lucide-box',
    to: '/application/details/' + route.params.uuid,
  },
])

//assign office & approve

const officeSchema = z.object({
  office: z.enum(OfficeNameEnum, { error: 'Office is required' }),
})
type OfficeSchema = z.infer<typeof officeSchema>

const selectedOffice = ref<Partial<OfficeSchema>>({
  office: undefined,
})

const submitOffice = async (payload: FormSubmitEvent<OfficeSchema>) => {
  if (!modalRef.value || !formRef.value) return

  try {
    modalOverlay.open({
      titel: 'Loading',
      close: false,
      body: 'Loading..',
    })
    loading.value = true
    await useAxios.post('/application/details/' + route.params.uuid, {
      office: OfficeNameLabels[payload.data.office],
    })
    toast.add({ title: 'Approved and Assign office: success', color: 'success' })

    if (details.value) {
      details.value.office = {
        id: details.value.office?.id ?? 0,
        officeName: String(payload.data.office),
        userId: details.value.office?.userId ?? '',
        isDeleted: details.value.office?.isDeleted ?? false,
        createdAt: details.value.office?.createdAt ?? new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        deletedAt: details.value.office?.deletedAt ?? null,
      }

      details.value.application.status = ApplicationStatusEnum.Approved
    }
  } catch {
    toast.add({ title: 'Approved and Assign office: Failed', color: 'error' })
  } finally {
    loading.value = false
    modalOverlay.close()
  }
}

const debounceSubmitOffice = useDebounceFn(submitOffice, 1000)

const disableForm = () => {
  isDisabled.value = !isDisabled.value
}

const printEndorsement = async () => {
  try {
    const { data } = await useAxios.get('/endorsement/' + route.params.uuid, {
      responseType: 'blob',
    })

    const url = URL.createObjectURL(data)
    const win = window.open(url, '_blank')
    win?.print()
    URL.revokeObjectURL(url)

    toast.add({
      title: 'Print initiated',
      description: 'Endorsement letter sent to printer',
      color: 'success',
    })
  } catch (error) {
    toast.add({
      title: 'Failed',
      description: 'Could not load endorsement',
      color: 'error',
    })
    console.error('Print failed:', error)
  }
}

const printPreview = async (id: number) => {
  if (isPrinting.value) return
  isPrinting.value = true
  const win = window.open('/api/application/requirements/download/' + id, '_blank', 'width=800,height=600')
  if (win) {
    win.addEventListener('load', () => {
      setTimeout(() => {
        win.print()
        isPrinting.value = false
      }, 500)
    })
    win.addEventListener('error', () => {
      isPrinting.value = false
    })
  } else {
    isPrinting.value = false
    toast.add({ title: 'Popup blocked. Please allow popups for this site.', color: 'error' })
  }
}

const debounceGenerateEndorsement = useDebounceFn(printEndorsement, 1000)
const debouncedRejectApi = useDebounceFn((uuid: string, reason: string) => {
  application.rejectApplication(uuid, reason)
}, 500)

const goToEdit = () => {
  const appUuid = route.params.uuid as string | undefined
  if (!appUuid) {
    toast.add({ title: 'Application ID is missing', color: 'error' })
    return
  }
  router.push({ name: 'application-edit', params: { uuid: appUuid } })
}

const rejectReason = ref('')
const rejectOpen = ref(false)

const openReject = () => {
  rejectReason.value = ''
  rejectOpen.value = true
}

const submitReject = async () => {
  const uuid = route.params.uuid as string | undefined
  if (!uuid) {
    toast.add({ title: 'Application ID is missing', color: 'error' })
    return
  }
  await debouncedRejectApi(uuid, rejectReason.value)
  rejectOpen.value = false
  toast.add({ title: 'Application rejected', color: 'success' })
}

const isRejected = computed(()=>details.value?.application.status === ApplicationStatusEnum.Rejected)
const isApproved = computed(()=>details.value?.application.status === ApplicationStatusEnum.Approved)

const transferOpen = ref(false)
const transferForm = ref({
  office: '',
  startDate: '',
  estimatedEndDate: '',
  accumulatedHours: 0,
})

watch(transferOpen, (isOpen) => {
  if (isOpen && details.value?.placement && details.value.office) {
    transferForm.value = {
      office: details.value.office.officeName,
      startDate: details.value.placement.startDate,
      estimatedEndDate: details.value.placement.estimatedEndDate,
      accumulatedHours: details.value.placement.accumulatedHours,
    }
  }
})

const submitTransfer = async () => {
  if (!route.params.uuid) return
  try {
    // validation
    if (!isNonEmpty(transferForm.value.office)) {
      toast.add({ title: 'Validation', description: 'New office is required', color: 'error' })
      return
    }

    const dateCheck = validateDateRange(transferForm.value.startDate, transferForm.value.estimatedEndDate)
    if (!dateCheck.valid) {
      toast.add({ title: 'Validation', description: dateCheck.message, color: 'error' })
      return
    }

    // Use student's total internship hours when available
    const maxHours = details.value?.student?.totalInternshipHours ?? undefined
    const accCheck = validateAccumulatedHours(transferForm.value.accumulatedHours, maxHours)
    if (!accCheck.valid) {
      toast.add({ title: 'Validation', description: accCheck.message, color: 'error' })
      return
    }

    loading.value = true
    const studentUuid = details.value?.student.studentUUID
    if (!studentUuid) throw new Error('Missing student UUID')

    await useAxios.put(`/admin/placement/${studentUuid}`, transferForm.value)
    toast.add({ title: 'Placement transferred successfully', color: 'success' })
    transferOpen.value = false

    if (details.value) {
      details.value.office!.officeName = transferForm.value.office
      details.value.placement!.startDate = transferForm.value.startDate
      details.value.placement!.estimatedEndDate = transferForm.value.estimatedEndDate
      details.value.placement!.accumulatedHours = transferForm.value.accumulatedHours
    }
  } catch {
    toast.add({ title: 'Failed to transfer placement', color: 'error' })
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <UMain>
    <!-- Header -->
    <div class="flex items-center gap-2 my-3">
      <UButton variant="ghost" color="neutral" icon="i-lucide-arrow-left" @click="goBack">
        Back
      </UButton>

      <UBreadcrumb :items="items" />
    </div>

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
      <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <!-- Left: Title & Meta -->
        <div class="space-y-2">
          <div class="flex flex-wrap items-center gap-2">
            <h1 class="text-2xl font-black text-highlighted">Application Details</h1>
            <UBadge :label="statusLabel(details.application.status)" :color="statusColor(details.application.status)"
              variant="outline" size="md" class="capitalize">
              {{ statusLabel(details.application.status).toString() }}
            </UBadge>
          </div>

          <div class="flex items-center gap-2 text-muted">
            <span class="text-sm font-mono">UUID: {{ details.application.uuid }}</span>
            <UButton size="xs" variant="ghost" color="neutral" icon="i-lucide-copy"
              @click="copyId(details.application.uuid)" aria-label="Copy UUID" />
          </div>
        </div>

        <!-- Right: Actions -->
        <div class="flex flex-wrap items-center gap-2 sm:gap-3">
          <UButton v-if="isApproved" @click="debounceGenerateEndorsement" color="neutral" icon="i-lucide-file-text"
            size="sm" variant="solid">
            Endorsement
          </UButton>

          <UButton @click="goToEdit" icon="i-lucide-pen" size="sm" variant="solid" color="neutral">
            Edit
          </UButton>

          <UButton v-if="isApproved" @click="transferOpen = true" color="primary" icon="i-lucide-building-2" size="sm" variant="solid">
            Transfer Office
          </UButton>

          <UButton @click="openReject" v-if="isPending" color="error" variant="solid" icon="i-lucide-x" size="sm">
            Reject
          </UButton>
        </div>
      </div>

      <USeparator class="my-4" />

      <!-- Tabs -->
      <UTabs :items="[
        { label: 'Student', icon: 'i-lucide-user', slot: 'student' },
        { label: 'Internship', icon: 'i-lucide-briefcase', slot: 'internship' },
        { label: 'Office', icon: 'i-lucide-building', slot: 'office' },
        { label: 'Requirements', icon: 'i-lucide-folder', slot: 'requirements' },
      ]" variant="pill" class="w-full">
        <!-- Student Tab -->
        <template #student>
          <UPageCard title="Student Information" icon="i-lucide-user-round" variant="outline" class="mt-4">
            <UForm ref="form" :disabled="isDisabled" class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Last Name">
                <UInput v-model="details.student.lastName" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="First Name">
                <UInput v-model="details.student.firstName" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Middle Name">
                <UInput v-model="details.student.middleName" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Email">
                <UInput v-model="details.student.email" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Contact Number">
                <UInput v-model="details.student.contactNumber" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Address">
                <UInput v-model="details.student.address" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Date of Birth">
                <UInput v-model="details.student.dateOfBirth" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Gender">
                <UInput :model-value="genderLabel(details.student.gender)" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Grade Level">
                <UInput :model-value="gradeLabel(details.student.gradeLevel)" class="w-full" variant="soft" />
              </UFormField>
            </UForm>
          </UPageCard>
        </template>

        <!-- Internship Tab -->
        <template #internship>
          <UPageCard title="Internship Information" icon="i-lucide-briefcase" variant="outline" class="mt-4">
            <UForm ref="form" :disabled="isDisabled" class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Internship Nature">
                <UInput :model-value="natureLabel(details.student.internshipNature)" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Strand">
                <UInput :model-value="strandLabel(details.student.strand)" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Degree">
                <UInput :model-value="degreeLabel(details.student.degree)" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Total Internship Hours">
                <UInput :model-value="details.student.totalInternshipHours" class="w-full" variant="soft" />
              </UFormField>
            </UForm>
          </UPageCard>
        </template>

        <!-- Office Tab -->
        <template #office>
          <UPageCard :title="details.office ? 'Assigned Office' : 'No Office Assigned'"
            :icon="details.office ? 'i-lucide-building' : 'i-lucide-building-x'" variant="outline" class="mt-4">
            <UForm v-if="details.office" disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Office Name">
                <UInput :model-value="details.office.officeName" class="w-full" variant="soft" />
              </UFormField>
            </UForm>
            <p v-else class="text-muted text-sm">
              This application has not been assigned to an office yet.
            </p>
          </UPageCard>
        </template>

        <!-- Requirements Tab -->
        <template #requirements>
          <UPageCard title="Requirements" icon="i-lucide-folder" variant="outline" class="mt-4">
            <UTable
              v-if="details.requirements?.length"
              :data="details.requirements"
              :columns="requirementColumns"
              class="w-full"
            />
            <p v-else class="text-muted text-sm">No requirements submitted for this application.</p>
          </UPageCard>
        </template>
      </UTabs>

      <div class="flex justify-end gap-3 pt-4">
        <UModal ref="modal" v-if="isPending" title="Assign office & approve" :description="route.params.uuid as string">
          <UButton label="Assign office & approve" color="success" icon="i-lucide-check" variant="solid" size="sm" />

          <template #body>
            <UForm class="flex flex-col gap-3" :schema="officeSchema" :state="selectedOffice"
              @submit="debounceSubmitOffice">
              <UFormField label="Office" required name="office">
                <USelectMenu :value="selectedOffice.office" class="w-full" v-model="selectedOffice.office"
                  :items="OfficesArray" placeholder="Select office" value-key="value" />
              </UFormField>

              <div class="ms-auto">
                <UButton type="submit" color="primary" icon="i-lucide-send" label="Approve" />
              </div>
            </UForm>
          </template>
        </UModal>

        <UModal v-model:open="rejectOpen" title="Reject Application" description="Please provide a reason for rejection">
          <template #body>
            <UFormField label="Reason" required>
              <UTextarea v-model="rejectReason" placeholder="Enter reason for rejection" class="w-full" :rows="3" />
            </UFormField>
          </template>
          <template #footer>
            <div class="flex justify-end gap-3">
              <UButton label="Cancel" variant="ghost" color="neutral" @click="rejectOpen = false" />
              <UButton label="Reject" color="error" variant="solid" @click="submitReject" />
            </div>
          </template>
        </UModal>

        <UModal v-model:open="transferOpen" title="Transfer Office" description="Move student to a different office">
          <template #body>
            <UForm class="flex flex-col gap-3">
              <UFormField label="New Office" required>
                <USelectMenu v-model="transferForm.office" :items="OfficesArray.map(o => ({ label: o.label, value: o.label }))" placeholder="Select office" class="w-full" value-key="value" />
              </UFormField>
              <UFormField label="Start Date" required>
                <UInput v-model="transferForm.startDate" type="date" class="w-full" />
              </UFormField>
              <UFormField label="Estimated End Date" required>
                <UInput v-model="transferForm.estimatedEndDate" type="date" class="w-full" />
              </UFormField>
              <UFormField label="Accumulated Hours" required>
                <UInput v-model.number="transferForm.accumulatedHours" type="number" class="w-full" />
              </UFormField>
            </UForm>
          </template>
          <template #footer>
            <div class="flex justify-end gap-3">
              <UButton label="Cancel" variant="ghost" color="neutral" @click="transferOpen = false" />
              <UButton label="Transfer" color="primary" variant="solid" @click="submitTransfer" :loading="loading" />
            </div>
          </template>
        </UModal>
      </div>
    </template>
      <div v-if="isApproved" class="flex justify-end gap-3 pt-4">
        <UButton @click="debounceGenerateEndorsement" color="primary" icon="i-lucide-printer" variant="solid" size="sm">
          Print Endorsement
        </UButton>
      </div>
    </UMain>
  </template>
