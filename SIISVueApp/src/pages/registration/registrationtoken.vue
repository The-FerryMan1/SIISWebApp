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
import { getPaginationRowModel } from '@tanstack/vue-table'
import ConfirmationModal from '../../components/confirmationModal.vue';
import { useAxios } from '../../fetch/axios.ts';

const regsToken = useRegistrationToken()
const { registrationTokenError, tokens } = storeToRefs(regsToken)
const router = useRouter()
const route = useRoute()
const table = useTemplateRef('table')
const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')
const isOpen = ref<boolean>(false)
const isExtendOpen = ref<boolean>(false)
const overlay = useOverlay()
const qrModal = overlay.create(Qrcodeview)
const confirmModal = overlay.create(ConfirmationModal)
const toast = useToast()

// Extend form state
const extendTokenId = ref<number | null>(null)
const extendCurrentDate = ref<string>('')

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
        header:'Status',
        cell: ({ row }) => {
            const date = new Date(row.getValue('expDate'))
            const status = date > new Date()? h(UBadge, {label:'Active', color:'success'}):h(UBadge, {label:'Expired', color:'warning'})
            return status
        }
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
                // Extend Button
                h(UButton, {
                    icon: 'i-lucide-pencil',
                    label: 'Extend',
                    size: 'xs',
                    color: 'primary',
                    variant: 'soft',
                    onClick: () => openExtend(row.original.id, row.original.expDate)
                }),
                // Delete Button
                h(UButton, {
                    icon: 'i-lucide-trash-2',
                    label: 'Delete',
                    size: 'xs',
                    color: 'error',
                    variant: 'soft',
                    onClick: () => onDelete(row.original.id)
                })
            ])
        }
    }
]

const openQr = async (qrstring: string) => {
    const instance = qrModal.open({ url: `http://100.10.1.201:5233/registration/${qrstring}`})
}

const openExtend = (id: number, currentExpDate: string) => {
    extendTokenId.value = id
    extendCurrentDate.value = new Date(currentExpDate).toISOString().split('T')[0]??''
    state.value.expDate = undefined
    isExtendOpen.value = true
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

const onExtendSubmit = async (event: FormSubmitEvent<ExpirySchema>) => {
    if (!extendTokenId.value) return
    
    try {
        await regsToken.extendRegistrationToken({
            id: extendTokenId.value,
            expDate: event.data.expDate
        })
        toast.add({
            title: "Success",
            description: 'Token expiry extended successfully',
            color: 'success'
        })
        await regsToken.GetAllTokens()
    } catch (error) {
        toast.add({
            title: "Error",
            description: 'Failed to extend token expiry',
            color: 'error'
        })
    } finally {
        isExtendOpen.value = false
        extendTokenId.value = null
        state.value.expDate = undefined
    }
}

const onDelete = async(id: number)=>{
     const modal = confirmModal.open()
     const result = await modal.result

     if(!result) {
        confirmModal.close()
     }

    try {
       await regsToken.deleteRegistrationToken(id)
       toast.add({title: "Action", description: 'Registration token deleted successfully', color: 'success'})
       regsToken.GetAllTokens()
    } catch (error) {
          toast.add({title: "Action", description: 'Failed to delete registration token', color: 'error'})
    }
}

const submitDebounce = useDebounceFn(onSubmit, 500)
const extendDebounce = useDebounceFn(onExtendSubmit, 500)
const filterItem = ref(['Active', 'Expired', 'All'])
const selectedFilter = ref('All')
const globalFilter = ref('')
const minDate = computed(() => new Date().toISOString().split('T')[0])
const extendMinDate = computed(() => extendCurrentDate.value || minDate.value)
const pagination = ref({ pageIndex: 0, pageSize: 5 })

const filterResult = computed(()=> tokens.value?.filter((t)=> {
    switch (selectedFilter.value) {
        case 'Active':
            return new Date() < new Date(t.expDate)
            break;
        case 'Expired':
            return new Date() > new Date(t.expDate)
            break;
        case 'All':
            return t
            break;
        default:
            break;
    }
}))

onMounted(async () => {
    await regsToken.GetAllTokens()
})

const back = () => {
    router.back()
}

</script>

  <template>
    <UMain class="space-y-6">
      <div>
        <UButton @click="back" label="Back" icon="i-lucide-arrow-left" variant="ghost" />
      </div>
      <div class="my-3">
        <h1 class="text-4xl font-black text-primary tracking-tight">Registration Token</h1>
        <p class="text-muted text-sm mt-1">Manage registration tokens and QR codes</p>
      </div>

      <UCard variant="outline">
        <template #header>
          <div class="w-full flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div class="flex flex-col gap-3 sm:flex-row sm:items-center">
              <UInput v-model="globalFilter" class="max-w-sm" placeholder="Filter tokens..." icon="i-lucide-search" size="md" />
              <USelect
                v-model="selectedFilter"
                :items="filterItem"
                class="w-auto"
                placeholder="Filter by status"
              />
            </div>

            <div class="flex items-center gap-2">
              <UModal v-model:open="isOpen" title="Generate Registration Token">
                <UButton label="Generate Token" icon="i-lucide-plus" color="primary" />
                <template #content>
                  <UForm
                    @error="(r) => console.log(r)"
                    @submit="submitDebounce"
                    :state="state"
                    :schema="expirySchema"
                    class="p-4 w-full space-y-4"
                  >
                    <UFormField name="expDate" label="Expiration date">
                      <UInput v-model="state.expDate" type="date" class="w-full" :min="minDate" />
                    </UFormField>
                    <div class="flex justify-end">
                      <UButton type="submit" label="Generate" icon="i-lucide-qr-code" />
                    </div>
                  </UForm>
                </template>
              </UModal>

              <UModal v-model:open="isExtendOpen" title="Extend Token Expiry">
                <UButton label="Extend Token" icon="i-lucide-pencil" color="primary" variant="soft" />
                <template #content>
                  <UForm
                    @error="(r) => console.log(r)"
                    @submit="extendDebounce"
                    :state="state"
                    :schema="expirySchema"
                    class="p-4 w-full space-y-4"
                  >
                    <p class="text-sm text-neutral-500">
                      Current expiry: {{ new Date(extendCurrentDate).toLocaleDateString() }}
                    </p>
                    <UFormField name="expDate" label="New expiration date">
                      <UInput
                        v-model="state.expDate"
                        type="date"
                        class="w-full"
                        :min="extendMinDate"
                      />
                    </UFormField>
                    <div class="flex justify-end gap-2">
                      <UButton color="neutral" label="Cancel" @click="isExtendOpen = false" />
                      <UButton type="submit" label="Extend" icon="i-lucide-check" />
                    </div>
                  </UForm>
                </template>
              </UModal>
            </div>
          </div>
        </template>

        <UTable
          ref="table"
          v-model:pagination="pagination"
          :pagination-options="{ getPaginationRowModel: getPaginationRowModel() }"
          :data="filterResult ?? []"
          :columns="columns"
          class="flex-1"
          v-model:global-filter="globalFilter"
        />

        <template #footer>
          <div class="w-full flex justify-end items-center">
            <UPagination
              :default-page="(pagination.pageIndex || 0) + 1"
              :items-per-page="pagination.pageSize"
              :total="tokens?.length ?? 0"
              @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)"
            />
          </div>
        </template>
      </UCard>
    </UMain>
  </template>