<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'
import { computed } from 'vue'
import { UseAuthStore } from '../stores/auth'
import { useAxios } from '../fetch/axios'
import { useRouter } from 'vue-router'
import { useDebounceFn } from '@vueuse/core'
import axios from 'axios'

const auth = UseAuthStore()
const toast = useToast()
const router = useRouter()

const items = computed<DropdownMenuItem[]>(() => [
  {
    label: 'Profile',
    icon: 'i-lucide-user-pen',
    onSelect: () => {
      router.push('/profile')
    },
  },
  {
    label: 'Logout',
    icon: 'i-lucide-log-out',
    onSelect: async () => {
      deboundLogout()
    },
  },
])

const deboundLogout = useDebounceFn(async () => {
  try {
    await axios.post('/logout')
    toast.add({ title: 'You logged out successfully', color: 'primary' })
    router.push({ name: 'login' })
  } catch (error) {
    console.log(error)
  }
}, 500)
</script>

<template>
  <UDropdownMenu v-if="auth.user" :items="items" class="ms-3">
    <UButton variant="ghost">
      <template #default>
        <UUser v-if="auth.user" :name="auth.user.username" description="Admin" />
      </template>
    </UButton>
  </UDropdownMenu>
</template>
