<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useVehicleStore } from '@/stores/vehicle.store'
import VehicleGrid from '@/components/vehicle/VehicleGrid.vue'
import { useSettings } from '@/composables/useSettings'

const store = useVehicleStore()
const { settings } = useSettings()

const home = computed(() => settings.value?.pages?.home)
const brands = computed(() => settings.value?.content?.brands ?? [])

const apiBase = import.meta.env.VITE_API_URL.replace('/api', '')

interface HeroConfig {
  heroType: 'none' | 'youtube' | 'mp4' | 'image'
  youtubeUrl?: string
  youtubeStartTime: number
  videoPath?: string
  imagePath?: string
  overlayOpacity: number
}

const heroConfig = ref<HeroConfig | null>(null)

function extractVideoId(url: string) {
  const match = url?.match(/(?:v=|youtu\.be\/)([^&\s]+)/)
  return match?.[1] ?? ''
}

const embedUrl = computed(() => {
  if (!heroConfig.value?.youtubeUrl) return ''
  const id = extractVideoId(heroConfig.value.youtubeUrl)
  const start = heroConfig.value.youtubeStartTime ?? 0
  return `https://www.youtube.com/embed/${id}?autoplay=1&mute=1&loop=1&controls=0&disablekb=1&modestbranding=1&rel=0&start=${start}&playlist=${id}`
})

onMounted(async () => {
  store.fetchVehicles({ page: 1, pageSize: 8, featured: true })
  try {
    const res = await fetch(`${apiBase}/api/hero`)
    if (res.ok) heroConfig.value = await res.json()
  } catch { /* use fallback gradient */ }
})
</script>

<template>
  <section class="hero">
    <!-- YouTube background -->
    <div v-if="heroConfig?.heroType === 'youtube' && embedUrl" class="hero__media">
      <iframe :src="embedUrl" class="hero__iframe" frameborder="0" allow="autoplay; encrypted-media" />
    </div>

    <!-- MP4 background -->
    <div v-else-if="heroConfig?.heroType === 'mp4' && heroConfig.videoPath" class="hero__media">
      <video autoplay muted loop playsinline class="hero__video">
        <source :src="`${apiBase}/uploads/${heroConfig.videoPath}`" type="video/mp4" />
      </video>
    </div>

    <!-- Image background -->
    <div
      v-else-if="heroConfig?.heroType === 'image' && heroConfig.imagePath"
      class="hero__media hero__media--image"
      :style="{ backgroundImage: `url(${apiBase}/uploads/${heroConfig.imagePath})` }"
    />

    <!-- Dark overlay -->
    <div
      v-if="heroConfig && heroConfig.heroType !== 'none'"
      class="hero__overlay"
      :style="{ opacity: heroConfig.overlayOpacity }"
    />

    <div class="hero__inner container">
      <h1 class="hero__title" v-html="home?.heroTitle || 'Your Powersports <span>Destination</span>'" />
      <p class="hero__sub">{{ home?.heroSubtitle || 'ATVs, UTVs, Dirt Bikes, Snowmobiles & More' }}</p>
      <div class="hero__actions">
        <RouterLink :to="home?.ctaPrimaryLink || '/inventory'" class="btn-primary">
          {{ home?.ctaPrimaryText || 'Browse Inventory' }}
        </RouterLink>
        <RouterLink :to="home?.ctaSecondaryLink || '/contact'" class="btn-outline">
          {{ home?.ctaSecondaryText || 'Contact Us' }}
        </RouterLink>
      </div>
    </div>
  </section>

  <section v-if="brands.length > 0" class="brands">
    <div class="container">
      <p class="brands__label">Brands We Carry</p>
      <div class="brands__list">
        <template v-for="(brand, i) in brands" :key="brand.name">
          <a v-if="brand.url" :href="brand.url" target="_blank" rel="noopener noreferrer" class="brands__item brands__item--link">{{ brand.name }}</a>
          <span v-else class="brands__item">{{ brand.name }}</span>
          <span v-if="i < brands.length - 1" class="brands__divider">·</span>
        </template>
      </div>
    </div>
  </section>

  <section class="featured">
    <div class="container">
      <div class="featured__header">
        <h2>{{ home?.featuredTitle || 'Featured Vehicles' }}</h2>
        <RouterLink to="/inventory" class="featured__all">View All →</RouterLink>
      </div>
      <VehicleGrid :vehicles="store.vehicles" :loading="store.loading" />
    </div>
  </section>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.hero {
  min-height: 70vh;
  @include flex-center;
  background: linear-gradient(135deg, $color-dark 0%, $color-surface 100%);
  position: relative;
  overflow: hidden;

  &::before {
    content: '';
    position: absolute;
    inset: 0;
    background: radial-gradient(ellipse at 60% 50%, rgba(var(--color-primary-rgb), 0.12) 0%, transparent 70%);
    z-index: 0;
  }

  &__media {
    position: absolute;
    inset: 0;
    z-index: 0;

    &--image {
      background-size: cover;
      background-position: center;
    }
  }

  // YouTube iframe trick — oversized and centered to avoid letterboxing
  &__iframe {
    position: absolute;
    top: 50%;
    left: 50%;
    width: 300%;
    height: 300%;
    transform: translate(-50%, -50%);
    pointer-events: none;
    border: none;
  }

  &__video {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &__overlay {
    position: absolute;
    inset: 0;
    background: #000;
    z-index: 1;
    pointer-events: none;
  }

  &__inner {
    text-align: center;
    position: relative;
    z-index: 2;
  }

  &__title {
    font-size: clamp(2.5rem, 7vw, 5rem);
    font-weight: 800;
    line-height: 1.1;
    letter-spacing: -0.03em;

    :deep(span) { color: var(--color-primary); }
  }

  &__sub {
    margin-top: $space-md;
    font-size: clamp(1rem, 2.5vw, 1.3rem);
    color: $color-muted;
  }

  &__actions {
    display: flex;
    gap: $space-md;
    justify-content: center;
    flex-wrap: wrap;
    margin-top: $space-xl;
  }
}

.btn-primary {
  padding: $space-md $space-xl;
  background: var(--color-primary);
  color: #fff;
  font-weight: 600;
  font-size: 1rem;
  border-radius: $border-radius;
  transition: background 0.2s;

  &:hover { background: var(--color-primary-dark); }
}

.btn-outline {
  padding: $space-md $space-xl;
  border: 1px solid $color-border;
  color: $color-text;
  font-weight: 600;
  font-size: 1rem;
  border-radius: $border-radius;
  transition: border-color 0.2s;

  &:hover { border-color: var(--color-primary); }
}

.brands {
  border-top: 1px solid $color-border;
  border-bottom: 1px solid $color-border;
  padding: $space-lg 0;
  background: $color-surface;

  &__label {
    text-align: center;
    font-size: 0.7rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.12em;
    color: $color-muted;
    margin-bottom: $space-md;
  }

  &__list {
    display: flex;
    align-items: center;
    justify-content: center;
    flex-wrap: wrap;
    gap: $space-sm $space-md;
  }

  &__item {
    font-size: 0.95rem;
    font-weight: 700;
    color: $color-text;
    letter-spacing: 0.02em;
    white-space: nowrap;

    &--link {
      color: $color-text;
      text-decoration: none;
      transition: color 0.15s;
      &:hover { color: var(--color-primary); }
    }
  }

  &__divider {
    color: $color-border;
    font-size: 1.2rem;
    line-height: 1;
    @media (max-width: 480px) { display: none; }
  }
}

.featured {
  padding: $space-2xl 0;

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: $space-xl;

    h2 {
      font-size: 1.75rem;
      font-weight: 700;
    }
  }

  &__all {
    font-size: 0.875rem;
    color: var(--color-primary);
    transition: opacity 0.2s;
    &:hover { opacity: 0.8; }
  }
}
</style>
