<template>
  <div class="feature-manager">
    <div class="manager-header">
      <h4>{{ title }}</h4>
      <button @click="openAddModal" class="btn btn-primary btn-sm">
        <span class="icon">➕</span>
        Add Item
      </button>
    </div>

    <div class="feature-list">
      <div v-for="(item, index) in items" :key="index" class="feature-item">
        <div class="feature-icon-display">{{ item.icon }}</div>
        <div class="feature-content">
          <h5 class="feature-title">{{ item.title }}</h5>
          <p class="feature-description">{{ item.description }}</p>
        </div>
        <div class="feature-actions">
          <button @click="openEditModal(index)" class="btn-icon" title="Edit">
            ✏️
          </button>
          <button @click="deleteItem(index)" class="btn-icon danger" title="Delete">
            🗑️
          </button>
        </div>
      </div>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ editIndex !== null ? 'Edit' : 'Add' }} Item</h3>
          <button @click="closeModal" class="modal-close">×</button>
        </div>
        
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label required">Icon/Emoji</label>
            <div class="icon-selector">
              <input 
                v-model="editForm.icon" 
                type="text" 
                class="form-control icon-input"
                placeholder="Click an emoji below or paste one"
                maxlength="2"
              />
              <div class="emoji-grid">
                <button 
                  v-for="emoji in commonEmojis" 
                  :key="emoji" 
                  @click="editForm.icon = emoji"
                  class="emoji-btn"
                  type="button"
                  :class="{ selected: editForm.icon === emoji }"
                >
                  {{ emoji }}
                </button>
              </div>
            </div>
            <p class="form-hint">Choose an emoji or enter your own</p>
          </div>

          <div class="form-group">
            <label class="form-label required">Title</label>
            <input 
              v-model="editForm.title" 
              type="text" 
              class="form-control"
              placeholder="e.g., Quality First"
            />
          </div>

          <div class="form-group">
            <label class="form-label required">Description</label>
            <textarea 
              v-model="editForm.description" 
              class="form-control"
              rows="3"
              placeholder="Brief description..."
            ></textarea>
          </div>
        </div>

        <div class="modal-footer">
          <button @click="closeModal" class="btn btn-outline">Cancel</button>
          <button @click="saveItem" class="btn btn-primary" :disabled="!isFormValid">
            💾 Save
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';

interface FeatureItem {
  icon: string;
  title: string;
  description: string;
}

interface Props {
  modelValue: string;
  title?: string;
}

interface Emits {
  (e: 'update:modelValue', value: string): void;
}

const props = withDefaults(defineProps<Props>(), {
  title: 'Features'
});

const emit = defineEmits<Emits>();

const commonEmojis = [
  '⭐', '🎯', '🚀', '💎', '🔧', '🛡️',
  '🌟', '⚡', '🎨', '📱', '💡', '🔥',
  '✨', '🏆', '💪', '👍', '❤️', '🌈',
  '🛍️', '📦', '🎁', '🤝', '🌍', '♻️',
  '📊', '📈', '💰', '🎓', '🏅', '🔔'
];

const items = ref<FeatureItem[]>([]);
const showModal = ref(false);
const editIndex = ref<number | null>(null);

const editForm = ref<FeatureItem>({
  icon: '',
  title: '',
  description: ''
});

// Parse JSON from prop
watch(() => props.modelValue, (newValue) => {
  try {
    const parsed = JSON.parse(newValue || '[]');
    items.value = Array.isArray(parsed) ? parsed : [];
  } catch {
    items.value = [];
  }
}, { immediate: true });

// Emit changes
const emitChanges = () => {
  emit('update:modelValue', JSON.stringify(items.value));
};

const isFormValid = computed(() => {
  return editForm.value.icon.trim() && 
         editForm.value.title.trim() && 
         editForm.value.description.trim();
});

const openAddModal = () => {
  editIndex.value = null;
  editForm.value = { icon: '', title: '', description: '' };
  showModal.value = true;
};

const openEditModal = (index: number) => {
  editIndex.value = index;
  editForm.value = { ...items.value[index] };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  editIndex.value = null;
  editForm.value = { icon: '', title: '', description: '' };
};

const saveItem = () => {
  if (!isFormValid.value) return;

  if (editIndex.value !== null) {
    items.value[editIndex.value] = { ...editForm.value };
  } else {
    items.value.push({ ...editForm.value });
  }

  emitChanges();
  closeModal();
};

const deleteItem = (index: number) => {
  if (confirm('Are you sure you want to delete this item?')) {
    items.value.splice(index, 1);
    emitChanges();
  }
};
</script>

<style scoped>
.feature-manager {
  margin-bottom: 2rem;
}

.manager-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.manager-header h4 {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
}

.feature-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.feature-item {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1.25rem;
  display: flex;
  align-items: flex-start;
  gap: 1rem;
}

.feature-icon-display {
  font-size: 2.5rem;
  line-height: 1;
  flex-shrink: 0;
}

.feature-content {
  flex: 1;
  min-width: 0;
}

.feature-title {
  margin: 0 0 0.5rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: #1f2937;
}

.feature-description {
  margin: 0;
  font-size: 0.875rem;
  color: #6b7280;
  line-height: 1.5;
}

.feature-actions {
  display: flex;
  gap: 0.5rem;
  flex-shrink: 0;
}

.btn-icon {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  transition: background 0.2s;
}

.btn-icon:hover {
  background: #f3f4f6;
}

.btn-icon.danger:hover {
  background: #fee2e2;
}

.icon-selector {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.icon-input {
  font-size: 2rem;
  text-align: center;
  padding: 1rem;
}

.emoji-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(48px, 1fr));
  gap: 0.5rem;
  padding: 1rem;
  background: #f9fafb;
  border-radius: 8px;
  max-height: 200px;
  overflow-y: auto;
}

.emoji-btn {
  font-size: 1.5rem;
  padding: 0.5rem;
  background: white;
  border: 2px solid #e5e7eb;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.emoji-btn:hover {
  border-color: #4f46e5;
  transform: scale(1.1);
}

.emoji-btn.selected {
  border-color: #4f46e5;
  background: #eef2ff;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal-content {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
}

.modal-close {
  background: none;
  border: none;
  font-size: 2rem;
  line-height: 1;
  color: #6b7280;
  cursor: pointer;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-close:hover {
  color: #1f2937;
}

.modal-body {
  padding: 1.5rem;
  overflow-y: auto;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1.5rem;
  border-top: 1px solid #e5e7eb;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group:last-child {
  margin-bottom: 0;
}

.form-label {
  display: block;
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #374151;
  font-size: 0.875rem;
}

.form-label.required::after {
  content: ' *';
  color: #ef4444;
}

.form-control {
  width: 100%;
  padding: 0.625rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  transition: all 0.2s;
}

.form-control:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.form-hint {
  font-size: 0.75rem;
  color: #6b7280;
  margin: 0.375rem 0 0 0;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
  font-size: 0.875rem;
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
}

.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.813rem;
}

.btn-primary {
  background: #4f46e5;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #4338ca;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-outline {
  background: white;
  color: #4f46e5;
  border: 1px solid #4f46e5;
}

.btn-outline:hover {
  background: #eef2ff;
}

.icon {
  display: inline-block;
}
</style>
