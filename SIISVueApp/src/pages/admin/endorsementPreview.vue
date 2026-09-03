<script setup lang="ts">
interface Props {
  settings: {
    header: {
      country: string
      province: string
      officeTitle: string
      city: string
      logoPath?: string
    }
    body: {
      salutation: string
      greeting: string
      introTemplate: string
      attachmentNote: string
      thankYou: string
    }
    footer: {
      signingOfficerTitle: string
      closing: string
      footerAddress?: string
    }
    maxStudentsPerPage: number
  }
  sampleStudents: { fullName: string; totalInternshipHours: number }[]
}

const props = defineProps<Props>()

function getIntroText() {
  const template = props.settings.body.introTemplate || 'Respectfully endorsing the following {students} of the {school}, to conduct their on-the-job training{hours} in your office:'
  const studentWord = props.sampleStudents.length > 1 ? 'students' : 'student'
  const firstStudent = props.sampleStudents[0]
  const hoursText = props.sampleStudents.length === 1 && firstStudent ? ` (${firstStudent.totalInternshipHours} hours)` : ''
  return template
    .replace('{students}', studentWord)
    .replace('{school}', 'Sample School Name')
    .replace('{hours}', hoursText)
}
</script>

<template>
  <div class="border rounded-lg overflow-hidden bg-white shadow-sm">
    <div class="bg-gray-50 px-4 py-2 border-b flex items-center justify-between">
      <span class="text-sm font-semibold text-gray-700">Letter Preview</span>
      <span class="text-xs text-gray-500">A4 size approximation</span>
    </div>
    <div class="p-8 max-h-[800px] overflow-y-auto">
      <div class="max-w-[210mm] mx-auto bg-white border border-dashed border-gray-300 p-12 min-h-[297mm]">
        <div v-if="props.settings.header.logoPath" class="flex justify-center mb-4">
          <img :src="props.settings.header.logoPath" alt="Logo" class="max-h-16 object-contain" />
        </div>

        <div class="text-center mb-6">
          <p class="text-sm">{{ props.settings.header.country || 'Republic of the Philippines' }}</p>
          <p class="text-sm">{{ props.settings.header.province || 'Province of Cavite' }}</p>
          <p class="text-base font-bold">{{ props.settings.header.officeTitle || 'OFFICE OF THE PROVINCIAL GOVERNOR' }}</p>
          <p class="text-sm">{{ props.settings.header.city || 'Trece Martires City' }}</p>
        </div>

        <div class="mb-6">
          <p class="text-sm mb-4">{{ new Date().toLocaleString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) }}</p>

          <div class="mb-4">
            <p class="text-sm font-bold">The Provincial Administrator</p>
            <p class="text-sm">{{ props.settings.header.city || 'Trece Martires City' }}</p>
          </div>

          <p class="text-sm mb-4">{{ props.settings.body.salutation || 'Dear Sir/Madam,' }}</p>
          <p class="text-sm mb-4">{{ props.settings.body.greeting || 'Greetings,' }}</p>

          <p class="text-sm mb-4 leading-relaxed" v-html="getIntroText()"></p>

          <div class="ml-8 mb-6">
            <p v-for="(student, index) in props.sampleStudents" :key="index" class="text-sm mb-1">
              {{ index + 1 }}. {{ student.fullName }} - {{ student.totalInternshipHours }} hours
            </p>
          </div>

          <p class="text-sm mb-4">{{ props.settings.body.attachmentNote || 'Attached are the development letter(s) of the student(s) for your reference.' }}</p>
          <p class="text-sm mb-4">{{ props.settings.body.thankYou || 'Thank you very much.' }}</p>
          <p class="text-sm mb-1">{{ props.settings.footer.closing || 'Very truly yours,' }}</p>

          <div class="mt-8">
            <p class="text-sm font-bold">Admin User</p>
            <p class="text-sm">{{ props.settings.footer.signingOfficerTitle || 'Executive Assistant IV' }}</p>
          </div>

          <div v-if="props.settings.footer.footerAddress" class="mt-6 text-center">
            <p class="text-xs italic">{{ props.settings.footer.footerAddress }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
