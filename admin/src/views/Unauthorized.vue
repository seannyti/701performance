<template>
  <div class="error-container" style="min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #f8fafc;">
    <div class="card" style="width: 100%; max-width: 500px;">
      <div class="card-body text-center">
        <div style="font-size: 4rem; margin-bottom: 1rem;">🚫</div>
        <h1 class="card-title">Access Denied</h1>
        <p class="text-muted mb-6">
          You don't have sufficient permissions to access this area of the admin dashboard.
        </p>
        
        <div class="d-flex gap-4 justify-content-center">
          <router-link to="/" class="btn">
            🏠 Go to Dashboard
          </router-link>
          <a href="http://localhost:5173" class="btn btn-secondary">
            ← Back to Main Site
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// No additional logic needed for this simple error page
</script>