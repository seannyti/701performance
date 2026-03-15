<template>
  <div class="home">
    <!-- Hero Section -->
    <section class="hero">
      <div 
        class="hero-background"
        :style="parallaxEnabled ? { transform: `translateY(${parallaxOffset}px)` } : {}"
      >
        <div class="container">
          <div class="hero-content">
            <h1 class="hero-title">
              {{ getSetting('hero_title', 'Powersports Gear & Vehicles') }}
            </h1>
            <p class="hero-subtitle">
              {{ getSetting('hero_subtitle', 'Shop our selection of ATVs, Dirt Bikes, UTVs, Snowmobiles, and Gear!') }}
            </p>
            <div class="hero-actions">
              <router-link to="/products" class="btn btn-primary btn-lg">
                Shop Now
              </router-link>
              <router-link to="/about" class="btn btn-outline btn-lg">
                Learn More
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Shop by Category Section -->
    <section class="section categories-section">
      <div class="container">
        <h2 class="section-title" data-animate="fade">Shop by Category</h2>
        
        <!-- Loading State -->
        <div v-if="categoriesLoading" class="loading">
          <p>Loading categories...</p>
        </div>

        <!-- Categories Grid -->
        <div v-else-if="categories.length > 0" class="categories-grid">
          <router-link
            v-for="category in categories"
            :key="category.id"
            :to="`/products?category=${category.name}`"
            class="category-card"
            :style="category.imageUrl ? {
              backgroundImage: `linear-gradient(rgba(0, 0, 0, 0.4), rgba(0, 0, 0, 0.6)), url(${category.imageUrl})`,
              backgroundSize: 'cover',
              backgroundPosition: 'center'
            } : {}"
          >
            <div class="category-content">
              <div v-if="!category.imageUrl" class="category-icon">{{ getCategoryIcon(category.name) }}</div>
              <h3 class="category-name" :class="{ 'with-image': category.imageUrl }">{{ category.name }}</h3>
              <p class="category-description" :class="{ 'with-image': category.imageUrl }">{{ category.description }}</p>
            </div>
          </router-link>
        </div>

        <!-- Empty State -->
        <div v-else class="text-center">
          <p class="text-light">No categories available at the moment.</p>
        </div>
      </div>
    </section>

    <!-- Featured Products Section -->
    <section class="section">
      <div class="container">
        <h2 class="section-title" data-animate="fade">Featured Products</h2>
        <p class="section-subtitle" data-animate="fade">
          Discover our top-rated powersports vehicles and gear
        </p>

        <!-- Loading State -->
        <div v-if="loading" class="loading">
          <p>Loading featured products...</p>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="error">
          <p>{{ error }}</p>
          <button @click="loadFeaturedProducts" class="btn btn-primary">
            Try Again
          </button>
        </div>

        <!-- Featured Products Grid -->
        <div v-else-if="featuredProducts.length > 0" class="grid grid-3">
          <ProductCard
            v-for="product in featuredProducts"
            :key="product.id"
            :product="product"
            @view-details="handleViewDetails"
          />
        </div>

        <!-- Empty State -->
        <div v-else class="text-center">
          <p class="text-light">No featured products available at the moment.</p>
        </div>

        <!-- Call to Action -->
        <div class="text-center mt-4" v-if="!loading && !error">
          <router-link to="/products" class="btn btn-primary">
            View All Products
          </router-link>
        </div>
      </div>
    </section>

    <!-- Brands Section -->
    <section v-if="brands.length > 0" class="section brands-section">
      <div class="container">
        <h2 class="section-title" data-animate="fade">
          {{ getSetting('brands_section_title', 'Brands We Carry') }}
        </h2>
        <p class="section-subtitle" data-animate="fade">
          {{ getSetting('brands_section_subtitle', 'We partner with industry-leading manufacturers') }}
        </p>
        <div class="brands-grid">
          <div 
            v-for="(brand, index) in brands" 
            :key="index" 
            class="brand-card"
            data-animate="fade"
          >
            <a 
              v-if="brand.website" 
              :href="brand.website" 
              target="_blank" 
              rel="noopener noreferrer"
              class="brand-link"
            >
              <img 
                v-if="brand.logoUrl" 
                :src="getMediaUrl(brand.logoUrl)" 
                :alt="brand.name"
                class="brand-logo"
              />
              <span v-else class="brand-name">{{ brand.name }}</span>
            </a>
            <div v-else class="brand-link">
              <img 
                v-if="brand.logoUrl" 
                :src="getMediaUrl(brand.logoUrl)" 
                :alt="brand.name"
                class="brand-logo"
              />
              <span v-else class="brand-name">{{ brand.name }}</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="section features-section">
      <div class="container">
        <h2 class="section-title" data-animate="fade">Why Choose Us?</h2>
        <div class="grid grid-3">
          <div 
            v-for="(feature, index) in features" 
            :key="index" 
            class="feature-card"
            data-animate="zoom"
          >
            <div class="feature-icon">{{ feature.icon }}</div>
            <h3 class="feature-title">{{ feature.title }}</h3>
            <p class="feature-description">
              {{ feature.description }}
            </p>
          </div>
        </div>
      </div>
    </section>
  </div>
  
  <!-- Product Details Modal -->
  <ProductModal
    :is-open="isModalOpen"
    :product="selectedProduct"
    @close="closeModal"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import ProductCard from '@/components/ProductCard.vue';
import ProductModal from '@/components/ProductModal.vue';
import { productService, categoryService } from '@/services/api';
import type { Product, Category } from '@/types';
import { useSettings } from '@/composables/useSettings';
import { useTheme } from '@/composables/useTheme';
import { useParallax } from '@/composables/useParallax';
import { useScrollAnimation } from '@/composables/useScrollAnimation';
import { logError } from '@/services/logger';
import { getMediaUrl } from '@/utils/api-config';

const { getSetting } = useSettings();
useTheme();
useScrollAnimation();

const router = useRouter();

// Parallax effect
const { parallaxOffset } = useParallax(0.5);
const parallaxEnabled = computed(() => getSetting('theme_parallax_enabled', 'false') === 'true');

// Reactive data
const featuredProducts = ref<Product[]>([]);
const categories = ref<Category[]>([]);
const loading = ref(true);
const categoriesLoading = ref(true);
const error = ref<string | null>(null);

// Modal state
const isModalOpen = ref(false);
const selectedProduct = ref<Product | null>(null);

// Map category names to icons
const getCategoryIcon = (categoryName: string): string => {
  const name = categoryName.toLowerCase();
  
  const iconMap: { [key: string]: string } = {
    'atv': '🏍️',
    'atvs': '🏍️',
    'utv': '🚙',
    'utvs': '🚙',
    'dirt bike': '🏍️',
    'dirt bikes': '🏍️',
    'motorcycle': '🏍️',
    'motorcycles': '🏍️',
    'snowmobile': '❄️',
    'snowmobiles': '❄️',
    'accessories': '🔧',
    'parts': '🔧',
    'gear': '🛡️',
    'apparel': '👕',
    'helmet': '🪖',
    'helmets': '🪖',
  };
  
  // Check for exact match
  if (iconMap[name]) {
    return iconMap[name];
  }
  
  // Check for partial match
  for (const key in iconMap) {
    if (name.includes(key)) {
      return iconMap[key];
    }
  }
  
  // Default icon
  return '🏁';
};

// Parse features from settings
const features = computed(() => {
  try {
    const featuresJson = getSetting('home_features', '[]');
    return JSON.parse(featuresJson);
  } catch {
    return [
      { icon: '🛍️', title: 'Wide Selection', description: 'From ATVs to snowmobiles, we have everything you need for your next adventure.' },
      { icon: '⭐', title: 'Quality Brands', description: 'We partner with top manufacturers to bring you reliable, high-performance vehicles.' },
      { icon: '🔧', title: 'Expert Support', description: 'Our knowledgeable team is here to help you find the perfect gear for your needs.' }
    ];
  }
});

// Parse brands from settings
const brands = computed(() => {
  try {
    const brandsJson = getSetting('partner_brands', '[]');
    return JSON.parse(brandsJson).filter((brand: any) => brand.name);
  } catch {
    return [];
  }
});

// Load featured products
const loadFeaturedProducts = async () => {
  try {
    loading.value = true;
    error.value = null;
    featuredProducts.value = await productService.getFeaturedProducts();
  } catch (err) {
    error.value = 'Failed to load featured products. Please try again later.';
  } finally {
    loading.value = false;
  }
};

// Load categories
const loadCategories = async () => {
  try {
    categoriesLoading.value = true;
    categories.value = await categoryService.getAllCategories();
  } catch (err) {
    logError('Failed to load categories', err);
    // Silently fail - categories section will just be empty
  } finally {
    categoriesLoading.value = false;
  }
};

// Handle view product details
const handleViewDetails = (product: Product) => {
  selectedProduct.value = product;
  isModalOpen.value = true;
};

const closeModal = () => {
  isModalOpen.value = false;
  // Delay clearing the product to allow modal animation to complete
  setTimeout(() => {
    selectedProduct.value = null;
  }, 300);
};

// Load data on component mount
onMounted(() => {
  loadFeaturedProducts();
  loadCategories();
});
</script>

<style scoped>
.home {
  margin: 0 !important;
  padding: 0 !important;
  display: block;
}

.hero {
  position: relative;
  min-height: 70vh;
  display: flex;
  align-items: center;
  color: white;
  overflow: hidden;
  margin: 0 !important;
  padding: 2rem 0 0 0 !important;
}

.hero-background {
  position: relative;
  width: 100%;
  background: linear-gradient(
    var(--gradient-direction, 135deg),
    var(--gradient-start, #ff6b35),
    var(--gradient-end, #1a1a1a)
  ),
  url('https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=1200&h=800&fit=crop') center/cover;
  padding: 4rem 0;
}

.hero-content {
  text-align: center;
  max-width: 800px;
  margin: 0 auto;
}

.hero-title {
  font-size: 3.5rem;
  font-weight: bold;
  margin-bottom: 1.5rem;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
}

.hero-subtitle {
  font-size: 1.5rem;
  margin-bottom: 2.5rem;
  opacity: 0.95;
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.5);
}

.hero-actions {
  display: flex;
  gap: 1.5rem;
  justify-content: center;
  flex-wrap: wrap;
}

.btn-lg {
  padding: 1rem 2rem;
  font-size: 1.125rem;
}

/* Categories Section */
.categories-section {
  background-color: #f8f9fa;
  padding-top: 0 !important;
}

.categories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.category-card {
  background: white;
  border-radius: 12px;
  min-height: 300px;
  text-align: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  text-decoration: none;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  position: relative;
}

.category-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
}

.category-content {
  padding: 3rem 2rem;
  width: 100%;
  z-index: 1;
}

.category-icon {
  font-size: 4rem;
  margin-bottom: 1.5rem;
  filter: grayscale(0%);
}

.category-name {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-dark);
  margin-bottom: 0.75rem;
  text-shadow: none;
}

.category-name.with-image {
  color: white;
  text-shadow: 2px 2px 8px rgba(0, 0, 0, 0.8);
  font-size: 2rem;
}

.category-description {
  color: #2563eb;
  font-size: 1rem;
  line-height: 1.5;
  margin: 0;
}

.category-description.with-image {
  color: white;
  text-shadow: 1px 1px 4px rgba(0, 0, 0, 0.8);
  font-size: 1.1rem;
}

.features-section {
  background-color: white;
}

.feature-card {
  text-align: center;
  padding: 2rem 1rem;
  background: white;
  border-radius: 12px;
  box-shadow: var(--shadow-md);
  transition: transform 0.3s ease;
}

.feature-card:hover {
  transform: translateY(-4px);
}

.feature-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.feature-title {
  font-size: 1.5rem;
  font-weight: bold;
  margin-bottom: 1rem;
  color: var(--text-dark);
}

.feature-description {
  color: var(--text-light);
  line-height: 1.6;
}

/* Mobile responsive */
@media (max-width: 768px) {
  .hero {
    min-height: 60vh;
    padding: 1.5rem 0 0 0 !important;
  }

  .hero-title {
    font-size: 2.5rem;
  }

  .hero-subtitle {
    font-size: 1.25rem;
  }

  .hero-actions {
    flex-direction: column;
    align-items: center;
  }

  .btn-lg {
    width: 100%;
    max-width: 300px;
  }

  .categories-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }

  .category-card {
    min-height: 250px;
  }

  .category-content {
    padding: 2rem 1.5rem;
  }

  .category-icon {
    font-size: 3rem;
  }

  .category-name {
    font-size: 1.25rem;
  }

  .category-name.with-image {
    font-size: 1.75rem;
  }

  .feature-card {
    padding: 1.5rem 1rem;
  }

  .feature-title {
    font-size: 1.25rem;
  }
}

/* Brands Section */
.brands-section {
  background-color: #ffffff;
  padding: 4rem 0;
}

.brands-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 2rem;
  margin-top: 2.5rem;
  align-items: center;
  justify-items: center;
}

.brand-card {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100px;
  transition: transform 0.3s ease;
}

.brand-card:hover {
  transform: translateY(-5px);
}

.brand-link {
  display: flex;
  align-items: center;
  justify-content: center;
  text-decoration: none;
  width: 100%;
  height: 100%;
  padding: 1rem;
}

.brand-logo {
  max-width: 160px;
  max-height: 80px;
  width: auto;
  height: auto;
  object-fit: contain;
  filter: grayscale(100%);
  opacity: 0.7;
  transition: all 0.3s ease;
}

.brand-logo:hover {
  filter: grayscale(0%);
  opacity: 1;
}

.brand-name {
  font-size: 1.25rem;
  font-weight: 600;
  color: #666;
  transition: color 0.3s ease;
}

.brand-link:hover .brand-name {
  color: var(--primary-color, #ff6b35);
}

@media (max-width: 480px) {
  .hero {
    padding: 1rem 0 0 0 !important;
  }

  .hero-title {
    font-size: 2rem;
  }

  .hero-subtitle {
    font-size: 1.125rem;
  }

  .hero-background {
    padding: 2rem 0;
  }
}
</style>