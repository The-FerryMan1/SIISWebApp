<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui';
import { computed } from 'vue';
import { useRoute } from 'vue-router';

const prop = defineProps<{
    collapsed: boolean
}>();

const route = useRoute();


const items = computed<NavigationMenuItem[]>(() => [
    {
        label: 'Dasboard',
        icon: 'i-lucide-layout-dashboard',
        active: route.name == 'dashboard',
          to: {name: 'dashboard'}
    },
    {
        label: 'Applications',
        icon: 'i-lucide-file',
        active: route.name == 'application',
        to: {name: 'application'}
    },
    {
        label: 'OJTs',
        icon: 'i-lucide-user',
        active: route.name == ''
    },
    {
        label: 'Offices',
        icon: 'i-lucide-user',
        active: route.name == ''
    },
    {
        label: 'Admin',
        icon: 'i-lucide-shield-user',
        active: route.name == ''
    },
    {
        label: 'Trash',
        icon: 'i-lucide-trash',
        active: route.name == ''
    },
    {
        label: 'Logs',
        icon: 'i-lucide-logs',
        active: route.name == ''
    },
]);
</script>

<template>
    <UNavigationMenu  :collapsed="collapsed" :items="items" orientation="vertical" />
</template>