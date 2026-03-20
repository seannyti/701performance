<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { VehicleFilters, VehicleCondition } from '@/types/vehicle.types'
import { getActiveCategories, type PublicCategory } from '@/services/category.service'

const model = defineModel<VehicleFilters>({ required: true })

const categories = ref<PublicCategory[]>([])

onMounted(async () => {
  categories.value = await getActiveCategories()
})

function onCategoryChange(e: Event) {
  const val = (e.target as HTMLSelectElement).value
  model.value = { ...model.value, categoryId: val ? Number(val) : undefined, page: 1 }
}

function onConditionChange(e: Event) {
  const val = (e.target as HTMLSelectElement).value
  model.value = { ...model.value, condition: (val as VehicleCondition) || undefined, page: 1 }
}

function onSearch(e: Event) {
  model.value = { ...model.value, search: (e.target as HTMLInputElement).value || undefined, page: 1 }
}
</script>

<template>
  <div class="filters">
    <input
      type="search"
      placeholder="Search make or model..."
      :value="model.search ?? ''"
      @input="onSearch"
      class="filters__input"
    />

    <select class="filters__select" :value="model.categoryId ?? ''" @change="onCategoryChange">
      <option value="">All Categories</option>
      <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
    </select>

    <select class="filters__select" :value="model.condition ?? ''" @change="onConditionChange">
      <option value="">New or Used</option>
      <option value="New">New</option>
      <option value="Used">Used</option>
    </select>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;

.filters {
  display: flex;
  gap: $space-md;
  align-items: center;
  flex-wrap: wrap;
  margin-bottom: $space-xl;
}

.filters__input {
  flex: 1;
  min-width: 200px;
  max-width: 360px;
  padding: $space-sm $space-md;
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: $border-radius;
  color: $color-text;
  font-size: 0.9rem;

  &:focus {
    outline: none;
    border-color: var(--color-primary);
  }

  &::placeholder { color: $color-muted; }
}

.filters__select {
  padding: $space-sm $space-md;
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: $border-radius;
  color: $color-text;
  font-size: 0.9rem;
  cursor: pointer;
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23777' d='M6 8L1 3h10z'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 12px center;
  padding-right: 32px;
  transition: border-color 0.2s;

  &:focus {
    outline: none;
    border-color: var(--color-primary);
  }

  option { background: $color-surface; }
}
</style>
