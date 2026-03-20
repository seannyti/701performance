<script setup lang="ts">
import type { VehicleListItem } from '@/types/vehicle.types'

const props = defineProps<{ vehicle: VehicleListItem }>()

const apiBase = import.meta.env.VITE_API_URL.replace('/api', '')

function imageUrl(fileName: string | null) {
  return fileName ? `${apiBase}/uploads/${fileName}` : '/placeholder.jpg'
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(price)
}
</script>

<template>
  <RouterLink :to="`/inventory/${vehicle.id}`" class="vehicle-card">
    <div class="vehicle-card__image">
      <img :src="imageUrl(vehicle.primaryImage)" :alt="`${vehicle.year} ${vehicle.make} ${vehicle.model}`" loading="lazy" />
      <span class="vehicle-card__condition" :class="vehicle.condition.toLowerCase()">{{ vehicle.condition }}</span>
    </div>
    <div class="vehicle-card__body">
      <span class="vehicle-card__category">{{ vehicle.category }}</span>
      <h3 class="vehicle-card__title">{{ vehicle.year }} {{ vehicle.make }} {{ vehicle.model }}</h3>
      <div class="vehicle-card__meta">
        <span v-if="vehicle.mileage !== null">{{ vehicle.mileage.toLocaleString() }} mi</span>
        <span v-else>New</span>
      </div>
      <div class="vehicle-card__price">{{ formatPrice(vehicle.price) }}</div>
    </div>
  </RouterLink>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.vehicle-card {
  @include card;
  @include hover-lift;
  display: block;
  text-decoration: none;

  &__image {
    position: relative;
    aspect-ratio: 4/3;
    overflow: hidden;
    background: $color-surface-2;

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform 0.3s ease;
    }

    &:hover img { transform: scale(1.04); }
  }

  &__condition {
    position: absolute;
    top: $space-sm;
    right: $space-sm;
    font-size: 0.7rem;
    font-weight: 700;
    text-transform: uppercase;
    padding: 2px 8px;
    border-radius: 999px;

    &.new { background: $color-success; color: #000; }
    &.used { background: $color-surface-2; color: $color-muted; border: 1px solid $color-border; }
  }

  &__body {
    padding: $space-md;
  }

  &__category {
    font-size: 0.7rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--color-primary);
  }

  &__title {
    font-size: 1rem;
    font-weight: 600;
    margin-top: 2px;
    color: $color-text;
  }

  &__meta {
    font-size: 0.8rem;
    color: $color-muted;
    margin-top: $space-xs;
  }

  &__price {
    font-size: 1.2rem;
    font-weight: 700;
    color: var(--color-primary);
    margin-top: $space-sm;
  }
}
</style>
