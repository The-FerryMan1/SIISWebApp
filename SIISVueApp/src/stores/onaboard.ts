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
      dateOfBirth: '',
      email: '',
      gender: 0,
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
  })

  const toDataForm = (): FormData => {
    const formData = new FormData()

    const s = state.value.student

    formData.append('student.lastName', String(s.lastName))
    formData.append('student.firstName', String(s.firstName))
    formData.append('student.middleName', String(s.middleName))
    formData.append('student.address', String(s.address))
    formData.append('student.contactNumber', String(s.contactNumber))
    formData.append('student.dateOfBirth', String(s.dateOfBirth))
    formData.append('student.email', String(s.email))
    formData.append('student.gender', String(s.gender))
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

    if (state.value.requirements && state.value.requirements.length > 0) {
      state.value.requirements.forEach((file) => {
        if (file instanceof File) {
          formData.append('files', file)
        }
      })
    }

    return formData
  }

  const onSubmit = async (event: FormSubmitEvent<OnBoardUpdateDto>, token: string) => {
    try {
      const formData = toDataForm()
      await useAxios.post('/onboading/' + token, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      })
    } catch (error) {
      const erroMess = (error as AxiosError).response?.data as string
      if (erroMess) {
        errorMessage.value = erroMess
      }
      console.log(erroMess)

      throw new Error(erroMess)
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
        dateOfBirth: '',
        email: '',
        gender: 0,
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
