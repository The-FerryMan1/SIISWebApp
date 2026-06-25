<script setup lang="ts">
import { isTemplateSpan } from 'typescript';
import { computed, ref } from 'vue';


const active = ref(1);
const items = computed(() => [
    {
        label: 'Student',
        icon: 'i-lucide-user',
        slot: 'student',
        value: 1,
        disabled: false
    },
    {
        label: 'School',
        icon: 'i-lucide-building',
        slot: 'school',
        value: 2,
        disabled: true
    },
    {
        label: 'Internship',
        icon: 'i-lucide-file',
        slot: 'internship',
        value: 3,
        disabled: true
    },
    {
        label: 'Requirements',
        icon: 'i-lucide-folder',
        slot: 'requirements',
        value: 4,
        disabled: true
    }

])

const toggleDisable = (value: number) => {
    const tab = items.value.find(t => t.value = value)
    if (tab) tab.disabled = !tab.disabled;
}

const isTabDisable = () => {

    const item = items.value.find(t => t.value == active.value)

    return item?.disabled
}


const next = () => {
    const count = items.value.length
    if (count > active.value) {
        active.value++
    }
}

const back = () => {
    const count = items.value.length
    if (active.value > 1) {
        active.value--
    }

}






</script>

<template>
    <div class="p-5">
        <UPage>
            <UPageBody>
                <div class="flex justify-between gap-3 my-3 p-3">
                    <UButton icon="i-lucide-arrow-left" :disabled="active <= 1" @click="back" label="Back" />
                    <UButton v-if="active >= items.length" icon="i-lucide-save" @click="next" label="Submit" />
                    <UButton v-else icon="i-lucide-arrow-right" @click="next" label="Next" />
                </div>
                <UForm>
                    <KeepAlive>
                        <UTabs :unmount-on-hide="false" v-model="active" :items="items" :content="true"
                            activation-mode="automatic" :ui="{
                                list: 'justify-around gap-2 w-full ',
                                trigger: 'grow flex-col gap-1 py-1',
                            }" class="w-full">
                            <template #student>
                                <UPageCard title="Student" description="student information" icon="i-lucide-user-round">
                                    <UFormField label="Last name" title="Last Name" name="lastName" required>
                                        <UInput placeholder="enter your last name" class="w-full" />
                                    </UFormField>
                                    <UFormField label="First name" title="First name" name="firstName" required>
                                        <UInput placeholder="enter your first name" class="w-full" />
                                    </UFormField>
                                    <UFormField label="Middle name" title="Middle Name" name="middleName" required>
                                        <UInput placeholder="enter your middle name" class="w-full" />
                                    </UFormField>
                                    <UFormField label="Email address" title="Email Address" name="EmaillAddress"
                                        required>
                                        <UInput type="email" placeholder="enter your email address" class="w-full" />
                                    </UFormField>
                                    <UFormField label="Date of birth" title="Date of Birth" name="DateofBirth" required>
                                        <UInput type="date" placeholder="enter your email address" class="w-full" />
                                    </UFormField>
                                    <UFormField label="Gender" title="Gender" name="Gender" required>
                                        <USelect placeholder="Select Gender" class="w-full" :items="[
                                            { label: 'Male', value: 0 },
                                            { label: 'Female', value: 1 },
                                            { label: 'others', value: 2 }
                                        ]" />
                                    </UFormField>
                                    <UFormField label="Contact No." title="Contact number" name="Contact" required>
                                        <UInput placeholder="Enter your contact number" class="w-full" />
                                    </UFormField>
                                    <UFormField label="Grade level" title="Grade level" name="GradeLevel" required>
                                        <USelect placeholder="Select grade level" class="w-full" :items="[
                                            { label: 'College First Year', value: 1 },
                                            { label: 'College Second Year', value: 2 },
                                            { label: 'College Third Year', value: 3 },
                                            { label: 'College Fourth Year', value: 4 },
                                            { label: 'Grade 11', value: 11 },
                                            { label: 'Grade 12', value: 12 }
                                        ]" />
                                    </UFormField>
                                    <UFormField label="Address" title="address" name="address" required>
                                        <UTextarea class="w-full" placeholder="Enter your address" />
                                    </UFormField>
                                </UPageCard>
                            </template>
                            <template #school>
                                <UPageCard title="School Details" description="School information"
                                    icon="i-lucide-building">

                                    <UFormField label="School name" title="name" name="SchoolName" required>
                                        <UInput placeholder="Enter your school name" class="w-full" />
                                    </UFormField>

                                    <UFormField label="School address" title="address" name="Schooladdress" required>
                                        <UInput placeholder="Enter your school addres" class="w-full" />
                                    </UFormField>

                                    <UFormField label="Contact person" title="Contact person" name="ContactPerson"
                                        required>
                                        <UInput placeholder="Enter school contact person" class="w-full" />
                                    </UFormField>

                                    <UFormField label="Contact person's email" title="Contact person email"
                                        name="ContactPersonEmail" required>
                                        <UInput placeholder="Enter school contact person's email" class="w-full" />
                                    </UFormField>

                                    <UFormField label="Contact person's number" title="Contact person number"
                                        name="ContactPersonNumber" required>
                                        <UInput type="tel" placeholder="Enter school contact person's number"
                                            class="w-full" />
                                    </UFormField>
                                </UPageCard>
                            </template>
                            <template #internship>
                                <UPageCard title="Internship" description="Internship details"
                                    icon="i-lucide-file-text">
                                    <UFormField label="Internship nature" title="Internship nature"
                                        name="InternshipNature">
                                        <USelect placeholder="Selecte Internship nature" class="w-full" :items="[
                                            { label: 'OJT', value: 0 },
                                            { label: 'Apprenticeship', value: 1 },
                                            { label: 'Internship', value: 2 },
                                            { label: 'Work Immersion', value: 3 },
                                        ]" />
                                    </UFormField>
                                    <UFormField label="Strand" title="strand" name="strand">
                                        <USelect placeholder="Select strand" class="w-full" :items="[
                                            { label: 'STEM', value: 0 },
                                            { label: 'ABM', value: 1 },
                                            { label: 'HUMSS', value: 2 },
                                            { label: 'GAS', value: 3 },
                                            { label: 'ICT', value: 4 },
                                        ]" />
                                    </UFormField>
                                    <UFormField label="Degree" title="degree" name="degree">
                                        <USelect placeholder="Select Degree" class="w-full" :items="[
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
                                        <UInput type="number" placeholder="Enter school contact person's number"
                                            class="w-full" />
                                    </UFormField>
                                    <UFormField label="Start date" title="Start date" name="startDate">
                                        <UInput type="date" placeholder="enter your email address" class="w-full"
                                            :min="new Date().toISOString().split('T')[0]" />
                                    </UFormField>

                                    <UFormField label="Estimated end date" title="Date of Birth" name="DateofBirth"
                                        description="Calculated field">
                                        <UInput type="date" placeholder="enter your email address" class="w-full "
                                            disabled />
                                    </UFormField>
                                </UPageCard>
                            </template>
                            <template #requirements>
                                <UPageCard title="Requirements" description="pdf, img" icon="i-lucide-file-text">
                                    <UPageList class="w-full flex justify-start items gap-5">
                                        <!-- <div class="w-full text-sm text-muted flex p-2 rounded bg-slate-50 items-center justify-between gap-3"
                                    v-for="requirements in details.requirements" as="button">

                                    <UIcon name="i-lucide-file" />
                                    <p class="italic">
                                        {{ requirements.fileName }}
                                    </p>

                                    <UButton icon="i-lucide-download" size="sm" variant="ghost" />
                                </div> -->
                                        <UFileUpload label="Drop your files here" class=" min-h-48" />
                                    </UPageList>
                                </UPageCard>
                            </template>
                        </UTabs>
                    </KeepAlive>
                </UForm>
            </UPageBody>
            <template #right>
                <UPageBody class="w-full">
                    <UPageCard icon="i-lucide-notebook-pen" title="Registration Preview" class="w-full">
                    </UPageCard>
                </UPageBody>
            </template>
        </UPage>
    </div>

</template>
