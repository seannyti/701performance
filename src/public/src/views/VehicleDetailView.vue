<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../services/api'

const route = useRoute()
const router = useRouter()

const vehicle = ref<any>(null)
const loading = ref(true)
const selectedImage = ref<string>('')
const lightboxOpen = ref(false)

onMounted(async () => {
  try {
    const { data } = await api.get(`/api/inventory/${route.params.id}`)
    vehicle.value = data
    const primary = data.images?.find((i: any) => i.isPrimary) || data.images?.[0]
    if (primary) selectedImage.value = primary.url
  } catch {
    router.push('/inventory')
  } finally {
    loading.value = false
  }
})

const sortedImages = computed(() =>
  vehicle.value?.images ? [...vehicle.value.images].sort((a: any, b: any) => a.sortOrder - b.sortOrder) : []
)

const onSale = computed(() =>
  vehicle.value && vehicle.value.msrp > vehicle.value.salePrice
)

const savings = computed(() =>
  onSale.value ? vehicle.value.msrp - vehicle.value.salePrice : 0
)

const parsedSpecs = computed<{ label: string; value: string }[]>(() => {
  try {
    const raw = vehicle.value?.specs
    return raw ? JSON.parse(raw) : []
  } catch { return [] }
})

function formatPrice(n: number) {
  return n.toLocaleString('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })
}

function selectImage(url: string) {
  selectedImage.value = url
}

</script>

<template>
  <div class="detail-page">
    <!-- Loading -->
    <div v-if="loading" class="loading-screen container">
      <div v-for="i in 3" :key="i" class="skeleton" :style="{ height: i === 1 ? '500px' : '80px' }" />
    </div>

    <div v-else-if="vehicle" class="container detail-body">
      <!-- Breadcrumb -->
      <nav class="breadcrumb">
        <RouterLink to="/">Home</RouterLink>
        <span>/</span>
        <RouterLink to="/inventory">Inventory</RouterLink>
        <span>/</span>
        <span>{{ vehicle.year }} {{ vehicle.make }} {{ vehicle.model }}</span>
      </nav>

      <div class="detail-grid">
        <!-- Gallery -->
        <div class="gallery">
          <div class="gallery__main" @click="lightboxOpen = true">
            <img
              v-if="selectedImage"
              :src="selectedImage"
              :alt="`${vehicle.year} ${vehicle.make} ${vehicle.model}`"
              class="gallery__main-img"
            />
            <div v-else class="gallery__no-image">
              <svg viewBox="0 0 64 64" fill="none" width="64" height="64">
                <rect x="8" y="20" width="48" height="28" rx="4" stroke="currentColor" stroke-width="2"/>
                <circle cx="20" cy="52" r="5" stroke="currentColor" stroke-width="2"/>
                <circle cx="44" cy="52" r="5" stroke="currentColor" stroke-width="2"/>
              </svg>
              <p>No images available</p>
            </div>
            <span v-if="sortedImages.length > 1" class="gallery__count">
              {{ sortedImages.length }} photos
            </span>
          </div>

          <div v-if="sortedImages.length > 1" class="gallery__thumbs">
            <button
              v-for="img in sortedImages" :key="img.id"
              class="gallery__thumb"
              :class="{ active: selectedImage === img.url }"
              @click="selectImage(img.url)"
            >
              <img :src="img.thumbnailUrl" :alt="`Photo ${img.sortOrder}`" />
            </button>
          </div>
        </div>

        <!-- Info Panel -->
        <div class="info-panel">
          <div class="info-badges">
            <span class="badge badge--type">{{ vehicle.type.toUpperCase() }}</span>
            <span class="badge badge--condition">{{ vehicle.condition }}</span>
            <span v-if="onSale" class="badge badge--sale">On Sale</span>
            <span v-if="vehicle.status === 'pending'" class="badge badge--pending">Pending Sale</span>
          </div>

          <h1 class="vehicle-title">{{ vehicle.year }} {{ vehicle.make }} {{ vehicle.model }}</h1>
          <p v-if="vehicle.trim" class="vehicle-trim">{{ vehicle.trim }}</p>

          <div class="stock-info">
            <span>Stock #{{ vehicle.stockNumber }}</span>
            <span v-if="vehicle.vin"> &middot; VIN: {{ vehicle.vin }}</span>
          </div>

          <!-- Price block -->
          <div class="price-block">
            <span class="price-main">{{ formatPrice(vehicle.salePrice) }}</span>
            <div v-if="onSale" class="price-details">
              <span class="price-msrp">MSRP {{ formatPrice(vehicle.msrp) }}</span>
              <span class="price-save">Save {{ formatPrice(savings) }}</span>
            </div>
          </div>

          <!-- Quick specs -->
          <div class="specs-grid">
            <div v-if="vehicle.mileage !== undefined && vehicle.mileage !== null" class="spec-item">
              <span class="spec-label">Mileage</span>
              <span class="spec-value">{{ vehicle.mileage.toLocaleString() }} mi</span>
            </div>
            <div v-if="vehicle.color" class="spec-item">
              <span class="spec-label">Color</span>
              <span class="spec-value">{{ vehicle.color }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Condition</span>
              <span class="spec-value" style="text-transform: capitalize">{{ vehicle.condition }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Type</span>
              <span class="spec-value">{{ vehicle.type.toUpperCase() }}</span>
            </div>
          </div>

          <!-- CTAs -->
          <div class="cta-group">
            <span v-if="vehicle.status === 'pending'" class="cta-btn cta-btn--disabled">
              Sale Pending
            </span>
            <RouterLink to="/finance" class="cta-btn cta-btn--secondary">
              Apply for Financing
            </RouterLink>
            <RouterLink to="/contact" class="cta-btn cta-btn--ghost">
              Ask a Question
            </RouterLink>
          </div>

          <!-- Description -->
          <div v-if="vehicle.description" class="description">
            <h3>Description</h3>
            <p>{{ vehicle.description }}</p>
          </div>

          <!-- Disclaimer -->
          <p class="disclaimer">
            Price does not include taxes, title, registration, or doc fee. Contact us for complete out-the-door pricing.
          </p>
        </div>
      </div>

      <!-- Full Specifications (full width below grid) -->
      <div v-if="parsedSpecs.length" class="full-specs">
        <div class="full-specs-header">
          <h2>Specifications</h2>
          <a v-if="vehicle.specSheetUrl" :href="vehicle.specSheetUrl" target="_blank" class="spec-sheet-link">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
            View Full Spec Sheet (PDF)
          </a>
        </div>
        <div class="full-specs-table">
          <div v-for="spec in parsedSpecs" :key="spec.label" class="full-specs-row">
            <span class="full-specs-label">{{ spec.label }}</span>
            <span class="full-specs-value">{{ spec.value }}</span>
          </div>
        </div>
      </div>

      <!-- Spec sheet link if no specs table -->
      <div v-else-if="vehicle.specSheetUrl" class="spec-sheet-link-standalone">
        <a :href="vehicle.specSheetUrl" target="_blank" class="spec-sheet-link">
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
          View Full Spec Sheet (PDF)
        </a>
      </div>

      <!-- Related vehicles placeholder -->
      <div class="related-section">
        <h2>You Might Also Like</h2>
        <p class="text-muted">Browse more <RouterLink :to="`/inventory?type=${vehicle.type}`" class="related-link">{{ vehicle.type.toUpperCase() }}s</RouterLink> in our inventory.</p>
      </div>
    </div>

    <!-- Lightbox -->
    <Teleport to="body">
      <div v-if="lightboxOpen" class="lightbox" @click="lightboxOpen = false">
        <img :src="selectedImage" class="lightbox__img" @click.stop />
        <button class="lightbox__close" @click="lightboxOpen = false">✕</button>
        <div class="lightbox__thumbs" @click.stop>
          <button
            v-for="img in sortedImages" :key="img.id"
            class="lightbox__thumb"
            :class="{ active: selectedImage === img.url }"
            @click="selectImage(img.url)"
          >
            <img :src="img.thumbnailUrl" />
          </button>
        </div>
      </div>
    </Teleport>

  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;
@use '../assets/styles/mixins' as *;

.detail-page { padding-top: $navbar-height; min-height: 100vh; padding-bottom: $spacing-3xl; }

.loading-screen { display: flex; flex-direction: column; gap: $spacing-lg; padding-top: $spacing-2xl; }
.skeleton { background: $bg-card; border-radius: $radius-lg; animation: pulse 1.5s ease infinite; }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

.detail-body { padding-top: $spacing-xl; }

.breadcrumb { display: flex; align-items: center; gap: $spacing-sm; font-size: $font-size-sm; color: $text-muted; margin-bottom: $spacing-xl; flex-wrap: wrap; a { color: $text-muted; transition: color $transition-fast; &:hover { color: $primary; } } span { color: $border; } }

.detail-grid { display: grid; grid-template-columns: 1fr 420px; gap: $spacing-2xl; align-items: start; @media (max-width: 1024px) { grid-template-columns: 1fr; } }

// Gallery
.gallery { display: flex; flex-direction: column; gap: $spacing-md; }
.gallery__main { position: relative; border-radius: $radius-xl; overflow: hidden; background: $bg-card; cursor: zoom-in; aspect-ratio: 4/3; }
.gallery__main-img { width: 100%; height: 100%; object-fit: cover; transition: transform $transition-slow; &:hover { transform: scale(1.02); } }
.gallery__no-image { width: 100%; height: 100%; @include flex-center; flex-direction: column; gap: $spacing-md; color: $border; }
.gallery__count { position: absolute; bottom: $spacing-md; right: $spacing-md; background: rgba(0,0,0,0.7); color: white; padding: 0.25rem 0.75rem; border-radius: $radius-full; font-size: $font-size-sm; font-weight: 600; }
.gallery__thumbs { display: flex; gap: $spacing-sm; overflow-x: auto; padding-bottom: 4px; }
.gallery__thumb { width: 80px; height: 60px; border-radius: $radius-md; overflow: hidden; border: 2px solid transparent; cursor: pointer; flex-shrink: 0; transition: border-color $transition-fast; img { width: 100%; height: 100%; object-fit: cover; } &.active { border-color: $primary; } &:hover { border-color: rgba($primary, 0.5); } }

// Info Panel
.info-panel { display: flex; flex-direction: column; gap: $spacing-lg; }
.info-badges { display: flex; gap: $spacing-sm; flex-wrap: wrap; }
.badge { padding: 0.25rem 0.75rem; border-radius: $radius-full; font-size: 0.7rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
.badge--type { background: rgba($primary, 0.15); color: $primary; }
.badge--condition { background: $bg-elevated; color: $text-muted; }
.badge--sale { background: $primary; color: white; }
.badge--pending { background: rgba($warning, 0.15); color: $warning; }

.vehicle-title { font-size: clamp($font-size-2xl, 3vw, $font-size-3xl); font-weight: 900; line-height: 1.1; }
.vehicle-trim { color: $text-muted; font-size: $font-size-lg; margin-top: -$spacing-sm; }
.stock-info { font-size: $font-size-sm; color: $text-muted; }

.price-block { padding: $spacing-lg; background: $bg-card; border: 1px solid $border; border-radius: $radius-lg; }
.price-main { font-size: 2.5rem; font-weight: 900; display: block; }
.price-details { display: flex; gap: $spacing-md; align-items: center; margin-top: $spacing-xs; }
.price-msrp { font-size: $font-size-sm; color: $text-muted; text-decoration: line-through; }
.price-save { font-size: $font-size-sm; color: $success; font-weight: 600; }

.specs-grid { display: grid; grid-template-columns: 1fr 1fr; gap: $spacing-sm; }
.spec-item { padding: $spacing-sm $spacing-md; background: $bg-card; border-radius: $radius-md; }
.spec-label { font-size: 0.7rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: $text-muted; display: block; }
.spec-value { font-size: $font-size-sm; font-weight: 600; color: $text; }

.cta-group { display: flex; flex-direction: column; gap: $spacing-sm; }
.cta-btn { display: block; text-align: center; padding: 1rem; border-radius: $radius-md; font-weight: 700; font-size: $font-size-base; transition: all $transition-fast; }
.cta-btn--primary { background: $primary; color: white; border: none; cursor: pointer; font-family: inherit; &:hover { background: $primary-dark; } }
.cta-btn--secondary { background: $bg-elevated; color: $text; border: 1px solid $border; &:hover { border-color: $text-muted; } }
.cta-btn--ghost { background: transparent; border: 1px solid $border; color: $text; &:hover { border-color: $text; background: $bg-elevated; } }
.cta-btn--save { background: transparent; border: 1px solid $border; color: $text-muted; cursor: pointer; font-family: inherit; display: flex; align-items: center; justify-content: center; gap: 0.5rem; &:hover:not(:disabled) { border-color: $primary; color: $primary; } &:disabled { opacity: 0.5; cursor: not-allowed; } }
.cta-btn--disabled { background: $bg-elevated; color: $text-muted; border: 1px solid $border; cursor: default; font-weight: 700; text-align: center; border-radius: $radius-md; padding: 1rem; display: block; }

.description { h3 { font-size: $font-size-lg; font-weight: 700; margin-bottom: $spacing-sm; } p { color: $text-secondary; line-height: 1.8; font-size: $font-size-sm; } }

.full-specs {
  margin-top: $spacing-2xl;
  padding-top: $spacing-2xl;
  border-top: 1px solid $border;
}
.full-specs-header {
  display: flex; align-items: center; justify-content: space-between; gap: 1rem;
  margin-bottom: $spacing-lg; flex-wrap: wrap;
  h2 { font-size: $font-size-2xl; font-weight: 800; }
}
.full-specs-table {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  border: 1px solid $border;
  border-radius: $radius-lg;
  overflow: hidden;
  @media (max-width: 640px) { grid-template-columns: 1fr; }
}
.full-specs-row {
  display: flex; justify-content: space-between; align-items: baseline; gap: 1rem;
  padding: 0.6rem 1rem; font-size: $font-size-sm;
  border-bottom: 1px solid $border;
  border-right: 1px solid $border;
  &:nth-child(2n) { border-right: none; }
  &:nth-last-child(-n+2) { border-bottom: none; }
  &:nth-child(odd) { background: $bg-card; }
  @media (max-width: 640px) { border-right: none; &:last-child { border-bottom: none; } &:nth-last-child(2) { border-bottom: 1px solid $border; } }
}
.full-specs-label { color: $text-muted; font-weight: 500; flex-shrink: 0; }
.full-specs-value { color: $text; font-weight: 600; text-align: right; }

.spec-sheet-link {
  display: inline-flex; align-items: center; gap: 0.4rem;
  font-size: $font-size-sm; color: $primary; text-decoration: none; font-weight: 600;
  &:hover { text-decoration: underline; }
}
.spec-sheet-link-standalone { margin-top: $spacing-lg; }
.disclaimer { font-size: 0.75rem; color: $text-muted; line-height: 1.6; padding-top: $spacing-sm; border-top: 1px solid $border; }

.related-section { margin-top: $spacing-3xl; padding-top: $spacing-2xl; border-top: 1px solid $border; h2 { font-size: $font-size-2xl; font-weight: 800; margin-bottom: $spacing-sm; } }
.text-muted { color: $text-muted; font-size: $font-size-sm; }
.related-link { color: $primary; &:hover { text-decoration: underline; } }

// Lightbox
.lightbox { position: fixed; inset: 0; background: rgba(0,0,0,0.95); z-index: 9999; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: $spacing-xl; }
.lightbox__img { max-width: 90vw; max-height: 80vh; object-fit: contain; border-radius: $radius-lg; }
.lightbox__close { position: absolute; top: $spacing-xl; right: $spacing-xl; background: rgba(255,255,255,0.1); border: none; color: white; width: 40px; height: 40px; border-radius: 50%; font-size: 1.25rem; cursor: pointer; @include flex-center; }
.lightbox__thumbs { position: absolute; bottom: $spacing-xl; display: flex; gap: $spacing-sm; }
.lightbox__thumb { width: 60px; height: 45px; border-radius: $radius-sm; overflow: hidden; border: 2px solid transparent; cursor: pointer; opacity: 0.6; transition: all $transition-fast; img { width: 100%; height: 100%; object-fit: cover; } &.active { border-color: $primary; opacity: 1; } &:hover { opacity: 1; } }

</style>
