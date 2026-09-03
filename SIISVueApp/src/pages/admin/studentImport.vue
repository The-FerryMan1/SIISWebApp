<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAxios } from '../../fetch/axios'

const file = ref<File | null>(null)
const uploading = ref(false)
const errorMessage = ref<string | null>(null)
const result = ref<null | {
  totalRows: number
  importedCount: number
  skippedCount: number
  errors: Array<{ rowNumber: number; email?: string; message: string }>
}>(null)

const router = useRouter()
const toast = useToast()

const onFileChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  file.value = target.files?.[0] ?? null
  errorMessage.value = null
  result.value = null
}

const upload = async () => {
  if (!file.value) {
    errorMessage.value = 'Please select an Excel file to upload.'
    return
  }

  uploading.value = true
  errorMessage.value = null
  result.value = null

  try {
    const formData = new FormData()
    formData.append('file', file.value)

    const { data } = await useAxios.post('student-import/', formData)

    result.value = data
    toast.add({ title: 'Import complete', description: 'Student import finished successfully.', color: 'success' })
  } catch (error) {
    const message = (error as any)?.response?.data || (error as Error).message
    errorMessage.value = String(message)
    toast.add({ title: 'Import failed', description: errorMessage.value, color: 'error' })
  } finally {
    uploading.value = false
  }
}

const back = () => router.back()
</script>

<template>
  <UMain>
    <div class="flex flex-col gap-4 py-4">
      <div class="flex flex-col gap-2">
        <UButton label="Back" icon="i-lucide-arrow-left" variant="ghost" @click="back" />
        <h1 class="text-3xl font-black text-primary">Student Import</h1>
        <p class="text-muted text-sm max-w-2xl">
          Upload an Excel file containing student internship information. Valid columns include Email, LastName,
          FirstName, MiddleName, ContactNumber, Address, GradeLevel, SchoolName, SchoolAddress,
          SchoolContactPerson, SchoolContactPersonEmail, SchoolContactPersonPhone, InternshipNature, Strand, Degree,
          TotalInternshipHours, Office deployment, Internship Start Date, and Estimated Internship End Date.
          Imported students will be automatically approved. If office deployment is provided, a placement record will be created.
        </p>
      </div>
      <UCard>
        <template #header>
          <div class="flex items-center justify-between gap-4">
            <div>
              <h2 class="text-xl font-semibold">Excel Import</h2>
              <p class="text-muted text-sm">Upload a .xlsx or .xls file and import students in bulk.</p>
            </div>
          </div>
        </template>

        <div class="space-y-4">
          <div class="grid gap-2">
            <label class="font-medium">Select Excel file</label>
            <UFileUpload
              v-model="file"
              accept=".xlsx,.xls"
              file-icon="i-lucide-file"
              description="Choose a .xlsx or .xls Excel file to import"
              class="w-full"
            />
            <p class="text-xs text-muted">Only .xlsx or .xls files are accepted.</p>
          </div>

          <div class="flex items-center gap-3">
            <UButton
              label="Upload and Import"
              color="primary"
              :loading="uploading"
              :disabled="uploading"
              @click="upload"
            />
            <span v-if="file" class="text-sm text-secondary">Selected file: {{ file.name }}</span>
          </div>

          <div v-if="errorMessage" class="rounded-lg border border-error p-4 bg-error/10 text-error">
            {{ errorMessage }}
          </div>

          <div v-if="result" class="space-y-4">
            <div class="rounded-lg border border-success p-4 bg-success/10 text-success">
              <p><strong>Total rows processed:</strong> {{ result.totalRows }}</p>
              <p><strong>Imported:</strong> {{ result.importedCount }}</p>
              <p><strong>Skipped:</strong> {{ result.skippedCount }}</p>
            </div>

            <div v-if="result.errors.length > 0" class="rounded-lg border border-warning p-4 bg-warning/10">
              <h3 class="font-semibold">Row errors</h3>
              <ul class="list-disc pl-5 text-sm">
                <li v-for="error in result.errors" :key="error.rowNumber">
                  Row {{ error.rowNumber }}{{ error.email ? ` (${error.email})` : '' }}: {{ error.message }}
                </li>
              </ul>
            </div>
          </div>
        </div>
      </UCard>
    </div>
  </UMain>
</template>
