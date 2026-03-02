<template>
  <div id="app">
    <!-- Header component - hidden during maintenance and on special pages -->
    <Header v-if="!hideHeaderFooter" />
    
    <!-- Main content area -->
    <main class="main-content" :class="{ 'full-viewport': hideHeaderFooter }">
      <router-view v-slot="{ Component, route }">
        <Transition name="fade" mode="out-in">
          <component :is="Component" :key="route.path" />
        </Transition>
      </router-view>
    </main>
    
    <!-- Footer component - hidden during maintenance and on special pages -->
    <Footer v-if="!hideHeaderFooter" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Header from './components/Header.vue';
import Footer from './components/Footer.vue';

const route = useRoute()

// Hide header/footer only on maintenance, login, and signup pages
const hideHeaderFooter = computed(() => {
  const specialPages = ['/maintenance', '/login', '/signup']
  return specialPages.includes(route.path)
})
</script>

<style>
/* Global styles */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html {
  font-size: 16px;
}

body {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  line-height: 1.6;
  color: #333;
  background-color: #f8f9fa;
}

#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.main-content {
  flex: 1;
  padding-top: 2rem;
  padding-bottom: 2rem;
}

/* Full viewport for pages without header/footer */
.main-content.full-viewport {
  padding: 0;
  min-height: 100vh;
}

/* Container utility */
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

/* Grid utilities */
.grid {
  display: grid;
  gap: 2rem;
}

.grid-1 { grid-template-columns: 1fr; }
.grid-2 { grid-template-columns: repeat(2, 1fr); }
.grid-3 { grid-template-columns: repeat(3, 1fr); }
.grid-4 { grid-template-columns: repeat(4, 1fr); }

@media (max-width: 1024px) {
  .grid-4 { grid-template-columns: repeat(3, 1fr); }
}

@media (max-width: 768px) {
  .grid-3,
  .grid-4 { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 480px) {
  .grid-2,
  .grid-3,
  .grid-4 { grid-template-columns: 1fr; }
}

/* Typography utilities */
.text-center { text-align: center; }
.text-left { text-align: left; }
.text-right { text-align: right; }

.font-bold { font-weight: bold; }
.font-semibold { font-weight: 600; }
.font-medium { font-weight: 500; }

/* Spacing utilities */
.mb-1 { margin-bottom: 0.5rem; }
.mb-2 { margin-bottom: 1rem; }
.mb-3 { margin-bottom: 1.5rem; }
.mb-4 { margin-bottom: 2rem; }

.mt-1 { margin-top: 0.5rem; }
.mt-2 { margin-top: 1rem; }
.mt-3 { margin-top: 1.5rem; }
.mt-4 { margin-top: 2rem; }

.p-1 { padding: 0.5rem; }
.p-2 { padding: 1rem; }
.p-3 { padding: 1.5rem; }
.p-4 { padding: 2rem; }

/* Button styles */
.btn {
  display: inline-block;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  text-decoration: none;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 1rem;
}

.btn-primary {
  background-color: #ff6b35;
  color: white;
}

.btn-primary:hover {
  background-color: #e55a2b;
  transform: translateY(-1px);
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background-color: #545b62;
}

.btn-outline {
  background-color: transparent;
  border: 2px solid #ff6b35;
  color: #ff6b35;
}

.btn-outline:hover {
  background-color: #ff6b35;
  color: white;
}

/* Form styles */
.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #333;
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: 2px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s ease;
}

.form-control:focus {
  outline: none;
  border-color: #ff6b35;
}

.form-control.error {
  border-color: #dc3545;
}

/* Loading and error states */
.loading {
  text-align: center;
  padding: 2rem;
  color: #666;
}

.error {
  background-color: #f8d7da;
  color: #721c24;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  border: 1px solid #f5c6cb;
}

.success {
  background-color: #d4edda;
  color: #155724;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  border: 1px solid #c3e6cb;
}

/* Section styles */
.section {
  padding: 3rem 0;
}

.section-title {
  font-size: 2.5rem;
  font-weight: bold;
  text-align: center;
  margin-bottom: 1rem;
  color: #333;
}

.section-subtitle {
  font-size: 1.25rem;
  text-align: center;
  color: #666;
  margin-bottom: 3rem;
  max-width: 600px;
  margin-left: auto;
  margin-right: auto;
}

/* Page transition animations */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Responsive container adjustments */
@media (max-width: 1200px) {
  .container {
    max-width: 1000px;
  }
}

@media (max-width: 768px) {
  .main-content {
    padding-top: 1rem;
    padding-bottom: 1rem;
  }

  .section {
    padding: 2rem 0;
  }

  .section-title {
    font-size: 2rem;
  }

  .section-subtitle {
    font-size: 1.1rem;
  }
}

@media (max-width: 480px) {
  .container {
    padding: 0 0.75rem;
  }

  .section-title {
    font-size: 1.75rem;
  }
}
</style>