<script setup lang="ts">
import type { SelectItem } from '@nuxt/ui'
import { ref, computed } from 'vue'

const props = defineProps<{
  title?: string
  description?: string
  items?: SelectItem[]
  selectPlaceholder?: string
  formats?: ('pdf' | 'csv')[]
}>()

const emit = defineEmits<{
  close: [{ format: 'pdf' | 'csv' | 'none'; selected?: number }]
}>()

const selected = ref<number | undefined>(undefined)
const selectedFormat = ref<'pdf' | 'csv' | 'none'>()

const showPdf = computed(() => !props.formats || props.formats.includes('pdf'))
const showCsv = computed(() => !props.formats || props.formats.includes('csv'))

function handleExport() {
  emit('close', { format: selectedFormat.value ?? 'none', selected: selected.value })
}
</script>

<template>
  <UModal>
    <template #content>
      <div class="space-y-4 p-6 text-center">
        <h2 class="text-lg font-semibold">{{ title || 'Select Format' }}</h2>
        <p v-if="description" class="text-sm text-gray-500">{{ description }}</p>

        <USelect
          :placeholder="selectPlaceholder"
          v-model="selected"
          :items
          class="w-full"
        />

        <div class="flex justify-end gap-2 my-2">
          <UButton
            v-if="showPdf"
            icon="i-lucide-file-text"
            color="primary"
            @click="selectedFormat = 'pdf'; handleExport()"
          >
            PDF
          </UButton>
          <UButton
            v-if="showCsv"
            icon="i-lucide-table"
            color="success"
            @click="selectedFormat = 'csv'; handleExport()"
          >
            CSV
          </UButton>
        </div>
      </div>
    </template>
    <template #footer>
      <UButton label="Cancel" color="neutral" variant="subtle" @click="$emit('close', { format: 'none' })" />
    </template>
  </UModal>
</template>
