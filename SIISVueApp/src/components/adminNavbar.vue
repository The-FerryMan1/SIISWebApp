<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { UseAuthStore } from '../stores/auth'
import { useInboxStore } from '../stores/inbox'

const prop = defineProps<{
  collapsed: boolean
}>()

const route = useRoute()
const auth = UseAuthStore()
const inbox = useInboxStore()

const items = computed<NavigationMenuItem[]>(() => {
  const baseItems: NavigationMenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'i-lucide-layout-dashboard',
      active: route.name == 'dashboard',
      to: { name: 'dashboard' },
    },
    {
      label: 'Inbox',
      icon: 'i-lucide-bell',
      active: route.name == 'admin-inbox',
      to: { name: 'admin-inbox' },
      badge: inbox.unreadCount || undefined,
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
      label: 'Student Import',
      icon: 'i-lucide-upload',
      active: route.name == 'student-import',
      to: { name: 'student-import' },
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
    // {
    //   label: 'Requirements',
    //   icon: 'i-lucide-file-text',
    //   active: route.name == 'requirements',
    //   to: { name: 'requirements' },
    // },
    {
      label: 'Logs',
      icon: 'i-lucide-scroll-text',
      active: route.name == 'logs',
      to: { name: 'logs' },
    },
  ]

  if (auth.isAdmin) {
    return [
      ...baseItems,
      {
        label: 'Endorsement Settings',
        icon: 'i-lucide-settings',
        active: route.name == 'endorsement-settings',
        to: { name: 'endorsement-settings' },
      },
      {
        label: 'System Settings',
        icon: 'i-lucide-settings-2',
        active: route.name == 'system-settings',
        to: { name: 'system-settings' },
      },
      {
        label: 'Analytics',
        icon: 'i-lucide-pie-chart',
        active: route.name == 'analytics',
        to: { name: 'analytics' },
      },
    ]
  }

  return baseItems
})
</script>

<template>
  <UNavigationMenu :collapsed="collapsed" :items="items" orientation="vertical" />
</template>
