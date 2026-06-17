<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'
import { computed } from 'vue'
import { UseAuthStore } from '../stores/auth'

const auth = UseAuthStore()
const items = computed<DropdownMenuItem[]>(() => [
  {
    label: 'Profile',
    icon: 'i-lucide-user-pen',
  },
  {
    label: 'Logout',
    icon: 'i-lucide-log-out',
  },
])
</script>

<template>
  <UDropdownMenu v-if="auth.user" :items="items" class="ms-3">
    <UButton variant="ghost">
      <template #default>
        <UUser :name="auth.user.split('@gmail.com')[0]" description="Admin" />
      </template>
    </UButton>
  </UDropdownMenu>
</template>
