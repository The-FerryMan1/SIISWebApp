<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import type {FormSubmitEvent} from '@nuxt/ui'
import { OnBoardUpdateDtoSchema, type OnBoardUpdateDto } from './validator/onboardingValidator'
import { useOnBoardStore } from '../../stores/onaboard'
import { storeToRefs } from 'pinia'


const onaboard = useOnBoardStore()
const {state} = storeToRefs(onaboard)
// Form state


// Computed properties for conditional fields
const isSeniorHigh = computed(() => [11, 12].includes(state.value.student.gradeLevel))
const isCollege = computed(() => [1, 2, 3, 4].includes(state.value.student.gradeLevel))

// Estimated end date calculation (assuming 8 hours per day, 5 days per week)
const estimatedEndDate = computed(() => {
  if (!state.value.internship.startDate || !state.value.internship.internshipTotalHours) return ''
  const start = new Date(state.value.internship.startDate)
  const totalDays = Math.ceil(state.value.internship.internshipTotalHours / 8)
  const end = new Date(start)
  end.setDate(start.getDate() + totalDays)
  return end.toISOString().split('T')[0]
})

// Gender options
const genderItems = [
  { value: 0, label: 'Male' },
  { value: 1, label: 'Female' },
  { value: 2, label: 'Others' }
]

// Grade level options
const gradeLevelItems = [
  { value: 11, label: 'Grade 11' },
  { value: 12, label: 'Grade 12' },
  { value: 1, label: 'First-year college' },
  { value: 2, label: 'Second-year college' },
  { value: 3, label: 'Third-year college' },
  { value: 4, label: 'Fourth-year college' }
]

// Internship nature options
const internshipNatureItems = [
  { value: 0, label: 'OJT' },
  { value: 1, label: 'Apprenticeship' },
  { value: 2, label: 'Internship' },
  { value: 3, label: 'Work Immersion' }
]

// Strand options (SHS)
const strandItems = [
  { value: 0, label: 'STEM' },
  { value: 1, label: 'ABM' },
  { value: 2, label: 'HUMSS' },
  { value: 3, label: 'GAS' },
  { value: 4, label: 'ICT' }
]

// Degree options (College)
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
  { value: 11, label: 'BSPsych' }
]




const onSubmit = async(payload:FormSubmitEvent<OnBoardUpdateDto>)=>{
  console.log(payload)
}

// File upload accept types
const fileAcceptTypes = ['image/png', 'application/pdf', 'image/jpeg', 'image/jpg']
</script>

<template>
  <UPage class="p-5">
    <UContainer class="flex flex-col items-center my-10 ">

        
        <div class="flex flex-col text-center justify-center items-center p-3 my-3 gap-2">
            <h1 class="text-4xl font-bold text-primary">Student Internship Registration</h1>
            <small class="text-muted text-xs">Lorem ipsum dolor sit amet consectetur adipisicing elit. Recusandae, rerum tempora. Numquam maiores ut molestiae iusto voluptatibus ipsa laboriosam eius mollitia sed, animi neque similique voluptate vero, consequuntur tempore dignissimos.</small>
        </div>

      <UForm @submit="onSubmit" :schema="OnBoardUpdateDtoSchema" :state="state" class="space-y-6">
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

            <UFormField name="student.firstName"  label="First Name" required>
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
                placeholder="Enter your valid email"
                  class="w-full"
              />
            </UFormField>

            <UFormField name="student.dateOfBirth" label="Date of Birth" required>
              <UInput
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
                v-model="state.internship.internshipNature"
                placeholder="Select internship nature"
                :items="internshipNatureItems"
                  class="w-full"
              />
            </UFormField>

            <!-- Show only if grade level is NOT college (SHS) -->
            <UFormField
              name="internship.strand"
              v-if="isSeniorHigh"
              label="Strand"
              required
            >
              <USelect
                v-model="state.internship.strand"
                placeholder="Select strand"
                :items="strandItems"
                  class="w-full"
              />
            </UFormField>

            <!-- Show only if grade level is NOT senior high (College) -->
            <UFormField
              v-if="isCollege"
              name="internship.degree"
              label="Degree"
              required
            >
              <USelect
                v-model="state.internship.degree"
                placeholder="Select degree"
                :items="degreeItems"
                  class="w-full"
              />
            </UFormField>

            <UFormField name="internship.startDate" label="Start Date" required>
              <UInput
                v-model="state.internship.startDate"
                type="date"
                  class="w-full"
              />
            </UFormField>

            <UFormField name="internship.internshipTotalHours" label="Total Internship Hours" required>
              <UInput
                v-model="state.internship.internshipTotalHours"
                type="number"
                placeholder="Enter the total hours of internship"
                min="0"
                  class="w-full"
              />
            </UFormField>

            <!-- Auto-calculated based on start date and total hours -->
            <UFormField
              label="Estimated End Date"
              required
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
          <UFormField  name="requirements">
              <UFileUpload
           
            v-model="state.requirements"
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
          <UButton type="submit" color="primary" size="lg">
            Submit Application
          </UButton>
        </div>
      </UForm>
    </UContainer>
  </UPage>
</template>