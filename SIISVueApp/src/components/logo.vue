<script setup lang="ts">
const props = defineProps<{ collapesed: boolean }>()
</script>

<template>
  <ULink
    to="/admin"
    class="flex justify-start w-full items-center gap-2"
    active-class="border-primary"
    inactive-class="border-transparent"
    aria-current="page"
  >
    <img src="../assets/img/logo.png" class="size-10 object-contain" />
    <h1 v-if="!collapesed" class="font-black text-xl">SIIS</h1>
  </ULink>
</template>
