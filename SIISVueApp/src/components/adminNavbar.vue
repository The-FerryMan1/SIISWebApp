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
    label: 'Dasboard',
    icon: 'i-lucide-layout-dashboard',
    active: route.name == 'dashboard',
    to: { name: 'dashboard' },
  },
  {
    label: 'Applications',
    icon: 'i-lucide-file',
    active: route.path.includes('/application'),
    to: { name: 'application' },
  },
  {
    label: 'OJTs',
    icon: 'i-lucide-user',
    active: route.name == 'ojt',
    to: { name: 'ojt' },
  },
  {
    label: 'Registration',
    icon: 'i-lucide-file',
    active: route.name == 'registration-generator',
    to: { name: 'registration-generator' },
  },
  {
    label: 'Endorsement',
    icon: 'i-lucide-file',
    active: route.name == 'endorsement' || route.name == 'endorsement-by-school',
    to: { name: 'endorsement-by-school' },
  },
  {
    label: 'Offices',
    icon: 'i-lucide-building',
    active: route.name == 'office',
    to: { name: 'office' },
  },
  {
    label: 'Reports',
    icon: 'i-lucide-summary',
    active: route.name == 'report',
    to: { name: 'report' },
  },
  {
    label: 'Logs',
    icon: 'i-lucide-logs',
    active: route.name == '',
  },
])
</script>

<template>
  <UNavigationMenu :collapsed="collapsed" :items="items" orientation="vertical" />
</template>
