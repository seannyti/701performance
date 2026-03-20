<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useChatStore } from '@/stores/chat.store'

const route = useRoute()
const catalogOpen = ref(route.path.startsWith('/catalog'))
const salesOpen = ref(route.path.startsWith('/sales'))
const chatStore = useChatStore()
</script>

<template>
  <aside class="sidebar">
    <div class="sidebar__logo">
      M Performance <strong>Power</strong>
    </div>
    <nav class="sidebar__nav">
      <RouterLink
        to="/dashboard"
        class="sidebar__item"
        :class="{ active: route.path === '/dashboard' }"
      >
        <i class="pi pi-home" />
        <span>Dashboard</span>
      </RouterLink>

      <!-- Catalog group -->
      <div class="sidebar__group">
        <button
          class="sidebar__item sidebar__group-toggle"
          :class="{ active: route.path.startsWith('/catalog') }"
          @click="catalogOpen = !catalogOpen"
        >
          <i class="pi pi-tag" />
          <span>Catalog</span>
          <i class="sidebar__chevron pi" :class="catalogOpen ? 'pi-chevron-up' : 'pi-chevron-down'" />
        </button>

        <div v-show="catalogOpen" class="sidebar__sub">
          <RouterLink
            to="/catalog/categories"
            class="sidebar__item sidebar__item--sub"
            :class="{ active: route.path === '/catalog/categories' }"
          >
            <i class="pi pi-list" />
            <span>Categories</span>
          </RouterLink>
          <RouterLink
            to="/catalog/vehicles"
            class="sidebar__item sidebar__item--sub"
            :class="{ active: route.path.startsWith('/catalog/vehicles') }"
          >
            <i class="pi pi-car" />
            <span>Vehicles</span>
          </RouterLink>
        </div>
      </div>

      <!-- Sales group -->
      <div class="sidebar__group">
        <button
          class="sidebar__item sidebar__group-toggle"
          :class="{ active: route.path.startsWith('/sales') }"
          @click="salesOpen = !salesOpen"
        >
          <i class="pi pi-dollar" />
          <span>Sales</span>
          <i class="sidebar__chevron pi" :class="salesOpen ? 'pi-chevron-up' : 'pi-chevron-down'" />
        </button>

        <div v-show="salesOpen" class="sidebar__sub">
          <RouterLink
            to="/sales/orders"
            class="sidebar__item sidebar__item--sub"
            :class="{ active: route.path.startsWith('/sales/orders') }"
          >
            <i class="pi pi-shopping-cart" />
            <span>Orders</span>
          </RouterLink>
          <RouterLink
            to="/sales/finance"
            class="sidebar__item sidebar__item--sub"
            :class="{ active: route.path === '/sales/finance' }"
          >
            <i class="pi pi-chart-bar" />
            <span>Finance</span>
          </RouterLink>
        </div>
      </div>

      <RouterLink
        to="/inquiries"
        class="sidebar__item"
        :class="{ active: route.path === '/inquiries' }"
      >
        <i class="pi pi-envelope" />
        <span>Inquiries</span>
      </RouterLink>

      <RouterLink
        to="/calendar"
        class="sidebar__item"
        :class="{ active: route.path === '/calendar' }"
      >
        <i class="pi pi-calendar" />
        <span>Calendar</span>
      </RouterLink>

      <RouterLink
        to="/chat"
        class="sidebar__item"
        :class="{ active: route.path === '/chat' }"
      >
        <i class="pi pi-comments" />
        <span>Live Chat</span>
        <span v-if="chatStore.totalUnread > 0" class="sidebar__badge">{{ chatStore.totalUnread }}</span>
      </RouterLink>

      <RouterLink
        to="/users"
        class="sidebar__item"
        :class="{ active: route.path === '/users' }"
      >
        <i class="pi pi-users" />
        <span>Users</span>
      </RouterLink>

      <RouterLink
        to="/hero"
        class="sidebar__item"
        :class="{ active: route.path === '/hero' }"
      >
        <i class="pi pi-video" />
        <span>Hero</span>
      </RouterLink>

      <RouterLink
        to="/settings"
        class="sidebar__item"
        :class="{ active: route.path === '/settings' }"
      >
        <i class="pi pi-cog" />
        <span>Settings</span>
      </RouterLink>
    </nav>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 220px;
  background: #111;
  border-right: 1px solid #222;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
}

.sidebar__logo {
  padding: 20px;
  font-size: 0.95rem;
  font-weight: 600;
  border-bottom: 1px solid #222;
  color: #f0f0f0;
}

.sidebar__logo strong { color: #e63946; }

.sidebar__nav {
  display: flex;
  flex-direction: column;
  padding: 12px 8px;
  gap: 4px;
}

.sidebar__item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 6px;
  font-size: 0.875rem;
  color: #9a9a9a;
  text-decoration: none;
  transition: all 0.15s;
  width: 100%;
  text-align: left;
  background: none;
  border: none;
  cursor: pointer;
}

.sidebar__item:hover { background: #1a1a1a; color: #f0f0f0; }
.sidebar__item.active { background: #1e1e1e; color: #e63946; }

.sidebar__group-toggle { justify-content: flex-start; }

.sidebar__chevron {
  margin-left: auto;
  font-size: 0.7rem;
  color: #555;
}

.sidebar__sub {
  display: flex;
  flex-direction: column;
  gap: 2px;
  margin-top: 2px;
}

.sidebar__item--sub {
  padding-left: 36px;
  font-size: 0.825rem;
}

.sidebar__badge {
  margin-left: auto;
  background: #e63946;
  color: #fff;
  font-size: 0.6rem;
  font-weight: 800;
  min-width: 18px;
  height: 18px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 5px;
}
</style>
