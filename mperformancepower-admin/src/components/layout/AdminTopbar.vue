<script setup lang="ts">
import { useAuthStore } from '@/stores/auth.store'
import { useInquiryStore } from '@/stores/inquiry.store'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'

const auth = useAuthStore()
const inquiries = useInquiryStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <header class="topbar">
    <div class="topbar__left">
      <span class="topbar__page">Admin Panel</span>
    </div>
    <div class="topbar__right">
      <RouterLink to="/inquiries" class="topbar__badge" v-if="inquiries.stats.new > 0">
        <i class="pi pi-envelope" />
        <span class="badge">{{ inquiries.stats.new }}</span>
      </RouterLink>
      <span class="topbar__user">{{ auth.email }}</span>
      <Button label="Logout" icon="pi pi-sign-out" size="small" severity="secondary" @click="logout" />
    </div>
  </header>
</template>

<style scoped>
.topbar {
  height: 56px;
  background: #111;
  border-bottom: 1px solid #222;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  flex-shrink: 0;
}

.topbar__page { font-size: 0.875rem; color: #9a9a9a; }

.topbar__right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.topbar__badge {
  position: relative;
  color: #f0f0f0;
  font-size: 1.1rem;
}

.badge {
  position: absolute;
  top: -6px;
  right: -8px;
  background: #e63946;
  color: #fff;
  font-size: 0.65rem;
  font-weight: 700;
  padding: 1px 5px;
  border-radius: 999px;
}

.topbar__user { font-size: 0.8rem; color: #9a9a9a; }
</style>
