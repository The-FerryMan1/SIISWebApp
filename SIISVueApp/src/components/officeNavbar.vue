<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useInboxStore } from '../stores/inbox'

const prop = defineProps<{
  collapsed: boolean
}>()

const route = useRoute()
const inbox = useInboxStore()

const items = computed<NavigationMenuItem[]>(() => [
  {
    label: 'Dashboard',
    icon: 'i-lucide-layout-dashboard',
    active: route.name == 'office-dashboard',
    to: { name: 'office-dashboard' },
  },
  {
    label: 'Inbox',
    icon: 'i-lucide-bell',
    active: route.name == 'office-inbox',
    to: { name: 'office-inbox' },
    badge: inbox.unreadCount || undefined,
  },
  {
    label: 'Reports',
    icon: 'i-lucide-summary',
    active: route.name == 'office-reports',
    to: { name: 'office-reports' },
  },
  {
    label: 'Weekly History',
    icon: 'i-lucide-calendar-clock',
    active: route.name == 'office-weekly-history',
    to: { name: 'office-weekly-history' },
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
