<script setup lang="ts">
import { ref, watch, computed, useTemplateRef } from 'vue'
import type { FormSubmitEvent } from '@nuxt/ui'
import { OnBoardUpdateDtoSchema, type OnBoardUpdateDto } from './validator/onboardingValidator'
import { useOnBoardStore } from '../../stores/onaboard'
import { storeToRefs } from 'pinia'
import { useDebounceFn } from '@vueuse/core'
import { useRoute } from 'vue-router'
import { useAxios } from '../../fetch/axios'
import PageExpired from '../../components/pageExpired.vue'

const toast = useToast()
const onaboard = useOnBoardStore()
const { state, errorMessage } = storeToRefs(onaboard)
const isOpen = ref<boolean>(false)
const moaFile = ref<File | null>(null)
const resumeFile = ref<File | null>(null)
const isSubmitting = ref(false)
const isSuccess = ref(false)
const form = useTemplateRef('form')
const route = useRoute()
const isTokenValid = ref<boolean>(false)

const reviewPayload = ref<FormSubmitEvent<OnBoardUpdateDto> | null>(null)

const isSeniorHigh = computed(() => state.value.student.gradeLevel === 0)
const isCollege = computed(() => state.value.student.gradeLevel === 1)

const estimatedEndDate = computed(() => {
  if (!state.value.student.internshipStartDate || !state.value.student.totalInternshipHours) return ''
  const start = new Date(state.value.student.internshipStartDate)
  const totalDays = Math.ceil(state.value.student.totalInternshipHours / 8)
  const end = new Date(start)
  end.setDate(start.getDate() + totalDays)
  return end.toISOString().split('T')[0]
})

watch(()=>route.params.token, async(value)=>{
    try{
      await useAxios.get('/registrationtoken/verify/' + value)
      isTokenValid.value = true
    }catch(e){
      isTokenValid.value = false
    }
}, {immediate: true})

const genderItems = [
  { value: 0, label: 'Male' },
  { value: 1, label: 'Female' },
  { value: 2, label: 'Others' },
]

const gradeLevelItems = [
  { value: 0, label: 'Senior High School' },
  { value: 1, label: 'College' },
]

const internshipNatureItems = [
  { value: 0, label: 'On-the-Job-Training' },
  { value: 1, label: 'Work Immersion' },
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

watch([moaFile, resumeFile], () => {
  state.value.moaFile = moaFile.value
  state.value.resumeFile = resumeFile.value
  state.value.requirements = [
    ...(moaFile.value ? [moaFile.value] : []),
    ...(resumeFile.value ? [resumeFile.value] : []),
  ]
})

const maxDate = computed(() => {
  const date = new Date()
  date.setFullYear(date.getFullYear() - 15)
  return date.toISOString().split('T')[0]
})
const minStartDate = computed(() => {
  const date = new Date()
  date.setDate(date.getDate() + 7)
  return date
})

// Step 1: Form passes validation → validate files and store payload
const onReview = (payload: FormSubmitEvent<OnBoardUpdateDto>) => {
  if (!moaFile.value) {
    toast.add({ title: 'MOA document is required', color: 'error' })
    return
  }
  if (!resumeFile.value) {
    toast.add({ title: 'Resume / CV is required', color: 'error' })
    return
  }

  reviewPayload.value = payload
  isOpen.value = true
}

// Step 2: User confirms in modal → actually submit
const onConfirm = async () => {
  if (!reviewPayload.value) return

  console.log('MOA file before sync:', moaFile.value)
  console.log('Resume file before sync:', resumeFile.value)

  state.value.moaFile = moaFile.value
  state.value.resumeFile = resumeFile.value

  try {
    isSubmitting.value = true
    await onaboard.onSubmit(reviewPayload.value, route.params.token as string)
    isOpen.value = false
    isSuccess.value = true

    onaboard.stateReset()
    moaFile.value = null
    resumeFile.value = null
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
}

const genderFinder = (index: number) => genderItems.find((t) => t.value === index)?.label
const gradeLevelFinder = (index: number) => gradeLevelItems.find((t) => t.value === index)?.label
const internshipNatureFinder = (index: number) =>
  internshipNatureItems.find((t) => t.value === index)?.label
const degreeFinder = (index: number) => degreeItems.find((t) => t.value === index)?.label
const strandFinder = (index: number) => strandItems.find((t) => t.value === index)?.label
</script>

<template>
  <UPage v-if="isTokenValid" class="p-5">
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
          @error="(e: any) => console.log(e)"
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
                  :max="maxDate"
                  v-model="state.student.dateOfBirth"
                  type="date"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.gender" label="Gender" required>
                <USelectMenu
                  v-model="state.student.gender"
                  placeholder="Select gender"
                  :items="genderItems"
                  class="w-full"
                  value-key="value"
                />
              </UFormField>
              <UFormField name="student.gradeLevel" label="Grade Level" required>
                <USelectMenu
                  v-model="state.student.gradeLevel"
                  placeholder="Select grade level"
                  :items="gradeLevelItems"
                  class="w-full"
                  value-key="value"
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
              <UFormField name="student.schoolName" label="Name of School" required>
                <UInput
                  v-model="state.student.schoolName"
                  placeholder="Enter your school name"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.schoolContactPerson" label="Contact Person" required>
                <UInput
                  v-model="state.student.schoolContactPerson"
                  placeholder="Enter the contact person's name"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.schoolContactPersonEmail" label="Contact Person's Email" required>
                <UInput
                  v-model="state.student.schoolContactPersonEmail"
                  type="email"
                  placeholder="Enter the contact person's email"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="student.schoolContactPersonPhone" label="Contact Person's Number" required>
                <UInput
                  v-model="state.student.schoolContactPersonPhone"
                  placeholder="Enter the contact person's phone number"
                  class="w-full"
                />
              </UFormField>
            </div>
            <UFormField name="student.schoolAddress" label="School Address" required class="mt-4">
              <UTextarea
                v-model="state.student.schoolAddress"
                  placeholder="Enter your school address"
                :rows="3"
                class="w-full"
              />
            </UFormField>
          </UPageCard>

          <!-- Internship Details -->
          <UPageCard title="Internship Details" icon="i-lucide-file" variant="outline">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField name="student.internshipNature" label="Nature of Internship" required>
                <USelectMenu
                  v-model.number="state.student.internshipNature"
                  placeholder="Select internship nature"
                  :items="internshipNatureItems"
                  class="w-full"
                  value-key="value"
                />
              </UFormField>
              <UFormField name="student.strand" v-if="isSeniorHigh" label="Strand" required>
                <USelectMenu
                  v-model.number="state.student.strand"
                  placeholder="Select strand"
                  :items="strandItems"
                  class="w-full"
                  value-key="value"
                />
              </UFormField>
              <UFormField v-if="isCollege" name="student.degree" label="Degree" required>
                <USelectMenu
                  v-model.number="state.student.degree"
                  placeholder="Select degree"
                  :items="degreeItems"
                  class="w-full"
                  value-key="value"
                />
              </UFormField>
              <UFormField name="student.internshipStartDate" label="Start Date" required>
                <UInput
                  :min="minStartDate.toISOString().split('T')[0]"
                  v-model="state.student.internshipStartDate"
                  type="date"
                  class="w-full"
                />
              </UFormField>
              <UFormField
                name="student.internshipTotalHours"
                label="Total Internship Hours"
                required
              >
                <UInput
                  v-model="state.student.totalInternshipHours"
                  type="number"
                  placeholder="Enter total internship hours"
                  min="0"
                  class="w-full"
                />
              </UFormField>
              <UFormField
                label="Estimated End Date"
                description="Auto-calculated based on start date and total hours"
              >
                <UInput
                  :model-value="estimatedEndDate"
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
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <UFormField name="moaFile" label="MOA Document" required>
                <UFileUpload
                  v-model="moaFile"
                  file-icon="i-lucide-file-text"
                  description="Upload your Memorandum of Agreement (PDF only)"
                  accept=".pdf"
                  class="w-full"
                />
              </UFormField>
              <UFormField name="resumeFile" label="Resume / CV" required>
                <UFileUpload
                  v-model="resumeFile"
                  file-icon="i-lucide-file-user"
                  description="Upload your Resume or CV (PDF, DOC, DOCX)"
                  accept=".pdf,.doc,.docx"
                  class="w-full"
                />
              </UFormField>
            </div>
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
                    <template v-else-if="key === 'internshipNature' && typeof value === 'number'">{{
                      internshipNatureFinder(value)
                    }}</template>
                    <template v-else-if="key === 'degree' && typeof value === 'number'">{{
                      degreeFinder(value)
                    }}</template>
                    <template v-else-if="key === 'strand' && typeof value === 'number'">{{
                      strandFinder(value)
                    }}</template>
                    <template v-else>{{ value }}</template>
                  </span>
                </li>
              </ul>
            </UPageCard>

            <UPageCard title="Uploaded Requirements" icon="i-lucide-folder">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <p class="text-sm text-muted mb-1">MOA Document:</p>
                  <p class="font-bold">{{ moaFile?.name || 'No file uploaded' }}</p>
                </div>
                <div>
                  <p class="text-sm text-muted mb-1">Resume / CV:</p>
                  <p class="font-bold">{{ resumeFile?.name || 'No file uploaded' }}</p>
                </div>
              </div>
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

  <PageExpired v-else/>
</template>
