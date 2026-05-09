<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import VehicleCard from '../components/inventory/VehicleCard.vue'
import api from '../services/api'

const route = useRoute()
const router = useRouter()

const vehicles = ref<any[]>([])
const loading = ref(true)
const total = ref(0)
const page = ref(1)
const pageSize = 12

const GEAR_TYPES = ['apparel']
const isGearType = (t: string) => GEAR_TYPES.includes(t)

// If URL specifies an apparel type, the gear tab should be active automatically.
function tabFromQuery(): 'vehicles' | 'gear' {
  if (route.query.tab === 'gear') return 'gear'
  if (route.query.type && isGearType(route.query.type as string)) return 'gear'
  return 'vehicles'
}

const activeTab = ref<'vehicles' | 'gear'>(tabFromQuery())

const filters = ref({
  type: (route.query.type as string) || '',
  condition: (route.query.condition as string) || '',
  make: (route.query.make as string) || '',
  priceMin: route.query.priceMin ? Number(route.query.priceMin) : null as number | null,
  priceMax: route.query.priceMax ? Number(route.query.priceMax) : null as number | null,
  sort: (route.query.sort as string) || 'newest',
})

const allTypes = ref([
  { label: 'ATV', value: 'atv' },
  { label: 'UTV / Side-by-Side', value: 'utv' },
  { label: 'Motorcycle', value: 'motorcycle' },
  { label: 'Dirt Bike', value: 'dirtbike' },
  { label: 'Snowmobile', value: 'snowmobile' },
  { label: 'PWC', value: 'pwc' },
  { label: 'Apparel & Gear', value: 'apparel' },
  { label: 'Other', value: 'other' },
])

const types = computed(() =>
  activeTab.value === 'gear'
    ? allTypes.value.filter(t => isGearType(t.value))
    : allTypes.value.filter(t => !isGearType(t.value))
)

const conditions = computed(() => {
  const base = [
    { label: 'New', value: 'new' },
    { label: 'Used', value: 'used' },
  ]
  return activeTab.value === 'gear' ? base : [...base, { label: 'Consignment', value: 'consignment' }]
})

const sortOptions = [
  { label: 'Newest First', value: 'newest' },
  { label: 'Price: Low to High', value: 'price_asc' },
  { label: 'Price: High to Low', value: 'price_desc' },
  { label: 'Year: Newest', value: 'year_desc' },
]

const totalPages = computed(() => Math.ceil(total.value / pageSize))
const hasFilters = computed(() => filters.value.type || filters.value.condition || filters.value.make || filters.value.priceMin || filters.value.priceMax)

let abortController: AbortController | null = null

async function load() {
  abortController?.abort()
  abortController = new AbortController()

  loading.value = true
  try {
    // Tab determines the type slice we fetch:
    // - gear tab: fetch only the apparel type via API (specific type filter)
    // - vehicles tab: fetch a wide page and strip apparel client-side
    const params: Record<string, any> = { sort: filters.value.sort }
    if (activeTab.value === 'gear') {
      params.type = filters.value.type || 'apparel'
      params.page = page.value
      params.pageSize = pageSize
    } else if (filters.value.type) {
      params.type = filters.value.type
      params.page = page.value
      params.pageSize = pageSize
    } else {
      params.pageSize = 200
    }
    if (filters.value.condition) params.condition = filters.value.condition
    if (filters.value.make) params.make = filters.value.make
    if (filters.value.priceMin) params.priceMin = filters.value.priceMin
    if (filters.value.priceMax) params.priceMax = filters.value.priceMax

    const { data } = await api.get('/api/inventory', { params, signal: abortController.signal })
    const all = data.data ?? []
    if (activeTab.value === 'vehicles' && !filters.value.type) {
      vehicles.value = all.filter((v: any) => !isGearType(v.type))
      total.value = vehicles.value.length
    } else {
      vehicles.value = all
      total.value = data.total ?? all.length
    }
  } catch (err: any) {
    if (err.code !== 'ERR_CANCELED') throw err
  } finally {
    loading.value = false
  }
}

function switchTab(tab: 'vehicles' | 'gear') {
  activeTab.value = tab
  filters.value.type = ''
  filters.value.condition = ''
  page.value = 1
  syncQuery()
  load()
}

// React to URL changes (footer links, browser back/forward) when already on /inventory
watch(() => route.query, (q) => {
  activeTab.value = tabFromQuery()
  filters.value.type = (q.type as string) || ''
  filters.value.condition = (q.condition as string) || ''
  filters.value.make = (q.make as string) || ''
  filters.value.priceMin = q.priceMin ? Number(q.priceMin) : null
  filters.value.priceMax = q.priceMax ? Number(q.priceMax) : null
  filters.value.sort = (q.sort as string) || 'newest'
  page.value = 1
  load()
})

onUnmounted(() => abortController?.abort())

function applyFilter(key: string, value: any) {
  (filters.value as any)[key] = value
  page.value = 1
  syncQuery()
  load()
}

function clearFilters() {
  filters.value = { type: '', condition: '', make: '', priceMin: null, priceMax: null, sort: 'newest' }
  page.value = 1
  syncQuery()
  load()
}

function syncQuery() {
  const q: Record<string, string> = {}
  if (activeTab.value === 'gear') q.tab = 'gear'
  if (filters.value.type) q.type = filters.value.type
  if (filters.value.condition) q.condition = filters.value.condition
  if (filters.value.make) q.make = filters.value.make
  if (filters.value.sort && filters.value.sort !== 'newest') q.sort = filters.value.sort
  router.replace({ query: q })
}

onMounted(async () => {
  // Load managed types from settings
  api.get('/api/settings').then(({ data }) => {
    if (data['inventory_types']) {
      try { allTypes.value = JSON.parse(data['inventory_types']) } catch {}
    }
  }).catch(() => {})

  await load()
})
</script>

<template>
  <div class="inventory-page">
    <div class="inventory-header">
      <div class="container">
        <h1>Inventory</h1>
        <p>{{ total }} {{ activeTab === 'gear' ? 'item' : 'vehicle' }}{{ total !== 1 ? 's' : '' }} available</p>
        <div class="tabs">
          <button class="tab-btn" :class="{ active: activeTab === 'vehicles' }" @click="switchTab('vehicles')">
            Vehicles
          </button>
          <button class="tab-btn" :class="{ active: activeTab === 'gear' }" @click="switchTab('gear')">
            Apparel & Gear
          </button>
        </div>
      </div>
    </div>

    <div class="container inventory-body">
      <aside class="filters-sidebar">
        <div class="filters-header">
          <h3>Filter</h3>
          <button v-if="hasFilters" class="clear-btn" @click="clearFilters">Clear all</button>
        </div>

        <div class="filter-group">
          <h4>Category</h4>
          <div class="filter-options">
            <button
              v-for="t in types" :key="t.value"
              class="filter-chip"
              :class="{ active: filters.type === t.value }"
              @click="applyFilter('type', filters.type === t.value ? '' : t.value)"
            >{{ t.label }}</button>
          </div>
        </div>

        <div class="filter-group">
          <h4>Condition</h4>
          <div class="filter-options">
            <button
              v-for="c in conditions" :key="c.value"
              class="filter-chip"
              :class="{ active: filters.condition === c.value }"
              @click="applyFilter('condition', filters.condition === c.value ? '' : c.value)"
            >{{ c.label }}</button>
          </div>
        </div>

        <div class="filter-group">
          <h4>Price Range</h4>
          <div class="price-inputs">
            <input
              type="number"
              placeholder="Min $"
              :value="filters.priceMin ?? ''"
              @change="applyFilter('priceMin', ($event.target as HTMLInputElement).value ? Number(($event.target as HTMLInputElement).value) : null)"
              class="price-input"
            />
            <span class="price-sep">—</span>
            <input
              type="number"
              placeholder="Max $"
              :value="filters.priceMax ?? ''"
              @change="applyFilter('priceMax', ($event.target as HTMLInputElement).value ? Number(($event.target as HTMLInputElement).value) : null)"
              class="price-input"
            />
          </div>
        </div>
      </aside>

      <main class="inventory-main">
        <div class="sort-bar">
          <span class="results-count">{{ total }} results</span>
          <div class="sort-select-wrap">
            <label>Sort by</label>
            <select v-model="filters.sort" @change="applyFilter('sort', filters.sort)" class="sort-select">
              <option v-for="o in sortOptions" :key="o.value" :value="o.value">{{ o.label }}</option>
            </select>
          </div>
        </div>

        <div v-if="loading" class="vehicle-grid">
          <div v-for="i in pageSize" :key="i" class="skeleton-card" />
        </div>

        <div v-else-if="vehicles.length" class="vehicle-grid">
          <VehicleCard v-for="v in vehicles" :key="v.id" :vehicle="v" />
        </div>

        <div v-else class="empty-state">
          <svg class="empty-icon" viewBox="0 0 64 64" fill="none">
            <rect x="8" y="20" width="48" height="28" rx="4" stroke="currentColor" stroke-width="2"/>
            <circle cx="20" cy="52" r="5" stroke="currentColor" stroke-width="2"/>
            <circle cx="44" cy="52" r="5" stroke="currentColor" stroke-width="2"/>
            <path d="M8 32h48M20 20l4-8h16l4 8" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
          <h3>No vehicles found</h3>
          <p>Try adjusting your filters or <button class="text-btn" @click="clearFilters">clear all</button>.</p>
        </div>

        <div v-if="totalPages > 1" class="pagination">
          <button class="page-btn" :disabled="page === 1" @click="page--; load()">← Prev</button>
          <span class="page-info">{{ page }} / {{ totalPages }}</span>
          <button class="page-btn" :disabled="page >= totalPages" @click="page++; load()">Next →</button>
        </div>
      </main>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;
@use '../assets/styles/mixins' as *;

.inventory-page { padding-top: $navbar-height; min-height: 100vh; }

.inventory-header {
  background: $bg-card;
  border-bottom: 1px solid $border;
  padding: $spacing-2xl $spacing-lg;
  h1 { font-size: $font-size-3xl; font-weight: 900; }
  p { color: $text-muted; margin-top: $spacing-xs; }
}

.tabs {
  display: flex;
  gap: 0;
  margin-top: $spacing-lg;
  border-bottom: 1px solid $border;

  .tab-btn {
    background: none;
    border: none;
    padding: $spacing-sm $spacing-lg;
    color: $text-muted;
    font-size: $font-size-base;
    font-weight: 600;
    cursor: pointer;
    border-bottom: 2px solid transparent;
    margin-bottom: -1px;
    transition: color $transition-fast, border-color $transition-fast;

    &:hover { color: $text; }
    &.active { color: $primary; border-bottom-color: $primary; }
  }
}

.inventory-body {
  display: grid;
  grid-template-columns: 220px 1fr;
  gap: $spacing-2xl;
  padding-top: $spacing-2xl;
  padding-bottom: $spacing-3xl;
  @media (max-width: 768px) { grid-template-columns: 1fr; }
}

.filters-sidebar { display: flex; flex-direction: column; gap: $spacing-xl; }
.filters-header { @include flex-between; h3 { font-size: $font-size-lg; font-weight: 700; } }
.clear-btn { background: none; border: none; color: $primary; font-size: $font-size-sm; cursor: pointer; padding: 0; }
.filter-group { h4 { font-size: $font-size-sm; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: $text-muted; margin-bottom: $spacing-sm; } }
.filter-options { display: flex; flex-direction: column; gap: $spacing-xs; }

.filter-chip {
  background: none; border: 1px solid $border; border-radius: $radius-md;
  padding: 0.4rem 0.875rem; color: $text-muted; font-size: $font-size-sm;
  cursor: pointer; text-align: left; transition: all $transition-fast;
  &:hover { border-color: $primary; color: $text; }
  &.active { background: rgba($primary, 0.12); border-color: $primary; color: $primary; font-weight: 600; }
}

.price-inputs { display: flex; align-items: center; gap: $spacing-sm; }
.price-input {
  flex: 1; background: $bg-elevated; border: 1px solid $border;
  border-radius: $radius-md; padding: 0.4rem 0.625rem; color: $text;
  font-size: $font-size-sm; min-width: 0;
  &:focus { outline: none; border-color: $primary; }
}
.price-sep { color: $text-muted; flex-shrink: 0; }

.sort-bar { @include flex-between; margin-bottom: $spacing-lg; flex-wrap: wrap; gap: $spacing-sm; }
.results-count { color: $text-muted; font-size: $font-size-sm; }
.sort-select-wrap { display: flex; align-items: center; gap: $spacing-sm; font-size: $font-size-sm; color: $text-muted; }
.sort-select { background: $bg-elevated; border: 1px solid $border; border-radius: $radius-md; padding: 0.4rem 0.75rem; color: $text; font-size: $font-size-sm; cursor: pointer; }

.vehicle-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: $spacing-lg; }
.skeleton-card { height: 320px; background: $bg-card; border-radius: $radius-lg; border: 1px solid $border; animation: pulse 1.5s ease infinite; }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

.empty-state { text-align: center; padding: 4rem 0; color: $text-muted; display: flex; flex-direction: column; align-items: center; gap: $spacing-md; }
.empty-icon { width: 64px; height: 64px; color: $border; }
.text-btn { background: none; border: none; color: $primary; cursor: pointer; text-decoration: underline; font-size: inherit; padding: 0; }

.pagination { display: flex; align-items: center; justify-content: center; gap: $spacing-lg; margin-top: $spacing-2xl; }
.page-btn { background: $bg-elevated; border: 1px solid $border; border-radius: $radius-md; padding: 0.5rem 1.25rem; color: $text; cursor: pointer; transition: border-color $transition-fast; &:hover:not(:disabled) { border-color: $primary; } &:disabled { opacity: 0.4; cursor: not-allowed; } }
.page-info { color: $text-muted; font-size: $font-size-sm; }
</style>
