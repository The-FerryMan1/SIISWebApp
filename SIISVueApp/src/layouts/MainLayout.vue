<script setup lang="ts">
import { RouterView } from 'vue-router'
import AdminNavbar from '../components/adminNavbar.vue'
import { UseAuthStore } from '../stores/auth.ts'
import Logo from '../components/logo.vue'
import Clocker from '../components/clocker.vue'
</script>

<template>
  <UDashboardGroup>
    <UDashboardSidebar resizable collapsible>
      <template #header="{ collapsed }">
        <Logo :collapesed="collapsed" />
      </template>
      <template #default="{ collapsed }">
        <AdminNavbar :collapsed="collapsed" />
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
            <AdminAvartar />
          </template>
        </UDashboardNavbar>
      </template>

      <template #body>
        <RouterView />
      </template>
    </UDashboardPanel>
  </UDashboardGroup>
</template>
