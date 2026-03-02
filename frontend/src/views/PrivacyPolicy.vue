<template>
  <div class="privacy-page">
    <div class="container">
      <!-- Page Header -->
      <div class="page-header">
        <h1 class="page-title">{{ getSetting('privacy_title', 'Privacy Policy') }}</h1>
        <p class="page-subtitle">
          {{ getSetting('privacy_subtitle', 'How we collect, use, and protect your information') }}
        </p>
      </div>

      <!-- Page Content -->
      <div class="page-content" v-html="getSetting('privacy_content', '<p>Loading privacy policy...</p>')"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useSettings } from '@/composables/useSettings';

const { getSetting } = useSettings();
</script>

<style scoped>
.privacy-page {
  padding: 2rem 0 4rem;
}

.page-header {
  text-align: center;
  margin-bottom: 3rem;
}

.page-title {
  font-size: 2.5rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 1rem 0;
}

.page-subtitle {
  font-size: 1.25rem;
  color: #6b7280;
  margin: 0;
}

.page-content {
  max-width: 800px;
  margin: 0 auto;
  line-height: 1.8;
  color: #374151;
}

.page-content :deep(h2) {
  font-size: 1.875rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 2rem 0 1rem 0;
}

.page-content :deep(h3) {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1f2937;
  margin: 1.5rem 0 0.75rem 0;
}

.page-content :deep(p) {
  margin: 0 0 1rem 0;
}

.page-content :deep(strong) {
  font-weight: 600;
  color: #1a1a1a;
}

.page-content :deep(ul),
.page-content :deep(ol) {
  margin: 0 0 1rem 0;
  padding-left: 2rem;
}

.page-content :deep(li) {
  margin: 0.5rem 0;
}

.page-content :deep(a) {
  color: #ff6b35;
  text-decoration: none;
  font-weight: 500;
}

.page-content :deep(a:hover) {
  text-decoration: underline;
}

@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }

  .page-subtitle {
    font-size: 1.125rem;
  }
}
</style>
