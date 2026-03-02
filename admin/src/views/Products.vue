<template>
  <AdminLayout>
    <div>
      <div class="d-flex justify-content-between align-items-center mb-6">
        <h1 class="card-title">Product Management</h1>
        <button @click="showCreateModal = true" class="btn">
          ➕ Add New Product
        </button>
      </div>

      <!-- Products Table -->
      <div class="card">
        <div class="card-header">
          <h2 class="card-title">All Products</h2>
        </div>
        <div class="card-body" style="padding: 0;">
          <table class="table">
            <thead>
              <tr>
                <th>Image</th>
                <th>Name</th>
                <th>Category</th>
                <th>Price</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="product in products" :key="product.id">
                <td>
                  <img 
                    :src="product.imageUrl" 
                    :alt="product.name"
                    style="width: 50px; height: 50px; object-fit: cover; border-radius: 4px;"
                  />
                </td>
                <td>{{ product.name }}</td>
                <td>{{ product.category }}</td>
                <td>${{ product.price.toLocaleString() }}</td>
                <td>
                  <div class="d-flex gap-2">
                    <button @click="editProduct(product)" class="btn btn-sm btn-secondary">
                      ✏️ Edit
                    </button>
                    <button @click="deleteProduct(product.id)" class="btn btn-sm btn-danger">
                      🗑️ Delete
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!products.length">
                <td colspan="5" class="text-center text-muted">
                  No products found. Add your first product to get started.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Create/Edit Modal placeholder -->
      <div v-if="showCreateModal" style="position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000;">
        <div class="card" style="width: 100%; max-width: 500px; margin: 1rem;">
          <div class="card-header d-flex justify-content-between align-items-center">
            <h3 class="card-title">{{ editingProduct ? 'Edit Product' : 'Add New Product' }}</h3>
            <button @click="closeModal" style="background: none; border: none; font-size: 1.5rem;">&times;</button>
          </div>
          <div class="card-body">
            <p class="text-center text-muted">Product creation/editing coming soon...</p>
            <button @click="closeModal" class="btn" style="width: 100%;">Close</button>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import { useAuthStore } from '@/stores/auth'
import type { Product } from '@/types'
import { logError } from '@/services/logger'

const authStore = useAuthStore()
const products = ref<Product[]>([])
const showCreateModal = ref(false)
const editingProduct = ref<Product | null>(null)

const loadProducts = async () => {
  try {
    const response = await fetch('http://localhost:5226/api/v1/products')
    if (response.ok) {
      const data = await response.json()
      products.value = data.data || []
    }
  } catch (error) {
    logError('Failed to load products', error);
  }
}

const editProduct = (product: Product) => {
  editingProduct.value = product
  showCreateModal.value = true
}

const deleteProduct = async (productId: number) => {
  if (!confirm('Are you sure you want to delete this product?')) return
  
  try {
    const response = await fetch(`http://localhost:5226/api/v1/admin/products/${productId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      }
    })
    
    if (response.ok) {
      await loadProducts() // Refresh the list
    } else {
      alert('Failed to delete product')
    }
  } catch (error) {
    logError('Failed to delete product', error, { productId });
    alert('Failed to delete product')
  }
}

const closeModal = () => {
  showCreateModal.value = false
  editingProduct.value = null
}

onMounted(() => {
  loadProducts()
})
</script>