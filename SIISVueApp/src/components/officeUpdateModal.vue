<script setup lang="ts">
import { ref } from 'vue';


interface Props {
  id: number
  title?: string
}

const props = defineProps<Props>()
const model = defineModel<string>()
const emit = defineEmits<{
  close: [boolean],
}>()



const save = () => {
  emit('close', true)
}

const cancel = () => {
  emit('close', true)
}





</script>

<template>
  <UModal :title="title">
    <template #body>

      <UForm>
        <UFormField label="Office Name">
          <UInput v-model="model" placeholder="Enter office name" class="w-full" />
        </UFormField>
      </UForm>

    </template>
    <template #footer>
      <UButton label="Cancel" color="neutral" variant="outline" @click="cancel" />
      <UButton label="Save" @click="save" />
    </template>
  </UModal>
</template>