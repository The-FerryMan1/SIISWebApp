<template>
  <UMain class="min-h-screen flex items-center justify-center bg-default">
    <UContainer>
      <div class="flex flex-col items-center justify-center py-20 gap-6 text-center">
        <div class="relative">
          <div class="text-[120px] font-black text-primary/10 leading-none select-none">404</div>
          <div class="absolute inset-0 flex items-center justify-center">
            <UIcon name="i-lucide-compass" class="w-16 h-16 text-muted" />
          </div>
        </div>

        <div class="space-y-2">
          <h1 class="text-3xl font-bold text-primary">Page not found</h1>
          <p class="text-muted max-w-md">
            The page you are looking for does not exist or has been moved. Let's get you back on track.
          </p>
        </div>

        <UButton to="/" label="Go Home" icon="i-lucide-home" size="lg" color="primary" />
      </div>
    </UContainer>
  </UMain>
</template>

<script setup lang="ts">
</script>
