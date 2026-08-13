<script setup lang="ts">
import type { ButtonProps } from '@nuxt/ui'
import { ref } from 'vue'

const links = ref<ButtonProps[]>([
  {
    label: 'login',
    to: '/login',
    icon: 'i-lucide-arrow-right',
  },
])
</script>

<template>
  <div class="bg-[url('/cover-bg.png')] bg-cover bg-center min-h-screen">
    <UContainer>
      <UPageHero
        title="Student Internship Information System"
        description="Welcome to Student Information System"
        :links="links"
      />
    </UContainer>
  </div>
</template>
