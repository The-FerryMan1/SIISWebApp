import { defineStore } from "pinia";
import { ref } from "vue";

import { useDebounceFn } from "@vueuse/core";
import { useAxios } from "../fetch/axios";
import type { OnBoardUpdateDto } from "../pages/onBoarding/validator/onboardingValidator";

export const useOnBoardStore = defineStore('onboard', () => {
    const errorMessage = ref()
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
            name: ''
        },
        internship: {
            degree: 0,
            strand: 0,
            estimatedEndDate: '',
            internshipNature: 0,
            internshipTotalHours: 0,
            startDate: ''
        },
        requirements: []
    })
    return {
        state,
        errorMessage
    }
})