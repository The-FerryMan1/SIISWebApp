import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAxios } from '../fetch/axios'
import type { OnBoardingDto } from '../pages/onBoarding/validator/onboardingValidator'
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
      dateOfBirth: '',
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
    school: {
      name: '',
      address: '',
      contactPerson: '',
      email: '',
      contactNumber: '',
    },
    internship: {
      internshipNature: 0,
      strand: 0,
      degree: 0,
      startDate: '',
      estimatedEndDate: '',
      internshipTotalHours: 0,
      accumulatedHours: 0,
    },
    moaFile: null as File | null,
    resumeFile: null as File | null,
  })

  const toDataForm = (): FormData => {
    const formData = new FormData()

    const s = state.value.student
    const school = state.value.school
    const internship = state.value.internship

    formData.append('Student.LastName', String(s.lastName))
    formData.append('Student.FirstName', String(s.firstName))
    formData.append('Student.MiddleName', String(s.middleName))
    formData.append('Student.Address', String(s.address))
    formData.append('Student.ContactNumber', String(s.contactNumber))
    formData.append('Student.Email', String(s.email))
    formData.append('Student.DateOfBirth', String(s.dateOfBirth))
    formData.append('Student.Gender', String(s.gender))
    formData.append('Student.GradeLevel', String(s.gradeLevel))

    formData.append('School.Name', String(school.name))
    formData.append('School.Address', String(school.address))
    formData.append('School.ContactPerson', String(school.contactPerson))
    formData.append('School.Email', String(school.email))
    formData.append('School.ContactNumber', String(school.contactNumber))

    formData.append('Internship.InternshipNature', String(internship.internshipNature))
    formData.append('Internship.Strand', String(internship.strand))
    formData.append('Internship.Degree', String(internship.degree))
    formData.append('Internship.StartDate', String(internship.startDate))

    if (internship.startDate && internship.internshipTotalHours) {
      const start = new Date(internship.startDate)
      const totalDays = Math.ceil(internship.internshipTotalHours / 8)
      const end = new Date(start)
      end.setDate(start.getDate() + totalDays)
      const estimatedEndDate = end.toISOString().split('T')[0]!
      formData.append('Internship.EstimatedEndDate', estimatedEndDate)
      internship.estimatedEndDate = estimatedEndDate
    } else {
      formData.append('Internship.EstimatedEndDate', String(internship.estimatedEndDate))
    }

    formData.append('Internship.InternshipTotalHours', String(internship.internshipTotalHours))
    formData.append('Internship.AccumulatedHours', String(internship.accumulatedHours))

    const moa = normalizeFile(state.value.moaFile)
    if (moa) {
      formData.append('MoaFile', moa, moa.name)
    }

    const resume = normalizeFile(state.value.resumeFile)
    if (resume) {
      formData.append('ResumeFile', resume, resume.name)
    }

     return formData
  }

  const normalizeFile = (file: File | File[] | null | undefined): File | null => {
    if (file instanceof File) return file
    if (Array.isArray(file) && file.length > 0 && file[0] instanceof File) return file[0]
    return null
  }

  const onSubmit = async (event: FormSubmitEvent<OnBoardingDto>, token: string) => {
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
        dateOfBirth: '',
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
      school: {
        name: '',
        address: '',
        contactPerson: '',
        email: '',
        contactNumber: '',
      },
      internship: {
        internshipNature: 0,
        strand: 0,
        degree: 0,
        startDate: '',
        estimatedEndDate: '',
        internshipTotalHours: 0,
        accumulatedHours: 0,
      },
      moaFile: null,
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
