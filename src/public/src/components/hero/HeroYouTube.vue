<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '../../stores/settings'

const settings = useSettingsStore()

const videoId = computed(() => {
  const url = settings.get('hero_youtube_url', '')
  const match = url.match(/(?:v=|youtu\.be\/)([^&\s]+)/)
  return match ? match[1] : ''
})

const startTime = computed(() => settings.get('hero_start_time', '0'))

const embedUrl = computed(() =>
  videoId.value
    ? `https://www.youtube.com/embed/${videoId.value}?autoplay=1&mute=1&loop=1&controls=0&playlist=${videoId.value}&start=${startTime.value}&disablekb=1&modestbranding=1&rel=0`
    : ''
)
</script>

<template>
  <div v-if="videoId" class="yt-wrapper">
    <iframe
      :src="embedUrl"
      frameborder="0"
      allow="autoplay; encrypted-media"
      allowfullscreen
      class="yt-iframe"
    />
  </div>
</template>

<style lang="scss" scoped>
.yt-wrapper {
  position: absolute;
  inset: -60px;
  pointer-events: none;
}

.yt-iframe {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
</style>
