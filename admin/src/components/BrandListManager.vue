<template>
  <div class="brand-manager">
    <div class="manager-header">
      <h4>{{ title }}</h4>
      <button @click="addBrand" class="btn btn-sm btn-primary">+ Add Brand</button>
    </div>

    <div v-if="brands.length === 0" class="empty-state">
      <p>No brands added yet. Click "Add Brand" to get started.</p>
    </div>

    <div v-else class="brands-list">
      <div v-for="(brand, index) in brands" :key="index" class="brand-item">
        <div class="brand-number">{{ index + 1 }}</div>
        
        <div class="brand-fields">
          <div class="form-group">
            <label>Brand Name</label>
            <input 
              v-model="brand.name" 
              type="text" 
              class="form-control"
              placeholder="e.g., Vitacci"
              @input="emitUpdate"
            />
          </div>

          <div class="form-group">
            <label>Logo</label>
            <div v-if="brand.logoUrl" class="current-logo">
              <img :src="getMediaUrl(brand.logoUrl)" alt="Brand Logo" />
              <button @click="clearLogo(index)" type="button" class="btn btn-sm btn-danger">Remove</button>
            </div>
            <button @click="openMediaPicker(index)" type="button" class="btn btn-sm btn-secondary">
              {{ brand.logoUrl ? 'Change Logo' : 'Select Logo' }}
            </button>
          </div>

          <div class="form-group">
            <label>Website URL (Optional)</label>
            <input 
              v-model="brand.website" 
              type="url" 
              class="form-control"
              placeholder="https://example.com"
              @input="emitUpdate"
            />
          </div>
        </div>

        <button @click="removeBrand(index)" class="btn-remove" title="Remove Brand">
          ✕
        </button>
      </div>
    </div>

    <!-- Media Picker Modal -->
    <MediaPicker
      v-if="showMediaPicker"
      :is-open="showMediaPicker"
      @close="showMediaPicker = false"
      @select="handleMediaSelection"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import MediaPicker from './MediaPicker.vue';
import { getMediaUrl } from '@/utils/api-config';

interface Brand {
  name: string;
  logoUrl: string;
  website: string;
}

const props = defineProps<{
  modelValue: string;
  title?: string;
}>();

const emit = defineEmits<{
  'update:modelValue': [value: string];
}>();

const brands = ref<Brand[]>([]);
const showMediaPicker = ref(false);
const selectedBrandIndex = ref<number | null>(null);

// Parse initial value
watch(() => props.modelValue, (newValue) => {
  try {
    if (newValue) {
      brands.value = JSON.parse(newValue);
    } else {
      brands.value = [];
    }
  } catch {
    brands.value = [];
  }
}, { immediate: true });

const addBrand = () => {
  brands.value.push({
    name: '',
    logoUrl: '',
    website: ''
  });
  emitUpdate();
};

const removeBrand = (index: number) => {
  brands.value.splice(index, 1);
  emitUpdate();
};

const clearLogo = (index: number) => {
  brands.value[index].logoUrl = '';
  emitUpdate();
};

const openMediaPicker = (index: number) => {
  selectedBrandIndex.value = index;
  showMediaPicker.value = true;
};

const handleMediaSelection = (mediaFile: any) => {
  if (selectedBrandIndex.value !== null) {
    brands.value[selectedBrandIndex.value].logoUrl = mediaFile.filePath;
    emitUpdate();
  }
  showMediaPicker.value = false;
  selectedBrandIndex.value = null;
};

const emitUpdate = () => {
  emit('update:modelValue', JSON.stringify(brands.value));
};
</script>

<style scoped>
.brand-manager {
  border: 1px solid #e1e5e9;
  border-radius: 8px;
  padding: 1.5rem;
  background: #f9fafb;
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
}

.empty-state {
  text-align: center;
  padding: 2rem;
  color: #6b7280;
}

.brands-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.brand-item {
  display: grid;
  grid-template-columns: 40px 1fr auto;
  gap: 1rem;
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  border: 1px solid #e1e5e9;
  align-items: start;
}

.brand-number {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: #4f46e5;
  color: white;
  border-radius: 50%;
  font-weight: 600;
  font-size: 0.875rem;
  flex-shrink: 0;
}

.brand-fields {
  display: grid;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group label {
  font-weight: 600;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
  color: #374151;
}

.form-control {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.938rem;
  transition: border-color 0.2s;
}

.form-control:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.current-logo {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.75rem;
  padding: 0.75rem;
  background: #f9fafb;
  border-radius: 6px;
}

.current-logo img {
  max-width: 120px;
  max-height: 60px;
  object-fit: contain;
}

.btn-remove {
  width: 32px;
  height: 32px;
  border: none;
  background: #ef4444;
  color: white;
  border-radius: 50%;
  cursor: pointer;
  font-size: 1.125rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s;
  flex-shrink: 0;
}

.btn-remove:hover {
  background: #dc2626;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s;
}

.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.875rem;
}

.btn-primary {
  background: #4f46e5;
  color: white;
}

.btn-primary:hover {
  background: #4338ca;
}

.btn-secondary {
  background: #6b7280;
  color: white;
}

.btn-secondary:hover {
  background: #4b5563;
}

.btn-danger {
  background: #ef4444;
  color: white;
}

.btn-danger:hover {
  background: #dc2626;
}
</style>
