<script setup lang="ts">
import type { ButtonProps } from '@nuxt/ui'
import { ref } from 'vue'

const links = ref<ButtonProps[]>([
  {
    label: 'Get Started',
    to: '/login',
    icon: 'i-lucide-arrow-right',
  },
])
</script>

<template>
  <div class="bg-[url('/images/cover-bg.png')] bg-cover bg-center min-h-screen">
    <UContainer class="flex items-center justify-center min-h-screen">
      <UPageHero
        title="Student Internship Information System"
        description="Streamline your internship management from application to endorsement. Built for the Provincial Government of Cavite."
        :links="links"
        class="max-w-3xl"
      />
    </UContainer>
  </div>
</template>
