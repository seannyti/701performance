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
      <div v-if="isLoading" class="loading-overlay">
        <div class="spinner"></div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <p>{{ error }}</p>
        <button @click="loadData" class="btn btn-primary">Retry</button>
      </div>

      <!-- Categories Tab -->
      <div v-show="activeTab === 'categories' && !isLoading && !error" class="tab-content">
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
                :disabled="isActionLoading(`toggleCategory-${category.id}`)"
              >
                <span v-if="isActionLoading(`toggleCategory-${category.id}`)" class="btn-spinner"></span>
                {{ category.isActive ? 'Disable' : 'Enable' }}
              </button>
              <button
                @click="confirmDeleteCategory(category)"
                class="btn btn-sm btn-danger"
                :disabled="getProductCount(category.id) > 0"
                :title="getProductCount(category.id) > 0 ? 'Cannot delete a category that has products. Move or delete the products first.' : 'Delete this category'"
              >
                {{ getProductCount(category.id) > 0 ? `Has Products (${getProductCount(category.id)})` : 'Delete' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Products Tab -->
      <div v-show="activeTab === 'products' && !isLoading && !error" class="tab-content">
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
                    <button
                      @click="toggleProductStatus(product)"
                      :class="['btn', 'btn-sm', product.isActive ? 'btn-warning' : 'btn-success']"
                      :disabled="isActionLoading(`toggleProduct-${product.id}`)"
                    >
                      <span v-if="isActionLoading(`toggleProduct-${product.id}`)" class="btn-spinner"></span>
                      {{ product.isActive ? 'Disable' : 'Enable' }}
                    </button>
                    <button
                      @click="toggleProductFeatured(product)"
                      :class="['btn', 'btn-sm', product.isFeatured ? 'btn-warning' : 'btn-secondary']"
                      :disabled="isActionLoading(`featuredProduct-${product.id}`)"
                      :title="product.isFeatured ? 'Remove from homepage' : 'Feature on homepage'"
                    >
                      <span v-if="isActionLoading(`featuredProduct-${product.id}`)" class="btn-spinner"></span>
                      {{ product.isFeatured ? '⭐ Unfeature' : '☆ Feature' }}
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
      <div v-show="activeTab === 'inventory' && !isLoading && !error" class="tab-content">
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
              <tr v-for="product in filteredInventoryProducts" :key="product.id" :class="getStockInfo(product).rowClass">
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
                  <span v-if="inventoryEdits[product.id]?.costPrice" :class="['margin-badge', getMarginInfo(product.price, inventoryEdits[product.id].costPrice).cssClass]">
                    {{ getMarginInfo(product.price, inventoryEdits[product.id].costPrice).label }}
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
                  <span :class="['stock-badge', 'stock-' + getStockInfo(product).status]">
                    {{ getStockInfo(product).label }}
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
      <div v-show="activeTab === 'reports' && !isLoading && !error" class="tab-content">
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
                  <td>{{ getMarginInfo(p.price, p.costPrice ?? null).label }}</td>
                  <td>{{ p.stockQuantity }}</td>
                  <td>{{ getStockInfo(p).label }}</td>
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

            <!-- Category Image -->
            <div v-if="editingCategory" class="form-section">
              <h3 class="section-title">Category Image</h3>
              
              <div v-if="editingCategory.imageUrl" class="current-image">
                <img :src="getMediaUrl(editingCategory.imageUrl)" alt="Category image" class="preview-image" />
                <button @click="removeCategoryImage" type="button" class="btn btn-sm btn-danger">Remove Image</button>
              </div>
              
              <button @click="openCategoryMediaPicker" type="button" class="btn btn-secondary">
                {{ editingCategory.imageUrl ? 'Change Image' : 'Select from Library' }}
              </button>
              <small class="form-hint">Select an image from the Media Library</small>
            </div>
            <div v-else class="form-hint-box">
              💡 Save the category first to select an image
            </div>

            <div v-if="categoryFormError" class="alert alert-danger">
              {{ categoryFormError }}
            </div>
            
            <div class="modal-footer">
              <button type="button" @click="closeCategoryModal" class="btn btn-secondary">
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
              
              <div v-if="productImages.length > 0" class="images-grid">
                <div v-for="img in productImages" :key="img.id" class="image-card">
                  <img :src="getMediaUrl(img.thumbnailPath || img.filePath)" :alt="img.altText || img.fileName" />
                  <span v-if="img.isMain" class="main-badge">Main</span>
                  <div class="image-actions">
                    <button v-if="!img.isMain" @click="setProductImageAsMain(img.id)" type="button" class="btn btn-xs">⭐ Set Main</button>
                    <button @click="removeProductImage(img.id)" type="button" class="btn btn-xs btn-danger">Remove</button>
                  </div>
                </div>
              </div>
              
              <button @click="openProductMediaPicker" type="button" class="btn btn-secondary">
                {{ productImages.length > 0 ? 'Add More Images' : 'Select from Library' }}
              </button>
              <small class="form-hint">Up to 10 images supported</small>
            </div>
            <div v-else class="form-hint-box">
              💡 Save the product first to add images
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

            <!-- Product Specifications -->
            <div class="form-section">
              <h3 class="section-title">📋 Specifications</h3>
              <small class="form-hint mb-3">Add technical specifications for this product (e.g., Engine, Power, Weight)</small>
              
              <div v-if="productForm.specifications.length > 0" class="specs-list">
                <div v-for="(spec, index) in productForm.specifications" :key="index" class="spec-item">
                  <input 
                    v-model="spec.key" 
                    type="text" 
                    class="form-control spec-key" 
                    placeholder="e.g., Engine"
                    maxlength="100"
                  />
                  <input 
                    v-model="spec.value" 
                    type="text" 
                    class="form-control spec-value" 
                    placeholder="e.g., 999cc Inline-4"
                    maxlength="200"
                  />
                  <button 
                    @click="removeSpecification(index)" 
                    type="button" 
                    class="btn btn-sm btn-danger spec-remove"
                    title="Remove specification"
                  >
                    🗑️
                  </button>
                </div>
              </div>
              
              <button @click="addSpecification" type="button" class="btn btn-secondary mt-2">
                <span class="icon">+</span> Add Specification
              </button>
            </div>

            <div v-if="productFormError" class="alert alert-danger">
              {{ productFormError }}
            </div>
            
            <div class="modal-footer">
              <button type="button" @click="closeProductModal" class="btn btn-secondary">
                Cancel
              </button>
              <button type="submit" class="btn btn-primary" :disabled="isActionLoading('saveProduct')">
                <span v-if="isActionLoading('saveProduct')" class="btn-spinner"></span>
                {{ isActionLoading('saveProduct') ? 'Saving...' : 'Save Product' }}
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
              <button @click="deleteCategory" class="btn btn-danger" :disabled="isActionLoading('deleteCategory')">
                <span v-if="isActionLoading('deleteCategory')" class="btn-spinner"></span>
                {{ isActionLoading('deleteCategory') ? 'Deleting...' : 'Delete' }}
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
              <button @click="deleteProduct" class="btn btn-danger" :disabled="isActionLoading('deleteProduct')">
                <span v-if="isActionLoading('deleteProduct')" class="btn-spinner"></span>
                {{ isActionLoading('deleteProduct') ? 'Deleting...' : 'Delete' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Media Pickers -->
      <MediaPicker 
        :is-open="showCategoryMediaPicker"
        @close="showCategoryMediaPicker = false"
        @select="handleCategoryMediaSelection"
        media-type="Image"
      />
      
      <MediaPicker 
        :is-open="showProductMediaPicker"
        @close="showProductMediaPicker = false"
        @select="handleProductMediaSelection"
        media-type="Image"
      />
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import MediaPicker from '@/components/MediaPicker.vue'
import { useAuthStore } from '@/stores/auth'
import { logDebug, logError } from '@/services/logger'
import { useToast } from '@/composables/useToast'
import { useLoadingState } from '@/composables/useLoadingState'
import { apiClient, apiGet, apiPost, apiPut, apiDelete } from '@/utils/apiClient'
import { getMediaUrl } from '@/utils/api-config'

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
  specifications?: string | null
  createdAt: string
  updatedAt: string
}

const authStore = useAuthStore()
const toast = useToast()
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState()

// Tab state
const activeTab = ref<'categories' | 'products' | 'inventory' | 'reports'>('categories')

// Data
const categories = ref<Category[]>([])
const products = ref<Product[]>([])
const error = ref('')

// Category filter for products
const selectedCategoryFilter = ref<number | ''>('')

// Category modal state
const showCategoryModal = ref(false)
const editingCategory = ref<Category | null>(null)
const categoryForm = ref({
  name: '',
  description: '',
  imageUrl: undefined as string | undefined,
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
  costPrice: null as number | null,
  specifications: [] as Array<{ key: string; value: string }>
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

// Media Picker state
const showCategoryMediaPicker = ref(false)
const showProductMediaPicker = ref(false)
const productImages = ref<any[]>([])

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
  if (!imageUrl) return "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='150' viewBox='0 0 200 150'%3E%3Crect width='200' height='150' fill='%23f3f4f6'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%239ca3af' font-family='sans-serif' font-size='13'%3ENo Image%3C/text%3E%3C/svg%3E"
  return getMediaUrl(imageUrl)
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
  await executeWithLoading(async () => {
    error.value = ''

    try {
      const [categoriesData, productsJson] = await Promise.all([
        apiGet('/categories?includeInactive=true'),
        apiGet('/products?includeInactive=true')
      ])

      categories.value = categoriesData
      products.value = productsJson.data ?? productsJson

      logDebug('Loaded products from API', {
        count: products.value.length,
        withSpecifications: products.value.filter((p: Product) => p.specifications).length
      });

      syncInventoryEdits()
    } catch (err: any) {
      error.value = err.message || 'Failed to load data'
    }
  })
}

// Category functions
const openCategoryModal = () => {
  editingCategory.value = null
  categoryForm.value = {
    name: '',
    description: '',
    imageUrl: undefined,
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
    imageUrl: category.imageUrl,
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
  
  await executeWithLoading(async () => {
    categoryFormError.value = ''
    
    try {
      if (editingCategory.value) {
        await apiPut(`/categories/${editingCategory.value.id}`, categoryForm.value)
      } else {
        await apiPost('/categories', categoryForm.value)
      }

      await loadData()
      closeCategoryModal()
    } catch (err: any) {
      categoryFormError.value = err.message || 'Failed to save category'
    }
  }, 'saveCategory')
}

const toggleCategoryStatus = async (category: Category) => {
  await executeWithLoading(async () => {
    try {
      await apiPut(`/categories/${category.id}`, {
        name: category.name,
        description: category.description,
        imageUrl: category.imageUrl,
        isActive: !category.isActive
      })

      await loadData()
      toast.success('Category status updated')
    } catch (err: any) {
      toast.error(err.message || 'Failed to update category')
    }
  }, `toggleCategory-${category.id}`)
}

const confirmDeleteCategory = (category: Category) => {
  categoryToDelete.value = category
  showDeleteCategoryModal.value = true
}

const deleteCategory = async () => {
  if (!categoryToDelete.value) return
  
  await executeWithLoading(async () => {
    try {
      await apiDelete(`/categories/${categoryToDelete.value.id}`)

      await loadData()
      showDeleteCategoryModal.value = false
      categoryToDelete.value = null
      toast.deleteSuccess('Category')
    } catch (err: any) {
      toast.error(err.message || 'Failed to delete category')
    }
  }, 'deleteCategory')
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
    costPrice: null,
    specifications: []
  }
  productFormErrors.value = { name: '', categoryId: '', price: '' }
  productFormError.value = ''
  showProductModal.value = true
}

const editProduct = (product: Product) => {
  editingProduct.value = product
  
  logDebug('Editing product', {
    productId: product.id,
    hasSpecifications: !!product.specifications,
    specificationsType: typeof product.specifications
  });
  
  // Parse specifications from JSON string
  let specs: Array<{ key: string; value: string }> = []
  if (product.specifications) {
    try {
      specs = JSON.parse(product.specifications)
      if (!Array.isArray(specs)) specs = []
      logDebug('Parsed product specifications', { count: specs.length });
    } catch (e) {
      logError('Failed to parse product specifications', e);
      specs = []
    }
  }
  
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
    costPrice: product.costPrice ?? null,
    specifications: specs
  }
  productFormErrors.value = { name: '', categoryId: '', price: '' }
  productFormError.value = ''
  showProductModal.value = true
  loadProductImages() // Load product images when editing
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
  
  await executeWithLoading(async () => {
    productFormError.value = ''
    
    try {
      logDebug('Saving product', {
        method: editingProduct.value ? 'PUT' : 'POST',
        productId: editingProduct.value?.id,
        name: productForm.value.name
      });
      
      // Prepare product data with specifications as JSON string
      const productData = {
        ...productForm.value,
        specifications: productForm.value.specifications.length > 0 
          ? JSON.stringify(productForm.value.specifications) 
          : null
      }
      
      logDebug('Saving product data', {
        hasSpecifications: productForm.value.specifications.length > 0,
        specificationsCount: productForm.value.specifications.length,
        productName: productData.name
      });
      
      if (editingProduct.value) {
        await apiPut(`/products/${editingProduct.value.id}`, productData)
      } else {
        await apiPost('/products', productData)
      }

      await loadData()
      closeProductModal()
    } catch (err: any) {
      logError('Failed to save product', err);
      productFormError.value = err.message || 'Failed to save product'
    }
  }, 'saveProduct')
}

const confirmDeleteProduct = (product: Product) => {
  productToDelete.value = product
  showDeleteProductModal.value = true
}

const deleteProduct = async () => {
  if (!productToDelete.value) return
  
  await executeWithLoading(async () => {
    try {
      await apiDelete(`/products/${productToDelete.value.id}`)

      await loadData()
      showDeleteProductModal.value = false
      productToDelete.value = null
      toast.deleteSuccess('Product')
    } catch (err: any) {
      toast.error(err.message || 'Failed to delete product')
    }
  }, 'deleteProduct')
}

const toggleProductStatus = async (product: Product) => {
  await executeWithLoading(async () => {
    try {
      await apiPut(`/products/${product.id}`, {
        name: product.name,
        description: product.description,
        price: product.price,
        categoryId: product.categoryId,
        imageUrl: product.imageUrl,
        isFeatured: product.isFeatured,
        isActive: !product.isActive,
        sku: product.sku,
        stockQuantity: product.stockQuantity,
        lowStockThreshold: product.lowStockThreshold,
        costPrice: product.costPrice,
        specifications: product.specifications
      })
      product.isActive = !product.isActive
      toast.success('Product status updated')
    } catch (err: any) {
      toast.error(err.message || 'Failed to update product status')
    }
  }, `toggleProduct-${product.id}`)
}

const toggleProductFeatured = async (product: Product) => {
  await executeWithLoading(async () => {
    try {
      await apiPut(`/products/${product.id}`, {
        name: product.name,
        description: product.description,
        price: product.price,
        categoryId: product.categoryId,
        imageUrl: product.imageUrl,
        isFeatured: !product.isFeatured,
        isActive: product.isActive,
        sku: product.sku,
        stockQuantity: product.stockQuantity,
        lowStockThreshold: product.lowStockThreshold,
        costPrice: product.costPrice,
        specifications: product.specifications
      })
      product.isFeatured = !product.isFeatured
      toast.success(product.isFeatured ? 'Product featured on homepage' : 'Product removed from homepage')
    } catch (err: any) {
      toast.error(err.message || 'Failed to update featured status')
    }
  }, `featuredProduct-${product.id}`)
}

// Specification management functions
const addSpecification = () => {
  productForm.value.specifications.push({ key: '', value: '' })
}

const removeSpecification = (index: number) => {
  productForm.value.specifications.splice(index, 1)
}

// Media Picker functions
const openCategoryMediaPicker = () => {
  showCategoryMediaPicker.value = true
}

const openProductMediaPicker = () => {
  showProductMediaPicker.value = true
}

const handleCategoryMediaSelection = async (mediaFile: any) => {
  if (!editingCategory.value) return
  
  await executeWithLoading(async () => {
    try {
      // Update both editingCategory and categoryForm
      editingCategory.value.imageUrl = mediaFile.filePath
      categoryForm.value.imageUrl = mediaFile.filePath
      showCategoryMediaPicker.value = false
      toast.success('Image selected')
    } catch (err: any) {
      toast.error(err.message || 'Failed to select image')
    }
  }, 'categoryMedia')
}

const removeCategoryImage = () => {
  if (!editingCategory.value) return
  if (!confirm('Remove this image? Click Save to apply changes.')) return
  
  // Update both editingCategory and categoryForm
  editingCategory.value.imageUrl = undefined
  categoryForm.value.imageUrl = undefined
}

const loadProductImages = async () => {
  if (!editingProduct.value) return
  
  await executeWithLoading(async () => {
    try {
      const data = await apiGet(`/admin/products/${editingProduct.value.id}/images`)
      // API returns array directly, map to flat structure
      productImages.value = data.map((img: any) => ({
        id: img.id,
        mediaFileId: img.mediaFileId,
        isMain: img.isMain,
        sortOrder: img.sortOrder,
        fileName: img.mediaFile.fileName,
        filePath: img.mediaFile.filePath,
        thumbnailPath: img.mediaFile.thumbnailPath,
        altText: img.mediaFile.altText
      }))
    } catch (err: any) {
      logError('Failed to load product images', err);
    }
  }, 'loadProductImages')
}

const handleProductMediaSelection = async (mediaFile: any) => {
  if (!editingProduct.value) return
  
  await executeWithLoading(async () => {
    try {
      await apiPost(`/admin/products/${editingProduct.value.id}/images`, {
        mediaFileId: mediaFile.id,
        isMain: productImages.value.length === 0
      })

      await loadProductImages()
      showProductMediaPicker.value = false
      toast.success('Image linked to product')
    } catch (err: any) {
      toast.error(err.message || 'Failed to link image')
    }
  }, 'productMedia')
}

const removeProductImage = async (imageId: number) => {
  if (!confirm('Remove this image?')) return
  if (!editingProduct.value) return
  
  await executeWithLoading(async () => {
    try {
      await apiDelete(`/admin/products/${editingProduct.value.id}/images/${imageId}`)

      await loadProductImages()
      toast.deleteSuccess('Image')
    } catch (err: any) {
      toast.deleteError(err.message || 'Failed to remove image')
    }
  }, `removeImage-${imageId}`)
}

const setProductImageAsMain = async (imageId: number) => {
  if (!editingProduct.value) return
  
  await executeWithLoading(async () => {
    try {
      // Find the image to get its current sort order
      const image = productImages.value.find(img => img.id === imageId)
      if (!image) return

      await apiPut(`/admin/products/${editingProduct.value.id}/images/${imageId}`, {
        isMain: true,
        sortOrder: image.sortOrder || 0
      })

      await loadProductImages()
      toast.success('Main image updated')
    } catch (err: any) {
      toast.error(err.message || 'Failed to set main image')
    }
  }, `setMainImage-${imageId}`)
}

// ─── Inventory Tab ───────────────────────────────────────────────────────────
type InventoryEdit = { sku: string | null; costPrice: number | null; stockQuantity: number; lowStockThreshold: number }
const inventoryEdits = ref<Record<number, InventoryEdit>>({})
const inventorySearch = ref('')
const inventoryCategoryFilter = ref<number | ''>()
const inventoryStockFilter = ref<'' | 'in' | 'low' | 'out'>('')

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

const getStockInfo = (product: Product): { status: 'out' | 'low' | 'ok' | 'unknown'; label: string; rowClass: string } => {
  const e = inventoryEdits.value[product.id]
  if (!e) return { status: 'unknown', label: '—', rowClass: '' }
  if (e.stockQuantity === 0) return { status: 'out', label: '🔴 Out of Stock', rowClass: 'row-danger' }
  if (e.stockQuantity <= e.lowStockThreshold) return { status: 'low', label: '🟡 Low Stock', rowClass: 'row-warning' }
  return { status: 'ok', label: '🟢 In Stock', rowClass: '' }
}

const getMarginInfo = (price: number, cost: number | null): { value: number | null; label: string; cssClass: string } => {
  if (!cost || cost <= 0) return { value: null, label: '—', cssClass: '' }
  const m = Math.round(((price - cost) / price) * 100)
  const cssClass = m >= 30 ? 'margin-good' : m >= 15 ? 'margin-ok' : 'margin-low'
  return { value: m, label: m + '%', cssClass }
}

const saveInventoryRow = async (productId: number) => {
  const e = inventoryEdits.value[productId]
  if (!e) return
  
  await executeWithLoading(async () => {
    try {
      // Use apiClient - note: PATCH method will need custom handling
      const response = await apiClient(`/products/${productId}/stock`, {
        method: 'PATCH',
        body: JSON.stringify({ 
          stockQuantity: e.stockQuantity, 
          lowStockThreshold: e.lowStockThreshold, 
          sku: e.sku, 
          costPrice: e.costPrice 
        })
      })
      const product = products.value.find(p => p.id === productId)
      if (product) {
        product.stockQuantity = e.stockQuantity
        product.lowStockThreshold = e.lowStockThreshold
        product.sku = e.sku
        product.costPrice = e.costPrice
      }
    } catch (err) {
      logError('Failed to save inventory', err as Error)
    }
  }, `inventory-${productId}`)
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
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
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
  white-space: nowrap;
  flex-shrink: 0;
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

/* ---- Image Preview Styles ---- */
.current-image {
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.preview-image {
  width: 120px;
  height: 120px;
  object-fit: cover;
  border-radius: 8px;
  border: 2px solid #e5e7eb;
}

.images-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.image-card {
  position: relative;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  overflow: hidden;
  background: white;
}

.image-card img {
  width: 100%;
  height: 140px;
  object-fit: cover;
  display: block;
}

.image-actions {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  padding: 0.4rem;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  gap: 0.4rem;
}

.image-actions .btn-xs {
  padding: 0.25rem 0.4rem;
  font-size: 0.7rem;
  flex: 1 1 auto;
  white-space: nowrap;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
}

.main-badge {
  position: absolute;
  top: 0.5rem;
  left: 0.5rem;
  background: #059669;
  color: white;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 600;
}

/* Specifications styling */
.specs-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.spec-item {
  display: grid;
  grid-template-columns: 1fr 2fr auto;
  gap: 0.5rem;
  align-items: center;
}

.spec-key,
.spec-value {
  padding: 0.5rem;
  border: 1px solid var(--border);
  border-radius: 4px;
  font-size: 0.875rem;
}

.spec-remove {
  flex-shrink: 0;
  padding: 0.375rem 0.5rem;
  min-width: auto;
}
</style>
