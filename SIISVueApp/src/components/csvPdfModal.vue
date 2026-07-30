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
const showSelect = computed(() => props.items != null && props.items.length > 0)

function handleExport() {
  emit('close', { format: selectedFormat.value ?? 'none', selected: selected.value })
}
</script>

<template>
  <UModal>
    <template #content>
      <div class="p-6">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h2 class="text-lg font-semibold text-primary">{{ title || 'Export Report' }}</h2>
            <p v-if="description" class="text-sm text-muted mt-0.5">{{ description }}</p>
          </div>
          <UButton
            icon="i-lucide-x"
            color="neutral"
            variant="ghost"
            size="xs"
            @click="$emit('close', { format: 'none' })"
          />
        </div>

        <USelect
          v-if="showSelect"
          :placeholder="selectPlaceholder"
          v-model="selected"
          :items
          class="w-full mb-4"
        />

        <div class="flex justify-end gap-2">
          <UButton
            v-if="showPdf"
            icon="i-lucide-file-text"
            color="primary"
            variant="solid"
            @click="selectedFormat = 'pdf'; handleExport()"
          >
            Export PDF
          </UButton>
          <UButton
            v-if="showCsv"
            icon="i-lucide-table"
            color="success"
            variant="solid"
            @click="selectedFormat = 'csv'; handleExport()"
          >
            Export CSV
          </UButton>
        </div>
      </div>
    </template>
  </UModal>
</template>
