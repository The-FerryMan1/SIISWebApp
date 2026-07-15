<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui/runtime/components/Table.vue.js';
import { useRegistrationToken, type RegistrationToken } from '../../stores/registrationToken';
import { storeToRefs } from 'pinia';
import { onMounted, useTemplateRef, ref, resolveComponent, h, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useDebounceFn } from '@vueuse/core';
import z from 'zod';
import type { FormSubmitEvent } from '@nuxt/ui';
import Qrcodeview from '../../components/qrcodeview.vue';

const regsToken = useRegistrationToken()
const { registrationTokenError, tokens } = storeToRefs(regsToken)
const router = useRouter()
const route = useRoute()
const table = useTemplateRef('table')
const UButton = resolveComponent('UButton')
const isOpen = ref<boolean>(false)
const isExtendOpen = ref<boolean>(false)
const overlay = useOverlay()
const qrModal = overlay.create(Qrcodeview)


const expirySchema = z.object({
    expDate: z.string()
})

type ExpirySchema = z.infer<typeof expirySchema>

const state = ref<Partial<ExpirySchema>>({
    expDate: undefined
})

const columns: TableColumn<RegistrationToken>[] = [
    {
        accessorKey: 'uuid',
        header: 'Token',
        cell: ({ row }) => `${row.getValue('uuid')}`
    },
    {
        accessorKey: 'expDate',
        header: 'Expiry date',
        cell: ({ row }) => `${new Date(row.getValue('expDate')).toLocaleDateString()}`
    },
    {
        accessorKey: 'createdAt',
        header: 'Created at',
        cell: ({ row }) => `${new Date(row.getValue('createdAt')).toLocaleDateString()}`
    },
    {
        id: 'actions',
        header: 'Actions',
        cell: ({ row }) => {
            return h('div', { class: 'flex items-center gap-2' }, [
                // QR Button
                h(UButton, {
                    icon: 'i-lucide-qr-code',
                    label: 'QR',
                    size: 'xs',
                    variant: 'outline',
                    onClick: () => openQr(row.original.uuid)
                }),
                // Update Button
                h(UButton, {
                    icon: 'i-lucide-pencil',
                    label: 'Extend',
                    size: 'xs',
                    color: 'primary',
                    variant: 'soft',
                    onClick: () => isExtendOpen.value = !isExtendOpen.value
                }),
                // Delete Button
                h(UButton, {
                    icon: 'i-lucide-trash-2',
                    label: 'Delete',
                    size: 'xs',
                    color: 'error',
                    variant: 'soft',
                    onClick: () => console.log('Delete clicked', row.original)
                })
            ])
        }
    }
]


const openQr = (qrstring: string) => {
    qrModal.open({ url: `http://100.10.1.201:5233/registration/${qrstring}` })
}

onMounted(async () => {
    await regsToken.GetAllTokens()
})

const back = () => {
    router.back()
}

const onSubmit = async (event: FormSubmitEvent<ExpirySchema>) => {
    try {
        await regsToken.createRegistrationToken(event.data)
        regsToken.GetAllTokens()
    } catch (error) {
        console.log(error)
    } finally {
        isOpen.value = false
    }
}

const submitDebounce = useDebounceFn(onSubmit, 500)


const minDate = computed(() => new Date().toISOString().split('T')[0])

</script>

<template>
    <UMain>
        <div>
            <UButton @click="back" label="Back" icon="i-lucide-arrow-left" variant="ghost" />
        </div>
        <div>
            <h1 class="text-2xl font-bold text-primary">Registration Token</h1>
        </div>
        <UPageCard>


            <template #header>
                <div class="w-full flex justify-end items-center">
                    <UModal v-model:open="isOpen" class="" title="Add expiration date">
                        <UButton label="Generate registration token" />
                        <template #content>
                            <UForm @error="(r) => console.log(r)" @submit="submitDebounce" :state="state"
                                :schema="expirySchema" class="p-4 w-full">
                                <UFormField name="expDate" label="Expiration date">
                                    <UInput v-model="state.expDate" type="date" class="w-full" :min="minDate" />
                                </UFormField>
                                <UButton type="submit" label="Submit" />
                            </UForm>
                        </template>

                    </UModal>



                    <UModal v-model:open="isExtendOpen" class="" title="Add expiration date">

                        <template #content>
                            <div>ratbu</div>
                            <!-- <UForm @error="(r) => console.log(r)" @submit="submitDebounce" :state="state"
                                :schema="expirySchema" class="p-4 w-full">
                                <UFormField name="expDate" label="Expiration date">
                                    <UInput v-model="state.expDate" type="date" class="w-full" :min="minDate"/>
                                </UFormField>
                                <UButton type="submit" label="Submit" />
                            </UForm> -->
                        </template>

                    </UModal>
                </div>

            </template>


            <UTable ref="table" :data="tokens ?? []" :columns="columns" class="w-full max-h-96 flex-1" />


        </UPageCard>

    </UMain>
</template>