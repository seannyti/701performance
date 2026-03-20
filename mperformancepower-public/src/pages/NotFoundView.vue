<template>
  <div class="not-found">
    <h1>404</h1>
    <p>Page not found.</p>
    <RouterLink to="/">← Back to Home</RouterLink>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;

.not-found {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
  gap: $space-md;
  text-align: center;

  h1 { font-size: 6rem; font-weight: 800; color: var(--color-primary); }
  p { color: $color-muted; }
  a { color: $color-text; text-decoration: underline; }
}
</style>
