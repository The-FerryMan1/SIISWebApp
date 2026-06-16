<script setup lang="ts">
import { UseAuthStore } from '../../stores/auth';

const auth = UseAuthStore();
</script>

<template>
    <div>hello world</div>

    <button v-if="auth.user">Logout</button>
</template>