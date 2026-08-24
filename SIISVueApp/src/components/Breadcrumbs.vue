<script setup lang="ts">
import type { BreadcrumbItem } from '@nuxt/ui'
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const breadcrumbs = computed<BreadcrumbItem[]>(() => {
  const crumbs: BreadcrumbItem[] = []

  if (route.path.startsWith('/admin')) {
    crumbs.push({ label: 'Admin', to: '/admin' })
  } else if (route.path.startsWith('/office')) {
    crumbs.push({ label: 'Office', to: '/office' })
  }

  const title = route.meta?.title as string | undefined
  if (title && crumbs[crumbs.length - 1]?.to !== route.path) {
    crumbs.push({ label: title })
  }

  return crumbs
})
</script>

<template>
  <nav v-if="breadcrumbs.length > 1" class="flex items-center mb-4">
    <UBreadcrumb :items="breadcrumbs" />
  </nav>
</template>
