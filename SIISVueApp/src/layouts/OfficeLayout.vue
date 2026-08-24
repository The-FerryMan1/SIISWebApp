<script setup lang="ts">
import { RouterView, useRouter } from 'vue-router'
import OfficeNavbar from '../components/officeNavbar.vue'
import Logo from '../components/logo.vue'
import { useOfficeAccountStore } from '../stores/officeAuth.ts'
import Breadcrumbs from '../components/Breadcrumbs.vue'

const officeAuth = useOfficeAccountStore()
const router = useRouter()

function logout() {
  officeAuth.logout()
  router.push({ name: 'office-login' })
}

</script>

<template>
  <UDashboardGroup>
    <UDashboardSidebar resizable collapsible>
      <template #header="{ collapsed }">
        <Logo :collapsed="collapsed" />
      </template>
      <template #default="{ collapsed }">
        <OfficeNavbar :collapsed="collapsed" />
      </template>
    </UDashboardSidebar>

    <UDashboardPanel>
      <template #header>
        <UDashboardNavbar class="bg-primary">
          <template #leading>
            <UDashboardSidebarCollapse variant="subtle" />
          </template>

          <template #right>
            <UColorModeSwitch />
               <UButton icon="i-lucide-log-out" label="Logout" variant="solid" color="error" @click="logout" />
          </template>
        </UDashboardNavbar>
      </template>

      <template #body>
        <UMain class="p-4 md:p-6">
          <Breadcrumbs />
          <RouterView />
        </UMain>
      </template>
    </UDashboardPanel>
  </UDashboardGroup>
</template>
