<script setup lang="ts">
const props = defineProps<{ collapesed: boolean }>()
</script>

<template>
  <div class="flex justify-start w-full items-center gap-2">
    <img src="../assets/img/logo.png" class="size-10 object-contain" />
    <h1 v-if="!collapesed" class="font-black text-xl">SIIS</h1>
  </div>
</template>
