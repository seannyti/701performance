<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import HeroSection from '../components/hero/HeroSection.vue'
import VehicleCard from '../components/inventory/VehicleCard.vue'
import api from '../services/api'

const featured = ref<any[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const { data } = await api.get('/api/inventory/featured')
    featured.value = data
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div>
    <HeroSection />

    <!-- Featured Vehicles -->
    <section class="featured section">
      <div class="container">
        <div class="featured__header">
          <h2 class="featured__title">Featured Vehicles</h2>
          <RouterLink to="/inventory" class="featured__view-all">View All →</RouterLink>
        </div>

        <!-- Loading -->
        <div v-if="loading" class="featured__grid">
          <div v-for="i in 4" :key="i" class="skeleton-card" />
        </div>

        <!-- Vehicles -->
        <div v-else-if="featured.length" class="featured__grid">
          <VehicleCard v-for="v in featured" :key="v.id" :vehicle="v" />
        </div>

        <!-- Empty state -->
        <div v-else class="featured__empty">
          <svg class="empty-icon" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect x="8" y="20" width="48" height="28" rx="4" stroke="currentColor" stroke-width="2"/>
            <circle cx="20" cy="52" r="5" stroke="currentColor" stroke-width="2"/>
            <circle cx="44" cy="52" r="5" stroke="currentColor" stroke-width="2"/>
            <path d="M8 32h48M20 20l4-8h16l4 8" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
          <h3>Inventory Coming Soon</h3>
          <p>We're stocking up on the best powersports machines around.<br>Check back soon or contact us to ask about specific models.</p>
          <RouterLink to="/contact" class="btn-primary">Contact Us</RouterLink>
        </div>
      </div>
    </section>
  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;
@use '../assets/styles/mixins' as *;

.featured {
  &__header {
    @include flex-between;
    margin-bottom: $spacing-2xl;
  }

  &__title {
    font-size: $font-size-2xl;
    font-weight: 800;
  }

  &__view-all {
    color: $primary;
    font-weight: 600;
    font-size: $font-size-sm;
    transition: opacity $transition-fast;
    &:hover { opacity: 0.8; }
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: $spacing-xl;
  }

  &__empty {
    text-align: center;
    padding: $spacing-3xl 0;
    color: $text-muted;

    .empty-icon {
      width: 64px;
      height: 64px;
      margin: 0 auto $spacing-lg;
      color: $border;
    }

    h3 {
      font-size: $font-size-xl;
      font-weight: 700;
      color: $text;
      margin-bottom: $spacing-md;
    }

    p {
      font-size: $font-size-sm;
      line-height: 1.8;
      margin-bottom: $spacing-xl;
    }
  }
}

.skeleton-card {
  height: 320px;
  background: $bg-card;
  border-radius: $radius-lg;
  border: 1px solid $border;
  animation: pulse 1.5s ease infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
</style>
