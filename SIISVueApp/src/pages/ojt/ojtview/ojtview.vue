<script setup lang="ts">
import { computed, watch, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useOJtStore, type OjtDetails } from '../../../stores/ojt'
import { storeToRefs } from 'pinia'
import { OfficesArray } from '../../admin/types/officeSelectValue'
import { useDebounceFn } from '@vueuse/core'
import ConfirmationModal from '../../../components/confirmationModal.vue'
import { validateDateRange, validateAccumulatedHours, isNonEmpty } from '../../../utils/validators'
import { useAxios } from '../../../fetch/axios'



const overlay = useOverlay()
const confModal = overlay.create(ConfirmationModal)
const ojt = useOJtStore()
const { ojtDetails } = storeToRefs(ojt)
const route = useRoute()
const router = useRouter()
const toast = useToast()
watch(
    () => route.params.uuid,
    async (value) => {
        if (!value) return
        await ojt.ojtDetailsInit(value as string)
    },
    { immediate: true },
)

// ─── Formatters ─────────────────────────────────────────────

const formatKey = (key: string) =>
    key
        .replace(/([A-Z])/g, ' $1')
        .replace(/^./, (str) => str.toUpperCase())

const formatDate = (date: string) =>
    new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
    })

const genderLabel = (g: number) =>
    ['Male', 'Female', 'Other'][g] ?? 'Unknown'

const officeLabel = (g: number) =>
    OfficesArray[g]?.label ?? `Office ${g}`

const gradeLabel = (g: number) => {
    const grades: Record<number, string> = {
        0: 'Senior High School',
        1: 'College',
    }
    return grades[g] ?? 'Unknown'
}

const initials = computed(() => {
    const d = ojtDetails.value
    if (!d) return ''
    const f = d.firstName?.charAt(0) ?? ''
    const l = d.lastName?.charAt(0) ?? ''
    return `${f}${l}`.toUpperCase()
})

const fullName = computed(() => {
    const d = ojtDetails.value
    if (!d) return ''
    const middle = d.middleName ? ` ${d.middleName}` : ''
    return `${d.lastName}, ${d.firstName}${middle}`
})

// ─── Field Definitions (icon + color per field) ─────────────

const fieldMeta: Record<string, { icon: string; color: string }> = {
    studentUUID: { icon: 'i-lucide-fingerprint', color: 'neutral' },
    email: { icon: 'i-lucide-mail', color: 'primary' },
    lastName: { icon: 'i-lucide-user', color: 'indigo' },
    firstName: { icon: 'i-lucide-user', color: 'indigo' },
    middleName: { icon: 'i-lucide-user', color: 'indigo' },
    contactNumber: { icon: 'i-lucide-phone', color: 'green' },
    address: { icon: 'i-lucide-map-pin', color: 'amber' },
    office: { icon: 'i-lucide-building-2', color: 'blue' },
    dateOfBirth: { icon: 'i-lucide-calendar', color: 'rose' },
    gender: { icon: 'i-lucide-users', color: 'purple' },
    gradeLevel: { icon: 'i-lucide-graduation-cap', color: 'teal' },
}

const formatValue = (key: keyof OjtDetails, value: unknown): string => {
    if (value === null || value === undefined) return '—'
    if (key === 'gender' && typeof value === 'number') return genderLabel(value)
    if (key === 'office' && typeof value === 'number') return officeLabel(value)
    if (key === 'gradeLevel' && typeof value === 'number') return gradeLabel(value)
    if (key === 'dateOfBirth' && typeof value === 'string') return formatDate(value)
    return String(value)
}

const isFullWidth = (key: string) => key === 'address' || key === 'studentUUID'



const back = () => {
    router.back()
}


const debounceDelete = useDebounceFn(async () => {
    const instance = await confModal.open({ title: 'Delete ojt', description: 'All connected record will be deleted' })
    if (instance) {
        await ojt.deleteRequest(route.params.uuid as string)
        toast.add({ title: 'OJT deleted successfully', color: 'success' })
        router.push({ name: 'ojt' })
    }

}, 500)

const copyUuid = async () => {
    if (ojtDetails.value?.studentUUID) {
        await navigator.clipboard.writeText(ojtDetails.value.studentUUID)
        toast.add({ title: 'UUID copied to clipboard', color: 'success' })
    }
}

const transferOpen = ref(false)
const transferForm = ref({
  office: '',
  startDate: '',
  estimatedEndDate: '',
  accumulatedHours: 0,
})
const loading = ref(false)

watch(transferOpen, (isOpen) => {
  if (isOpen && ojtDetails.value) {
    transferForm.value = {
      office: ojtDetails.value.office ?? '',
      startDate: ojtDetails.value.startDate ?? '',
      estimatedEndDate: ojtDetails.value.estimatedEndDate ?? '',
      accumulatedHours: ojtDetails.value.accumulatedHours ?? 0,
    }
  }
})

const submitTransfer = async () => {
  if (!ojtDetails.value?.studentUUID) return
  try {
        // validation
        if (!isNonEmpty(transferForm.value.office)) {
            toast.add({ title: 'Validation', description: 'New office is required', color: 'error' })
            return
        }

        const dateCheck = validateDateRange(transferForm.value.startDate, transferForm.value.estimatedEndDate)
        if (!dateCheck.valid) {
            toast.add({ title: 'Validation', description: dateCheck.message, color: 'error' })
            return
        }

        // No total internship hours available in `ojtDetails`; skip max check
        const maxHours = undefined
        const accCheck = validateAccumulatedHours(transferForm.value.accumulatedHours, maxHours)
        if (!accCheck.valid) {
            toast.add({ title: 'Validation', description: accCheck.message, color: 'error' })
            return
        }

        loading.value = true
        await useAxios.put(`/admin/placement/${ojtDetails.value.studentUUID}`, transferForm.value)
    toast.add({ title: 'Placement transferred successfully', color: 'success' })
    transferOpen.value = false
    await ojt.ojtDetailsInit(ojtDetails.value.studentUUID)
  } catch {
    toast.add({ title: 'Failed to transfer placement', color: 'error' })
  } finally {
    loading.value = false
  }
}

</script>

<template>
    <UMain class="py-8">

        <!-- Loading State -->
        <div v-if="!ojtDetails" class="flex flex-col items-center justify-center py-20 gap-4">
            <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-gray-400" />
            <p class="text-gray-500">Loading student details...</p>
        </div>

        <!-- Main Card -->
        <UCard v-else class="overflow-hidden">
            <UButton @click="back" variant="ghost" />

            <!-- ── Header ─────────────────────────────── -->
            <template #header>
                <div class="flex items-start justify-between">
                    <div>
                        <h2 class="text-2xl font-bold text-primary">OJT Details</h2>
                        <div class="flex items-center gap-2 mt-1.5">
                            <span class="w-2 h-2 rounded-full bg-green-500" />
                            <small class="text-xs text-gray-400 font-mono">
                                {{ ojtDetails.studentUUID }}
                            </small>
                        </div>
                    </div>
                    <div>
                        <UTooltip text="Copy UUID">
                            <UButton @click="copyUuid" color="neutral" variant="ghost" size="sm" icon="i-lucide-copy" />
                        </UTooltip>

                        <UTooltip text="Transfer Office">
                            <UButton @click="transferOpen = true" color="primary" variant="ghost" size="sm"
                                icon="i-lucide-building-2" />
                        </UTooltip>

                        <UTooltip text="Delete ojt">
                            <UButton @click="debounceDelete" color="error" variant="ghost" size="sm"
                                icon="i-lucide-trash" />
                        </UTooltip>
                    </div>


                </div>
            </template>

            <!-- ── Profile Banner ─────────────────────── -->
            <div
                class="px-6 py-5 bg-gradient-to-r from-gray-50 to-gray-100/60 dark:from-gray-800/40 dark:to-gray-900/40 border-y border-gray-100 dark:border-gray-800">
                <div class="flex items-center gap-4">
                    <UAvatar :text="initials" size="xl"
                        class="ring-2 ring-white dark:ring-gray-800 bg-primary-100 dark:bg-primary-900 text-primary-600 dark:text-primary-400 text-lg font-bold" />
                    <div>
                        <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                            {{ fullName }}
                        </h3>
                        <div class="flex flex-wrap gap-2 mt-1.5">
                            <UBadge color="primary" variant="soft" size="sm">
                                {{ gradeLabel(ojtDetails.gradeLevel) }}
                            </UBadge>
                            <UBadge color="info" variant="soft" size="sm">
                                {{ ojtDetails.office }}
                            </UBadge>
                            <UBadge color="success" variant="soft" size="sm">
                                {{ genderLabel(ojtDetails.gender) }}
                            </UBadge>
                        </div>
                    </div>
                </div>
            </div>

            <!-- ── Details Grid ───────────────────────── -->
            <div class="p-6">
                <h4 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-4">
                    Personal Information
                </h4>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <div v-for="(value, key) in ojtDetails" :key="key"
                        class="flex items-start gap-3 p-3 rounded-lg bg-gray-50/80 dark:bg-gray-800/40 border border-gray-100 dark:border-gray-800/60 hover:border-gray-200 dark:hover:border-gray-700 transition-colors"
                        :class="{ 'sm:col-span-2': isFullWidth(key as string) }">
                        <!-- Icon -->
                        <div class="w-9 h-9 rounded-lg flex items-center justify-center shrink-0"
                            :class="`bg-${fieldMeta[key as string]?.color}-50 dark:bg-${fieldMeta[key as string]?.color}-900/20 text-${fieldMeta[key as string]?.color}-500`">
                            <UIcon :name="fieldMeta[key as string]?.icon ?? 'i-lucide-circle-dot'" class="w-4 h-4" />
                        </div>

                        <!-- Label + Value -->
                        <div class="min-w-0 flex-1">
                            <p class="text-[11px] text-gray-400 uppercase tracking-wide">
                                {{ formatKey(key as string) }}
                            </p>
                            <p class="text-sm font-medium truncate"
                                :class="key === 'studentUUID' ? 'font-mono text-gray-600 dark:text-gray-400' : 'text-gray-900 dark:text-white'">
                                {{ formatValue(key as keyof OjtDetails, value) }}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </UCard>

        <UModal v-model:open="transferOpen" title="Transfer Office" description="Move student to a different office">
          <template #body>
            <UForm class="flex flex-col gap-3">
              <UFormField label="New Office" required>
                <USelect v-model="transferForm.office" :items="OfficesArray.map(o => ({ label: o.label, value: o.label }))" placeholder="Select office" class="w-full" />
              </UFormField>
              <UFormField label="Start Date" required>
                <UInput v-model="transferForm.startDate" type="date" class="w-full" />
              </UFormField>
              <UFormField label="Estimated End Date" required>
                <UInput v-model="transferForm.estimatedEndDate" type="date" class="w-full" />
              </UFormField>
              <UFormField label="Accumulated Hours" required>
                <UInput v-model.number="transferForm.accumulatedHours" type="number" class="w-full" />
              </UFormField>
            </UForm>
          </template>
          <template #footer>
            <div class="flex justify-end gap-3">
              <UButton label="Cancel" variant="ghost" color="neutral" @click="transferOpen = false" />
              <UButton label="Transfer" color="primary" variant="solid" @click="submitTransfer" :loading="loading" />
            </div>
          </template>
        </UModal>
    </UMain>
</template>