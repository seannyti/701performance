<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '../stores/settings'
import DOMPurify from 'dompurify'

const settings = useSettingsStore()

const siteName = computed(() => settings.get('site_name', 'PerformancePower'))

const aboutContent = computed(() => {
  const raw = settings.get('about_content', '')
  return raw ? DOMPurify.sanitize(raw) : ''
})

const hasContent = computed(() => aboutContent.value.trim().length > 0)
</script>

<template>
  <div class="about-page">
    <!-- Hero -->
    <div class="about-hero">
      <div class="container">
        <h1>About <span class="accent">{{ siteName }}</span></h1>
        <p class="about-hero__sub">Your local powersports destination.</p>
      </div>
    </div>

    <div class="container about-body">
      <!-- Dynamic content from settings -->
      <div v-if="hasContent" class="about-content" v-html="aboutContent" />

      <!-- Default content when nothing is configured -->
      <div v-else class="about-default">
        <div class="about-section">
          <div class="about-section__icon">
            <svg viewBox="0 0 48 48" fill="none" width="40" height="40">
              <circle cx="24" cy="24" r="22" stroke="currentColor" stroke-width="2"/>
              <path d="M24 14v10l6 6" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"/>
            </svg>
          </div>
          <div>
            <h2>Our Story</h2>
            <p>
              {{ siteName }} has been serving powersports enthusiasts for years, providing top-quality ATVs,
              UTVs, dirt bikes, snowmobiles, and more. We're passionate riders ourselves — which is why we're
              committed to helping every customer find their perfect machine.
            </p>
          </div>
        </div>

        <div class="about-section">
          <div class="about-section__icon">
            <svg viewBox="0 0 48 48" fill="none" width="40" height="40">
              <path d="M12 36L8 12l16 8 16-8-4 24H12z" stroke="currentColor" stroke-width="2" stroke-linejoin="round"/>
            </svg>
          </div>
          <div>
            <h2>What We Offer</h2>
            <ul class="about-list">
              <li>New & used ATVs, UTVs, motorcycles, dirt bikes, and snowmobiles</li>
              <li>In-house financing through Synchrony and Octane</li>
              <li>Full-service repair and maintenance facility</li>
              <li>Genuine OEM parts and accessories</li>
              <li>Consignment sales for private sellers</li>
            </ul>
          </div>
        </div>

        <div class="about-section">
          <div class="about-section__icon">
            <svg viewBox="0 0 48 48" fill="none" width="40" height="40">
              <path d="M24 6L6 18v24h12V30h12v12h12V18L24 6z" stroke="currentColor" stroke-width="2" stroke-linejoin="round"/>
            </svg>
          </div>
          <div>
            <h2>Visit Us</h2>
            <p>
              Come see our showroom floor — we'd love to let you sit on some machines and talk through your options.
              No pressure, just great conversation about great machines.
            </p>
            <RouterLink to="/contact" class="about-cta">Get Directions →</RouterLink>
          </div>
        </div>
      </div>

      <!-- Values grid -->
      <div class="values-grid">
        <div class="value-card">
          <div class="value-card__icon"><i class="icon-check" />✓</div>
          <h3>Trusted</h3>
          <p>We build long-term relationships with every customer and stand behind every sale.</p>
        </div>
        <div class="value-card">
          <div class="value-card__icon">⚡</div>
          <h3>Fast Financing</h3>
          <p>Get approved in minutes with Synchrony and Octane — two trusted lenders in powersports.</p>
        </div>
        <div class="value-card">
          <div class="value-card__icon">🔧</div>
          <h3>Expert Service</h3>
          <p>Our factory-trained technicians keep your machine running at peak performance.</p>
        </div>
        <div class="value-card">
          <div class="value-card__icon">❤️</div>
          <h3>Rider-First</h3>
          <p>We ride. We know what matters. Every recommendation comes from real-world experience.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;
@use '../assets/styles/mixins' as *;

.about-page { padding-top: $navbar-height; min-height: 100vh; }

.about-hero {
  background: $bg-card;
  border-bottom: 1px solid $border;
  padding: $spacing-3xl $spacing-lg;
  text-align: center;

  h1 {
    font-size: clamp($font-size-3xl, 5vw, $font-size-5xl);
    font-weight: 900;
    margin-bottom: $spacing-sm;
  }

  &__sub {
    color: $text-muted;
    font-size: $font-size-lg;
  }
}

.accent { color: $primary; }

.about-body {
  padding-top: $spacing-3xl;
  padding-bottom: $spacing-3xl;
}

// Dynamic HTML content
.about-content {
  max-width: 800px;
  margin: 0 auto $spacing-3xl;
  font-size: $font-size-base;
  line-height: 1.8;
  color: $text-secondary;

  :deep(h1), :deep(h2), :deep(h3) { color: $text; font-weight: 700; margin: $spacing-xl 0 $spacing-md; }
  :deep(h2) { font-size: $font-size-2xl; }
  :deep(h3) { font-size: $font-size-xl; }
  :deep(p) { margin-bottom: $spacing-lg; }
  :deep(ul), :deep(ol) { padding-left: $spacing-xl; margin-bottom: $spacing-lg; }
  :deep(li) { margin-bottom: $spacing-sm; }
  :deep(a) { color: $primary; &:hover { text-decoration: underline; } }
  :deep(strong) { color: $text; font-weight: 600; }
  :deep(img) { max-width: 100%; border-radius: $radius-lg; margin: $spacing-lg 0; }
  :deep(blockquote) {
    border-left: 3px solid $primary;
    padding-left: $spacing-lg;
    margin: $spacing-xl 0;
    color: $text-muted;
    font-style: italic;
  }
}

// Default content layout
.about-default { max-width: 800px; margin: 0 auto $spacing-3xl; }

.about-section {
  display: flex;
  gap: $spacing-xl;
  margin-bottom: $spacing-3xl;
  align-items: flex-start;

  &__icon {
    width: 64px;
    height: 64px;
    background: rgba($primary, 0.1);
    color: $primary;
    border-radius: $radius-lg;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }

  h2 { font-size: $font-size-2xl; font-weight: 800; margin-bottom: $spacing-md; }
  p { color: $text-muted; line-height: 1.8; }

  @media (max-width: $bp-sm) { flex-direction: column; }
}

.about-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: $spacing-sm;
  color: $text-muted;

  li::before { content: '✓ '; color: $primary; font-weight: 700; }
}

.about-cta {
  display: inline-block;
  margin-top: $spacing-md;
  color: $primary;
  font-weight: 700;
  font-size: $font-size-sm;
  &:hover { text-decoration: underline; }
}

// Values grid
.values-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: $spacing-lg;
  border-top: 1px solid $border;
  padding-top: $spacing-3xl;
}

.value-card {
  @include card;
  text-align: center;
  padding: $spacing-xl $spacing-lg;

  h3 { font-size: $font-size-lg; font-weight: 700; margin: $spacing-md 0 $spacing-sm; }
  p { font-size: $font-size-sm; color: $text-muted; line-height: 1.6; }
}

.value-card__icon { font-size: 2rem; line-height: 1; }
</style>
