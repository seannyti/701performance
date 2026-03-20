<script setup lang="ts">
const props = defineProps<{ page: number; totalPages: number }>()
const emit = defineEmits<{ (e: 'update:page', val: number): void }>()
</script>

<template>
  <div v-if="totalPages > 1" class="pagination">
    <button :disabled="page <= 1" @click="emit('update:page', page - 1)" class="pagination__btn">← Prev</button>
    <span class="pagination__info">{{ page }} / {{ totalPages }}</span>
    <button :disabled="page >= totalPages" @click="emit('update:page', page + 1)" class="pagination__btn">Next →</button>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: $space-md;
  margin-top: $space-xl;

  &__btn {
    padding: $space-sm $space-lg;
    background: $color-surface;
    border: 1px solid $color-border;
    border-radius: $border-radius;
    color: $color-text;
    font-size: 0.875rem;
    transition: all 0.2s;

    &:hover:not(:disabled) {
      border-color: var(--color-primary);
      color: var(--color-primary);
    }

    &:disabled {
      opacity: 0.3;
      cursor: not-allowed;
    }
  }

  &__info {
    font-size: 0.875rem;
    color: $color-muted;
  }
}
</style>
