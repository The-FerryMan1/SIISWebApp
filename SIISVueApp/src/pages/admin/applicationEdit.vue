<script setup lang="ts">
import { computed, ref, useTemplateRef, watch } from 'vue'
import type { ApplicationGetByIdResponse } from './types/applicationType'
import { useRoute, useRouter } from 'vue-router'
import { useAxios } from '../../fetch/axios'

const router = useRouter()
const route = useRoute()
const toast = useToast()
const onaboard = useOnBoardStore()
import { CalendarDate } from '@internationalized/date'
import { useDebounceFn } from '@vueuse/core'
import { OfficeNameLabels, OfficesArray } from './types/officeSelectValue'
import { OnBoardUpdateDtoSchema, type OnBoardUpdateDto } from './types/applicationUpdateValidator'
import type { FormSubmitEvent } from '@nuxt/ui'
import { useOnBoardStore } from '../../stores/onaboard'
import { storeToRefs } from 'pinia'
import axios from 'axios'
const onboard = useOnBoardStore()
const {state:bstate} = storeToRefs(onboard)

const loading = ref<boolean>(false)
const error = ref()
const open = ref<boolean>(false)
const details = ref<ApplicationGetByIdResponse | null>(null)
const form = useTemplateRef('form')
const fileUploaded = ref<File[]>([])


const state = ref<Partial<OnBoardUpdateDto>>()

watch(
  () => route.params.uuid,
  async () => {
    try {
      loading.value = true
      const { data } = await useAxios.get('/application/' + route.params.uuid)
      console.log(data)
      details.value = data
    } catch (err) {
      error.value = err
    } finally {
      loading.value = false
    }
  },
  { immediate: true, once: true },
)

watch(details, (value)=>{
  bstate.value = value as any
})

function goBack() {
  router.back()
}

function calculateEndDate(startDate: string, totalHours: number, hoursPerDay: number = 8) {
  if (!startDate || !totalHours) return ''

  const start = new Date(startDate)
  const totalDays = Math.ceil(totalHours / hoursPerDay)

  const end = new Date(start)
  end.setDate(start.getDate() + totalDays - 1)

  return end.toISOString().split('T')[0]
}

watch(
  [
    () => details.value?.internship?.startDate ?? '',
    () => details.value?.internship?.internshipTotalHours ?? 0,
  ],
  ([start, total]) => {
    if (details.value?.internship) {
      details.value.internship.estimatedEndDate = calculateEndDate(start, total, 8)
    }
  },
  { immediate: true },
)

const onSubmit = async () => {

  try {
    loading.value = true
    await axios.put('/api/onboading/details/' + route.params.uuid, onboard.toDataForm(), {
       headers: {
          'Content-Type': 'multipart/form-data',
        },
    })
    toast.add({ title: 'Application updated successfully', color: 'success' })
  } catch (error) {
    console.log(error)
    toast.add({ title: 'Update failed', color: 'error' })
  } finally {
    loading.value = false
    open.value = false
    console.log(loading.value)
  }
}

const save = () => {
  debounceOnSubmit()
}

const debounceOnSubmit = useDebounceFn(onSubmit, 1000)

const getRequirementName = (req: any): string => {
  return req.fileName || req.name || ''
}

watch(fileUploaded, (value)=>{
  bstate.value.requirements.push(...value)
})
</script>

  <template>
    <UMain class="space-y-8">
      <div class="flex items-center justify-between">
        <UButton variant="ghost" color="neutral" icon="i-lucide-arrow-left" @click="goBack">
          Back
        </UButton>
      </div>

      <div>
        <h1 class="text-4xl font-black text-primary tracking-tight">Edit Application</h1>
        <p class="text-muted text-sm mt-1">Modify application details and requirements</p>
      </div>

      <template v-if="loading">
        <div class="space-y-4">
          <USkeleton class="h-8 w-1/3" />
          <USkeleton class="h-4 w-1/4" />
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <USkeleton class="h-64 w-full" />
            <USkeleton class="h-64 w-full" />
          </div>
        </div>
      </template>

      <template v-else>
        <UForm :schema="OnBoardUpdateDtoSchema" :state="state" ref="form" @submit="debounceOnSubmit" class="space-y-6">
          <UPageCard
            v-if="details?.student"
            title="Student Information"
            description="Update student details"
            icon="i-lucide-user-round"
            variant="outline"
          >
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Last name" title="Last Name" name="lastName">
                <UInput
                  v-model="details.student.lastName"
                  placeholder="Enter last name"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="First name" title="First Name" name="firstName">
                <UInput
                  v-model="details.student.firstName"
                  placeholder="Enter first name"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="Middle name" title="Middle Name" name="middleName">
                <UInput
                  v-model="details.student.middleName"
                  placeholder="Enter middle name"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="Email address" title="Email Address" name="email">
                <UInput
                  v-model="details.student.email"
                  placeholder="Enter email"
                  class="w-full"
                  disabled
                />
              </UFormField>

              <UFormField label="Date of birth" title="Date of Birth" name="dateOfBirth">
                <UInput
                  type="date"
                  v-model="details.student.dateOfBirth"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="Gender" title="Gender" name="gender">
                <USelect
                  class="w-full"
                  v-model="details.student.gender"
                  :items="[
                    { label: 'Male', value: 0 },
                    { label: 'Female', value: 1 },
                    { label: 'Others', value: 2 },
                  ]"
                />
              </UFormField>

              <UFormField label="Contact No." title="Contact Number" name="contact">
                <UInput
                  v-model="details.student.contactNumber"
                  placeholder="Enter contact number"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="Grade level" title="Grade Level" name="gradeLevel">
                <USelect
                  class="w-full"
                  v-model="details.student.gradeLevel"
                  :items="[
                    { label: 'College First Year', value: 1 },
                    { label: 'College Second Year', value: 2 },
                    { label: 'College Third Year', value: 3 },
                    { label: 'College Fourth Year', value: 4 },
                    { label: 'Grade 11', value: 11 },
                    { label: 'Grade 12', value: 12 },
                  ]"
                />
              </UFormField>

              <UFormField label="Address" title="Address" name="address" class="md:col-span-2">
                <UTextarea
                  v-model="details.student.address"
                  placeholder="Enter address"
                  :rows="3"
                  class="w-full"
                />
              </UFormField>
            </div>
          </UPageCard>

          <UPageCard
            v-if="details?.school"
            title="School Information"
            description="Update school details"
            icon="i-lucide-building"
            variant="outline"
          >
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="School name" title="School Name" name="schoolName">
                <UInput
                  v-model="details.school.name"
                  placeholder="Enter school name"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="School address" title="School Address" name="schoolAddress">
                <UInput
                  v-model="details.school.address"
                  placeholder="Enter school address"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="Contact person" title="Contact Person" name="contactPerson">
                <UInput
                  v-model="details.school.contactPerson"
                  placeholder="Enter contact person"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="Contact email" title="Contact Email" name="contactEmail">
                <UInput
                  v-model="details.school.email"
                  placeholder="Enter contact email"
                  type="email"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="Contact number" title="Contact Number" name="contactNumber">
                <UInput
                  v-model="details.school.contactNumber"
                  placeholder="Enter contact number"
                  type="tel"
                  class="w-full"
                />
              </UFormField>
            </div>
          </UPageCard>

          <UPageCard
            v-if="details?.internship"
            title="Internship Details"
            description="Update internship information"
            icon="i-lucide-briefcase"
            variant="outline"
          >
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Internship nature" title="Internship Nature" name="internshipNature">
                <USelect
                  class="w-full"
                  v-model="details.internship.internshipNature"
                  :items="[
                    { label: 'OJT', value: 0 },
                    { label: 'Apprenticeship', value: 1 },
                    { label: 'Internship', value: 2 },
                    { label: 'Work Immersion', value: 3 },
                  ]"
                />
              </UFormField>

              <UFormField v-if="details.internship.strand" label="Strand" title="Strand" name="strand">
                <USelect
                  class="w-full"
                  v-model="details.internship.strand"
                  :items="[
                    { label: 'STEM', value: 0 },
                    { label: 'ABM', value: 1 },
                    { label: 'HUMSS', value: 2 },
                    { label: 'GAS', value: 3 },
                    { label: 'ICT', value: 4 },
                  ]"
                />
              </UFormField>

              <UFormField v-if="details.internship.degree" label="Degree" title="Degree" name="degree">
                <USelect
                  class="w-full"
                  v-model="details.internship.degree"
                  :items="[
                    { label: 'BSIT', value: 0 },
                    { label: 'BSCS', value: 1 },
                    { label: 'BSN', value: 2 },
                    { label: 'BSA', value: 3 },
                    { label: 'BSBA', value: 4 },
                    { label: 'BSEd', value: 5 },
                    { label: 'BSCE', value: 6 },
                    { label: 'BSEE', value: 7 },
                    { label: 'BSME', value: 8 },
                    { label: 'BSArch', value: 9 },
                    { label: 'BSPharma', value: 10 },
                    { label: 'BSPsych', value: 11 },
                  ]"
                />
              </UFormField>

              <UFormField label="Total hours" title="Total Hours" name="totalHours">
                <UInput
                  type="number"
                  v-model="details.internship.internshipTotalHours"
                  placeholder="Enter total hours"
                  class="w-full"
                />
              </UFormField>

              <UFormField label="Start date" title="Start Date" name="startDate">
                <UInput
                  type="date"
                  v-model="details.internship.startDate"
                  class="w-full"
                  :min="new Date().toISOString().split('T')[0]"
                />
              </UFormField>

              <UFormField label="Estimated end date" title="Estimated End Date" name="estimatedEndDate">
                <UInput
                  type="date"
                  v-model="details.internship.estimatedEndDate"
                  class="w-full"
                  disabled
                />
              </UFormField>
            </div>
          </UPageCard>

          <UPageCard
            v-if="details?.office"
            title="Assigned Office"
            description="Office assignment details"
            icon="i-lucide-building"
            variant="outline"
          >
            <UForm disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField label="Office Name">
                <UInput
                  :model-value="OfficeNameLabels[details.office.name]"
                  class="w-full"
                  variant="soft"
                />
              </UFormField>
            </UForm>
          </UPageCard>

          <UPageCard
            v-if="details?.requirements"
            title="Requirements"
            description="Uploaded documents"
            icon="i-lucide-file-text"
            variant="outline"
          >
            <UFormField label="Uploaded files" name="requirements">
              <UList :items="details.requirements" class="w-full">
                <template #default="{ item }">
                  <div class="flex items-center justify-between p-3 rounded-lg bg-default/50">
                    <div class="flex items-center gap-3">
                      <UIcon name="i-lucide-file" class="text-muted" />
                      <span class="text-sm font-medium">{{ getRequirementName(item) }}</span>
                    </div>
                    <UButton
                      v-if="item.id"
                      icon="i-lucide-download"
                      size="xs"
                      variant="ghost"
                      color="primary"
                      :href="'/api/application/requirements/download/' + item.id"
                      target="_blank"
                    />
                  </div>
                </template>
              </UList>
            </UFormField>

            <USeparator class="my-4" />

            <UFormField label="Upload new files" name="newRequirements">
              <UFileUpload
                v-model="fileUploaded"
                multiple
                icon="i-lucide-upload"
                class="w-full"
              />
            </UFormField>
          </UPageCard>

          <div class="flex w-full justify-end gap-3">
            <UModal
              v-model="open"
              title="Confirm Changes"
              description="Are you sure you want to update this application?"
            >
              <UButton
                icon="i-lucide-save"
                label="Save Changes"
                color="primary"
                variant="solid"
              />
              <template #footer>
                <div class="flex justify-end gap-3">
                  <UButton
                    label="Cancel"
                    variant="ghost"
                    color="neutral"
                    @click="open = false"
                  />
                  <UButton
                    :loading="loading"
                    @click="save"
                    icon="i-lucide-save"
                    label="Confirm"
                    variant="solid"
                    color="primary"
                  />
                </div>
              </template>
            </UModal>
          </div>
        </UForm>
      </template>
    </UMain>
  </template>
