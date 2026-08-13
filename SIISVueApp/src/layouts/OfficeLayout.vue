<script setup lang="ts">
import { RouterView } from 'vue-router'
import OfficeNavbar from '../components/officeNavbar.vue'
import Logo from '../components/logo.vue'
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
          </template>
        </UDashboardNavbar>
      </template>

      <template #body>
        <RouterView />
      </template>
    </UDashboardPanel>
  </UDashboardGroup>
</template>
