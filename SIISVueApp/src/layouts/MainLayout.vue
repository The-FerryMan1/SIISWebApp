<script setup lang="ts">

</script>

<template>
    <UDashboardGroup>
        <UDashboardSidebar />
        <template #header>
            <UDashboardNavbar />
        </template>
        <RouterView />
    </UDashboardGroup>

</template>