import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import type { OnBoardUpdateDto } from '../pages/onBoarding/validator/onboardingValidator'
import type { FormSubmitEvent } from '@nuxt/ui'
import type { AxiosError } from 'axios'

export const useOnBoardStore = defineStore('onboard', () => {
  const errorMessage = ref<string | null>(null)
  const state = ref({
    student: {
      lastName: '',
      firstName: '',
      middleName: '',
      address: '',
       contactNumber: '',
      email: '',
      gradeLevel: 1,
      schoolName: '',
      schoolAddress: '',
      schoolContactPerson: '',
      schoolContactPersonEmail: '',
      schoolContactPersonPhone: '',
      internshipNature: 0,
      strand: 0,
      degree: 0,
      internshipStartDate: '',
      totalInternshipHours: 0,
    },
    requirements: [] as any[],
     moaFile: null as File | null,
     developmentLetterFile: null as File | null,
     resumeFile: null as File | null,
  })

  const toDataForm = (): FormData => {
    const formData = new FormData()

    const s = state.value.student

    formData.append('student.lastName', String(s.lastName))
    formData.append('student.firstName', String(s.firstName))
    formData.append('student.middleName', String(s.middleName))
    formData.append('student.address', String(s.address))
     formData.append('student.contactNumber', String(s.contactNumber))
    formData.append('student.email', String(s.email))
    formData.append('student.gradeLevel', String(s.gradeLevel))

    formData.append('school.name', String(s.schoolName))
    formData.append('school.address', String(s.schoolAddress))
    formData.append('school.contactPerson', String(s.schoolContactPerson))
    formData.append('school.email', String(s.schoolContactPersonEmail))
    formData.append('school.contactNumber', String(s.schoolContactPersonPhone))

    formData.append('internship.internshipNature', String(s.internshipNature))
    formData.append('internship.strand', String(s.strand))
    formData.append('internship.degree', String(s.degree))
    formData.append('internship.startDate', String(s.internshipStartDate))

    if (s.internshipStartDate && s.totalInternshipHours) {
      const start = new Date(s.internshipStartDate)
      const totalDays = Math.ceil(s.totalInternshipHours / 8)
      const end = new Date(start)
      end.setDate(start.getDate() + totalDays)
      const estimatedEndDate = end.toISOString().split('T')[0]!
      formData.append('internship.estimatedEndDate', estimatedEndDate)
    }

    formData.append('internship.internshipTotalHours', String(s.totalInternshipHours))
    formData.append('internship.accumulatedHours', '0')

     const moa = normalizeFile(state.value.moaFile)
     if (moa) {
       formData.append('moaFile', moa, moa.name)
     }

      const developmentLetter = normalizeFile(state.value.developmentLetterFile)
      if (developmentLetter) {
        formData.append('developmentLetterFile', developmentLetter, developmentLetter.name)
      }

      const resume = normalizeFile(state.value.resumeFile)
      if (resume) {
        formData.append('resumeFile', resume, resume.name)
      }

     return formData
  }

  const normalizeFile = (file: File | File[] | null | undefined): File | null => {
    if (file instanceof File) return file
    if (Array.isArray(file) && file.length > 0 && file[0] instanceof File) return file[0]
    return null
  }

  const onSubmit = async (event: FormSubmitEvent<OnBoardUpdateDto>, token: string) => {
    try {
      const formData = toDataForm()
      console.log('Submitting onboarding form...')
      for (const [key, value] of formData.entries()) {
        console.log('FormData:', key, value)
      }
      await useAxios.post('/onboading/' + token, formData)
    } catch (error) {
      const responseData = (error as AxiosError).response?.data
      if (responseData) {
        if (typeof responseData === 'string') {
          errorMessage.value = responseData
        } else if (typeof responseData === 'object' && responseData !== null) {
          const errors = (responseData as any).errors
          if (errors && typeof errors === 'object') {
            errorMessage.value = Object.values(errors).flat().join(' ')
          } else {
            errorMessage.value = (responseData as any).title || 'Submission failed'
          }
        }
      }
      console.log(responseData)

      throw new Error(errorMessage.value || 'Submission failed')
    }
  }

  const stateReset = () => {
    state.value = {
      student: {
        lastName: '',
        firstName: '',
        middleName: '',
        address: '',
        contactNumber: '',
        email: '',
        gradeLevel: 1,
        schoolName: '',
        schoolAddress: '',
        schoolContactPerson: '',
        schoolContactPersonEmail: '',
        schoolContactPersonPhone: '',
        internshipNature: 0,
        strand: 0,
        degree: 0,
        internshipStartDate: '',
        totalInternshipHours: 0,
      },
      requirements: [] as any[],
      moaFile: null,
      developmentLetterFile: null,
      resumeFile: null,
    }

    errorMessage.value = null
  }

  return {
    state,
    errorMessage,
    toDataForm,
    onSubmit,
    stateReset,
  }
})
