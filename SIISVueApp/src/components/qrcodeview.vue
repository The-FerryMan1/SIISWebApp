<script setup lang="ts">
import { useQRCode } from '@vueuse/integrations/useQRCode'

const props = defineProps<{
    url: string
}>()

const emit = defineEmits<{ close: [boolean] }>()

const qrcode = useQRCode(props.url)

// Controlled from the parent via v-model:open="showQr"
const isOpen = defineModel<boolean>('open', { default: false })

const handleClose = (printed: boolean) => {
    isOpen.value = false
    emit('close', printed)
}

const handlePrint = () => {
    window.addEventListener('afterprint', () => {
        handleClose(true)
    }, { once: true })

    window.print()
}
</script>

<template>
    <UModal v-model:open="isOpen" title="QR Code">
        <template #body>
            <div class="w-full print-area">
                <img :src="qrcode" alt="QR Code" loading="lazy" class="object-contain w-full h-full">
            </div>
        </template>

        <template #footer>
            <div class="flex justify-end items-center gap-2 w-full">
                <UButton color="neutral" label="Close" @click="handleClose(false)" />
                <UButton label="Print" @click="handlePrint" />
            </div>
        </template>
    </UModal>
</template>