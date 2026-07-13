<script setup lang="ts">
import { watch, ref, computed, h, resolveComponent, useTemplateRef, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useClipboard, useDebounceFn } from '@vueuse/core'
import type { TableColumn, BreadcrumbItem, FormSubmitEvent } from '@nuxt/ui'
import { ApplicationStatusEnum, type ApplicationGetByIdResponse } from './types/applicationType'
import { useAxios } from '../../fetch/axios'
import { OfficeNameLabels, OfficesArray, OfficeNameEnum } from './types/officeSelectValue'
import z from 'zod'
import { PDFViewer } from '@embedpdf/vue-pdf-viewer'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const overlay = useOverlay()
const UModal = resolveComponent('UModal')
const modalOverlay = overlay.create(h(UModal))
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
const pdfSource = ref<string>('')

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
    case ApplicationStatusEnum.Pending:
      return 'warning'
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
    1: '1st Year',
    2: '2nd Year',
    3: '3rd Year',
    4: '4th Year',
    11: 'Grade 11',
    12: 'Grade 12',
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
  ({ 0: 'OJT', 1: 'Apprenticeship', 2: 'Internship', 3: 'Work Immersion' })[n] ?? 'Unknown'

// --- Requirements Table Columns ---
const requirementColumns: TableColumn<any>[] = [
  { accessorKey: 'fileName', header: 'File Name' },
  { accessorKey: 'fileType', header: 'Type' },
  {
    accessorKey: 'filePath',
    header: 'Action',
    cell: ({ row }) => {
      const id = row.original.id
      return h(resolveComponent('UButton'), {
        size: 'xs',
        variant: 'ghost',
        color: 'primary',
        icon: 'i-lucide-download',
        label: 'Download',
        href: '/api/application/requirements/download/' + id,
        target: '_blank',
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
      office: payload.data.office,
    })
    toast.add({ title: 'Approved and Assign office: success', color: 'success' })

    if (details.value) {
      details.value.office = {
        id: details.value.office?.id ?? 0, // or whatever default
        name: payload.data.office as OfficeNameEnum,
        currentOIC: details.value.office?.currentOIC ?? null,
        isDeleted: details.value.office?.isDeleted ?? false,
        createAt: details.value.office?.createAt ?? new Date().toISOString(),
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

const generateEndorsement = async () => {
  try {
    const { data } = await useAxios.get('/endorsement/' + route.params.uuid, {
      responseType: 'blob',
    })

    pdfSource.value = URL.createObjectURL(data)

    toast.add({
      title: 'Preview Ready',
      description: 'Endorsement letter loaded',
      color: 'info',
    })
  } catch (error) {
    toast.add({
      title: 'Failed',
      description: 'Could not load preview',
      color: 'error',
    })
    console.error('Preview failed:', error)
  }
}

const downloadPdf = () => {
  if (!pdfSource.value) return

  const link = document.createElement('a')
  link.href = pdfSource.value
  link.download = `endorsement-letter-${route.params.uuid}.pdf` // ← Control name here
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

const closePreview = () => {
  if (!pdfSource.value) return
  if (typeof pdfSource.value === 'string') URL.revokeObjectURL(pdfSource.value)
  pdfSource.value = ''
}
onBeforeUnmount(() => {
  if (typeof pdfSource.value === 'string') URL.revokeObjectURL(pdfSource.value)
})

const debounceGenerateEndorsement = useDebounceFn(generateEndorsement, 1000)
const debounceDownloadEndorsement = useDebounceFn(downloadPdf, 1000)
const debounceClosePreview = useDebounceFn(closePreview, 500)

const goToEdit = () => {
  router.push({ name: 'application-edit', params: { uuid: route.params.uuid } })
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
            <UBadge
              :label="statusLabel(details.application.status)"
              :color="statusColor(details.application.status)"
              variant="outline"
              size="md"
              class="capitalize"
            >
              <UIcon :name="isPending ? 'i-lucide-circle-dashed' : 'i-lucide-check'" />
              {{ isPending ? 'Pending' : 'Approved' }}
            </UBadge>
          </div>

          <div class="flex items-center gap-2 text-muted">
            <span class="text-sm font-mono">UUID: {{ details.application.applicationUUID }}</span>
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

        <!-- Right: Actions -->
        <div class="flex flex-wrap items-center gap-2 sm:gap-3">
          <UButton
            v-if="!isPending"
            @click="debounceGenerateEndorsement"
            color="neutral"
            icon="i-lucide-file-text"
            size="sm"
            variant="solid"
          >
            Endorsement
          </UButton>

          <UButton @click="goToEdit" icon="i-lucide-pen" size="sm" variant="solid" color="neutral">
            Edit
          </UButton>

          <UButton v-if="isPending" color="error" variant="outline" icon="i-lucide-x" size="sm">
            Reject
          </UButton>

          <UButton v-if="!isPending" color="error" variant="solid" icon="i-lucide-trash" size="sm">
            Delete
          </UButton>
        </div>
      </div>

      <USeparator class="my-4" />

      <!-- Tabs -->
      <UTabs
        :items="[
          { label: 'Student', icon: 'i-lucide-user', slot: 'student' },
          { label: 'School', icon: 'i-lucide-school', slot: 'school' },
          { label: 'Internship', icon: 'i-lucide-briefcase', slot: 'internship' },
          { label: 'Requirements', icon: 'i-lucide-file-text', slot: 'requirements' },
          { label: 'Office', icon: 'i-lucide-building', slot: 'office' },
        ]"
        variant="pill"
        class="w-full"
      >
        <!-- Student Tab -->
        <template #student>
          <UPageCard
            title="Student Information"
            icon="i-lucide-user-round"
            variant="outline"
            class="mt-4"
          >
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
                <UInput
                  :model-value="genderLabel(details.student.gender)"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
              <UFormField label="Grade Level">
                <UInput
                  :model-value="gradeLabel(details.student.gradeLevel)"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
            </UForm>
          </UPageCard>
        </template>

        <!-- School Tab -->
        <template #school>
          <UPageCard
            title="School Information"
            icon="i-lucide-school"
            variant="outline"
            class="mt-4"
          >
            <UForm disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="School Name">
                <UInput v-model="details.school.name" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Address">
                <UInput v-model="details.school.address" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Contact Person">
                <UInput v-model="details.school.contactPerson" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Email">
                <UInput v-model="details.school.email" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Contact Number">
                <UInput v-model="details.school.contactNumber" class="w-full" variant="soft" />
              </UFormField>
            </UForm>
          </UPageCard>
        </template>

        <!-- Internship Tab -->
        <template #internship>
          <UPageCard
            title="Internship Information"
            icon="i-lucide-briefcase"
            variant="outline"
            class="mt-4"
          >
            <UForm disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Nature">
                <UInput
                  :model-value="natureLabel(details.internship.internshipNature)"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
              <UFormField label="Strand">
                <UInput
                  :model-value="strandLabel(details.internship.strand)"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
              <UFormField label="Degree">
                <UInput
                  :model-value="degreeLabel(details.internship.degree) ?? 'N/A'"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
              <UFormField label="Start Date">
                <UInput v-model="details.internship.startDate" class="w-full" variant="soft" />
              </UFormField>
              <UFormField label="Estimated End Date">
                <UInput
                  v-model="details.internship.estimatedEndDate"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
              <UFormField label="Total Hours">
                <UInput
                  v-model="details.internship.internshipTotalHours"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
            </UForm>
          </UPageCard>
        </template>

        <!-- Requirements Tab -->
        <template #requirements>
          <UPageCard
            title="Submitted Requirements"
            icon="i-lucide-file-text"
            variant="outline"
            class="mt-4"
          >
            <UTable
              v-if="details.requirements?.length"
              :data="details.requirements"
              :columns="requirementColumns"
              class="w-full"
            />
            <UAlert
              v-else
              color="neutral"
              icon="i-lucide-inbox"
              title="No requirements submitted yet."
            />
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
                <UInput
                  :model-value="OfficeNameLabels[details.office.name]"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
              <UFormField label="Current OIC">
                <UInput
                  :model-value="details.office.currentOIC ?? 'N/A'"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
            </UForm>
            <p v-else class="text-muted text-sm">
              This application has not been assigned to an office yet.
            </p>
          </UPageCard>
        </template>
      </UTabs>

      <div class="flex justify-end gap-3 pt-4">
        <UModal
          ref="modal"
          v-if="isPending"
          title="Assign office & approve"
          :description="route.params.uuid as string"
        >
          <UButton
            label=" Assign office & approve"
            color="success"
            icon="i-lucide-check"
            variant="subtle"
            size="sm"
          />

          <template #body>
            <UForm
              class="flex flex-col gap-3"
              :schema="officeSchema"
              :state="selectedOffice"
              @submit="debounceSubmitOffice"
            >
              <UFormField label="Office" required name="office">
                <USelect
                  :value="selectedOffice.office"
                  class="w-full"
                  v-model="selectedOffice.office"
                  :items="OfficesArray"
                  placeholder="Select office"
                />
              </UFormField>

              <div class="ms-auto">
                <UButton type="submit" color="primary" icon="i-lucide-send" label="approve" />
              </div>
            </UForm>
          </template>
        </UModal>
      </div>
    </template>
    <UPageCard v-if="!isPending" :class="pdfSource ? 'min-h-125 md:min-h-150' : ''">
      <!-- Minimum height, can grow -->
      <template #header>
        <div v-if="pdfSource" class="flex items-center gap-2 justify-end">
          <UButton
            icon="i-lucide-download"
            variant="soft"
            label="Download pdf"
            @click="debounceDownloadEndorsement"
          />
          <UButton
            icon="i-lucide-x"
            variant="soft"
            label="Close preview"
            @click="debounceClosePreview"
          />
        </div>
      </template>
      <PDFViewer
        class="h-full"
        v-if="pdfSource"
        :config="{ src: pdfSource, theme: { preference: 'light' } }"
        :style="{ width: '100%', height: '100%' }"
      />
      <UAlert v-else description="No preview" />
    </UPageCard>
  </UMain>
</template>
