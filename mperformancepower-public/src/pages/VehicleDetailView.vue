<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useVehicleStore } from '@/stores/vehicle.store'
import { useSafeHtml } from '@/composables/useSafeHtml'
import InquiryForm from '@/components/inquiry/InquiryForm.vue'

const route = useRoute()
const store = useVehicleStore()
const { sanitize } = useSafeHtml()

const apiBase = import.meta.env.VITE_API_URL.replace('/api', '')
const activeIndex = ref(0)

onMounted(() => {
  store.fetchVehicle(Number(route.params.id))
})

const vehicle = computed(() => store.selectedVehicle)

// When images load, start on the primary image
watch(vehicle, (v) => {
  if (!v) return
  const idx = v.images.findIndex(i => i.isPrimary)
  activeIndex.value = idx >= 0 ? idx : 0
}, { immediate: true })

const sortedImages = computed(() => {
  if (!vehicle.value) return []
  return [...vehicle.value.images].sort((a, b) => a.displayOrder - b.displayOrder)
})

const activeImage = computed(() => {
  const img = sortedImages.value[activeIndex.value]
  return img ? `${apiBase}/uploads/${img.fileName}` : '/placeholder.jpg'
})

function prev() {
  if (sortedImages.value.length === 0) return
  activeIndex.value = (activeIndex.value - 1 + sortedImages.value.length) % sortedImages.value.length
}

function next() {
  if (sortedImages.value.length === 0) return
  activeIndex.value = (activeIndex.value + 1) % sortedImages.value.length
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(price)
}
</script>

<template>
  <div class="container" style="padding-top: 40px; padding-bottom: 80px;">
    <div v-if="store.loading" class="loading">Loading...</div>
    <div v-else-if="!vehicle" class="error">Vehicle not found.</div>

    <div v-else class="detail">
      <div class="detail__main">
        <div class="detail__gallery">
          <div class="detail__hero-wrap">
            <img :src="activeImage" :alt="`${vehicle.year} ${vehicle.make} ${vehicle.model}`" class="detail__hero-img" />
            <template v-if="sortedImages.length > 1">
              <button class="gallery-nav gallery-nav--prev" @click="prev" aria-label="Previous image">
                <i class="pi pi-chevron-left" />
              </button>
              <button class="gallery-nav gallery-nav--next" @click="next" aria-label="Next image">
                <i class="pi pi-chevron-right" />
              </button>
            </template>
          </div>
          <div v-if="sortedImages.length > 1" class="detail__thumbs">
            <button
              v-for="(img, i) in sortedImages"
              :key="img.id"
              class="detail__thumb"
              :class="{ active: i === activeIndex }"
              @click="activeIndex = i"
            >
              <img :src="`${apiBase}/uploads/${img.fileName}`" :alt="`Photo ${i + 1}`" />
            </button>
          </div>
        </div>

        <div class="detail__info">
          <span class="detail__category">{{ vehicle.category }}</span>
          <h1 class="detail__title">{{ vehicle.year }} {{ vehicle.make }} {{ vehicle.model }}</h1>
          <div class="detail__price">{{ formatPrice(vehicle.price) }}</div>

          <table class="detail__specs">
            <tbody>
              <tr><th>Condition</th><td>{{ vehicle.condition }}</td></tr>
              <tr v-if="vehicle.mileage !== null"><th>Mileage</th><td>{{ vehicle.mileage?.toLocaleString() }} mi</td></tr>
              <tr><th>Category</th><td>{{ vehicle.category }}</td></tr>
              <tr><th>Stock</th><td>{{ vehicle.stock > 0 ? `${vehicle.stock} available` : 'Call for availability' }}</td></tr>
            </tbody>
          </table>

          <div v-if="vehicle.specs?.length" class="detail__extra-specs">
            <h3>Specifications</h3>
            <table class="detail__specs">
              <tbody>
                <tr v-for="spec in vehicle.specs" :key="spec.label">
                  <th>{{ spec.label }}</th>
                  <td>{{ spec.value }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div v-if="vehicle.description" class="detail__description">
            <h3>Description</h3>
            <!-- eslint-disable-next-line vue/no-v-html -->
            <div v-html="sanitize(vehicle.description)" />
          </div>
        </div>
      </div>

      <aside class="detail__sidebar">
        <InquiryForm :vehicle-id="vehicle.id" />
      </aside>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.detail {
  display: grid;
  grid-template-columns: 1fr;
  gap: $space-xl;

  @include respond-to(lg) {
    grid-template-columns: 1fr 360px;
  }

  &__hero-wrap {
    position: relative;
  }

  &__hero-img {
    width: 100%;
    aspect-ratio: 16/9;
    object-fit: cover;
    border-radius: $border-radius;
    background: $color-surface;
    display: block;
  }

  &__thumbs {
    display: flex;
    gap: 8px;
    margin-top: 10px;
    overflow-x: auto;
    padding-bottom: 4px;

    &::-webkit-scrollbar { height: 4px; }
    &::-webkit-scrollbar-track { background: transparent; }
    &::-webkit-scrollbar-thumb { background: $color-border; border-radius: 2px; }
  }

  &__thumb {
    flex-shrink: 0;
    width: 80px;
    height: 56px;
    border-radius: 4px;
    overflow: hidden;
    border: 2px solid transparent;
    padding: 0;
    cursor: pointer;
    background: $color-surface;
    transition: border-color 0.15s;

    &.active { border-color: var(--color-primary); }
    &:hover:not(.active) { border-color: $color-border; }

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      display: block;
    }
  }

  &__category {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: var(--color-primary);
  }

  &__title {
    font-size: 2rem;
    font-weight: 700;
    margin-top: $space-xs;
  }

  &__price {
    font-size: 1.75rem;
    font-weight: 800;
    color: var(--color-primary);
    margin: $space-md 0;
  }

  &__specs {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: $space-lg;

    th, td {
      padding: $space-sm;
      text-align: left;
      border-bottom: 1px solid $color-border;
      font-size: 0.875rem;
    }

    th {
      color: $color-muted;
      font-weight: 500;
      width: 40%;
    }
  }

  &__extra-specs {
    margin-bottom: $space-lg;
    h3 { font-size: 1rem; font-weight: 600; margin-bottom: $space-sm; }
  }

  &__description {
    h3 { font-size: 1rem; font-weight: 600; margin-bottom: $space-sm; }
    font-size: 0.9rem;
    color: $color-muted;
    line-height: 1.7;
  }
}

.gallery-nav {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(0, 0, 0, 0.55);
  border: none;
  color: #fff;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.15s;
  z-index: 1;

  &:hover { background: rgba(0, 0, 0, 0.8); }

  &--prev { left: 10px; }
  &--next { right: 10px; }
}

.loading, .error {
  text-align: center;
  padding: $space-2xl;
  color: $color-muted;
}
</style>
