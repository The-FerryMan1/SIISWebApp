<script setup lang="ts">
import { RouterView } from 'vue-router'
import AdminNavbar from '../components/adminNavbar.vue'
import { UseAuthStore } from '../stores/auth.ts'
</script>

<template>
  <UDashboardGroup>
    <UDashboardSidebar resizable collapsible>
      <template #header="{ collapsed }">
        <h1>SIIS</h1>
      </template>
      <template #default="{ collapsed }">
        <AdminNavbar :collapsed="collapsed" />
      </template>
    </UDashboardSidebar>

    <UDashboardPanel>
      <template #header>
        <UDashboardNavbar>
          <template #leading>
            <UDashboardSidebarCollapse variant="subtle" />
          </template>
          <template #right>
            <UColorModeSwitch />
            <AdminAvartar/>
          </template>
        </UDashboardNavbar>
      </template>

      <template #body>
        <RouterView />
      </template>
    </UDashboardPanel>
  </UDashboardGroup>
</template>