<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'

const props = defineProps<{
  vehicle: {
    id: number
    year: number
    make: string
    model: string
    trim?: string
    type: string
    condition: string
    salePrice: number
    msrp: number
    mileage?: number
    images: Array<{ url: string; thumbnailUrl: string; isPrimary: boolean }>
  }
}>()

const primaryImage = computed(() =>
  props.vehicle.images.find(i => i.isPrimary)?.thumbnailUrl ||
  props.vehicle.images[0]?.thumbnailUrl ||
  null
)

const onSale = computed(() => props.vehicle.msrp > props.vehicle.salePrice)

function formatPrice(n: number) {
  return n.toLocaleString('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })
}
</script>

<template>
  <div class="vehicle-card">
    <RouterLink :to="`/inventory/${vehicle.id}`" class="vehicle-card__image-link">
      <div class="vehicle-card__image">
        <img v-if="primaryImage" :src="primaryImage" :alt="`${vehicle.year} ${vehicle.make} ${vehicle.model}`" loading="lazy" />
        <div v-else class="vehicle-card__no-image">
          <svg viewBox="0 0 64 64" fill="none"><rect x="8" y="20" width="48" height="28" rx="4" stroke="currentColor" stroke-width="2"/><circle cx="20" cy="52" r="5" stroke="currentColor" stroke-width="2"/><circle cx="44" cy="52" r="5" stroke="currentColor" stroke-width="2"/></svg>
        </div>
        <span class="badge badge--condition">{{ vehicle.condition }}</span>
        <span v-if="onSale" class="badge badge--sale">Sale</span>
      </div>
    </RouterLink>

    <RouterLink :to="`/inventory/${vehicle.id}`" class="vehicle-card__body">
      <p class="vehicle-card__type">{{ vehicle.type.toUpperCase() }}</p>
      <h3 class="vehicle-card__name">{{ vehicle.year }} {{ vehicle.make }} {{ vehicle.model }}</h3>
      <p v-if="vehicle.trim" class="vehicle-card__trim">{{ vehicle.trim }}</p>
      <div class="vehicle-card__price">
        <span class="price-main">{{ formatPrice(vehicle.salePrice) }}</span>
        <span v-if="onSale" class="price-msrp">{{ formatPrice(vehicle.msrp) }}</span>
      </div>
      <p v-if="vehicle.mileage" class="vehicle-card__mileage">{{ vehicle.mileage.toLocaleString() }} miles</p>
    </RouterLink>
  </div>
</template>

<style lang="scss" scoped>
@use '../../assets/styles/variables' as *;
@use '../../assets/styles/mixins' as *;

.vehicle-card {
  display: flex;
  flex-direction: column;
  background: $bg-card;
  border: 1px solid $border;
  border-radius: $radius-lg;
  overflow: hidden;
  transition: border-color $transition-fast, transform $transition-fast, box-shadow $transition-fast;

  &:hover {
    border-color: $primary;
    transform: translateY(-2px);
    box-shadow: $shadow-md;
  }

  &__image-link { display: block; }

  &__image {
    position: relative;
    aspect-ratio: 4/3;
    overflow: hidden;
    background: $bg-elevated;

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform $transition-slow;
    }

    &:hover img { transform: scale(1.04); }
  }

  &__no-image {
    width: 100%;
    height: 100%;
    @include flex-center;
    svg { width: 48px; height: 48px; color: $border; }
  }

  &__body {
    padding: $spacing-lg;
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: $spacing-xs;
    text-decoration: none;
    color: inherit;
  }

  &__type {
    font-size: 0.7rem;
    font-weight: 700;
    letter-spacing: 1px;
    color: $primary;
  }

  &__name {
    font-size: $font-size-lg;
    font-weight: 700;
    line-height: 1.3;
  }

  &__trim {
    font-size: $font-size-sm;
    color: $text-muted;
  }

  &__price {
    display: flex;
    align-items: baseline;
    gap: $spacing-sm;
    margin-top: auto;
    padding-top: $spacing-sm;
  }

  &__mileage {
    font-size: $font-size-sm;
    color: $text-muted;
  }
}

.price-main {
  font-size: $font-size-xl;
  font-weight: 800;
  color: $text;
}

.price-msrp {
  font-size: $font-size-sm;
  color: $text-muted;
  text-decoration: line-through;
}

.badge {
  position: absolute;
  top: $spacing-sm;
  padding: 0.25rem 0.625rem;
  border-radius: $radius-full;
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;

  &--condition {
    left: $spacing-sm;
    background: rgba(0,0,0,0.7);
    color: $text;
  }

  &--sale {
    right: $spacing-sm;
    background: $primary;
    color: white;
  }
}
</style>
