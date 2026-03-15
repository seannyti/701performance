<template>
  <AdminLayout>
    <div class="categories-page">
      <div class="page-header">
        <div>
          <h1 class="page-title">Categories</h1>
          <p class="page-subtitle">Manage product categories</p>
        </div>
        <button @click="openCreateModal" class="btn btn-primary">
          <span class="icon">+</span> New Category
        </button>
      </div>

    <div v-if="isLoading" class="loading-overlay">
      <div class="spinner"></div>
    </div>

    <div v-else-if="error" class="error-state">
      <p>{{ error }}</p>
      <button @click="loadCategories" class="btn">Retry</button>
    </div>

    <div v-else class="categories-grid">
      <div v-for="category in categories" :key="category.id" class="category-card">
        <div class="category-header">
          <h3 class="category-name">{{ category.name }}</h3>
          <span :class="['status-badge', category.isActive ? 'active' : 'inactive']">
            {{ category.isActive ? 'Active' : 'Disabled' }}
          </span>
        </div>
        
        <p v-if="category.description" class="category-description">{{ category.description }}</p>
        <p v-else class="category-description text-muted">No description</p>
        
        <div class="category-meta">
          <span class="meta-item">
            <span class="icon">📦</span> {{ getProductCount(category.id) }} products
          </span>
          <span class="meta-item">
            <span class="icon">📅</span> {{ formatDate(category.createdAt) }}
          </span>
        </div>

        <div class="category-actions">
          <button @click="editCategory(category)" class="btn btn-sm btn-secondary">
            Edit
          </button>
          <button 
            @click="toggleCategoryStatus(category)" 
            :class="['btn', 'btn-sm', category.isActive ? 'btn-warning' : 'btn-success']"
            :disabled="isActionLoading(`toggleCategory-${category.id}`)"
          >
            <span v-if="isActionLoading(`toggleCategory-${category.id}`)" class="btn-spinner"></span>
            {{ category.isActive ? 'Disable' : 'Enable' }}
          </button>
          <button 
            @click="confirmDelete(category)" 
            class="btn btn-sm btn-danger"
            :disabled="getProductCount(category.id) > 0"
          >
            Delete
          </button>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingCategory ? 'Edit Category' : 'New Category' }}</h2>
          <button @click="closeModal" class="close-btn">&times;</button>
        </div>
        
        <form @submit.prevent="saveCategory" class="modal-body">
          <div class="form-group">
            <label class="form-label required">Name</label>
            <input 
              v-model="form.name" 
              type="text" 
              class="form-control"
              :class="{ error: formErrors.name }"
              required 
              maxlength="100"
            />
            <span v-if="formErrors.name" class="error-message">{{ formErrors.name }}</span>
          </div>

          <div class="form-group">
            <label class="form-label">Description</label>
            <textarea 
              v-model="form.description" 
              class="form-control"
              rows="3"
              maxlength="500"
            ></textarea>
          </div>

          <div class="form-group">
            <label class="form-label">Image URL</label>
            <input 
              v-model="form.imageUrl" 
              type="url" 
              class="form-control"
              maxlength="500"
              placeholder="https://..."
            />
          </div>

          <div class="form-group">
            <label class="checkbox-label">
              <input v-model="form.isActive" type="checkbox" />
              <span class="checkmark"></span>
              Active
            </label>
          </div>

          <div v-if="formError" class="alert alert-danger">
            {{ formError }}
          </div>
          
          <div class="modal-footer">
            <button type="button" @click="closeModal" class="btn btn-secondary">
              Cancel
            </button>
            <button type="submit" class="btn btn-primary" :disabled="isActionLoading('saveCategory')">
              <span v-if="isActionLoading('saveCategory')" class="btn-spinner"></span>
              {{ isActionLoading('saveCategory') ? 'Saving...' : 'Save Category' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="modal-overlay" @click.self="showDeleteModal = false">
      <div class="modal modal-sm">
        <div class="modal-header">
          <h2>Delete Category</h2>
          <button @click="showDeleteModal = false" class="close-btn">&times;</button>
        </div>
        
        <div class="modal-body">
          <p>Are you sure you want to delete <strong>{{ categoryToDelete?.name }}</strong>?</p>
          <p class="text-muted">This action cannot be undone.</p>
          
          <div class="modal-footer">
            <button @click="showDeleteModal = false" class="btn btn-secondary">
              Cancel
            </button>
            <button @click="deleteCategory" class="btn btn-danger" :disabled="isActionLoading('deleteCategory')">
              <span v-if="isActionLoading('deleteCategory')" class="btn-spinner"></span>
              {{ isActionLoading('deleteCategory') ? 'Deleting...' : 'Delete' }}
            </button>
          </div>
        </div>
      </div>
    </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { useLoadingState } from '@/composables/useLoadingState'
import { API_URL } from '@/utils/api-config'

interface Category {
  id: number
  name: string
  description: string
  imageUrl?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

interface Product {
  id: number
  categoryId: number
  isActive: boolean
}

const authStore = useAuthStore()
const toast = useToast()
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState()

const categories = ref<Category[]>([])
const products = ref<Product[]>([])
const error = ref('')
const showModal = ref(false)
const editingCategory = ref<Category | null>(null)
const formError = ref('')
const showDeleteModal = ref(false)
const categoryToDelete = ref<Category | null>(null)

const form = ref({
  name: '',
  description: '',
  imageUrl: '',
  isActive: true
})

const formErrors = ref({
  name: ''
})

const getProductCount = (categoryId: number) => {
  return products.value.filter(p => p.categoryId === categoryId && p.isActive).length
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const loadCategories = async () => {
  await executeWithLoading(async () => {
    error.value = ''
    
    try {
      const [categoriesRes, productsRes] = await Promise.all([
        fetch(`${API_URL}/categories?includeInactive=true`, {
          headers: { 'Authorization': `Bearer ${authStore.token}` }
        }),
        fetch(`${API_URL}/products`, {
          headers: { 'Authorization': `Bearer ${authStore.token}` }
        })
      ])

      if (!categoriesRes.ok) throw new Error('Failed to load categories')
      if (!productsRes.ok) throw new Error('Failed to load products')

      categories.value = await categoriesRes.json()
      products.value = await productsRes.json()
    } catch (err: any) {
      error.value = err.message || 'Failed to load data'
    }
  })
}

const openCreateModal = () => {
  editingCategory.value = null
  form.value = {
    name: '',
    description: '',
    imageUrl: '',
    isActive: true
  }
  formErrors.value = { name: '' }
  formError.value = ''
  showModal.value = true
}

const editCategory = (category: Category) => {
  editingCategory.value = category
  form.value = {
    name: category.name,
    description: category.description,
    imageUrl: category.imageUrl || '',
    isActive: category.isActive
  }
  formErrors.value = { name: '' }
  formError.value = ''
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
  editingCategory.value = null
}

const validateForm = () => {
  formErrors.value = { name: '' }
  
  if (!form.value.name.trim()) {
    formErrors.value.name = 'Category name is required'
    return false
  }
  
  return true
}

const saveCategory = async () => {
  if (!validateForm()) return
  
  await executeWithLoading(async () => {
    formError.value = ''
    
    try {
      const url = editingCategory.value 
        ? `${API_URL}/categories/${editingCategory.value.id}`
        : `${API_URL}/categories`
      
      const method = editingCategory.value ? 'PUT' : 'POST'
      
      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authStore.token}`
        },
        body: JSON.stringify(form.value)
      })

      if (!response.ok) {
        const errorData = await response.json()
        throw new Error(errorData.message || 'Failed to save category')
      }

      await loadCategories()
      closeModal()
    } catch (err: any) {
      formError.value = err.message || 'Failed to save category'
    }
  }, 'saveCategory')
}

const toggleCategoryStatus = async (category: Category) => {
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/categories/${category.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authStore.token}`
        },
        body: JSON.stringify({
          ...category,
          isActive: !category.isActive
        })
      })

      if (!response.ok) throw new Error('Failed to update category status')

      await loadCategories()
      toast.success('Category status updated')
    } catch (err: any) {
      toast.error(err.message || 'Failed to update category')
    }
  }, `toggleCategory-${category.id}`)
}

const confirmDelete = (category: Category) => {
  categoryToDelete.value = category
  showDeleteModal.value = true
}

const deleteCategory = async () => {
  if (!categoryToDelete.value) return
  
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/categories/${categoryToDelete.value.id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        }
      })

      if (!response.ok) {
        const errorData = await response.json()
        throw new Error(errorData.message || 'Failed to delete category')
      }

      await loadCategories()
      showDeleteModal.value = false
      categoryToDelete.value = null
      toast.deleteSuccess('Category')
    } catch (err: any) {
      toast.error(err.message || 'Failed to delete category')
    }
  }, 'deleteCategory')
}

onMounted(() => {
  loadCategories()
})
</script>

<style scoped>
.categories-page {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
  gap: 1rem;
}

.page-title {
  font-size: 2rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0;
}

.page-subtitle {
  color: #6b7280;
  margin: 0.25rem 0 0 0;
}

.loading-state, .error-state {
  text-align: center;
  padding: 4rem 2rem;
}

.spinner {
  border: 3px solid #f3f4f6;
  border-top: 3px solid #4f46e5;
  border-radius: 50%;
  width: 48px;
  height: 48px;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.categories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

.category-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.5rem;
  transition: all 0.2s;
}

.category-card:hover {
  border-color: #4f46e5;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.1);
}

.category-header {
  display: flex;
  justify-content: space-between;
  align-items: start;
  gap: 1rem;
  margin-bottom: 1rem;
}

.category-name {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  white-space: nowrap;
}

.status-badge.active {
  background: #d1fae5;
  color: #065f46;
}

.status-badge.inactive {
  background: #fee2e2;
  color: #991b1b;
}

.category-description {
  color: #6b7280;
  margin: 0 0 1rem 0;
  line-height: 1.5;
}

.category-meta {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e5e7eb;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #6b7280;
}

.meta-item .icon {
  font-size: 1rem;
}

.category-actions {
  display: flex;
  gap: 0.5rem;
}

.btn {
  padding: 0.625rem 1.25rem;
  border: none;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: #4f46e5;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #4338ca;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}

.btn-secondary {
  background: #f3f4f6;
  color: #374151;
}

.btn-secondary:hover:not(:disabled) {
  background: #e5e7eb;
}

.btn-success {
  background: #10b981;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #059669;
}

.btn-warning {
  background: #f59e0b;
  color: white;
}

.btn-warning:hover:not(:disabled) {
  background: #d97706;
}

.btn-danger {
  background: #ef4444;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #dc2626;
}

.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.813rem;
  flex: 1;
}

.icon {
  display: inline-block;
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

.modal {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
}

.modal-sm {
  max-width: 400px;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0;
}

.close-btn {
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

.close-btn:hover {
  color: #1a1a1a;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: block;
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #374151;
}

.form-label.required::after {
  content: ' *';
  color: #ef4444;
}

.form-control {
  width: 100%;
  padding: 0.625rem 0.875rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.938rem;
  transition: all 0.2s;
}

.form-control:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.form-control.error {
  border-color: #ef4444;
}

.error-message {
  display: block;
  color: #ef4444;
  font-size: 0.813rem;
  margin-top: 0.25rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  user-select: none;
}

.checkbox-label input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.alert {
  padding: 0.75rem 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.alert-danger {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.modal-footer {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  padding-top: 1.5rem;
  border-top: 1px solid #e5e7eb;
}

.text-muted {
  color: #9ca3af;
  font-size: 0.875rem;
}
</style>
