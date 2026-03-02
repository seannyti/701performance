<template>
  <div class="home">
    <!-- Hero Section -->
    <section class="hero">
      <div class="hero-background">
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

    <!-- Featured Products Section -->
    <section class="section">
      <div class="container">
        <h2 class="section-title">Featured Products</h2>
        <p class="section-subtitle">
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

    <!-- Features Section -->
    <section class="section features-section">
      <div class="container">
        <h2 class="section-title">Why Choose Us?</h2>
        <div class="grid grid-3">
          <div class="feature-card">
            <div class="feature-icon">🛍️</div>
            <h3 class="feature-title">Wide Selection</h3>
            <p class="feature-description">
              From ATVs to snowmobiles, we have everything you need for your next adventure.
            </p>
          </div>
          <div class="feature-card">
            <div class="feature-icon">⭐</div>
            <h3 class="feature-title">Quality Brands</h3>
            <p class="feature-description">
              We partner with top manufacturers to bring you reliable, high-performance vehicles.
            </p>
          </div>
          <div class="feature-card">
            <div class="feature-icon">🔧</div>
            <h3 class="feature-title">Expert Support</h3>
            <p class="feature-description">
              Our knowledgeable team is here to help you find the perfect gear for your needs.
            </p>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import ProductCard from '@/components/ProductCard.vue';
import { productService } from '@/services/api';
import type { Product } from '@/types';
import { useSettings } from '@/composables/useSettings';

const { getSetting } = useSettings();

const router = useRouter();

// Reactive data
const featuredProducts = ref<Product[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);

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

// Handle view product details
const handleViewDetails = (_product: Product) => {
  // For now, just navigate to products page
  // In a real app, you might navigate to a product detail page
  router.push('/products');
};

// Load data on component mount
onMounted(() => {
  loadFeaturedProducts();
});
</script>

<style scoped>
.hero {
  position: relative;
  min-height: 70vh;
  display: flex;
  align-items: center;
  color: white;
  overflow: hidden;
}

.hero-background {
  position: relative;
  width: 100%;
  background: linear-gradient(
    135deg,
    rgba(255, 107, 53, 0.9) 0%,
    rgba(26, 26, 26, 0.8) 100%
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

  .feature-card {
    padding: 1.5rem 1rem;
  }

  .feature-title {
    font-size: 1.25rem;
  }
}

@media (max-width: 480px) {
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