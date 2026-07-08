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
    },
    school: {
      address: '',
      contactNumber: '',
      contactPerson: '',
      email: '',
      name: '',
    },
    internship: {
      degree: undefined as number | undefined, // ✅ Properly typed
      strand: undefined as number | undefined, // ✅ Properly typed
      estimatedEndDate: '',
      internshipNature: 0,
      internshipTotalHours: 0,
      startDate: '',
    },
    requirements: [] as File[], // ✅ Use this for files, remove separate `files`
  })

  const toDataForm = (): FormData => {
    const formData = new FormData()

    // --- Student ---
    formData.append('student.lastName', state.value.student.lastName)
    formData.append('student.firstName', state.value.student.firstName)
    formData.append('student.middleName', state.value.student.middleName)
    formData.append('student.address', state.value.student.address)
    formData.append('student.contactNumber', state.value.student.contactNumber)
    formData.append('student.dateOfBirth', state.value.student.dateOfBirth)
    formData.append('student.email', state.value.student.email)
    formData.append('student.gender', String(state.value.student.gender))
    formData.append('student.gradeLevel', String(state.value.student.gradeLevel))

    // --- School ---
    formData.append('school.address', state.value.school.address)
    formData.append('school.contactNumber', state.value.school.contactNumber)
    formData.append('school.contactPerson', state.value.school.contactPerson)
    formData.append('school.email', state.value.school.email)
    formData.append('school.name', state.value.school.name)

    // --- Internship ---
    // ✅ Send empty string for undefined/null, not "undefined" string
    formData.append('internship.degree', state.value.internship.degree?.toString() ?? '')
    formData.append('internship.strand', state.value.internship.strand?.toString() ?? '')
    formData.append(
      'internship.estimatedEndDate',
      state.value.internship.estimatedEndDate?.toString() ?? '',
    )
    formData.append('internship.internshipNature', String(state.value.internship.internshipNature))
    formData.append(
      'internship.internshipTotalHours',
      String(state.value.internship.internshipTotalHours),
    )
    formData.append('internship.startDate', state.value.internship.startDate?.toString() ?? '')

    // --- Files ---
    // Use the DTO property name so the backend can bind the uploaded files correctly.
    if (state.value.requirements && state.value.requirements.length > 0) {
      state.value.requirements.forEach((file) => {
        formData.append('files', file)
      })
    }

    return formData
  }

  const onSubmit = async (event: FormSubmitEvent<OnBoardUpdateDto>) => {
    try {
      const formData = toDataForm()
      await useAxios.post('/onboading', formData, {
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
      },
      school: {
        address: '',
        contactNumber: '',
        contactPerson: '',
        email: '',
        name: '',
      },
      internship: {
        degree: undefined as number | undefined, // ✅ Properly typed
        strand: undefined as number | undefined, // ✅ Properly typed
        estimatedEndDate: '',
        internshipNature: 0,
        internshipTotalHours: 0,
        startDate: '',
      },
      requirements: [] as File[], // ✅ Use this for files, remove separate `files`
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
