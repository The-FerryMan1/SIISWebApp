<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const prop = defineProps<{
  collapsed: boolean
}>()

const route = useRoute()

const items = computed<NavigationMenuItem[]>(() => [
  {
    label: 'Dashboard',
    icon: 'i-lucide-layout-dashboard',
    active: route.name == 'office-dashboard',
    to: { name: 'office-dashboard' },
  },
  {
    label: 'Reports',
    icon: 'i-lucide-summary',
    active: route.name == 'office-reports',
    to: { name: 'office-reports' },
  },
  // {
  //   label: 'Requirements',
  //   icon: 'i-lucide-file-text',
  //   active: route.name == 'office-requirements',
  //   to: { name: 'office-requirements' },
  // },
  {
    label: 'Logs',
    icon: 'i-lucide-scroll-text',
    active: route.name == 'office-logs',
    to: { name: 'office-logs' },
  },
])
</script>

<template>
  <UNavigationMenu :collapsed="collapsed" :items="items" orientation="vertical" />
</template>
