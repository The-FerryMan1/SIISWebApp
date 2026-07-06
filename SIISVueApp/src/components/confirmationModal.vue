<script setup lang="ts">
const emit = defineEmits<{ close: [boolean] }>()





const close = (state: boolean) => {
    emit('close', state)
}

</script>

<template>
    <UModal title="Confirmtion" description="This action cannot be undone">
        <template #footer>
            <UButton @click="close(true)" label="Delete" color="error" />
            <UButton @click="close(false)" label="Cancel" color="neutral"/>
        </template>
    </UModal>
</template>