<script setup lang="ts">
const props = defineProps<{
  title?: string | 'Confirmation'
  description?: string | 'This action cannot be undone'
}>()
const emit = defineEmits<{ close: [boolean] }>()

const close = (state: boolean) => {
  emit('close', state)
}
</script>

<template>
  <UModal :title="title" description="This action cannot be undone">
    <template #footer>
      <UButton @click="close(true)" label="Confirm" color="error" />
      <UButton @click="close(false)" label="Cancel" color="neutral" />
    </template>
  </UModal>
</template>
