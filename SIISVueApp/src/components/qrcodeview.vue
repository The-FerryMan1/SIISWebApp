<script setup lang="ts">
import { useQRCode } from '@vueuse/integrations/useQRCode'
import { ref } from 'vue';

const props = defineProps<{
    url: string,
}>()

const emit = defineEmits<{close: [boolean]}>()

const qrcode = useQRCode(props.url)

const isOpen = ref<boolean>(false)

</script>

<template>
    <UModal v-model:open="isOpen">
        <template #header>

        </template>
        <template #content>
            <img :src="qrcode" alt="" lazy>
        </template>

        <template #footer>
      <div class="flex gap-2">
        <UButton color="neutral" label="Dismiss" @click="emit('close', false)" />
        <UButton label="Success" @click="emit('close', true)" />
      </div>
    </template>
    </UModal>
</template>