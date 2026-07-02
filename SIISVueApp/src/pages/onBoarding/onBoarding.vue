<script setup lang="ts">
import { ref, watch, computed, useTemplateRef } from 'vue'
import type { FormSubmitEvent } from '@nuxt/ui'
import { OnBoardUpdateDtoSchema, type OnBoardUpdateDto } from './validator/onboardingValidator'
import { useOnBoardStore } from '../../stores/onaboard'
import { storeToRefs } from 'pinia'
import { useDebounceFn } from '@vueuse/core'

const toast = useToast()
const onaboard = useOnBoardStore()
const { state, errorMessage } = storeToRefs(onaboard)
const isOpen = ref<boolean>(false)
const uploadedReq = ref<File[]>([])
const isSubmitting = ref(false)
const isSuccess = ref(false)
const form = useTemplateRef('form')

// Store the validated payload from form submit
const reviewPayload = ref<FormSubmitEvent<OnBoardUpdateDto> | null>(null)

// Computed properties for conditional fields
const isSeniorHigh = computed(() => [11, 12].includes(state.value.student.gradeLevel))
const isCollege = computed(() => [1, 2, 3, 4].includes(state.value.student.gradeLevel))

// Estimated end date calculation
const estimatedEndDate = computed(() => {
  if (!state.value.internship.startDate || !state.value.internship.internshipTotalHours) return ''
  const start = new Date(state.value.internship.startDate)
  const totalDays = Math.ceil(state.value.internship.internshipTotalHours / 8)
  const end = new Date(start)
  end.setDate(start.getDate() + totalDays)
  return end.toISOString().split('T')[0]
})

const genderItems = [
  { value: 0, label: 'Male' },
  { value: 1, label: 'Female' },
  { value: 2, label: 'Others' },
]

const gradeLevelItems = [
  { value: 11, label: 'Grade 11' },
  { value: 12, label: 'Grade 12' },
  { value: 1, label: 'First-year college' },
  { value: 2, label: 'Second-year college' },
  { value: 3, label: 'Third-year college' },
  { value: 4, label: 'Fourth-year college' },
]

const internshipNatureItems = [
  { value: 0, label: 'OJT' },
  { value: 1, label: 'Apprenticeship' },
  { value: 2, label: 'Internship' },
  { value: 3, label: 'Work Immersion' },
]

const strandItems = [
  { value: 0, label: 'STEM' },
  { value: 1, label: 'ABM' },
  { value: 2, label: 'HUMSS' },
  { value: 3, label: 'GAS' },
  { value: 4, label: 'ICT' },
]

const degreeItems = [
  { value: 0, label: 'BSIT' },
  { value: 1, label: 'BSCS' },
  { value: 2, label: 'BSN' },
  { value: 3, label: 'BSA' },
  { value: 4, label: 'BSBA' },
  { value: 5, label: 'BSEd' },
  { value: 6, label: 'BSCE' },
  { value: 7, label: 'BSEE' },
  { value: 8, label: 'BSME' },
  { value: 9, label: 'BSArch' },
  { value: 10, label: 'BSPharma' },
  { value: 11, label: 'BSPsych' },
]

watch(estimatedEndDate, (endDate) => {
  if (endDate) state.value.internship.estimatedEndDate = endDate
})

watch(uploadedReq, (value) => {
  state.value.requirements = value
})

watch(
  () => state.value.internship.degree,
  () => {
    state.value.internship.strand = undefined
  },
)

watch(
  () => state.value.internship.strand,
  () => {
    state.value.internship.degree = undefined
  },
)

const maxDate = ref(new Date(new Date().setFullYear(new Date().getFullYear() - 15)))
const minStartDate = computed(() => {
  const date = new Date()
  date.setDate(date.getDate() + 7)
  return date
})

// Step 1: Form passes validation → store payload and open modal
const onReview = (payload: FormSubmitEvent<OnBoardUpdateDto>) => {
  reviewPayload.value = payload
  isOpen.value = true
}

// Step 2: User confirms in modal → actually submit
const onConfirm = useDebounceFn(async () => {
  if (!reviewPayload.value) return

  try {
    isSubmitting.value = true
    await onaboard.onSubmit(reviewPayload.value)
    isOpen.value = false
    isSuccess.value = true

    // Reset everything
    onaboard.stateReset()
    uploadedReq.value = []
    reviewPayload.value = null
  } catch (e) {
    toast.add({ title: 'Submission failed', color: 'error' })

    if (errorMessage.value)
      if (errorMessage.value.includes('already registered')) {
        form.value?.setErrors([
          { name: 'student.email', message: onaboard.errorMessage ?? 'Unexpected error' },
        ])
      }

    isOpen.value = false
  } finally {
    isSubmitting.value = false
  }
}, 500)

const genderFinder = (index: number) => genderItems.find((t) => t.value === index)?.label
const gradeLevelFinder = (index: number) => gradeLevelItems.find((t) => t.value === index)?.label
const internshipNatureFinder = (index: number) =>
  internshipNatureItems.find((t) => t.value === index)?.label
const degreeFinder = (index: number) => degreeItems.find((t) => t.value === index)?.label
const strandFinder = (index: number) => strandItems.find((t) => t.value === index)?.label
</script>

<template>
  <UPage class="p-5">
    <UContainer class="flex flex-col items-center my-10">
      <!-- Header -->
      <div class="flex flex-col text-center justify-center items-center p-3 my-3 gap-2">
        <img
          src="../../assets/img/brand.png"
          class="w-full h-24 min-w-24 min-h-24 block object-contain"
          alt="Brand Logo"
        />
        <h1 class="text-4xl font-bold text-primary uppercase">Student Internship Registration</h1>
        <small class="text-xs italic flex items-center flex-wrap justify-center gap-1">
          The Provincial Government of Cavite complies with Republic Act No. 10173 or the Data
          Privacy Act of 2012 thus, personal information shared will remain confidential.
          <UIcon name="i-lucide-shield-check" class="text-primary" />
        </small>
      </div>

      <!-- Success Screen -->
      <template v-if="isSuccess">
        <UPageCard class="w-full" variant="outline">
          <div class="flex flex-col items-center gap-4 py-16">
            <UIcon name="i-lucide-circle-check-big" class="text-success size-20" />
            <h2 class="text-2xl font-bold text-primary">Application Submitted!</h2>
            <p class="text-muted text-center max-w-md">
              Your internship application has been successfully submitted. We will review your
              application and get back to you via email.
            </p>
            <UButton
              color="primary"
              variant="outline"
              icon="i-lucide-rotate-ccw"
              label="Submit Another Application"
              @click="isSuccess = false"
            />
          </div>
        </UPageCard>
      </template>

      <!-- Form -->
      <template v-else>
        <UForm
          ref="form"
          @submit="onReview"
          :schema="OnBoardUpdateDtoSchema"
          :state="state"
          class="space-y-6 w-full"
          @error="(e) => console.log(e)"
        >
          <!-- Student Information -->
          <UPageCard title="Student Information" icon="i-lucide-user" variant="outline">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField name="student.lastName" label="Last Name" required>
                <UInput
                  v-model="state.student.lastName"
                  placeholder="Enter your last name"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.firstName" label="First Name" required>
                <UInput
                  v-model="state.student.firstName"
                  placeholder="Enter your first name"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.middleName" label="Middle Name">
                <UInput
                  v-model="state.student.middleName"
                  placeholder="Enter your middle name"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.email" label="Email" required>
                <UInput
                  v-model="state.student.email"
                  type="email"
                  placeholder="Enter your email"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.dateOfBirth" label="Date of Birth" required>
                <UInput
                  :max="maxDate.toISOString().split('T')[0]"
                  v-model="state.student.dateOfBirth"
                  type="date"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.gender" label="Gender" required>
                <USelect
                  v-model="state.student.gender"
                  placeholder="Select gender"
                  :items="genderItems"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.gradeLevel" label="Grade Level" required>
                <USelect
                  v-model="state.student.gradeLevel"
                  placeholder="Select grade level"
                  :items="gradeLevelItems"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.contactNumber" label="Contact Number" required>
                <UInput
                  v-model="state.student.contactNumber"
                  placeholder="Enter your contact number"
                  class="w-full"
                />
              </UFormField>
            </div>
            <UFormField name="student.address" label="Address" required class="mt-4">
              <UTextarea
                v-model="state.student.address"
                placeholder="Enter your complete address"
                class="w-full"
                :rows="3"
              />
            </UFormField>
          </UPageCard>

          <!-- School Details -->
          <UPageCard title="School Details" icon="i-lucide-building" variant="outline">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField name="school.name" label="Name of School" required>
                <UInput
                  v-model="state.school.name"
                  placeholder="Enter the school name"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="school.contactPerson" label="Contact Person" required>
                <UInput
                  v-model="state.school.contactPerson"
                  placeholder="Enter the contact person"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="school.email" label="Contact Person's Email" required>
                <UInput
                  v-model="state.school.email"
                  type="email"
                  placeholder="Enter the email of the contact person"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="school.contactNumber" label="Contact Person's Number" required>
                <UInput
                  v-model="state.school.contactNumber"
                  placeholder="Enter the contact person's phone number"
                  class="w-full"
                />
              </UFormField>
            </div>
            <UFormField name="school.address" label="School Address" required class="mt-4">
              <UTextarea
                v-model="state.school.address"
                placeholder="Enter the school address"
                :rows="3"
                class="w-full"
              />
            </UFormField>
          </UPageCard>

          <!-- Internship Details -->
          <UPageCard title="Internship Details" icon="i-lucide-file" variant="outline">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField name="internship.internshipNature" label="Nature of Internship" required>
                <USelect
                  v-model.number="state.internship.internshipNature"
                  placeholder="Select internship nature"
                  :items="internshipNatureItems"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="internship.strand" v-if="isSeniorHigh" label="Strand" required>
                <USelect
                  v-model.number="state.internship.strand"
                  placeholder="Select strand"
                  :items="strandItems"
                  class="w-full"
                />
              </UFormField>
              <UFormField v-if="isCollege" name="internship.degree" label="Degree" required>
                <USelect
                  v-model="state.internship.degree"
                  placeholder="Select degree"
                  :items="degreeItems"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="internship.startDate" label="Start Date" required>
                <UInput
                  :min="minStartDate.toISOString().split('T')[0]"
                  v-model="state.internship.startDate"
                  type="date"
                  class="w-full"
                />
              </UFormField>
              <UFormField
                name="internship.internshipTotalHours"
                label="Total Internship Hours"
                required
              >
                <UInput
                  v-model="state.internship.internshipTotalHours"
                  type="number"
                  placeholder="Enter the total hours of internship"
                  min="0"
                  class="w-full"
                />
              </UFormField>
              <UFormField
                label="Estimated End Date"
                description="Auto-calculated based on start date and total hours"
              >
                <UInput
                  v-model="estimatedEndDate"
                  type="date"
                  disabled
                  placeholder="Auto-calculated"
                  class="w-full"
                />
              </UFormField>
            </div>
          </UPageCard>

          <!-- Requirements -->
          <UPageCard title="Requirements" icon="i-lucide-folder" variant="outline">
            <UFormField name="requirements">
              <UFileUpload
                v-model="uploadedReq"
                file-icon="i-lucide-file"
                description="Upload requirements (MOA, etc.)"
                accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
                multiple
                class="w-full"
              />
            </UFormField>
          </UPageCard>

          <!-- Submit Button -->
          <div class="flex justify-end pt-4">
            <UButton type="submit" color="primary" size="lg" icon="i-lucide-eye">
              Review Application
            </UButton>
          </div>
        </UForm>
      </template>

      <!-- Review Modal (outside UForm) -->
      <UModal v-model:open="isOpen" title="Review Application" size="xl">
        <template #body>
          <div class="flex flex-col gap-5" v-if="state.student">
            <UPageCard title="Student Information" icon="i-lucide-user">
              <ul class="space-y-1">
                <li
                  v-for="(value, key) in state.student"
                  :key="key"
                  class="flex items-center justify-between py-1 border-b border-default last:border-0"
                >
                  <span class="text-muted capitalize">{{ key }}:</span>
                  <span class="font-bold">
                    <template v-if="key === 'gender' && typeof value === 'number'">{{
                      genderFinder(value)
                    }}</template>
                    <template v-else-if="key === 'gradeLevel' && typeof value === 'number'">{{
                      gradeLevelFinder(value)
                    }}</template>
                    <template v-else>{{ value }}</template>
                  </span>
                </li>
              </ul>
            </UPageCard>

            <UPageCard title="School Details" icon="i-lucide-building">
              <ul class="space-y-1">
                <li
                  v-for="(value, key) in state.school"
                  :key="key"
                  class="flex items-center justify-between py-1 border-b border-default last:border-0"
                >
                  <span class="text-muted capitalize">{{ key }}:</span>
                  <span class="font-bold">{{ value }}</span>
                </li>
              </ul>
            </UPageCard>

            <UPageCard title="Internship Details" icon="i-lucide-file">
              <ul class="space-y-1">
                <li
                  v-for="(value, key) in state.internship"
                  :key="key"
                  class="flex items-center justify-between py-1 border-b border-default last:border-0"
                >
                  <span class="text-muted capitalize">{{ key }}:</span>
                  <span class="font-bold">
                    <template v-if="key === 'internshipNature' && typeof value === 'number'">{{
                      internshipNatureFinder(value)
                    }}</template>
                    <template v-else-if="key === 'degree' && typeof value === 'number'">{{
                      degreeFinder(value)
                    }}</template>
                    <template v-else-if="key === 'strand' && typeof value === 'number'">{{
                      strandFinder(value)
                    }}</template>
                    <template v-else>{{ value ?? 'Not applicable' }}</template>
                  </span>
                </li>
              </ul>
            </UPageCard>

            <UPageCard title="Uploaded Requirements" icon="i-lucide-folder">
              <ul class="space-y-1">
                <li
                  v-for="(file, index) in state.requirements"
                  :key="index"
                  class="flex items-center justify-between py-1 border-b border-default last:border-0"
                >
                  <span class="font-bold">{{ file.name }}</span>
                  <span class="text-muted text-xs">{{ (file.size / 1024).toFixed(1) }} KB</span>
                </li>
              </ul>
            </UPageCard>
          </div>
        </template>

        <template #footer>
          <div class="w-full flex items-center justify-between">
            <UButton
              variant="ghost"
              color="neutral"
              icon="i-lucide-arrow-left"
              @click="isOpen = false"
            >
              Go Back & Edit
            </UButton>
            <UButton
              color="primary"
              icon="i-lucide-send"
              label="Confirm & Submit"
              :loading="isSubmitting"
              @click="onConfirm"
            />
          </div>
        </template>
      </UModal>
    </UContainer>
  </UPage>
</template>
