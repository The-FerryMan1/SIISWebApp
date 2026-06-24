<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import type { ApplicationGetByIdResponse } from './types/applicationType';
import { useRoute, useRouter } from 'vue-router';
import { useAxios } from '../../fetch/axios';

const router = useRouter()
const route = useRoute()
const toast = useToast()

import { CalendarDate } from '@internationalized/date'
import { useDebounceFn } from '@vueuse/core';
import { OfficesArray } from './types/officeSelectValue';

const loading = ref<boolean>(false)
const error = ref();

const details = ref<ApplicationGetByIdResponse | null>(null);

watch(() => route.params.uuid, async () => {
    try {
        loading.value = true
        const { data } = await useAxios.get('/application/' + route.params.uuid)
        console.log(data)
        details.value = data
    } catch (err) {
        error.value = err
    } finally {
        loading.value = false
    }
}, { immediate: true, once: true })

function goBack() {
    router.back()
}

function calculateEndDate(startDate: string, totalHours: number, hoursPerDay: number = 8) {
    if (!startDate || !totalHours) return '';

    const start = new Date(startDate);
    const totalDays = Math.ceil(totalHours / hoursPerDay);

    const end = new Date(start);
    end.setDate(start.getDate() + totalDays - 1);

    return end.toISOString().split('T')[0];
}

watch(
    [
        () => details.value?.internship?.startDate ?? '',
        () => details.value?.internship?.internshipTotalHours ?? 0
    ],
    ([start, total]) => {
        if (details.value?.internship) {
            details.value.internship.estimatedEndDate = calculateEndDate(start, total, 8);
        }
    },
    { immediate: true }
);


const onSubmit = async () => {
    try {
        await useAxios.put('/onboading/details/' + route.params.uuid, details.value)
    } catch (error) {
        console.log(error)
    }
}

const debounceOnSubmit = useDebounceFn(onSubmit, 1000)

</script>

<template>
    <div class="flex items-center justify-between">
        <UButton variant="ghost" color="neutral" icon="i-lucide-arrow-left" @click="goBack">
            Back
        </UButton>
    </div>

    <div>
        <h1 class="text-2xl font-black text-primary">Edit</h1>
        <div class="flex items-center gap-2 mt-1">
            <p class="text-muted text-sm">Modify application details</p>
        </div>
    </div>

    <template v-if="loading">
        loading
    </template>

    <template v-else>
        <UForm @submit="debounceOnSubmit">

            <UContainer class="flex flex-col gap-6">


                <UPageCard v-if="details?.student" title="Student" description="student information"
                    icon="i-lucide-user-round">
                    <UFormField label="Last name" title="Last Name" name="lastName">
                        <UInput v-model="details.student.lastName" placeholder="enter your last name" class="w-full" />
                    </UFormField>

                    <UFormField label="First name" title="First name" name="firstName">
                        <UInput v-model="details.student.firstName" placeholder="enter your first name"
                            class="w-full" />
                    </UFormField>

                    <UFormField label="Middle name" title="Middle Name" name="middleName">
                        <UInput v-model="details.student.middleName" placeholder="enter your middle name"
                            class="w-full" />
                    </UFormField>

                    <UFormField label="Email address" title="Email Address" name="EmaillAddress">
                        <UInput v-model="details.student.email" placeholder="enter your email address" class="w-full"
                            disabled />
                    </UFormField>

                    <UFormField label="Date of birth" title="Date of Birth" name="DateofBirth">
                        <UInput type="date" v-model="details.student.dateOfBirth" placeholder="enter your email address"
                            class="w-full" />
                    </UFormField>

                    <UFormField label="Gender" title="Gender" name="Gender">
                        <USelect class="w-full" v-model="details.student.gender" :items="[
                            { label: 'Male', value: 0 },
                            { label: 'Female', value: 1 },
                            { label: 'others', value: 2 }
                        ]" />
                    </UFormField>

                    <UFormField label="Contact No." title="Contact number" name="Contact">
                        <UInput v-model="details.student.contactNumber" placeholder="Enter your contact number"
                            class="w-full" />
                    </UFormField>

                    <UFormField label="Grade level" title="Grade level" name="GradeLevel">
                        <USelect class="w-full" v-model="details.student.gradeLevel" :items="[
                            { label: 'College First Year', value: 1 },
                            { label: 'College Second Year', value: 2 },
                            { label: 'College Third Year', value: 3 },
                            { label: 'College Fourth Year', value: 4 },
                            { label: 'Grade 11', value: 11 },
                            { label: 'Grade 12', value: 12 }
                        ]" />
                    </UFormField>

                    <UFormField label="Address" title="address" name="address">
                        <UInput v-model="details.student.address" placeholder="Enter your contact number"
                            class="w-full" />
                    </UFormField>
                </UPageCard>

                <!-- school details -->
                <UPageCard v-if="details?.school" title="School" description="School information"
                    icon="i-lucide-building">

                    <UFormField label="School name" title="name" name="SchoolName">
                        <UInput v-model="details.school.name" placeholder="Enter your school name" class="w-full" />
                    </UFormField>

                    <UFormField label="School address" title="address" name="Schooladdress">
                        <UInput v-model="details.school.address" placeholder="Enter your school addres"
                            class="w-full" />
                    </UFormField>

                    <UFormField label="Contact person" title="Contact person" name="ContactPerson">
                        <UInput v-model="details.school.contactPerson" placeholder="Enter school contact person"
                            class="w-full" />
                    </UFormField>

                    <UFormField label="Contact person's email" title="Contact person email" name="ContactPersonEmail">
                        <UInput v-model="details.school.email" placeholder="Enter school contact person's email"
                            class="w-full" />
                    </UFormField>

                    <UFormField label="Contact person's number" title="Contact person number"
                        name="ContactPersonNumber">
                        <UInput type="tel" v-model="details.school.contactNumber"
                            placeholder="Enter school contact person's number" class="w-full" />
                    </UFormField>
                </UPageCard>

                <!-- internship details -->
                <UPageCard v-if="details?.internship" title="Internship" description="Internship details"
                    icon="i-lucide-file-text">

                    <UFormField label="Internship nature" title="Internship nature" name="InternshipNature">
                        <USelect class="w-full" v-model="details.internship.internshipNature" :items="[
                            { label: 'OJT', value: 0 },
                            { label: 'Apprenticeship', value: 1 },
                            { label: 'Internship', value: 2 },
                            { label: 'Work Immersion', value: 3 },
                        ]" />
                    </UFormField>

                    <UFormField v-if="details.internship.strand" label="Strand" title="strand" name="strand">
                        <USelect class="w-full" v-model="details.internship.strand" :items="[
                            { label: 'STEM', value: 0 },
                            { label: 'ABM', value: 1 },
                            { label: 'HUMSS', value: 2 },
                            { label: 'GAS', value: 3 },
                            { label: 'ICT', value: 4 },
                        ]" />
                    </UFormField>

                    <UFormField v-if="details.internship.degree" label="degree" title="degree" name="degree">
                        <USelect class="w-full" v-model="details.internship.degree" :items="[
                            { label: 'BSIT', value: 0 },
                            { label: 'BSCS', value: 1 },
                            { label: 'BSN', value: 2 },
                            { label: 'BSA', value: 3 },
                            { label: 'BSBA', value: 4 },
                            { label: 'BSEd', value: 5 },
                            { label: 'BSCE', value: 6 },
                            { label: 'BSEE', value: 7 },
                            { label: 'BSME', value: 8 },
                            { label: 'BSArch', value: 9 },
                            { label: 'BSPharma', value: 10 },
                            { label: 'BSPsych', value: 11 },
                        ]" />
                    </UFormField>

                    <UFormField label="Internhip total hours" title="totalHours" name="TotalHours">
                        <UInput type="number" v-model="details.internship.internshipTotalHours"
                            placeholder="Enter school contact person's number" class="w-full" />
                    </UFormField>


                    <UFormField label="Start date" title="Start date" name="startDate">
                        <UInput type="date" v-model="details.internship.startDate"
                            placeholder="enter your email address" class="w-full"
                            :min="new Date().toISOString().split('T')[0]" />
                    </UFormField>

                    <UFormField label="Estimated end date" title="Date of Birth" name="DateofBirth"
                        description="Calculated field">
                        <UInput type="date" v-model="details.internship.estimatedEndDate"
                            placeholder="enter your email address" class="w-full "
                            disabled />
                    </UFormField>
                </UPageCard>

                <UPageCard v-if="details?.requirements" title="Office" description="Assigned office"
                    icon="i-lucide-building">
                    <UFormField v-if="details.internship.degree" label="office" title="office" name="office">
                        <USelect class="w-full" v-model="details.internship.degree" :items="OfficesArray" />
                    </UFormField>
                </UPageCard>

                <!-- internship details -->
                <UPageCard v-if="details?.requirements" title="Requirements" description="Requiements"
                    icon="i-lucide-file-text">
                    <UPageList class="w-full flex justify-start items gap-5">
                        <div class="w-full text-sm text-muted flex p-2 rounded bg-slate-50 items-center justify-between gap-3"
                            v-for="requirements in details.requirements" as="button">

                            <UIcon name="i-lucide-file" />
                            <p class="italic">
                                {{ requirements.fileName }}
                            </p>

                            <UButton icon="i-lucide-download" size="sm" variant="ghost" />
                        </div>

                        <UFileUpload label="Drop your files here" class=" min-h-48" />
                    </UPageList>
                </UPageCard>
            </UContainer>

            <div class="flex w-full my-5">
                <UButton class="ms-auto me-8 " type="submit" label="Save" icon="i-lucide-send" size="sm"
                    variant="soft" />
            </div>

        </UForm>
    </template>
</template>