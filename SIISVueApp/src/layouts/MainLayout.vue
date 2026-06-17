<script setup lang="ts">
import { RouterView } from 'vue-router';
import AdminNavbar from '../components/adminNavbar.vue';
import { UseAuthStore } from '../stores/auth.ts';
const auth = UseAuthStore();

</script>

<template>
    <UDashboardGroup>
        <UDashboardSidebar resizable collapsible>
            <template #header="{ collapsed }">
                <h1>SIIS</h1>
                <!-- logo here -->
            </template>

            <template #default="{ collapsed }">
                <AdminNavbar :collapsed="collapsed" />
            </template>
        </UDashboardSidebar>
        <UDashboardPanel>
            <UDashboardNavbar>
                <template #leading>
                    <UDashboardSidebarCollapse variant="subtle" />
                </template>
                <template #right>
                    <UColorModeSwitch />
                    <AdminAvartar />
                </template>
            </UDashboardNavbar>

            <RouterView />

        </UDashboardPanel>
    </UDashboardGroup>

</template>