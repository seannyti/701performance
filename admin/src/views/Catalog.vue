<template>
  <AdminLayout>
    <div class="catalog-page">
      <!-- Page Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Product Catalog</h1>
          <p class="page-subtitle">Manage products and categories</p>
        </div>
        <button 
          v-if="activeTab === 'categories'" 
          @click="openCategoryModal" 
          class="btn btn-primary"
        >
          <span class="icon">+</span> New Category
        </button>
        <button 
          v-if="activeTab === 'products'" 
          @click="openProductModal" 
          class="btn btn-primary"
        >
          <span class="icon">+</span> New Product
        </button>
      </div>

      <!-- Tab Navigation -->
      <div class="tabs">
        <button 
          :class="['tab', { active: activeTab === 'categories' }]"
          @click="activeTab = 'categories'"
        >
          📁 Categories ({{ categories.length }})
        </button>
        <button 
          :class="['tab', { active: activeTab === 'products' }]"
          @click="activeTab = 'products'"
        >
          📦 Products ({{ products.length }})
        </button>
        <button
          :class="['tab', { active: activeTab === 'inventory' }]"
          @click="activeTab = 'inventory'"
        >
          📊 Inventory
          <span v-if="inventoryAlertCount > 0" class="tab-badge">{{ inventoryAlertCount }}⚠️</span>
        </button>
        <button
          :class="['tab', { active: activeTab === 'reports' }]"
          @click="activeTab = 'reports'"
        >
          🖨️ Reports
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <p>{{ error }}</p>
        <button @click="loadData" class="btn btn-primary">Retry</button>
      </div>

      <!-- Categories Tab -->
      <div v-show="activeTab === 'categories' && !loading && !error" class="tab-content">
        <div class="categories-grid">
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
              >
                {{ category.isActive ? 'Disable' : 'Enable' }}
              </button>
              <button 
                @click="confirmDeleteCategory(category)" 
                class="btn btn-sm btn-danger"
                :disabled="getProductCount(category.id) > 0"
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Products Tab -->
      <div v-show="activeTab === 'products' && !loading && !error" class="tab-content">
        <!-- Category Filter -->
        <div class="filters">
          <label class="filter-label">Filter by Category:</label>
          <select v-model="selectedCategoryFilter" class="form-control filter-select">
            <option value="">All Categories</option>
            <option v-for="cat in activeCategories" :key="cat.id" :value="cat.id">
              {{ cat.name }}
            </option>
          </select>
        </div>

        <!-- Products Table -->
        <div class="card">
          <table class="table">
            <thead>
              <tr>
                <th>Image</th>
                <th>Name</th>
                <th>Category</th>
                <th>Price</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="product in filteredProducts" :key="product.id">
                <td>
                  <img 
                    :src="getImageUrl(product.imageUrl)" 
                    :alt="product.name"
                    class="product-image"
                  />
                </td>
                <td>
                  <div class="product-name-cell">
                    {{ product.name }}
                    <span v-if="product.isFeatured" class="featured-badge" title="Featured on homepage">⭐</span>
                  </div>
                </td>
                <td>{{ getCategoryName(product.categoryId) }}</td>
                <td class="price-cell">${{ product.price.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}</td>
                <td>
                  <span :class="['status-badge', 'status-badge-sm', product.isActive ? 'active' : 'inactive']">
                    {{ product.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td>
                  <div class="action-buttons">
                    <button @click="editProduct(product)" class="btn btn-sm btn-secondary">
                      Edit
                    </button>
                    <button @click="confirmDeleteProduct(product)" class="btn btn-sm btn-danger">
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!filteredProducts.length">
                <td colspan="6" class="empty-state">
                  No products found. Add your first product to get started.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Inventory Tab -->
      <div v-show="activeTab === 'inventory' && !loading && !error" class="tab-content">
        <!-- Summary Cards -->
        <div class="inventory-summary">
          <div class="summary-card">
            <div class="summary-value">{{ products.length }}</div>
            <div class="summary-label">Total Products</div>
          </div>
          <div class="summary-card success">
            <div class="summary-value">{{ inStockCount }}</div>
            <div class="summary-label">In Stock</div>
          </div>
          <div class="summary-card warning">
            <div class="summary-value">{{ lowStockCount }}</div>
            <div class="summary-label">Low Stock</div>
          </div>
          <div class="summary-card danger">
            <div class="summary-value">{{ outOfStockCount }}</div>
            <div class="summary-label">Out of Stock</div>
          </div>
          <div class="summary-card info">
            <div class="summary-value">${{ totalInventoryValue.toLocaleString('en-US', { maximumFractionDigits: 0 }) }}</div>
            <div class="summary-label">Total Sell Value</div>
          </div>
        </div>

        <!-- Filters -->
        <div class="filters">
          <input v-model="inventorySearch" type="text" class="form-control" placeholder="Search products or SKU..." style="max-width: 240px;" />
          <select v-model="inventoryCategoryFilter" class="form-control filter-select">
            <option value="">All Categories</option>
            <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
          </select>
          <select v-model="inventoryStockFilter" class="form-control filter-select">
            <option value="">All Stock Levels</option>
            <option value="in">In Stock</option>
            <option value="low">Low Stock</option>
            <option value="out">Out of Stock</option>
          </select>
        </div>

        <!-- Inventory Table -->
        <div class="card">
          <table class="table">
            <thead>
              <tr>
                <th>Product</th>
                <th>SKU</th>
                <th>Category</th>
                <th>Cost</th>
                <th>Sell Price</th>
                <th>Margin</th>
                <th>Stock Qty</th>
                <th>Low Alert</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="product in filteredInventoryProducts" :key="product.id" :class="getInventoryRowClass(product)">
                <td>
                  <span class="product-name-cell">{{ product.name }}</span>
                  <span v-if="!product.isActive" class="inactive-tag">Inactive</span>
                </td>
                <td>
                  <input
                    v-if="inventoryEdits[product.id]"
                    v-model="inventoryEdits[product.id].sku"
                    type="text"
                    class="inventory-input"
                    placeholder="—"
                    @blur="saveInventoryRow(product.id)"
                  />
                </td>
                <td>{{ getCategoryName(product.categoryId) }}</td>
                <td>
                  <div v-if="inventoryEdits[product.id]" class="input-with-prefix input-sm">
                    <span class="input-prefix">$</span>
                    <input
                      v-model.number="inventoryEdits[product.id].costPrice"
                      type="number" step="0.01" min="0"
                      class="inventory-input with-prefix"
                      placeholder="—"
                      @blur="saveInventoryRow(product.id)"
                    />
                  </div>
                </td>
                <td>${{ product.price.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}</td>
                <td>
                  <span v-if="inventoryEdits[product.id]?.costPrice" :class="['margin-badge', getMarginClass(product.price, inventoryEdits[product.id].costPrice)]">
                    {{ getMarginLabel(product.price, inventoryEdits[product.id].costPrice) }}
                  </span>
                  <span v-else class="text-muted">—</span>
                </td>
                <td>
                  <input
                    v-if="inventoryEdits[product.id]"
                    v-model.number="inventoryEdits[product.id].stockQuantity"
                    type="number" min="0"
                    class="inventory-input inventory-stock-input"
                    @blur="saveInventoryRow(product.id)"
                  />
                </td>
                <td>
                  <input
                    v-if="inventoryEdits[product.id]"
                    v-model.number="inventoryEdits[product.id].lowStockThreshold"
                    type="number" min="0"
                    class="inventory-input"
                    @blur="saveInventoryRow(product.id)"
                  />
                </td>
                <td>
                  <span :class="['stock-badge', 'stock-' + getStockStatus(product.id)]">
                    {{ getStockStatusLabel(product.id) }}
                  </span>
                </td>
              </tr>
              <tr v-if="!filteredInventoryProducts.length">
                <td colspan="9" class="empty-state">No products match your filters.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Reports Tab -->
      <div v-show="activeTab === 'reports' && !loading && !error" class="tab-content">
        <div class="reports-toolbar no-print">
          <div class="form-row" style="align-items: flex-end; gap: 1rem; flex-wrap: wrap;">
            <div class="form-group" style="min-width: 140px;">
              <label class="form-label">Month</label>
              <select v-model="reportMonth" class="form-control">
                <option v-for="(name, idx) in monthNames" :key="idx" :value="idx + 1">{{ name }}</option>
              </select>
            </div>
            <div class="form-group" style="min-width: 100px;">
              <label class="form-label">Year</label>
              <input v-model.number="reportYear" type="number" class="form-control" min="2020" max="2099" />
            </div>
            <div class="form-group" style="min-width: 220px;">
              <label class="form-label">Report Type</label>
              <select v-model="reportType" class="form-control">
                <option value="inventory">Inventory Snapshot</option>
                <option value="lowstock">Low Stock / Out of Stock</option>
                <option value="catalog">Full Product Catalog</option>
                <option value="value">Inventory Value Summary</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label" style="visibility:hidden;">Action</label>
              <button @click="printReport" class="btn btn-primary">🖨️ Print / Save PDF</button>
            </div>
          </div>
        </div>

        <!-- Print Preview -->
        <div id="report-print-area" class="report-preview">
          <div class="report-header">
            <h1 class="report-title">701 Performance Power</h1>
            <h2 class="report-subtitle">{{ reportTitle }}</h2>
            <p class="report-meta">Period: {{ monthNames[reportMonth - 1] }} {{ reportYear }}
              &nbsp;&bull;&nbsp; Generated: {{ new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) }}
            </p>
          </div>

          <!-- Inventory Snapshot -->
          <div v-if="reportType === 'inventory'">
            <div class="report-summary-row">
              <div class="report-stat"><span class="stat-val">{{ products.length }}</span><span class="stat-lbl">Total Products</span></div>
              <div class="report-stat success"><span class="stat-val">{{ inStockCount }}</span><span class="stat-lbl">In Stock</span></div>
              <div class="report-stat warning"><span class="stat-val">{{ lowStockCount }}</span><span class="stat-lbl">Low Stock</span></div>
              <div class="report-stat danger"><span class="stat-val">{{ outOfStockCount }}</span><span class="stat-lbl">Out of Stock</span></div>
              <div class="report-stat"><span class="stat-val">${{ totalInventoryValue.toLocaleString('en-US', { maximumFractionDigits: 0 }) }}</span><span class="stat-lbl">Total Sell Value</span></div>
            </div>
            <table class="report-table">
              <thead><tr><th>Product</th><th>SKU</th><th>Category</th><th>Sell Price</th><th>Cost</th><th>Margin</th><th>Stock</th><th>Status</th></tr></thead>
              <tbody>
                <tr v-for="p in products" :key="p.id">
                  <td>{{ p.name }}</td>
                  <td>{{ p.sku || '—' }}</td>
                  <td>{{ getCategoryName(p.categoryId) }}</td>
                  <td>${{ p.price.toFixed(2) }}</td>
                  <td>{{ p.costPrice ? '$' + Number(p.costPrice).toFixed(2) : '—' }}</td>
                  <td>{{ getMarginLabel(p.price, p.costPrice ?? null) }}</td>
                  <td>{{ p.stockQuantity }}</td>
                  <td>{{ getStockStatusLabel(p.id) }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Low Stock Report -->
          <div v-else-if="reportType === 'lowstock'">
            <p v-if="!lowStockProducts.length" class="report-empty">All products are adequately stocked. No action required.</p>
            <table v-else class="report-table">
              <thead><tr><th>Product</th><th>SKU</th><th>Category</th><th>Stock Qty</th><th>Alert At</th><th>Sell Price</th><th>Status</th></tr></thead>
              <tbody>
                <tr v-for="p in lowStockProducts" :key="p.id" :class="p.stockQuantity === 0 ? 'report-row-danger' : 'report-row-warning'">
                  <td>{{ p.name }}</td>
                  <td>{{ p.sku || '—' }}</td>
                  <td>{{ getCategoryName(p.categoryId) }}</td>
                  <td><strong>{{ p.stockQuantity }}</strong></td>
                  <td>{{ p.lowStockThreshold }}</td>
                  <td>${{ p.price.toFixed(2) }}</td>
                  <td>{{ p.stockQuantity === 0 ? '🔴 Out of Stock' : '🟡 Low Stock' }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Full Catalog -->
          <div v-else-if="reportType === 'catalog'">
            <table class="report-table">
              <thead><tr><th>Product</th><th>SKU</th><th>Category</th><th>Price</th><th>Status</th><th>Featured</th></tr></thead>
              <tbody>
                <tr v-for="p in products" :key="p.id">
                  <td>{{ p.name }}</td>
                  <td>{{ p.sku || '—' }}</td>
                  <td>{{ getCategoryName(p.categoryId) }}</td>
                  <td>${{ p.price.toFixed(2) }}</td>
                  <td>{{ p.isActive ? 'Active' : 'Inactive' }}</td>
                  <td>{{ p.isFeatured ? '⭐ Yes' : 'No' }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Value Summary -->
          <div v-else-if="reportType === 'value'">
            <div class="report-summary-row">
              <div class="report-stat"><span class="stat-val">${{ totalInventoryValue.toLocaleString('en-US', { maximumFractionDigits: 0 }) }}</span><span class="stat-lbl">Total Sell Value</span></div>
              <div class="report-stat"><span class="stat-val">${{ totalCostValue.toLocaleString('en-US', { maximumFractionDigits: 0 }) }}</span><span class="stat-lbl">Total Cost Value</span></div>
              <div class="report-stat success"><span class="stat-val">${{ (totalInventoryValue - totalCostValue).toLocaleString('en-US', { maximumFractionDigits: 0 }) }}</span><span class="stat-lbl">Est. Gross Profit</span></div>
            </div>
            <table class="report-table">
              <thead><tr><th>Product</th><th>SKU</th><th>Stock</th><th>Unit Cost</th><th>Unit Price</th><th>Stock Cost</th><th>Stock Value</th><th>Potential Profit</th></tr></thead>
              <tbody>
                <tr v-for="p in products.filter(x => x.stockQuantity > 0)" :key="p.id">
                  <td>{{ p.name }}</td>
                  <td>{{ p.sku || '—' }}</td>
                  <td>{{ p.stockQuantity }}</td>
                  <td>{{ p.costPrice ? '$' + Number(p.costPrice).toFixed(2) : '—' }}</td>
                  <td>${{ p.price.toFixed(2) }}</td>
                  <td>{{ p.costPrice ? '$' + (Number(p.costPrice) * p.stockQuantity).toFixed(2) : '—' }}</td>
                  <td>${{ (p.price * p.stockQuantity).toFixed(2) }}</td>
                  <td>{{ p.costPrice ? '$' + ((p.price - Number(p.costPrice)) * p.stockQuantity).toFixed(2) : '—' }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="report-footer">
            <p>701 Performance Power &mdash; {{ monthNames[reportMonth - 1] }} {{ reportYear }} &mdash; Confidential</p>
          </div>
        </div>
      </div>

      <!-- Category Create/Edit Modal -->
      <div v-if="showCategoryModal" class="modal-overlay" @click.self="closeCategoryModal">
        <div class="modal">
          <div class="modal-header">
            <h2>{{ editingCategory ? 'Edit Category' : 'New Category' }}</h2>
            <button @click="closeCategoryModal" class="close-btn">&times;</button>
          </div>
          
          <form @submit.prevent="saveCategory" class="modal-body">
            <div class="form-group">
              <label class="form-label required">Name</label>
              <input 
                v-model="categoryForm.name" 
                type="text" 
                class="form-control"
                :class="{ error: categoryFormErrors.name }"
                required 
                maxlength="100"
              />
              <span v-if="categoryFormErrors.name" class="error-message">{{ categoryFormErrors.name }}</span>
            </div>

            <div class="form-group">
              <label class="form-label">Description</label>
              <textarea 
                v-model="categoryForm.description" 
                class="form-control"
                rows="3"
                maxlength="500"
              ></textarea>
            </div>

            <div class="form-group">
              <label class="checkbox-label">
                <input v-model="categoryForm.isActive" type="checkbox" />
                Active
              </label>
            </div>

            <!-- Category Image Upload -->
            <div v-if="editingCategory" class="form-section">
              <h3 class="section-title">Category Image</h3>
              <PhotoUploader 
                ref="categoryPhotoUploader"
                entity-type="category"
                :entity-id="editingCategory.id"
                :max-files="1"
              />
            </div>
            <div v-else class="form-hint-box">
              💡 Save the category first to upload an image
            </div>

            <div v-if="categoryFormError" class="alert alert-danger">
              {{ categoryFormError }}
            </div>
            
            <div class="modal-footer">
              <button type="button" @click="closeCategoryModal" class="btn btn-secondary">
                Cancel
              </button>
              <button type="submit" class="btn btn-primary" :disabled="saving">
                {{ saving ? 'Saving...' : 'Save Category' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Product Create/Edit Modal -->
      <div v-if="showProductModal" class="modal-overlay" @click.self="closeProductModal">
        <div class="modal modal-lg">
          <div class="modal-header">
            <h2>{{ editingProduct ? 'Edit Product' : 'New Product' }}</h2>
            <button @click="closeProductModal" class="close-btn">&times;</button>
          </div>
          
          <form @submit.prevent="saveProduct" class="modal-body">
            <!-- Basic Information -->
            <div class="form-section">
              <h3 class="section-title">Basic Information</h3>
              
              <div class="form-group">
                <label class="form-label required">Product Name</label>
                <input 
                  v-model="productForm.name" 
                  type="text" 
                  class="form-control"
                  :class="{ error: productFormErrors.name }"
                  required 
                  maxlength="200"
                  placeholder="e.g., Honda CRF450R"
                />
                <span v-if="productFormErrors.name" class="error-message">{{ productFormErrors.name }}</span>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label class="form-label required">Category</label>
                  <select 
                    v-model="productForm.categoryId" 
                    class="form-control"
                    :class="{ error: productFormErrors.categoryId }"
                    required
                  >
                    <option value="">Select a category</option>
                    <option v-for="cat in activeCategories" :key="cat.id" :value="cat.id">
                      {{ cat.name }}
                    </option>
                  </select>
                  <span v-if="productFormErrors.categoryId" class="error-message">{{ productFormErrors.categoryId }}</span>
                </div>

                <div class="form-group">
                  <label class="form-label required">Price</label>
                  <div class="input-with-prefix">
                    <span class="input-prefix">$</span>
                    <input 
                      v-model.number="productForm.price" 
                      type="number" 
                      step="0.01"
                      min="0.01"
                      class="form-control with-prefix"
                      :class="{ error: productFormErrors.price }"
                      required 
                      placeholder="0.00"
                    />
                  </div>
                  <span v-if="productFormErrors.price" class="error-message">{{ productFormErrors.price }}</span>
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Description</label>
                <textarea 
                  v-model="productForm.description" 
                  class="form-control"
                  rows="4"
                  maxlength="1000"
                  placeholder="Enter product description..."
                ></textarea>
                <small class="form-hint">{{ productForm.description.length }}/1000 characters</small>
              </div>
            </div>

            <!-- Product Images -->
            <div v-if="editingProduct" class="form-section">
              <h3 class="section-title">Product Images</h3>
              <PhotoUploader 
                ref="productPhotoUploader"
                entity-type="product"
                :entity-id="editingProduct.id"
                :max-files="10"
              />
            </div>
            <div v-else class="form-hint-box">
              💡 Save the product first to upload images (up to 10 images supported)
            </div>

            <!-- Product Options -->
            <div class="form-section">
              <h3 class="section-title">Product Options</h3>
              
              <div class="checkbox-group">
                <label class="checkbox-label">
                  <input v-model="productForm.isActive" type="checkbox" />
                  <span class="checkbox-text">
                    <strong>Active</strong> - Display product on the store
                  </span>
                </label>

                <label class="checkbox-label">
                  <input v-model="productForm.isFeatured" type="checkbox" />
                  <span class="checkbox-text">
                    <strong>Featured ⭐</strong> - Show on homepage
                  </span>
                </label>
              </div>
            </div>

            <!-- Stock & Inventory Section -->
            <div class="form-section">
              <h3 class="section-title">📊 Stock &amp; Inventory</h3>
              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">SKU</label>
                  <input v-model="productForm.sku" type="text" class="form-control" placeholder="e.g. YAM-GR700-2024" maxlength="100" />
                  <small class="form-hint">Unique product identifier (optional)</small>
                </div>
                <div class="form-group">
                  <label class="form-label">Cost Price ($)</label>
                  <div class="input-with-prefix">
                    <span class="input-prefix">$</span>
                    <input v-model.number="productForm.costPrice" type="number" step="0.01" min="0" class="form-control with-prefix" placeholder="0.00" />
                  </div>
                  <small class="form-hint">Your purchase cost (used for margin calculations)</small>
                </div>
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Stock Quantity</label>
                  <input v-model.number="productForm.stockQuantity" type="number" min="0" class="form-control" placeholder="0" />
                  <small class="form-hint">Units currently on hand</small>
                </div>
                <div class="form-group">
                  <label class="form-label">Low Stock Alert At</label>
                  <input v-model.number="productForm.lowStockThreshold" type="number" min="0" class="form-control" placeholder="5" />
                  <small class="form-hint">Warn when stock falls to or below this number</small>
                </div>
              </div>
            </div>

            <div v-if="productFormError" class="alert alert-danger">
              {{ productFormError }}
            </div>
            
            <div class="modal-footer">
              <button type="button" @click="closeProductModal" class="btn btn-secondary">
                Cancel
              </button>
              <button type="submit" class="btn btn-primary" :disabled="saving">
                {{ saving ? 'Saving...' : 'Save Product' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Delete Category Confirmation Modal -->
      <div v-if="showDeleteCategoryModal" class="modal-overlay" @click.self="showDeleteCategoryModal = false">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>Delete Category</h2>
            <button @click="showDeleteCategoryModal = false" class="close-btn">&times;</button>
          </div>
          
          <div class="modal-body">
            <p>Are you sure you want to delete <strong>{{ categoryToDelete?.name }}</strong>?</p>
            <p class="text-muted">This action cannot be undone.</p>
            
            <div class="modal-footer">
              <button @click="showDeleteCategoryModal = false" class="btn btn-secondary">
                Cancel
              </button>
              <button @click="deleteCategory" class="btn btn-danger" :disabled="deleting">
                {{ deleting ? 'Deleting...' : 'Delete' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Delete Product Confirmation Modal -->
      <div v-if="showDeleteProductModal" class="modal-overlay" @click.self="showDeleteProductModal = false">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h2>Delete Product</h2>
            <button @click="showDeleteProductModal = false" class="close-btn">&times;</button>
          </div>
          
          <div class="modal-body">
            <p>Are you sure you want to delete <strong>{{ productToDelete?.name }}</strong>?</p>
            <p class="text-muted">This action cannot be undone.</p>
            
            <div class="modal-footer">
              <button @click="showDeleteProductModal = false" class="btn btn-secondary">
                Cancel
              </button>
              <button @click="deleteProduct" class="btn btn-danger" :disabled="deleting">
                {{ deleting ? 'Deleting...' : 'Delete' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import PhotoUploader from '@/components/PhotoUploader.vue'
import { useAuthStore } from '@/stores/auth'
import { logDebug, logError } from '@/services/logger'

interface Category {
  id: number
  name: string
  description: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

interface Product {
  id: number
  name: string
  description: string
  price: number
  categoryId: number
  imageUrl?: string
  isActive: boolean
  isFeatured: boolean
  sku?: string | null
  stockQuantity: number
  lowStockThreshold: number
  costPrice?: number | null
  createdAt: string
  updatedAt: string
}

const authStore = useAuthStore()
const API_URL = 'http://localhost:5226/api/v1'

// Tab state
const activeTab = ref<'categories' | 'products' | 'inventory' | 'reports'>('categories')

// Data
const categories = ref<Category[]>([])
const products = ref<Product[]>([])
const loading = ref(true)
const error = ref('')

// Category filter for products
const selectedCategoryFilter = ref<number | ''>('')

// Category modal state
const showCategoryModal = ref(false)
const editingCategory = ref<Category | null>(null)
const categoryForm = ref({
  name: '',
  description: '',
  isActive: true
})
const categoryFormErrors = ref({ name: '' })
const categoryFormError = ref('')

// Product modal state
const showProductModal = ref(false)
const editingProduct = ref<Product | null>(null)
const productForm = ref({
  name: '',
  description: '',
  price: 0,
  categoryId: '' as number | '',
  imageUrl: null as string | null,
  isActive: true,
  isFeatured: false,
  sku: null as string | null,
  stockQuantity: 0,
  lowStockThreshold: 5,
  costPrice: null as number | null
})
const productFormErrors = ref({
  name: '',
  categoryId: '',
  price: ''
})
const productFormError = ref('')

// Delete modals
const showDeleteCategoryModal = ref(false)
const categoryToDelete = ref<Category | null>(null)
const showDeleteProductModal = ref(false)
const productToDelete = ref<Product | null>(null)

// Loading states
const saving = ref(false)
const deleting = ref(false)

// Computed
const activeCategories = computed(() => categories.value.filter(c => c.isActive))

const filteredProducts = computed(() => {
  if (!selectedCategoryFilter.value) {
    return products.value
  }
  return products.value.filter(p => p.categoryId === selectedCategoryFilter.value)
})

// Utility functions
const getProductCount = (categoryId: number) => {
  return products.value.filter(p => p.categoryId === categoryId && p.isActive).length
}

const getCategoryName = (categoryId: number) => {
  const category = categories.value.find(c => c.id === categoryId)
  return category?.name || 'Unknown'
}

const getImageUrl = (imageUrl: string | null) => {
  if (!imageUrl) return '/placeholder.png'
  if (imageUrl.startsWith('/uploads')) {
    return `http://localhost:5226${imageUrl}`
  }
  return imageUrl
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

// Data loading
const loadData = async () => {
  loading.value = true
  error.value = ''
  
  try {
    const [categoriesRes, productsRes] = await Promise.all([
      fetch(`${API_URL}/categories?includeInactive=true`, {
        headers: { 'Authorization': `Bearer ${authStore.token}` }
      }),
      fetch(`${API_URL}/products?includeInactive=true`, {
        headers: { 'Authorization': `Bearer ${authStore.token}` }
      })
    ])

    if (!categoriesRes.ok) throw new Error('Failed to load categories')
    if (!productsRes.ok) throw new Error('Failed to load products')

    categories.value = await categoriesRes.json()
    products.value = await productsRes.json()
    syncInventoryEdits()
  } catch (err: any) {
    error.value = err.message || 'Failed to load data'
  } finally {
    loading.value = false
  }
}

// Category functions
const openCategoryModal = () => {
  editingCategory.value = null
  categoryForm.value = {
    name: '',
    description: '',
    isActive: true
  }
  categoryFormErrors.value = { name: '' }
  categoryFormError.value = ''
  showCategoryModal.value = true
}

const editCategory = (category: Category) => {
  editingCategory.value = category
  categoryForm.value = {
    name: category.name,
    description: category.description,
    isActive: category.isActive
  }
  categoryFormErrors.value = { name: '' }
  categoryFormError.value = ''
  showCategoryModal.value = true
}

const closeCategoryModal = () => {
  showCategoryModal.value = false
  editingCategory.value = null
}

const validateCategoryForm = () => {
  categoryFormErrors.value = { name: '' }
  
  if (!categoryForm.value.name.trim()) {
    categoryFormErrors.value.name = 'Category name is required'
    return false
  }
  
  return true
}

const saveCategory = async () => {
  if (!validateCategoryForm()) return
  
  saving.value = true
  categoryFormError.value = ''
  
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
      body: JSON.stringify(categoryForm.value)
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.message || 'Failed to save category')
    }

    await loadData()
    closeCategoryModal()
  } catch (err: any) {
    categoryFormError.value = err.message || 'Failed to save category'
  } finally {
    saving.value = false
  }
}

const toggleCategoryStatus = async (category: Category) => {
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

    await loadData()
  } catch (err: any) {
    alert(err.message || 'Failed to update category')
  }
}

const confirmDeleteCategory = (category: Category) => {
  categoryToDelete.value = category
  showDeleteCategoryModal.value = true
}

const deleteCategory = async () => {
  if (!categoryToDelete.value) return
  
  deleting.value = true
  
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

    await loadData()
    showDeleteCategoryModal.value = false
    categoryToDelete.value = null
  } catch (err: any) {
    alert(err.message || 'Failed to delete category')
  } finally {
    deleting.value = false
  }
}

// Product functions
const openProductModal = () => {
  editingProduct.value = null
  productForm.value = {
    name: '',
    description: '',
    price: 0,
    categoryId: '',
    imageUrl: null,
    isActive: true,
    isFeatured: false,
    sku: null,
    stockQuantity: 0,
    lowStockThreshold: 5,
    costPrice: null
  }
  productFormErrors.value = { name: '', categoryId: '', price: '' }
  productFormError.value = ''
  showProductModal.value = true
}

const editProduct = (product: Product) => {
  editingProduct.value = product
  productForm.value = {
    name: product.name,
    description: product.description,
    price: product.price,
    categoryId: product.categoryId,
    imageUrl: product.imageUrl || null,
    isActive: product.isActive,
    isFeatured: product.isFeatured,
    sku: product.sku ?? null,
    stockQuantity: product.stockQuantity ?? 0,
    lowStockThreshold: product.lowStockThreshold ?? 5,
    costPrice: product.costPrice ?? null
  }
  productFormErrors.value = { name: '', categoryId: '', price: '' }
  productFormError.value = ''
  showProductModal.value = true
}

const closeProductModal = () => {
  showProductModal.value = false
  editingProduct.value = null
}

const validateProductForm = () => {
  productFormErrors.value = { name: '', categoryId: '', price: '' }
  let isValid = true
  
  if (!productForm.value.name.trim()) {
    productFormErrors.value.name = 'Product name is required'
    isValid = false
  }
  
  if (!productForm.value.categoryId) {
    productFormErrors.value.categoryId = 'Please select a category'
    isValid = false
  }
  
  if (!productForm.value.price || productForm.value.price <= 0) {
    productFormErrors.value.price = 'Price must be greater than 0'
    isValid = false
  }
  
  return isValid
}

const saveProduct = async () => {
  if (!validateProductForm()) return
  
  saving.value = true
  productFormError.value = ''
  
  try {
    const url = editingProduct.value 
      ? `${API_URL}/products/${editingProduct.value.id}`
      : `${API_URL}/products`
    
    const method = editingProduct.value ? 'PUT' : 'POST'
    
    logDebug('Saving product', {
      url,
      method,
      productId: editingProduct.value?.id,
      name: productForm.value.name
    });
    
    const response = await fetch(url, {
      method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authStore.token}`
      },
      body: JSON.stringify(productForm.value)
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.message || 'Failed to save product')
    }

    await loadData()
    closeProductModal()
  } catch (err: any) {
    logError('Failed to save product', err);
    productFormError.value = err.message || 'Failed to save product'
  } finally {
    saving.value = false
  }
}

const confirmDeleteProduct = (product: Product) => {
  productToDelete.value = product
  showDeleteProductModal.value = true
}

const deleteProduct = async () => {
  if (!productToDelete.value) return
  
  deleting.value = true
  
  try {
    const response = await fetch(`${API_URL}/products/${productToDelete.value.id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      }
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.message || 'Failed to delete product')
    }

    await loadData()
    showDeleteProductModal.value = false
    productToDelete.value = null
  } catch (err: any) {
    alert(err.message || 'Failed to delete product')
  } finally {
    deleting.value = false
  }
}

// ─── Inventory Tab ───────────────────────────────────────────────────────────
type InventoryEdit = { sku: string | null; costPrice: number | null; stockQuantity: number; lowStockThreshold: number }
const inventoryEdits = ref<Record<number, InventoryEdit>>({})
const inventorySearch = ref('')
const inventoryCategoryFilter = ref<number | ''>()
const inventoryStockFilter = ref<'' | 'in' | 'low' | 'out'>('')
const savingStock = ref<Record<number, boolean>>({})

const syncInventoryEdits = () => {
  const map: Record<number, InventoryEdit> = {}
  products.value.forEach(p => {
    map[p.id] = { sku: p.sku ?? null, costPrice: p.costPrice ?? null, stockQuantity: p.stockQuantity ?? 0, lowStockThreshold: p.lowStockThreshold ?? 5 }
  })
  inventoryEdits.value = map
}

const inStockCount = computed(() => products.value.filter(p => (inventoryEdits.value[p.id]?.stockQuantity ?? 0) > (inventoryEdits.value[p.id]?.lowStockThreshold ?? 5)).length)
const lowStockCount = computed(() => products.value.filter(p => { const e = inventoryEdits.value[p.id]; return e && e.stockQuantity > 0 && e.stockQuantity <= e.lowStockThreshold }).length)
const outOfStockCount = computed(() => products.value.filter(p => (inventoryEdits.value[p.id]?.stockQuantity ?? 0) === 0).length)
const inventoryAlertCount = computed(() => lowStockCount.value + outOfStockCount.value)
const totalInventoryValue = computed(() => products.value.reduce((s, p) => s + p.price * (inventoryEdits.value[p.id]?.stockQuantity ?? 0), 0))
const totalCostValue = computed(() => products.value.reduce((s, p) => { const e = inventoryEdits.value[p.id]; return e?.costPrice ? s + e.costPrice * e.stockQuantity : s }, 0))
const lowStockProducts = computed(() => products.value.filter(p => { const e = inventoryEdits.value[p.id]; return e && e.stockQuantity <= e.lowStockThreshold }))

const filteredInventoryProducts = computed(() => {
  let list = products.value
  if (inventorySearch.value) {
    const q = inventorySearch.value.toLowerCase()
    list = list.filter(p => p.name.toLowerCase().includes(q) || (p.sku && p.sku.toLowerCase().includes(q)))
  }
  if (inventoryCategoryFilter.value) {
    list = list.filter(p => p.categoryId === inventoryCategoryFilter.value)
  }
  const f = inventoryStockFilter.value
  if (f === 'out') list = list.filter(p => (inventoryEdits.value[p.id]?.stockQuantity ?? 0) === 0)
  else if (f === 'low') list = list.filter(p => { const e = inventoryEdits.value[p.id]; return e && e.stockQuantity > 0 && e.stockQuantity <= e.lowStockThreshold })
  else if (f === 'in') list = list.filter(p => (inventoryEdits.value[p.id]?.stockQuantity ?? 0) > (inventoryEdits.value[p.id]?.lowStockThreshold ?? 5))
  return list
})

const getInventoryRowClass = (product: Product) => {
  const e = inventoryEdits.value[product.id]
  if (!e) return ''
  if (e.stockQuantity === 0) return 'row-danger'
  if (e.stockQuantity <= e.lowStockThreshold) return 'row-warning'
  return ''
}

const getStockStatus = (productId: number) => {
  const e = inventoryEdits.value[productId]
  if (!e) return 'unknown'
  if (e.stockQuantity === 0) return 'out'
  if (e.stockQuantity <= e.lowStockThreshold) return 'low'
  return 'ok'
}

const getStockStatusLabel = (productId: number) => {
  const s = getStockStatus(productId)
  if (s === 'out') return '🔴 Out of Stock'
  if (s === 'low') return '🟡 Low Stock'
  return '🟢 In Stock'
}

const getMarginNum = (price: number, cost: number | null): number | null => {
  if (!cost || cost <= 0) return null
  return Math.round(((price - cost) / price) * 100)
}

const getMarginLabel = (price: number, cost: number | null) => {
  const m = getMarginNum(price, cost)
  return m === null ? '—' : m + '%'
}

const getMarginClass = (price: number, cost: number | null) => {
  const m = getMarginNum(price, cost)
  if (m === null) return ''
  if (m >= 30) return 'margin-good'
  if (m >= 15) return 'margin-ok'
  return 'margin-low'
}

const saveInventoryRow = async (productId: number) => {
  const e = inventoryEdits.value[productId]
  if (!e) return
  savingStock.value[productId] = true
  try {
    const response = await fetch(`${API_URL}/products/${productId}/stock`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${authStore.token}` },
      body: JSON.stringify({ stockQuantity: e.stockQuantity, lowStockThreshold: e.lowStockThreshold, sku: e.sku, costPrice: e.costPrice })
    })
    if (!response.ok) throw new Error('Failed to update stock')
    const product = products.value.find(p => p.id === productId)
    if (product) {
      product.stockQuantity = e.stockQuantity
      product.lowStockThreshold = e.lowStockThreshold
      product.sku = e.sku
      product.costPrice = e.costPrice
    }
  } catch (err) {
    logError('Failed to save inventory', err as Error)
  } finally {
    delete savingStock.value[productId]
  }
}

// ─── Reports Tab ─────────────────────────────────────────────────────────────
const monthNames = ['January','February','March','April','May','June','July','August','September','October','November','December']
const reportMonth = ref(new Date().getMonth() + 1)
const reportYear = ref(new Date().getFullYear())
const reportType = ref<'inventory' | 'lowstock' | 'catalog' | 'value'>('inventory')

const reportTitle = computed(() => {
  switch (reportType.value) {
    case 'inventory': return 'Inventory Snapshot Report'
    case 'lowstock': return 'Low Stock / Out of Stock Report'
    case 'catalog': return 'Full Product Catalog'
    case 'value': return 'Inventory Value Summary'
    default: return 'Report'
  }
})

const printReport = () => { window.print() }

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.catalog-page {
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

/* Tabs */
.tabs {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid #e5e7eb;
}

.tab {
  padding: 0.75rem 1.5rem;
  background: none;
  border: none;
  border-bottom: 3px solid transparent;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.938rem;
  color: #6b7280;
  transition: all 0.2s;
  margin-bottom: -2px;
}

.tab:hover {
  color: #4f46e5;
  background: #f9fafb;
}

.tab.active {
  color: #4f46e5;
  border-bottom-color: #4f46e5;
}

.tab-content {
  animation: fadeIn 0.3s;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Loading & Error States */
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

/* Categories Grid */
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

.category-actions {
  display: flex;
  gap: 0.5rem;
}

/* Products Table */
.filters {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.filter-label {
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.filter-select {
  max-width: 300px;
}

.card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  overflow: hidden;
}

.table {
  width: 100%;
  border-collapse: collapse;
}

.table thead {
  background: #f9fafb;
}

.table th {
  padding: 0.875rem 1rem;
  text-align: left;
  font-weight: 600;
  font-size: 0.813rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #6b7280;
  border-bottom: 1px solid #e5e7eb;
}

.table td {
  padding: 1rem;
  border-bottom: 1px solid #f3f4f6;
  vertical-align: middle;
}

.table tbody tr:hover {
  background: #f9fafb;
}

.product-image {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 8px;
  border: 1px solid #e5e7eb;
}

.product-name-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
}

.featured-badge {
  font-size: 1rem;
  line-height: 1;
}

.price-cell {
  font-weight: 600;
  color: #059669;
  font-size: 1rem;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.empty-state {
  text-align: center;
  color: #9ca3af;
  padding: 3rem 1rem !important;
  font-style: italic;
}

/* Status Badges */
.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  white-space: nowrap;
  display: inline-block;
}

.status-badge-sm {
  padding: 0.188rem 0.563rem;
  font-size: 0.688rem;
}

.status-badge.active {
  background: #d1fae5;
  color: #065f46;
}

.status-badge.inactive {
  background: #fee2e2;
  color: #991b1b;
}

/* Buttons */
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

/* Modals */
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

.modal-lg {
  max-width: 800px;
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
  transition: color 0.2s;
}

.close-btn:hover {
  color: #1a1a1a;
}

.modal-body {
  padding: 1.5rem;
}

/* Form Sections */
.form-section {
  margin-bottom: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid #e5e7eb;
}

.form-section:last-of-type {
  margin-bottom: 0;
  padding-bottom: 0;
  border-bottom: none;
}

.section-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 1.25rem 0;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group:last-child {
  margin-bottom: 0;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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
  padding: 0.625rem 0.875rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.938rem;
  transition: all 0.2s;
  font-family: inherit;
}

.form-control:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.form-control.error {
  border-color: #ef4444;
}

.input-with-prefix {
  position: relative;
}

.input-prefix {
  position: absolute;
  left: 0.875rem;
  top: 50%;
  transform: translateY(-50%);
  color: #6b7280;
  font-weight: 600;
  pointer-events: none;
}

.form-control.with-prefix {
  padding-left: 2rem;
}

.form-hint {
  display: block;
  color: #6b7280;
  font-size: 0.813rem;
  margin-top: 0.375rem;
}

.error-message {
  display: block;
  color: #ef4444;
  font-size: 0.813rem;
  margin-top: 0.25rem;
}

.checkbox-group {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.checkbox-label {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  cursor: pointer;
  user-select: none;
}

.checkbox-label input[type="checkbox"] {
  width: 20px;
  height: 20px;
  cursor: pointer;
  flex-shrink: 0;
  margin-top: 0.125rem;
}

.checkbox-text {
  line-height: 1.5;
  color: #374151;
}

.form-hint-box {
  padding: 1rem;
  background: #f0f9ff;
  border: 1px dashed #bae6fd;
  border-radius: 8px;
  color: #0369a1;
  text-align: center;
  font-size: 0.875rem;
  margin-top: 1rem;
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
  margin-top: 1.5rem;
}

.text-muted {
  color: #9ca3af;
  font-size: 0.875rem;
}

.icon {
  display: inline-block;
}

/* ---- Tab Badge ---- */
.tab-badge {
  display: inline-block;
  margin-left: 0.35rem;
  font-size: 0.7rem;
  vertical-align: middle;
}

/* ---- Inventory Tab ---- */
.inventory-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.summary-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.25rem;
  text-align: center;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}

.summary-card.success { border-top: 3px solid #22c55e; }
.summary-card.warning { border-top: 3px solid #f59e0b; }
.summary-card.danger  { border-top: 3px solid #ef4444; }
.summary-card.info    { border-top: 3px solid #3b82f6; }

.summary-value {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1a1a2e;
}

.summary-label {
  font-size: 0.8rem;
  color: #6b7280;
  margin-top: 0.25rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.inventory-input {
  width: 100%;
  min-width: 60px;
  padding: 0.3rem 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  background: #fafafa;
  transition: border-color 0.15s;
}

.inventory-input:focus {
  outline: none;
  border-color: #6366f1;
  background: white;
}

.inventory-stock-input {
  max-width: 70px;
  font-weight: 600;
}

.input-sm .input-prefix {
  padding: 0.3rem 0.4rem;
  font-size: 0.875rem;
}

.row-warning { background: #fffbeb !important; }
.row-danger  { background: #fef2f2 !important; }

.inactive-tag {
  display: inline-block;
  margin-left: 0.4rem;
  font-size: 0.7rem;
  padding: 0.1rem 0.4rem;
  background: #f3f4f6;
  color: #6b7280;
  border-radius: 4px;
}

.stock-badge {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 20px;
  font-size: 0.78rem;
  font-weight: 600;
  white-space: nowrap;
}

.stock-ok  { background: #dcfce7; color: #166534; }
.stock-low { background: #fef9c3; color: #854d0e; }
.stock-out { background: #fee2e2; color: #991b1b; }

.margin-badge {
  display: inline-block;
  padding: 0.15rem 0.45rem;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 600;
}

.margin-good { background: #dcfce7; color: #166534; }
.margin-ok   { background: #fef9c3; color: #854d0e; }
.margin-low  { background: #fee2e2; color: #991b1b; }

/* ---- Reports Tab ---- */
.reports-toolbar {
  margin-bottom: 1.5rem;
}

.report-preview {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}

.report-header {
  text-align: center;
  border-bottom: 2px solid #1a1a2e;
  padding-bottom: 1.25rem;
  margin-bottom: 1.5rem;
}

.report-title {
  font-size: 1.6rem;
  font-weight: 800;
  color: #1a1a2e;
  margin: 0 0 0.25rem;
}

.report-subtitle {
  font-size: 1.1rem;
  font-weight: 600;
  color: #4b5563;
  margin: 0 0 0.5rem;
}

.report-meta {
  font-size: 0.85rem;
  color: #6b7280;
  margin: 0;
}

.report-summary-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  margin-bottom: 1.5rem;
}

.report-stat {
  flex: 1;
  min-width: 120px;
  text-align: center;
  padding: 0.75rem;
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
}

.report-stat.success { border-top: 3px solid #22c55e; }
.report-stat.warning { border-top: 3px solid #f59e0b; }
.report-stat.danger  { border-top: 3px solid #ef4444; }

.stat-val {
  display: block;
  font-size: 1.4rem;
  font-weight: 700;
  color: #1a1a2e;
}

.stat-lbl {
  display: block;
  font-size: 0.75rem;
  color: #6b7280;
  text-transform: uppercase;
  margin-top: 0.2rem;
}

.report-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
  margin-bottom: 1.5rem;
}

.report-table th {
  background: #1a1a2e;
  color: white;
  padding: 0.6rem 0.75rem;
  text-align: left;
  font-weight: 600;
}

.report-table td {
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid #e5e7eb;
}

.report-table tr:nth-child(even) td { background: #f9fafb; }
.report-row-warning td { background: #fffbeb !important; }
.report-row-danger td  { background: #fef2f2 !important; }

.report-empty {
  text-align: center;
  padding: 2rem;
  color: #6b7280;
  font-style: italic;
}

.report-footer {
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid #e5e7eb;
  text-align: center;
  font-size: 0.8rem;
  color: #9ca3af;
}

/* ---- Print styles ---- */
@media print {
  .no-print, .tabs, .page-header, .filters, .reports-toolbar { display: none !important; }
  .tab-content { display: block !important; }
  .catalog-page { padding: 0; }
  .report-preview { border: none; box-shadow: none; padding: 0; }
  #report-print-area { display: block !important; }
}
</style>
