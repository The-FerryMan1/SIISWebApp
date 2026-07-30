<script setup lang="ts">
import type { BreadcrumbItem } from '@nuxt/ui'
import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const items = computed<BreadcrumbItem[]>(() => [
  {
    label: 'Application',
    icon: 'i-lucide-book-open',
    to: '/application',
  },
  {
    label: 'Application details',
    icon: 'i-lucide-box',
    to: '/application/details/' + route.params.uuid,
  },
  {
    label: 'Endorsement',
    icon: 'i-lucide-box',
    to: '/application/details/endorsement' + route.params.uuid,
  },
])

const goBack = () => {
  router.back()
}
</script>

  <template>
    <UMain class="space-y-6">
      <div class="flex items-center justify-between">
        <UButton variant="ghost" color="neutral" icon="i-lucide-arrow-left" @click="goBack">
          Back
        </UButton>
      </div>

      <UBreadcrumb :items="items" />

      <UPage>
        <template #left>
          <UCard variant="outline">
            <UForm>
              <p class="text-muted text-sm">Endorsement letter preview and download options will appear here.</p>
            </UForm>
          </UCard>
        </template>

        <template #right>
          <UCard variant="outline">
            <p class="text-muted text-sm">Additional details and actions will be available here.</p>
          </UCard>
        </template>
      </UPage>
    </UMain>
  </template>
