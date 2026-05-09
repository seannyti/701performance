<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useSettingsStore } from '../../stores/settings'
import HeroYouTube from './HeroYouTube.vue'
import HeroVideo from './HeroVideo.vue'
import HeroImage from './HeroImage.vue'

const settings = useSettingsStore()

const heroType = computed(() => settings.get('hero_type', 'none'))
const title = computed(() => settings.get('hero_title', 'Your Powersports Destination'))
const accentWord = computed(() => settings.get('hero_title_accent', 'Destination'))
const subtitle = computed(() => settings.get('hero_subtitle', 'ATVs, UTVs, Dirt Bikes, Snowmobiles & More'))
const btn1Label = computed(() => settings.get('hero_btn1_label', 'Browse Inventory'))
const btn1Link = computed(() => settings.get('hero_btn1_link', '/inventory'))
const btn2Label = computed(() => settings.get('hero_btn2_label', 'Contact Us'))
const btn2Link = computed(() => settings.get('hero_btn2_link', '/contact'))
const overlayOpacity = computed(() => Number(settings.get('hero_overlay_opacity', '50')) / 100)

// Split title to wrap accent word in red span
const titleParts = computed(() => {
  const t = title.value
  const accent = accentWord.value
  if (!accent || !t.includes(accent)) return { before: t, accent: '', after: '' }
  const idx = t.indexOf(accent)
  return {
    before: t.substring(0, idx),
    accent,
    after: t.substring(idx + accent.length)
  }
})
</script>

<template>
  <section class="hero">
    <!-- Background media -->
    <div class="hero__bg">
      <HeroYouTube v-if="heroType === 'youtube'" />
      <HeroVideo v-else-if="heroType === 'mp4'" />
      <HeroImage v-else-if="heroType === 'image'" />
    </div>

    <!-- Overlay -->
    <div class="hero__overlay" :style="{ opacity: overlayOpacity }" />

    <!-- Content -->
    <div class="hero__content container">
      <h1 class="hero__title">
        {{ titleParts.before }}<span class="accent">{{ titleParts.accent }}</span>{{ titleParts.after }}
      </h1>
      <p class="hero__subtitle">{{ subtitle }}</p>
      <div class="hero__actions">
        <RouterLink :to="btn1Link" class="btn btn--primary">{{ btn1Label }}</RouterLink>
        <RouterLink :to="btn2Link" class="btn btn--ghost">{{ btn2Label }}</RouterLink>
      </div>
    </div>
  </section>
</template>

<style lang="scss" scoped>
@use '../../assets/styles/variables' as *;
@use '../../assets/styles/mixins' as *;

.hero {
  position: relative;
  height: 70vh;
  min-height: 500px;
  @include flex-center;
  overflow: hidden;
  background: $bg;

  &__bg {
    position: absolute;
    inset: 0;
    z-index: 0;
  }

  &__overlay {
    position: absolute;
    inset: 0;
    background: #000;
    z-index: 1;
  }

  &__content {
    position: relative;
    z-index: 2;
    text-align: center;
    padding-top: $navbar-height;
  }

  &__title {
    font-size: clamp(2.5rem, 6vw, $font-size-5xl);
    font-weight: 900;
    line-height: 1.1;
    margin-bottom: $spacing-lg;
    letter-spacing: -1px;

    .accent { color: $primary; }
  }

  &__subtitle {
    font-size: clamp($font-size-base, 2vw, $font-size-xl);
    color: $text-muted;
    margin-bottom: $spacing-2xl;
  }

  &__actions {
    display: flex;
    gap: $spacing-md;
    justify-content: center;
    flex-wrap: wrap;
  }
}

.btn {
  padding: 0.875rem 2rem;
  border-radius: $radius-md;
  font-weight: 700;
  font-size: $font-size-base;
  cursor: pointer;
  transition: all $transition-fast;
  display: inline-block;

  &--primary {
    background: $primary;
    color: $text;
    border: 2px solid $primary;
    &:hover { background: $primary-dark; border-color: $primary-dark; }
  }

  &--ghost {
    background: transparent;
    color: $text;
    border: 2px solid rgba(255,255,255,0.5);
    &:hover { border-color: $text; background: rgba(255,255,255,0.08); }
  }
}
</style>
