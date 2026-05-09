<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '../../stores/settings'

const settings = useSettingsStore()
const videoUrl = computed(() => settings.get('hero_video_url', ''))
</script>

<template>
  <video
    v-if="videoUrl"
    :src="videoUrl"
    autoplay
    muted
    loop
    playsinline
    class="hero-video"
  />
</template>

<style lang="scss" scoped>
.hero-video {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
}
</style>
