<template>
  <div v-if="showPlayer" class="footer-music">
    <div class="footer-music__bar" @click="toggle">
      <span class="footer-music__note">🎵</span>
      <span class="footer-music__label">Now Playing</span>
      <span class="footer-music__chevron">{{ isMinimized ? '▲' : '▼' }}</span>
    </div>
    <div v-show="!isMinimized" class="footer-music__player">
      <div v-if="hasMounted" class="footer-music__embed" v-html="embedCode"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import DOMPurify from 'dompurify'
import { useSettings } from '@/composables/useSettings'

const { getSetting, loading } = useSettings()

const isMinimized = ref(true)
const hasMounted = ref(false)

function toggle() {
  if (isMinimized.value) hasMounted.value = true
  isMinimized.value = !isMinimized.value
}

const showPlayer = computed(() => {
  if (loading.value) return false
  return getSetting('music_enabled', 'false') === 'true' && getSetting('music_embed_code', '').trim().length > 0
})

const embedCode = computed(() => {
  const raw = getSetting('music_embed_code', '')
  const adjusted = raw
    .replace(/height=["']\d+["']/gi, 'height="80"')
    .replace(/height:\s*\d+px/gi, 'height: 80px')
    .replace(
      /allow="([^"]*)"/gi,
      (_, existing) => {
        const perms = existing.split(';').map((p: string) => p.trim()).filter(Boolean)
        if (!perms.includes('encrypted-media')) perms.push('encrypted-media')
        return `allow="${perms.join('; ')}"`
      }
    )
  return DOMPurify.sanitize(adjusted, {
    ALLOWED_TAGS: ['iframe'],
    ALLOWED_ATTR: ['src', 'width', 'height', 'frameborder', 'allow', 'allowfullscreen', 'style', 'title', 'loading'],
  })
})
</script>

<style scoped>
.footer-music {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.footer-music__bar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem 1.5rem;
  cursor: pointer;
  background: rgba(255, 255, 255, 0.04);
  transition: background 0.2s;
  user-select: none;
}

.footer-music__bar:hover {
  background: rgba(255, 255, 255, 0.08);
}

.footer-music__note {
  font-size: 1.1rem;
}

.footer-music__label {
  flex: 1;
  font-size: 0.85rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.85);
  letter-spacing: 0.02em;
}

.footer-music__chevron {
  font-size: 0.65rem;
  color: rgba(255, 255, 255, 0.45);
}

.footer-music__embed {
  height: 80px;
  overflow: hidden;
  background: #000;
}

.footer-music__embed :deep(iframe) {
  display: block;
  width: 100% !important;
  height: 80px !important;
  border: none !important;
}

.footer-music__embed :deep(div) {
  display: none !important;
}
</style>
