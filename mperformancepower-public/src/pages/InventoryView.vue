<script setup lang="ts">
import { watch } from 'vue'
import { useVehicleStore } from '@/stores/vehicle.store'
import { useVehicleFilters } from '@/composables/useVehicleFilters'
import VehicleFilters from '@/components/vehicle/VehicleFilters.vue'
import VehicleGrid from '@/components/vehicle/VehicleGrid.vue'
import AppPagination from '@/components/common/AppPagination.vue'

const store = useVehicleStore()
const { filters } = useVehicleFilters()

watch(filters, () => store.fetchVehicles(filters), { immediate: true, deep: true })

function onPageChange(page: number) {
  filters.page = page
}
</script>

<template>
  <div class="inventory">
    <div class="container">
      <div class="inventory__header">
        <h1>Vehicle Inventory</h1>
        <span class="inventory__count" v-if="!store.loading">{{ store.totalCount }} vehicles</span>
      </div>

      <VehicleFilters v-model="filters" />
      <VehicleGrid :vehicles="store.vehicles" :loading="store.loading" />
      <AppPagination :page="filters.page" :total-pages="store.totalPages" @update:page="onPageChange" />
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;

.inventory {
  padding: $space-xl 0 $space-2xl;

  &__header {
    display: flex;
    align-items: baseline;
    gap: $space-md;
    margin-bottom: $space-xl;

    h1 { font-size: 2rem; font-weight: 700; }
  }

  &__count {
    font-size: 0.875rem;
    color: $color-muted;
  }
}
</style>
