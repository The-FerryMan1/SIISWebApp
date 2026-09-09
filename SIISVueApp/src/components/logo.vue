<script setup lang="ts">
import { onMounted } from 'vue'
import { useSystemSettingsStore } from '../stores/systemSettings'

const props = defineProps<{ collapsed: boolean; to?: string }>()

const settingsStore = useSystemSettingsStore()

onMounted(async () => {
  await settingsStore.fetchSettings()
})
</script>

<template>
  <ULink
    :to="props.to || '/admin'"
    class="flex justify-start w-full items-center gap-2"
    active-class="border-primary"
    inactive-class="border-transparent"
    aria-current="page"
  >
    <img :src="settingsStore.getLogoSrc()" data-system-logo class="size-10 object-contain" />
    <h1 v-if="!collapsed" class="font-black text-md">Student Internship Information System</h1>
  </ULink>
</template>
