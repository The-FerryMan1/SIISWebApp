<script setup lang="ts">



const model = defineModel<number>()
const props = defineProps<{ 
    title?: string, 
    description?: string 
}>()

const emit = defineEmits<{ 
    close: ['pdf'|'csv' |'none']
}>()

function selectFormat(format: 'pdf' | 'csv') {
   emit('close', format)
}
</script>

<template>
    <UModal>
        <template #content>
            <div class="space-y-4 p-6 text-center">
                <h2 class="text-lg font-semibold">{{ title || 'Select Format' }}</h2>
                <p v-if="description" class="text-sm text-gray-500">{{ description }}</p>
                



                <UFormField label="Select Status">

                <USelect placeholder="Select Status" v-model="model" :items="[
                    {
                        label: 'Pending',
                        value: 0
                    },
                    {
                        label: 'Approved',
                        value: 1
                    },
                    {
                        label: 'Rejected',
                        value: 2
                    }
                ]" />
                  </UFormField>

                <div class="flex justify-center gap-3">
                    <UButton 
                        icon="i-lucide-file-text"
                        color="primary"
                        @click="selectFormat('pdf')"
                    >
                        PDF
                    </UButton>
                    <UButton 
                        icon="i-lucide-table"
                        color="success"
                        @click="selectFormat('csv')"
                    >
                        CSV
                    </UButton>
                </div>
            </div>
        </template>
        
        <template #footer>
            <UButton 
                label="Cancel" 
                color="neutral" 
                variant="subtle"
                @click="$emit('close', 'none')"
            />
        </template>
    </UModal>
</template>