<template>
  <div class="product-card" @click="onViewDetails">
    <div class="product-image">
      <img 
        :src="mainImageUrl" 
        :alt="product.name"
        :data-image-effect="imageEffect"
        @error="handleImageError"
        loading="lazy"
      />
      <div class="product-category">{{ product.category }}</div>
    </div>
    
    <div class="product-content">
      <h3 class="product-name">{{ product.name }}</h3>
      <p class="product-description">{{ product.description }}</p>
      <div class="product-footer">
        <span class="product-price">${{ formatPrice(product.price) }}</span>
        <button class="product-button" @click.stop="onViewDetails">
          View Details
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { Product } from '@/types';
import { useSettings } from '@/composables/useSettings';

// Props
interface Props {
  product: Product;
}

const props = defineProps<Props>();

// Settings
const { getSetting } = useSettings();

// Emits
const emit = defineEmits<{
  viewDetails: [product: Product];
}>();

// Constants
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226';

// Computed
const mainImageUrl = computed(() => {
  // Use the main image from productImages array, or fall back to imageUrl
  if (props.product.productImages && props.product.productImages.length > 0) {
    const mainImage = props.product.productImages.find(img => img.isMain);
    const url = mainImage?.url || props.product.productImages[0]?.url || props.product.imageUrl;
    // Prefix relative URLs with API base URL
    return url && !url.startsWith('http') ? `${API_BASE_URL}${url}` : url;
  }
  const url = props.product.imageUrl;
  return url && !url.startsWith('http') ? `${API_BASE_URL}${url}` : url;
});

const imageEffect = computed(() => {
  return getSetting('theme_image_hover', 'zoom');
});

// Methods
const formatPrice = (price: number): string => {
  return price.toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
};

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement;
  // Fallback to a placeholder image if the original fails to load
  target.src = 'https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=400&h=300&fit=crop';
};

const onViewDetails = () => {
  emit('viewDetails', props.product);
};
</script>

<style scoped>
.product-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: all 0.3s ease;
  height: 100%;
  display: flex;
  flex-direction: column;
  cursor: pointer;
}

.product-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
}

.product-image {
  position: relative;
  aspect-ratio: 4/3;
  overflow: hidden;
}

.product-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform var(--transition-duration, 0.3s) var(--transition-timing, ease);
}

.product-card:hover .product-image img {
  transform: scale(var(--hover-scale, 1.05));
}

.product-category {
  position: absolute;
  top: 12px;
  right: 12px;
  background-color: var(--color-primary, #ff6b35);
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: var(--button-radius, 20px);
  font-size: 0.875rem;
  font-weight: var(--button-font-weight, 600);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.product-content {
  padding: 1.5rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.product-name {
  font-size: 1.25rem;
  font-weight: bold;
  color: var(--color-text-primary, #ffffff);
  margin: 0 0 0.75rem 0;
  line-height: 1.4;
}

.product-description {
  color: var(--color-text-secondary, #C8C8C8);
  line-height: 1.6;
  margin: 0 0 1.5rem 0;
  flex: 1;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.product-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
}

.product-price {
  font-size: 1.5rem;
  font-weight: var(--font-weight-heading, bold);
  color: var(--color-primary, #ff6b35);
}

.product-button {
  background-color: var(--color-primary, #ff6b35);
  color: white;
  border: none;
  padding: var(--button-padding-y, 0.75rem) var(--button-padding-x, 1.5rem);
  border-radius: var(--button-radius, 8px);
  font-weight: var(--button-font-weight, 600);
  cursor: pointer;
  transition: all var(--transition-duration, 0.3s) var(--transition-timing, ease);
  font-size: 0.9rem;
}

.product-button:hover {
  background-color: var(--color-primary, #ff6b35);
  filter: brightness(0.9);
  transform: translateY(calc(var(--hover-lift-amount, 4px) * -0.25));
}

.product-button:active {
  transform: translateY(0);
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .product-content {
    padding: 1rem;
  }

  .product-name {
    font-size: 1.1rem;
  }

  .product-price {
    font-size: 1.3rem;
  }

  .product-button {
    padding: 0.6rem 1.2rem;
    font-size: 0.85rem;
  }

  .product-footer {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }

  .product-button {
    width: 100%;
    text-align: center;
  }
}
</style>