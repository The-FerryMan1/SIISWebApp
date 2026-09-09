<script setup lang="ts">
import { RouterView } from 'vue-router'
import AdminNavbar from '../components/adminNavbar.vue'
import { UseAuthStore } from '../stores/auth.ts'
import Logo from '../components/logo.vue'
import Breadcrumbs from '../components/Breadcrumbs.vue'
import { useSystemSettingsStore } from '../stores/systemSettings'
import { onMounted } from 'vue'

const settingsStore = useSystemSettingsStore()

onMounted(async () => {
  await settingsStore.fetchSettings()
})
</script>

<template>
  <UDashboardGroup>
    <UDashboardSidebar resizable collapsible>
      <template #header="{ collapsed }">
        <Logo :collapsed="collapsed" />
      </template>
      <template #default="{ collapsed }">
        <AdminNavbar :collapsed="collapsed" />
      </template>
    </UDashboardSidebar>

    <UDashboardPanel>
      <template #header>
        <UDashboardNavbar :style="{ backgroundColor: settingsStore.settings.themeColor || '#2f5cba' }">
          <template #leading>
            <UDashboardSidebarCollapse variant="subtle" />
          </template>

          
          <template #right>
            <UColorModeSwitch />
            <AdminAvartar />
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
