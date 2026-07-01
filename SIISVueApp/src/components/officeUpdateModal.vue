<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '@nuxt/ui'
import { ref } from 'vue'

interface Props {
  title?: string
  officeId?: number
  oic?: string | null,
  loading: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: { id: number; name: string } | null]
}>()

const schema = z.object({
  name: z.string().min(1, 'officer-in-charge is required')
})

type Schema = z.output<typeof schema>

const state = ref({
  name: props.oic ?? ''
})

const form = ref()

const onSubmit = (event: FormSubmitEvent<Schema>) => {
  emit('close', { id: props.officeId!, name: event.data.name })
}

const handleSave = () => form.value?.submit()
const cancel = () => emit('close', null)
</script>

<template>
  <UModal :title="title">
    <template #body>
      <UForm ref="form" :schema="schema" :state="state" @submit="onSubmit" :loading>
        <UFormField label="Officer-in-Charge" name="name">
          <UInput v-model="state.name" placeholder="Enter officer-in-charge" class="w-full" />
        </UFormField>
      </UForm>
    </template>

    <template #footer>
      <UButton label="Cancel" color="neutral" variant="outline" @click="cancel" />
      <UButton :loading label="Save" @click="handleSave" />
    </template>
  </UModal>
</template>