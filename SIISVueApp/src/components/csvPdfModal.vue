<script setup lang="ts">
import type {SelectItem, FormSubmitEvent } from '@nuxt/ui'
import { ref, useTemplateRef } from 'vue';
import z from 'zod';


const model = defineModel<number>()
const props = defineProps<{
    title?: string,
    description?: string
    items?: SelectItem[]
    selectPlaceholder?: string
}>()

const emit = defineEmits<{
    close: ['pdf' | 'csv' | 'none']
}>()

const form = useTemplateRef('form')


const schema = z.object({
    selected: z.number()
})
const selectedFormat = ref<'pdf'|'csv'|'none'>()

type Schema = z.infer<typeof schema>


function onSubmit(event: FormSubmitEvent<Schema>){
    if(selectedFormat.value){
         emit('close', selectedFormat.value)
    }
   
}

function selectFormat( format: 'pdf' | 'csv') {
    selectedFormat.value = format
    form.value?.submit()
}
</script>

<template>
    <UModal>
        <template #content>
            <div class="space-y-4 p-6 text-center">
                <h2 class="text-lg font-semibold">{{ title || 'Select Format' }}</h2>
                <p v-if="description" class="text-sm text-gray-500">{{ description }}</p>
                <UForm ref="form" :schema="schema" @submit="onSubmit" :state="{selected: model}">

               
                <UFormField name="selected" :label="selectPlaceholder">
                    <USelectMenu :placeholder="selectPlaceholder" v-model="model" :items class="w-full" value-key="value" />
                </UFormField>
                <div class="flex justify-end gap-2 my-2">
                    <UButton icon="i-lucide-file-text" color="primary" @click="selectFormat('pdf')">
                        PDF
                    </UButton>
                    <UButton icon="i-lucide-table" color="success" @click="selectFormat('csv')">
                        CSV
                    </UButton>
                </div>
                 </UForm>
            </div>
        </template>
        <template #footer>
            <UButton label="Cancel" color="neutral" variant="subtle" @click="$emit('close', 'none')" />
        </template>
    </UModal>
</template>