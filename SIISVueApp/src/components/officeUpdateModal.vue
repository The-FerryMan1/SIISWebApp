<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '@nuxt/ui'
import { ref } from 'vue'

interface Props {
  title?: string
  officeId?: number
  department?: string | null
  loading: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: { id: number; department: string; honorific: string } | null]
}>()

const schema = z.object({
  department: z.string().min(1, 'department is required'),
  honorific: z.string()
})

type Schema = z.output<typeof schema>

const state = ref({
  department: props.department ?? '',
  honorific: undefined
})

const form = ref()

const onSubmit = (event: FormSubmitEvent<Schema>) => {
  emit('close', { id: props.officeId!, department: event.data.department, honorific: event.data.honorific })
}

const handleSave = () => form.value?.submit()
const cancel = () => emit('close', null)



const honorificItems = ref([
  "Mr.",
  "Mrs.",
  "Ms.",
  "Miss",
  "Mx.",
  "Master",
  "Dr.",
  "Prof.",
  "Rev.",
  "Fr.",
  "Sr.",
  "Br.",
  "Pastor",
  "Rabbi",
  "Imam",
  "Sheikh",
  "Hon.",
  "Pres.",
  "Gov.",
  "Sen.",
  "Rep.",
  "Ambassador",
  "Justice",
  "Judge",
  "Sir",
  "Dame",
  "Lord",
  "Lady",
  "Duke",
  "Duchess",
  "Prince",
  "Princess",
  "King",
  "Queen",
  "Capt.",
  "Maj.",
  "Col.",
  "Gen.",
  "Adm.",
  "Sgt."
])
</script>

<template>
  <UModal :title="title">
    <template #body>
      <UForm ref="form" :schema="schema" :state="state" @submit="onSubmit" :loading>

        <UFormField label="Honorific" name="honorific">
          <USelect v-model="state.honorific" :items="honorificItems" placeholder="Select proper honorific"/>
        </UFormField>




<UFormField label="Department" name="department">
           <UInput v-model="state.department" placeholder="Enter department name" class="w-full" />
         </UFormField>


        
      </UForm>
    </template>

    <template #footer>
      <UButton label="Cancel" color="neutral" variant="outline" @click="cancel" />
      <UButton :loading label="Save" @click="handleSave" />
    </template>
  </UModal>
</template>
