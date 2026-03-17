<template>
  <Teleport to="body">
    <!-- Minimized: just a round button -->
    <button
      v-if="showPlayer && isMinimized"
      class="music-restore-btn"
      @click="restore"
      title="Show music player"
      aria-label="Show music player"
    >
      🎵
    </button>

    <!-- Mini player bar -->
    <div v-if="showPlayer && !isMinimized" class="music-bar">
      <!-- Header -->
      <div class="music-bar__header">
        <span class="music-bar__title">🎵 Now Playing</span>
        <button class="music-bar__minimize" @click="isMinimized = true" title="Minimize" aria-label="Minimize music player">−</button>
      </div>

      <!-- Embed — mounts once on first open so iframe has correct width -->
      <div v-if="hasMounted" class="music-bar__embed" v-html="embedCode"></div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useSettings } from '@/composables/useSettings'

const { getSetting, loading } = useSettings()

const isMinimized = ref(true)
const hasMounted = ref(false)

const restore = () => {
  hasMounted.value = true
  isMinimized.value = false
}

const showPlayer = computed(() => {
  if (loading.value) return false
  return getSetting('music_enabled', 'false') === 'true' && getSetting('music_embed_code', '').trim().length > 0
})

const embedCode = computed(() => {
  const raw = getSetting('music_embed_code', '')
  return raw
    // Force compact height (hides playlist, shows only current track + controls)
    .replace(/height=["']\d+["']/gi, 'height="80"')
    .replace(/height:\s*\d+px/gi, 'height: 80px')
    // Ensure encrypted-media permission (required by SoundCloud + Spotify)
    .replace(
      /allow="([^"]*)"/gi,
      (_, existing) => {
        const perms = existing.split(';').map((p: string) => p.trim()).filter(Boolean)
        if (!perms.includes('encrypted-media')) perms.push('encrypted-media')
        return `allow="${perms.join('; ')}"`
      }
    )
})
</script>

<style scoped>
/* Minimized restore button */
.music-restore-btn {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 9999;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  border: none;
  background: var(--color-primary, #6366f1);
  color: #fff;
  font-size: 1.2rem;
  cursor: pointer;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: transform 0.2s, background 0.2s;
}

.music-restore-btn:hover {
  transform: scale(1.08);
  background: var(--color-secondary, #ec4899);
}

/* Mini player bar */
.music-bar {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 9999;
  width: 380px;
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
  background: #181818;
  display: flex;
  flex-direction: column;
  animation: slideUp 0.25s ease;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(16px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* Header bar */
.music-bar__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background: #111;
  border-bottom: 1px solid rgba(255,255,255,0.07);
}

.music-bar__title {
  font-size: 0.78rem;
  font-weight: 600;
  color: #e2e8f0;
  letter-spacing: 0.03em;
}

.music-bar__minimize {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 1px solid rgba(255,255,255,0.2);
  background: rgba(255,255,255,0.08);
  color: #e2e8f0;
  font-size: 1rem;
  line-height: 1;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
  padding: 0;
}

.music-bar__minimize:hover {
  background: rgba(255,255,255,0.18);
}

/* Embed container — compact single-track height */
.music-bar__embed {
  width: 100%;
  height: 80px;
  overflow: hidden;
  background: #000;
}

.music-bar__embed :deep(iframe) {
  display: block;
  width: 100% !important;
  height: 80px !important;
  border: none !important;
}

/* Also handle the extra <div> SoundCloud appends after the iframe */
.music-bar__embed :deep(div) {
  display: none !important;
}

@media (max-width: 480px) {
  .music-bar {
    width: calc(100vw - 24px);
    right: 12px;
    bottom: 12px;
  }

  .music-restore-btn {
    right: 12px;
    bottom: 12px;
  }
}
</style>
