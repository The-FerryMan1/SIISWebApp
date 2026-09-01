<script setup lang="ts">
import { computed, ref, useTemplateRef, watch } from 'vue'
import type { ApplicationGetByIdResponse } from './types/applicationType'
import { useRoute, useRouter } from 'vue-router'
import { useAxios } from '../../fetch/axios'
import { CalendarDate } from '@internationalized/date'
import { useDebounceFn } from '@vueuse/core'
import { OfficesArray } from './types/officeSelectValue'
import { OnBoardUpdateDtoSchema, type OnBoardUpdateDto } from './types/applicationUpdateValidator'
import type { FormSubmitEvent } from '@nuxt/ui'
import { useOnBoardStore } from '../../stores/onaboard'
import { storeToRefs } from 'pinia'
const route = useRoute()
const router = useRouter()
const toast = useToast()
const onboard = useOnBoardStore()
const {state:bstate} = storeToRefs(onboard)

const loading = ref<boolean>(false)
const error = ref()
const details = ref<ApplicationGetByIdResponse | null>(null)
const form = useTemplateRef('form')
const fileUploaded = ref<File[]>([])


const state = ref<Partial<OnBoardUpdateDto>>()

watch(
  () => route.params.uuid,
  async (value) => {
    if (!value) return
    try {
      loading.value = true
      const { data } = await useAxios.get('/application/' + value)
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
    () => details.value?.placement?.startDate ?? '',
    () => details.value?.student?.totalInternshipHours ?? 0,
  ],
  ([start, total]) => {
    if (details.value?.placement && start) {
      details.value.placement.estimatedEndDate = calculateEndDate(start, total, 8) as string
    }
  },
  { immediate: true },
)

const onSubmit = async () => {
  console.log('onSubmit called, details:', details.value)
  try {
    loading.value = true
    const formData = new FormData()

    const student = details.value?.student
    const placement = details.value?.placement

    if (!student) {
      toast.add({ title: 'Missing student data', color: 'warning' })
      return
    }

    formData.append('student.lastName', student.lastName)
    formData.append('student.firstName', student.firstName)
    formData.append('student.middleName', student.middleName)
    formData.append('student.contactNumber', student.contactNumber)
    formData.append('student.address', student.address)
    formData.append('student.dateOfBirth', student.dateOfBirth)
    formData.append('student.email', student.email)
    formData.append('student.gender', String(student.gender))
    formData.append('student.gradeLevel', String(student.gradeLevel))

    formData.append('school.name', student.schoolName)
    formData.append('school.address', student.schoolAddress)
    formData.append('school.contactPerson', student.schoolContactPerson)
    formData.append('school.email', student.schoolContactPersonEmail)
    formData.append('school.contactNumber', student.schoolContactPersonPhone)

    formData.append('internship.internshipNature', String(student.internshipNature))
    formData.append('internship.strand', String(student.strand))
    formData.append('internship.degree', String(student.degree))
    formData.append('internship.internshipTotalHours', String(student.totalInternshipHours))

    if (placement) {
      formData.append('internship.startDate', placement.startDate)
      formData.append('internship.estimatedEndDate', placement.estimatedEndDate)
      formData.append('internship.accumulatedHours', String(placement.accumulatedHours))
    }

    if (fileUploaded.value.length > 0) {
      fileUploaded.value.forEach((file) => {
        if (file instanceof File) {
          formData.append('files', file)
        }
      })
    }

    const uuid = route.params.uuid as string
    console.log('Submitting to:', '/onboading/details/' + uuid)

    await useAxios.put('/onboading/details/' + uuid, formData)
    toast.add({ title: 'Application updated successfully', color: 'success' })
  } catch (error) {
    console.log('Error:', error)
    toast.add({ title: 'Update failed', color: 'error' })
  } finally {
    loading.value = false
  }
}

const save = () => {
  console.log('save clicked, details:', details.value)
  onSubmit()
}

const getRequirementName = (req: any): string => {
  return req.fileName || req.name || ''
}

watch(fileUploaded, (value)=>{
  bstate.value.requirements.push(...value)
})
</script>

<template>
  <div class="flex items-center justify-between">
    <UButton variant="ghost" color="neutral" icon="i-lucide-arrow-left" @click="goBack">
      Back
    </UButton>
  </div>

  <div>
    <h1 class="text-2xl font-black text-primary">Edit Application</h1>
    <div class="flex items-center gap-2 mt-1">
      <p class="text-muted text-sm">Modify application details</p>
    </div>
  </div>

  <template v-if="loading"> loading </template>

    <template v-else>
      <UForm :schema="OnBoardUpdateDtoSchema" :state="state" ref="form">
      <UPageCard
        v-if="details?.student"
        title="Student"
        description="student information"
        icon="i-lucide-user-round"
      >
        <UFormField label="Last name" title="Last Name" name="lastName">
          <UInput
            v-model="details.student.lastName"
            placeholder="Enter your last name"
            class="w-full"
          />
        </UFormField>

        <UFormField label="First name" title="First name" name="firstName">
          <UInput
            v-model="details.student.firstName"
            placeholder="Enter your first name"
            class="w-full"
          />
        </UFormField>

        <UFormField label="Middle name" title="Middle Name" name="middleName">
          <UInput
            v-model="details.student.middleName"
            placeholder="Enter your middle name"
            class="w-full"
          />
        </UFormField>

        <UFormField label="Email address" title="Email Address" name="EmaillAddress">
          <UInput
            v-model="details.student.email"
            placeholder="Enter your email address"
            class="w-full"
            disabled
          />
        </UFormField>

        <UFormField label="Date of birth" title="Date of Birth" name="DateofBirth">
          <UInput
            type="date"
            v-model="details.student.dateOfBirth"
            placeholder="Enter your date of birth"
            class="w-full"
          />
        </UFormField>

        <UFormField label="Gender" title="Gender" name="Gender">
          <USelectMenu
            class="w-full"
            v-model="details.student.gender"
            :items="[
              { label: 'Male', value: 0 },
              { label: 'Female', value: 1 },
              { label: 'others', value: 2 },
            ]"
            value-key="value"
          />
        </UFormField>

        <UFormField label="Contact No." title="Contact number" name="Contact">
          <UInput
            v-model="details.student.contactNumber"
            placeholder="Enter your contact number"
            class="w-full"
          />
        </UFormField>

        <UFormField label="Grade level" title="Grade level" name="GradeLevel">
          <USelectMenu
            class="w-full"
            v-model="details.student.gradeLevel"
            :items="[
              { label: 'Senior High School', value: 0 },
              { label: 'College', value: 1 },
            ]"
            value-key="value"
          />
        </UFormField>

        <UFormField label="Address" title="address" name="address">
          <UInput
            v-model="details.student.address"
            placeholder="Enter your contact number"
            class="w-full"
          />
        </UFormField>
      </UPageCard>

      <!-- school details -->
      <UPageCard
        v-if="details?.student"
        title="School"
        description="School information"
        icon="i-lucide-building"
      >
        <UFormField label="School name" title="name" name="SchoolName">
          <UInput
            v-model="details.student.schoolName"
            placeholder="Enter your school name"
            class="w-full"
          />
        </UFormField>

        <UFormField label="School address" title="address" name="Schooladdress">
          <UInput
            v-model="details.student.schoolAddress"
            placeholder="Enter your school address"
            class="w-full"
          />
        </UFormField>

        <UFormField label="Contact person" title="Contact person" name="ContactPerson">
          <UInput
            v-model="details.student.schoolContactPerson"
            placeholder="Enter school contact person"
            class="w-full"
          />
        </UFormField>

        <UFormField
          label="Contact person's email"
          title="Contact person email"
          name="ContactPersonEmail"
        >
          <UInput
            v-model="details.student.schoolContactPersonEmail"
            placeholder="Enter school contact person's email"
            class="w-full"
          />
        </UFormField>

        <UFormField
          label="Contact person's number"
          title="Contact person number"
          name="ContactPersonNumber"
        >
          <UInput
            type="tel"
            v-model="details.student.schoolContactPersonPhone"
            placeholder="Enter school contact person's number"
            class="w-full"
          />
        </UFormField>
      </UPageCard>

      <!-- internship details -->
      <UPageCard
        v-if="details?.placement"
        title="Internship"
        description="Internship details"
        icon="i-lucide-file-text"
      >
        <UFormField label="Internship nature" title="Internship nature" name="InternshipNature">
          <USelectMenu
            class="w-full"
            v-model="details.student.internshipNature"
            :items="[
              { label: 'On-the-Job-Training', value: 0 },
              { label: 'Work Immersion', value: 1 },
            ]"
            value-key="value"
          />
        </UFormField>

        <UFormField v-if="details.student.strand" label="Strand" title="strand" name="strand">
          <USelectMenu
            class="w-full"
            v-model="details.student.strand"
            :items="[
              { label: 'STEM', value: 0 },
              { label: 'ABM', value: 1 },
              { label: 'HUMSS', value: 2 },
              { label: 'GAS', value: 3 },
              { label: 'ICT', value: 4 },
            ]"
            value-key="value"
          />
        </UFormField>

        <UFormField v-if="details.student.degree" label="Degree" title="degree" name="degree">
          <USelectMenu
            class="w-full"
            v-model="details.student.degree"
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
            value-key="value"
          />
        </UFormField>

        <UFormField label="Internship total hours" title="totalHours" name="TotalHours">
          <UInput
            type="number"
            v-model="details.student.totalInternshipHours"
            placeholder="Enter total internship hours"
            class="w-full"
          />
        </UFormField>

        <UFormField label="Start date" title="Start date" name="startDate">
          <UInput
            type="date"
            v-model="details.placement.startDate"
            placeholder="Select start date"
            class="w-full"
            :min="new Date().toISOString().split('T')[0]"
          />
        </UFormField>

        <UFormField label="Accumulated Hours" title="accumulatedHours" name="AccumulatedHours">
           <UInput
             type="number"
             v-model="details.placement.accumulatedHours"
             placeholder="Enter accumulated hours"
             class="w-full"
           />
         </UFormField>

          <UFormField
            label="Estimated end date"
            title="Estimated end date"
            name="EstimatedEndDate"
            description="Calculated field"
          >
            <UInput
              type="date"
              v-model="details.placement.estimatedEndDate"
              placeholder="Select estimated end date"
              class="w-full"
            />
          </UFormField>
      </UPageCard>

      <UPageCard
        v-if="details?.office"
        title="Office"
        description="Assigned office"
        icon="i-lucide-building"
      >
        <UForm v-if="details.office" disabled class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <UFormField label="Office Name">
            <UInput
              :model-value="details.office.officeName"
              class="w-full"
              variant="soft"
            />
          </UFormField>
        </UForm>
      </UPageCard>
    </UForm>

    <div class="flex w-full my-5 justify-end" v-if="!loading">
      <button
        type="button"
        @click="save"
        class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-primary text-white font-medium hover:bg-primary/90 transition-colors"
      >
        Save
      </button>
    </div>
  </template>
</template>
