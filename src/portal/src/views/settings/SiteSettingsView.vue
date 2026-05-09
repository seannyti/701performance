<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import ToggleSwitch from 'primevue/toggleswitch'
import settingsService from '../../services/settings.service'

const router = useRouter()
const toast = useToast()
const maintenance = ref(false)
const saving = ref(false)

async function loadMaintenance() {
  const s = await settingsService.getAll()
  maintenance.value = s['maintenance_mode'] === 'true'
}

async function toggleMaintenance() {
  saving.value = true
  try {
    await settingsService.bulkUpdate({ maintenance_mode: String(maintenance.value) })
    toast.add({
      severity: maintenance.value ? 'warn' : 'success',
      summary: maintenance.value ? 'Maintenance mode ON — site is now offline' : 'Maintenance mode OFF — site is live',
      life: 3000
    })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to update maintenance mode', life: 3000 })
    maintenance.value = !maintenance.value // revert on error
  } finally {
    saving.value = false
  }
}

const sections = [
  { label: 'Hero Section', icon: 'pi pi-image', desc: 'Control the full-screen background — YouTube, video, or image', to: '/settings/hero' },
  { label: 'Contact & Hours', icon: 'pi pi-map-marker', desc: 'Phone, email, address, business hours, map embed', to: '/settings/contact' },
  { label: 'Financing', icon: 'pi pi-credit-card', desc: 'Synchrony and Octane lender card copy, URLs, and embed code', to: '/settings/finance' },
  { label: 'SEO', icon: 'pi pi-globe', desc: 'Meta title, description, Open Graph image and tags', to: '/settings/seo' },
  { label: 'Theme & Branding', icon: 'pi pi-palette', desc: 'Logo, favicon, primary color, announcement banner', to: '/settings/theme' },
  { label: 'Email & Notifications', icon: 'pi pi-bell', desc: 'SMTP config, notification recipients, reply-from address, and email triggers', to: '/settings/notifications' },
  { label: 'Catalog Categories', icon: 'pi pi-tags', desc: 'Inventory types shown as filters on the public site', to: '/settings/inventory-categories' },
  { label: 'Terms of Service', icon: 'pi pi-file-edit', desc: 'The Terms of Service displayed on the public /tos page', to: '/settings/tos' },
  { label: 'Privacy Policy', icon: 'pi pi-shield', desc: 'The GLBA-required privacy notice displayed on the public /privacy page', to: '/settings/privacy' },
]

onMounted(loadMaintenance)
</script>

<template>
  <div class="settings-hub">
    <div class="page-header">
      <h1>Site Settings</h1>
      <p class="page-sub">Control every dynamic element on the public website</p>
    </div>

    <!-- Maintenance Mode -->
    <div class="maintenance-card" :class="{ 'is-active': maintenance }">
      <div class="maintenance-card__left">
        <div class="maintenance-card__icon">
          <i class="pi pi-exclamation-triangle" />
        </div>
        <div>
          <div class="maintenance-card__title">Maintenance Mode</div>
          <div class="maintenance-card__desc">
            {{ maintenance
              ? 'Site is offline — visitors see the maintenance page'
              : 'Site is live and accessible to the public'
            }}
          </div>
        </div>
      </div>
      <div class="maintenance-card__right">
        <span class="maintenance-status" :class="{ 'online': !maintenance, 'offline': maintenance }">
          {{ maintenance ? 'OFFLINE' : 'ONLINE' }}
        </span>
        <ToggleSwitch v-model="maintenance" :disabled="saving" @change="toggleMaintenance" />
      </div>
    </div>

    <div class="settings-grid">
      <div
        v-for="s in sections"
        :key="s.to"
        class="settings-card"
        @click="router.push(s.to)"
      >
        <div class="settings-card__icon">
          <i :class="s.icon" />
        </div>
        <div class="settings-card__body">
          <h3>{{ s.label }}</h3>
          <p>{{ s.desc }}</p>
        </div>
        <i class="pi pi-angle-right settings-card__arrow" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.settings-hub { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }

/* Maintenance card */
.maintenance-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.25rem 1.5rem;
  background: #141414;
  border: 1px solid #2a2a2a;
  border-radius: 12px;
  flex-wrap: wrap;
  transition: border-color 0.2s, background 0.2s;
}
.maintenance-card.is-active {
  border-color: #f44336;
  background: rgba(244, 67, 54, 0.05);
}
.maintenance-card__left {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.maintenance-card__icon {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  background: rgba(229, 57, 53, 0.12);
  color: #e53935;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.125rem;
  flex-shrink: 0;
}
.maintenance-card__title { font-size: 0.9375rem; font-weight: 600; color: white; margin-bottom: 0.2rem; }
.maintenance-card__desc { font-size: 0.8rem; color: #9e9e9e; }
.maintenance-card__right { display: flex; align-items: center; gap: 1rem; }
.maintenance-status {
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 1.5px;
  padding: 0.25rem 0.625rem;
  border-radius: 999px;
}
.maintenance-status.online { background: rgba(76, 175, 80, 0.15); color: #4caf50; }
.maintenance-status.offline { background: rgba(244, 67, 54, 0.15); color: #f44336; }

.settings-grid { display: flex; flex-direction: column; gap: 0.5rem; }

.settings-card {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  padding: 1.25rem 1.5rem;
  background: #141414;
  border: 1px solid #2a2a2a;
  border-radius: 12px;
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
}
.settings-card:hover {
  border-color: #e53935;
  background: rgba(229, 57, 53, 0.04);
}

.settings-card__icon {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  background: rgba(229, 57, 53, 0.12);
  color: #e53935;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.125rem;
  flex-shrink: 0;
}

.settings-card__body { flex: 1; }
.settings-card__body h3 { font-size: 0.9375rem; font-weight: 600; color: white; margin-bottom: 0.2rem; }
.settings-card__body p { font-size: 0.8rem; color: #9e9e9e; }
.settings-card__arrow { color: #555; font-size: 0.875rem; }
</style>
