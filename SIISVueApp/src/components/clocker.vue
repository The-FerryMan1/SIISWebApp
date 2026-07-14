<script setup lang="ts">
import { computed } from 'vue';
const clocker = computed(() => {
    return {
        time: new Date(),
        date: new Date(),
        day: new Date().getDay(),
        month: new Date().getMonth(),
        year: new Date().getFullYear(),
    }
})
</script>

<template>
    <div class="text-sm text-white flex">
        <h1>{{ clocker.time.toLocaleTimeString() }}</h1>
        <h1>{{ clocker.date.toLocaleDateString() }}</h1>
        <h1>{{ clocker.day.toLocaleString() }}</h1>
        <h1>{{ clocker.month.toLocaleString() }}</h1>
        <h1>{{ clocker.year.toLocaleString() }}</h1>
    </div>
</template>