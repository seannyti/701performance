<template>
  <div class="products">
    <div class="container">
      <!-- Page Header -->
      <div class="page-header">
        <h1 class="page-title">{{ getSetting('products_title', 'Our Products') }}</h1>
        <p class="page-subtitle">
          {{ getSetting('products_subtitle', 'Explore our complete collection of powersports vehicles and gear') }}
        </p>
      </div>

      <!-- Products Layout -->
      <div class="products-layout">
        <!-- Sidebar Filters -->
        <aside class="filters-sidebar">
          <div class="filter-card">
            <h3 class="sidebar-title">Filters</h3>

            <!-- Search Filter -->
            <div class="filter-section">
              <label class="filter-label">Search</label>
              <input
                v-model="searchQuery"
                type="text"
                class="filter-input"
                placeholder="Search products..."
              />
            </div>

            <!-- Category Filter -->
            <div class="filter-section">
              <label class="filter-label">Category</label>
              <select v-model="selectedCategory" class="filter-select">
                <option value="">All Categories</option>
                <option v-for="category in categories" :key="category.id" :value="category.name">
                  {{ category.name }}
                </option>
              </select>
            </div>

            <!-- Sort By Filter -->
            <div class="filter-section">
              <label class="filter-label">Sort By</label>
              <select v-model="sortBy" class="filter-select">
                <option value="featured">Featured</option>
                <option value="price-low">Price: Low to High</option>
                <option value="price-high">Price: High to Low</option>
                <option value="name">Name: A to Z</option>
              </select>
            </div>

            <!-- Reset Filters Button -->
            <button @click="resetFilters" class="reset-btn">
              Reset Filters
            </button>
          </div>
        </aside>

        <!-- Products Grid -->
        <main class="products-main">
          <!-- Loading State -->
          <div v-if="loading" class="loading">
            <p>Loading products...</p>
          </div>

          <!-- Error State -->
          <div v-else-if="error" class="error">
            <p>{{ error }}</p>
            <button @click="loadProducts" class="btn btn-primary">
              Try Again
            </button>
          </div>

          <!-- Products -->
          <div v-else-if="sortedAndFilteredProducts.length > 0">
            <div class="products-header">
              <p class="products-count">
                {{ sortedAndFilteredProducts.length }} product{{ sortedAndFilteredProducts.length !== 1 ? 's' : '' }} found
              </p>
            </div>
            
            <div class="products-grid">
              <ProductCard
                v-for="product in sortedAndFilteredProducts"
                :key="product.id"
                :product="product"
                @view-details="handleViewDetails"
              />
            </div>
          </div>

          <!-- Empty State -->
          <div v-else class="empty-state">
            <div class="empty-content">
              <h3>No products found</h3>
              <p v-if="selectedCategory || searchQuery">
                Try adjusting your filters to find what you're looking for.
              </p>
              <p v-else>
                No products are currently available.
              </p>
              <button 
                v-if="selectedCategory || searchQuery" 
                @click="resetFilters"
                class="btn btn-primary"
              >
                Reset Filters
              </button>
            </div>
          </div>
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import ProductCard from '@/components/ProductCard.vue';
import { productService, categoryService } from '@/services/api';
import type { Product, Category } from '@/types';
import { useSettings } from '@/composables/useSettings';

const { getSetting } = useSettings();

// Reactive data
const products = ref<Product[]>([]);
const categories = ref<Category[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);
const selectedCategory = ref<string>('');
const searchQuery = ref<string>('');
const sortBy = ref<string>('featured');

// Computed properties
const sortedAndFilteredProducts = computed(() => {
  let filtered = products.value;

  // Filter by category
  if (selectedCategory.value) {
    filtered = filtered.filter(product => 
      product.category.toLowerCase() === selectedCategory.value.toLowerCase()
    );
  }

  // Filter by search query
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    filtered = filtered.filter(product =>
      product.name.toLowerCase().includes(query) ||
      product.description.toLowerCase().includes(query) ||
      product.category.toLowerCase().includes(query)
    );
  }

  // Sort products
  const sorted = [...filtered];
  switch (sortBy.value) {
    case 'price-low':
      sorted.sort((a, b) => a.price - b.price);
      break;
    case 'price-high':
      sorted.sort((a, b) => b.price - a.price);
      break;
    case 'name':
      sorted.sort((a, b) => a.name.localeCompare(b.name));
      break;
    case 'featured':
    default:
      sorted.sort((a, b) => {
        if (a.isFeatured && !b.isFeatured) return -1;
        if (!a.isFeatured && b.isFeatured) return 1;
        return 0;
      });
      break;
  }

  return sorted;
});

// Methods
const loadProducts = async () => {
  try {
    loading.value = true;
    error.value = null;
    const [productsData, categoriesData] = await Promise.all([
      productService.getAllProducts(),
      categoryService.getAllCategories()
    ]);
    products.value = productsData;
    categories.value = categoriesData;
  } catch (err) {
    error.value = 'Failed to load products. Please try again later.';
  } finally {
    loading.value = false;
  }
};

const resetFilters = () => {
  selectedCategory.value = '';
  searchQuery.value = '';
  sortBy.value = 'featured';
};

const handleViewDetails = (product: Product) => {
  // For now, just show an alert with product details
  // In a real app, you would navigate to a product detail page
  alert(`Product: ${product.name}\nPrice: $${product.price.toFixed(2)}\nCategory: ${product.category}`);
};

// Load data on component mount
onMounted(() => {
  loadProducts();
});
</script>

<style scoped>
.products {
  padding: 1rem 0 2rem;
}

.page-header {
  text-align: center;
  margin-bottom: 3rem;
}

.page-title {
  font-size: 2.5rem;
  font-weight: bold;
  color: var(--text-dark);
  margin-bottom: 1rem;
}

.page-subtitle {
  font-size: 1.25rem;
  color: var(--text-light);
  max-width: 600px;
  margin: 0 auto;
}

/* Products Layout */
.products-layout {
  display: grid;
  grid-template-columns: 300px 1fr;
  gap: 2rem;
  align-items: start;
}

/* Sidebar Filters */
.filters-sidebar {
  position: sticky;
  top: 1rem;
}

.filter-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.sidebar-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-dark);
  margin-bottom: 1.5rem;
}

.filter-section {
  margin-bottom: 1.5rem;
}

.filter-label {
  display: block;
  font-size: 1rem;
  font-weight: 600;
  color: var(--text-dark);
  margin-bottom: 0.5rem;
}

.filter-input,
.filter-select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.95rem;
  transition: all 0.3s ease;
}

.filter-input:focus,
.filter-select:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(255, 107, 53, 0.1);
}

.filter-input::placeholder {
  color: #9ca3af;
}

.reset-btn {
  width: 100%;
  padding: 0.75rem;
  background: #f3f4f6;
  color: var(--text-dark);
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.reset-btn:hover {
  background: #e5e7eb;
}

/* Products Main */
.products-main {
  min-height: 400px;
}

.products-header {
  margin-bottom: 1.5rem;
}

.products-count {
  color: var(--primary-color);
  font-size: 1.1rem;
  font-weight: 600;
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
}

.loading,
.error {
  text-align: center;
  padding: 3rem;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.empty-content {
  max-width: 400px;
  margin: 0 auto;
}

.empty-content h3 {
  font-size: 1.5rem;
  color: var(--text-dark);
  margin-bottom: 1rem;
}

.empty-content p {
  color: var(--text-light);
  margin-bottom: 2rem;
}

/* Tablet responsive */
@media (max-width: 1024px) {
  .products-layout {
    grid-template-columns: 250px 1fr;
    gap: 1.5rem;
  }

  .products-grid {
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  }
}

/* Mobile responsive */
@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }

  .page-subtitle {
    font-size: 1.1rem;
  }

  .products-layout {
    grid-template-columns: 1fr;
  }

  .filters-sidebar {
    position: relative;
    top: 0;
    margin-bottom: 2rem;
  }

  .products-grid {
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  }
}

@media (max-width: 480px) {
  .page-title {
    font-size: 1.75rem;
  }

  .products-grid {
    grid-template-columns: 1fr;
  }

  .filter-card {
    padding: 1.25rem;
  }

  .sidebar-title {
    font-size: 1.25rem;
  }

  .empty-state {
    padding: 2rem 1rem;
  }
}
</style>