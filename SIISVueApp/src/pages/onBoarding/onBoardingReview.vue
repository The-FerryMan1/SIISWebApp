<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useOnBoardStore } from '../../stores/onaboard'
import { onMounted } from 'vue'

const onaboard = useOnBoardStore()
const router = useRouter()

const back = () => {
  router.back()
}
onMounted(() => {
  if (!onaboard.state.student.address) {
    back()
  }
})
</script>

<template>
  <UButton @click="back" label="Back" icon="i-lucide-arrow-left" />
  <div v-if="onaboard.state.student.address">
    {{ onaboard.state }}
  </div>

  <div v-else>no data</div>
</template>
