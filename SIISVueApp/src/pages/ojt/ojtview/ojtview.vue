<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useOJtStore, type OjtDetails } from '../../../stores/ojt'
import { storeToRefs } from 'pinia'
import { OfficesArray } from '../../admin/types/officeSelectValue'
import { useDebounceFn } from '@vueuse/core'
import ConfirmationModal from '../../../components/confirmationModal.vue'

const overlay = useOverlay()
const confModal = overlay.create(ConfirmationModal)
const ojt = useOJtStore()
const { ojtDetails } = storeToRefs(ojt)
const route = useRoute()
const router = useRouter()
watch(
    () => route.params.uuid,
    async (value) => {
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
        1: '1st Year',
        2: '2nd Year',
        3: '3rd Year',
        4: '4th Year',
        11: 'Grade 11',
        12: 'Grade 12',
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
    return [d.firstName, d.middleName, d.lastName].filter(Boolean).join(' ')
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
    }

}, 500)

</script>

  <template>
    <UMain class="py-8">
      <!-- Loading State -->
      <div v-if="!ojtDetails" class="flex flex-col items-center justify-center py-20 gap-4">
        <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-gray-400" />
        <p class="text-gray-500">Loading student details...</p>
      </div>

      <!-- Main Card -->
      <UCard v-else class="overflow-hidden" variant="outline">
        <template #header>
          <div class="flex items-center justify-between">
            <UButton @click="back" variant="ghost" color="neutral" icon="i-lucide-arrow-left" label="Back" />
            <div class="flex items-center gap-2">
              <UTooltip text="Copy UUID">
                <UButton color="neutral" variant="ghost" size="sm" icon="i-lucide-copy" />
              </UTooltip>

              <UTooltip text="Edit ojt details">
                <UButton color="info" variant="ghost" size="sm" icon="i-lucide-pen" />
              </UTooltip>
              <UTooltip text="Delete ojt">
                <UButton @click="debounceDelete" color="error" variant="ghost" size="sm"
                  icon="i-lucide-trash" />
              </UTooltip>
            </div>
          </div>
        </template>

        <!-- ── Header ─────────────────────────────── -->
        <div class="px-6 py-5 bg-gradient-to-r from-gray-50 to-gray-100/60 dark:from-gray-800/40 dark:to-gray-900/40 border-y border-gray-100 dark:border-gray-800">
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
                  {{ officeLabel(ojtDetails.office) }}
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

    </UMain>
  </template>