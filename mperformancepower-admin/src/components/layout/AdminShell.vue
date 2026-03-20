<script setup lang="ts">
import AdminSidebar from './AdminSidebar.vue'
import AdminTopbar from './AdminTopbar.vue'
</script>

<template>
  <div class="shell">
    <AdminSidebar />
    <div class="shell__content">
      <AdminTopbar />
      <main class="shell__main">
        <slot />
      </main>
    </div>
  </div>
</template>

<style scoped>
.shell {
  display: flex;
  min-height: 100vh;
}

.shell__content {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.shell__main {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
}
</style>
