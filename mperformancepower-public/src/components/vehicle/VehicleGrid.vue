<script setup lang="ts">
import VehicleCard from './VehicleCard.vue'
import type { VehicleListItem } from '@/types/vehicle.types'

defineProps<{
  vehicles: VehicleListItem[]
  loading: boolean
}>()
</script>

<template>
  <div v-if="loading" class="vehicle-grid">
    <div v-for="n in 12" :key="n" class="skeleton-card" />
  </div>

  <div v-else-if="vehicles.length === 0" class="vehicle-grid__empty">
    <p>No vehicles found matching your search.</p>
  </div>

  <div v-else class="vehicle-grid">
    <VehicleCard v-for="v in vehicles" :key="v.id" :vehicle="v" />
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.vehicle-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: $space-lg;

  @include respond-to(sm) { grid-template-columns: repeat(2, 1fr); }
  @include respond-to(lg) { grid-template-columns: repeat(3, 1fr); }
  @include respond-to(xl) { grid-template-columns: repeat(4, 1fr); }

  &__empty {
    text-align: center;
    padding: $space-2xl;
    color: $color-muted;
  }
}

.skeleton-card {
  aspect-ratio: 3/4;
  background: $color-surface;
  border-radius: $border-radius;
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}
</style>
