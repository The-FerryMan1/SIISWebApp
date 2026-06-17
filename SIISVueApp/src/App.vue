<script setup lang="ts">
import { onMounted } from 'vue'
import { UseAuthStore } from './stores/auth'
const auth = UseAuthStore()
onMounted(async () => {
  await auth.useVerify()
})
</script>

<template>
  <UApp :toaster="{ position: 'top-right' }">
    <UMain>
      <RouterView />
    </UMain>
  </UApp>
</template>

<style scoped></style>
