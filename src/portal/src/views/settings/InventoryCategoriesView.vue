<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Panel from 'primevue/panel'
import Tag from 'primevue/tag'
import settingsService from '../../services/settings.service'
import inventoryService from '../../services/inventory.service'

const router = useRouter()
const toast = useToast()
const saving = ref(false)
const loading = ref(true)

const types = ref<{ label: string; value: string }[]>([])
const newTypeLabel = ref('')
const newTypeValue = computed(() => slugify(newTypeLabel.value))

function slugify(s: string) {
  return s.toLowerCase().trim().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '')
}

async function load() {
  loading.value = true
  try {
    const s = await settingsService.getAll()
    if (s['inventory_types']) {
      try { types.value = JSON.parse(s['inventory_types']) } catch {}
    }
    if (!types.value.length) types.value = [...inventoryService.getTypes()]
  } finally {
    loading.value = false
  }
}

function addType() {
  const label = newTypeLabel.value.trim()
  if (!label) return
  const slug = newTypeValue.value || slugify(label)
  if (types.value.some(t => t.value === slug)) {
    toast.add({ severity: 'warn', summary: `Type "${slug}" already exists`, life: 3000 })
    return
  }
  types.value.push({ label, value: slug })
  newTypeLabel.value = ''
}
function removeType(i: number) { types.value.splice(i, 1) }
function moveType(i: number, dir: -1 | 1) {
  const j = i + dir
  if (j < 0 || j >= types.value.length) return
  ;[types.value[i], types.value[j]] = [types.value[j], types.value[i]]
}

async function save() {
  saving.value = true
  try {
    await settingsService.bulkUpdate({
      inventory_types: JSON.stringify(types.value),
    })
    toast.add({ severity: 'success', summary: 'Categories saved', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save', life: 3000 })
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="categories-settings">
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/settings')" />
        <div>
          <h1>Catalog Categories</h1>
          <p class="page-sub">Inventory types shown as filter options on the public site and in the portal.</p>
        </div>
      </div>
      <Button label="Save Settings" icon="pi pi-check" :loading="saving" @click="save" />
    </div>

    <Panel header="Inventory Types">
      <p class="panel-desc">
        Types appear as filter options on the public inventory page and in the add-item form.
        The slug (shown in grey) is stored in the database — avoid editing slugs that already have items assigned.
        Types with the slug <code>apparel</code> are treated as Apparel &amp; Gear and skip vehicle-specific fields like VIN and mileage.
      </p>
      <div class="item-list">
        <div v-for="(type, i) in types" :key="i" class="item-row">
          <div class="item-body">
            <span class="item-label">{{ type.label }}</span>
            <Tag :value="type.value" severity="secondary" class="item-slug" />
          </div>
          <div class="item-actions">
            <button class="icon-btn" :disabled="i === 0" @click="moveType(i, -1)"><i class="pi pi-chevron-up" /></button>
            <button class="icon-btn" :disabled="i === types.length - 1" @click="moveType(i, 1)"><i class="pi pi-chevron-down" /></button>
            <button class="icon-btn danger" @click="removeType(i)"><i class="pi pi-times" /></button>
          </div>
        </div>
      </div>
      <div class="add-row">
        <div class="add-inputs">
          <InputText v-model="newTypeLabel" placeholder="Label (e.g. Side-by-Side)" @keyup.enter="addType" style="flex:1" />
          <span v-if="newTypeLabel.trim()" class="slug-preview">→ <code>{{ newTypeValue }}</code></span>
        </div>
        <Button icon="pi pi-plus" label="Add Type" outlined :disabled="!newTypeLabel.trim()" @click="addType" />
      </div>
    </Panel>
  </div>
</template>

<style scoped>
.categories-settings { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.header-left { display: flex; align-items: center; gap: 0.75rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; margin: 0; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }
.panel-desc { font-size: 0.8rem; color: #9e9e9e; margin-bottom: 1.25rem; line-height: 1.5; }
.panel-desc code { background: #1e1e1e; padding: 0.1rem 0.35rem; border-radius: 4px; font-size: 0.75rem; color: #ddd; }

.item-list { display: flex; flex-direction: column; margin-bottom: 1rem; }
.item-row { display: flex; align-items: center; justify-content: space-between; gap: 0.75rem; padding: 0.625rem 0; border-bottom: 1px solid #1e1e1e; }
.item-row:last-child { border-bottom: none; }
.item-body { display: flex; align-items: center; gap: 0.6rem; flex: 1; min-width: 0; }
.item-label { font-size: 0.875rem; color: #ddd; font-weight: 500; }
.item-slug { font-size: 0.65rem; flex-shrink: 0; }
.item-actions { display: flex; gap: 0.25rem; flex-shrink: 0; }

.icon-btn { background: none; border: 1px solid #2a2a2a; border-radius: 6px; color: #666; cursor: pointer; padding: 0.3rem 0.4rem; display: flex; align-items: center; justify-content: center; font-size: 0.7rem; transition: color 0.15s, border-color 0.15s; }
.icon-btn:hover:not(:disabled) { color: #ccc; border-color: #444; }
.icon-btn:disabled { opacity: 0.25; cursor: not-allowed; }
.icon-btn.danger:hover:not(:disabled) { color: #e53935; border-color: #e53935; }

.add-row { display: flex; align-items: center; gap: 0.75rem; padding-top: 0.875rem; border-top: 1px solid #1e1e1e; margin-top: 0.25rem; }
.add-inputs { display: flex; align-items: center; gap: 0.75rem; flex: 1; min-width: 0; }
.slug-preview { font-size: 0.75rem; color: #555; white-space: nowrap; }
.slug-preview code { color: #888; font-family: monospace; }
</style>
