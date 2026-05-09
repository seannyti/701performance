<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '../../stores/settings'

const settings = useSettingsStore()
const imageUrl = computed(() => settings.get('hero_image_url', ''))
</script>

<template>
  <div
    v-if="imageUrl"
    class="hero-image"
    :style="{ backgroundImage: `url(${imageUrl})` }"
  />
</template>

<style lang="scss" scoped>
.hero-image {
  position: absolute;
  inset: 0;
  background-size: cover;
  background-position: center;
}
</style>
