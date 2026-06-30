<script setup lang="ts">
import { RouterView } from 'vue-router'
import AdminNavbar from '../components/adminNavbar.vue'
import { UseAuthStore } from '../stores/auth.ts'
const auth = UseAuthStore()
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
            <AdminAvatar />
          </template>
        </UDashboardNavbar>
      </template>

      <!-- ✅ #body slot handles scrolling internally -->
      <template #body>
        <RouterView />
      </template>
    </UDashboardPanel>
  </UDashboardGroup>
</template>