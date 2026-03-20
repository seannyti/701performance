<script setup lang="ts">
import { computed } from 'vue'
import { useSettings } from '@/composables/useSettings'

const { settings } = useSettings()
const content = computed(() => settings.value?.legal?.shipping ?? '')
</script>

<template>
  <div class="legal-page container">
    <div class="legal-page__inner">
      <h1 class="legal-page__title">Shipping &amp; Returns</h1>
      <div v-if="content" class="legal-page__body" v-html="content" />
      <p v-else class="legal-page__empty">
        Our shipping and returns policy is being finalized. Please
        <RouterLink to="/contact">contact us</RouterLink> with any questions.
      </p>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;

.legal-page {
  padding-top: 4rem;
  padding-bottom: 5rem;

  &__inner {
    max-width: 760px;
    margin: 0 auto;
  }

  &__title {
    font-size: clamp(1.8rem, 4vw, 2.5rem);
    font-weight: 800;
    margin-bottom: 2rem;
    border-bottom: 1px solid $color-border;
    padding-bottom: 1rem;
  }

  &__body {
    color: $color-muted;
    line-height: 1.8;
    font-size: 0.95rem;

    :deep(h2) { font-size: 1.15rem; font-weight: 700; color: $color-text; margin: 2rem 0 0.75rem; }
    :deep(h3) { font-size: 1rem; font-weight: 700; color: $color-text; margin: 1.5rem 0 0.5rem; }
    :deep(p)  { margin-bottom: 1rem; }
    :deep(ul), :deep(ol) { padding-left: 1.5rem; margin-bottom: 1rem; }
    :deep(li) { margin-bottom: 0.4rem; }
    :deep(a)  { color: var(--color-primary); }
    :deep(strong) { color: $color-text; }
    :deep(hr) { border: none; border-top: 1px solid $color-border; margin: 2rem 0; }
  }

  &__empty {
    color: $color-muted;
    font-size: 0.95rem;
    line-height: 1.7;

    a { color: var(--color-primary); }
  }
}
</style>
