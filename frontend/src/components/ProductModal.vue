<template>
  <Teleport to="body">
    <Transition name="modal" appear>
      <div v-if="isOpen" key="modal" class="modal-overlay" @click.self="closeModal">
        <div class="modal-container">
          <button class="modal-close" @click="closeModal" aria-label="Close modal">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
          
          <div class="modal-content">
            <div class="product-modal-layout">
              <!-- Product Image Section with Gallery -->
              <div class="product-modal-image">
                <div class="image-gallery">
                  <!-- Main Image Display -->
                  <div class="main-image-container">
                    <img 
                      :src="currentImage" 
                      :alt="product?.name"
                      @error="handleImageError"
                      class="main-image"
                    />
                    
                    <!-- Navigation Arrows (only show if multiple images) -->
                    <template v-if="hasMultipleImages">
                      <button 
                        class="gallery-nav gallery-nav-prev" 
                        @click="previousImageManual"
                        aria-label="Previous image"
                      >
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                          <polyline points="15 18 9 12 15 6"></polyline>
                        </svg>
                      </button>
                      <button 
                        class="gallery-nav gallery-nav-next" 
                        @click="nextImageManual"
                        aria-label="Next image"
                      >
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                          <polyline points="9 18 15 12 9 6"></polyline>
                        </svg>
                      </button>
                    </template>
                    
                    <!-- Image Counter -->
                    <div v-if="hasMultipleImages" class="image-counter">
                      {{ currentImageIndex + 1 }} / {{ imageUrls.length }}
                    </div>
                  </div>
                  
                  <!-- Thumbnail Navigation (only show if multiple images) -->
                  <div v-if="hasMultipleImages" class="thumbnail-strip">
                    <button
                      v-for="(url, index) in imageUrls"
                      :key="index"
                      class="thumbnail"
                      :class="{ active: index === currentImageIndex }"
                      @click="selectImage(index)"
                    >
                      <img :src="url" :alt="`${product?.name} - Image ${index + 1}`" />
                    </button>
                  </div>
                </div>
              </div>
              
              <!-- Product Details Section -->
              <div class="product-modal-details">
                <div class="product-modal-header">
                  <h2 class="product-modal-title">{{ product?.name }}</h2>
                  <span class="product-modal-category">{{ product?.category }}</span>
                </div>
                
                <div class="product-modal-price">
                  <span class="price-label">Price:</span>
                  <span class="price-value">${{ formatPrice(product?.price ?? 0) }}</span>
                </div>
                
                <div class="product-modal-description">
                  <h3>Description</h3>
                  <p>{{ product?.description || 'No description available.' }}</p>
                </div>
                
                <div class="product-modal-info">
                  <div v-if="product?.sku" class="info-item">
                    <span class="info-label">SKU:</span>
                    <span class="info-value">{{ product.sku }}</span>
                  </div>
                  
                  <div class="info-item">
                    <span class="info-label">Availability:</span>
                    <span class="info-value" :class="stockStatusClass">
                      {{ stockStatus }}
                    </span>
                  </div>
                  
                  <div v-if="product?.stockQuantity !== undefined" class="info-item">
                    <span class="info-label">Stock:</span>
                    <span class="info-value">{{ product.stockQuantity }} units</span>
                  </div>
                </div>
                
                <div class="product-modal-actions">
                  <button class="btn-primary" @click="handleInquire">
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
                    </svg>
                    Inquire About This Product
                  </button>
                </div>
              </div>
            </div>
            
            <!-- Specifications Section - Full Width Below -->
            <div v-if="specifications.length > 0" class="product-modal-specs-section">
              <h2 class="specs-title">{{ product?.name }} &ndash; Specs</h2>
              <table class="specs-table">
                <tbody>
                  <tr v-for="spec in specifications" :key="spec.key" class="spec-row">
                    <td class="spec-label">{{ spec.key }}</td>
                    <td class="spec-value">{{ spec.value }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import type { Product } from '@/types';
import { logDebug, logError } from '@/services/logger';

// Props
interface Props {
  isOpen: boolean;
  product: Product | null;
}

const props = defineProps<Props>();

// Emits
const emit = defineEmits<{
  close: [];
}>();

// Router
const router = useRouter();

// Gallery state
const currentImageIndex = ref(0);
const autoScrollInterval = ref<number | null>(null);

// Methods
const closeModal = () => {
  emit('close');
};

const formatPrice = (price: number): string => {
  return price.toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
};

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement;
  target.src = 'https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=800&h=600&fit=crop';
};

const handleInquire = () => {
  closeModal();
  router.push('/contact');
};

const nextImage = () => {
  if (currentImageIndex.value < imageUrls.value.length - 1) {
    currentImageIndex.value++;
  } else {
    currentImageIndex.value = 0; // Loop back to first image
  }
};

const previousImage = () => {
  if (currentImageIndex.value > 0) {
    currentImageIndex.value--;
  } else {
    currentImageIndex.value = imageUrls.value.length - 1; // Loop to last image
  }
};

const nextImageManual = () => {
  nextImage();
  resetAutoScroll(); // Reset timer on manual navigation
};

const previousImageManual = () => {
  previousImage();
  resetAutoScroll(); // Reset timer on manual navigation
};

const startAutoScroll = () => {
  stopAutoScroll(); // Clear any existing interval
  
  if (hasMultipleImages.value && props.isOpen) {
    autoScrollInterval.value = window.setInterval(() => {
      nextImage();
    }, 6000); // 6 seconds
  }
};

const stopAutoScroll = () => {
  if (autoScrollInterval.value !== null) {
    clearInterval(autoScrollInterval.value);
    autoScrollInterval.value = null;
  }
};

const resetAutoScroll = () => {
  stopAutoScroll();
  startAutoScroll();
};

const selectImage = (index: number) => {
  currentImageIndex.value = index;
  resetAutoScroll(); // Reset timer when user manually selects image
};

// Constants
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226';

// Computed
const imageUrls = computed(() => {
  if (!props.product) return [];
  
  // If product has productImages array with data, use those
  if (props.product.productImages && props.product.productImages.length > 0) {
    return props.product.productImages.map(img => {
      const url = img.url || img.thumbnailUrl || '';
      // Prefix relative URLs with API base URL
      return url.startsWith('http') ? url : `${API_BASE_URL}${url}`;
    });
  }
  
  // Fallback to legacy imageUrl
  if (props.product.imageUrl) {
    const url = props.product.imageUrl;
    return [url.startsWith('http') ? url : `${API_BASE_URL}${url}`];
  }
  
  return [];
});

const currentImage = computed(() => {
  return imageUrls.value[currentImageIndex.value] || 'https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=800&h=600&fit=crop';
});

const hasMultipleImages = computed(() => {
  return imageUrls.value.length > 1;
});

const specifications = computed<Array<{ key: string; value: string }>>(() => {
  logDebug('ProductModal - Specifications check', {
    hasProduct: !!props.product,
    rawSpecsLength: props.product?.specifications?.length || 0,
    specsType: typeof props.product?.specifications
  });
  
  if (!props.product?.specifications) return [];
  
  try {
    const parsed = JSON.parse(props.product.specifications);
    logDebug('Parsed specifications successfully', { itemCount: parsed.length });
    return Array.isArray(parsed) ? parsed : [];
  } catch (e) {
    logError('Failed to parse product specifications', e);
    return [];
  }
});

const stockStatus = computed(() => {
  if (!props.product) return 'Unknown';
  
  const { stockQuantity, lowStockThreshold } = props.product;
  
  if (stockQuantity === 0) return 'Out of Stock';
  if (stockQuantity <= lowStockThreshold) return 'Low Stock';
  return 'In Stock';
});

const stockStatusClass = computed(() => {
  if (!props.product) return '';
  
  const { stockQuantity, lowStockThreshold } = props.product;
  
  if (stockQuantity === 0) return 'status-out';
  if (stockQuantity <= lowStockThreshold) return 'status-low';
  return 'status-in';
});

// Reset image index when product changes
watch(() => props.product, () => {
  currentImageIndex.value = 0;
  resetAutoScroll();
});

// Watch for modal open/close to start/stop auto-scroll
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    startAutoScroll();
  } else {
    stopAutoScroll();
  }
});

// Keyboard navigation
const handleKeydown = (event: KeyboardEvent) => {
  if (!props.isOpen) return;
  
  if (event.key === 'Escape') {
    closeModal();
  } else if (event.key === 'ArrowLeft') {
    previousImageManual();
  } else if (event.key === 'ArrowRight') {
    nextImageManual();
  }
};

// Add/remove event listener
onMounted(() => {
  if (typeof window !== 'undefined') {
    window.addEventListener('keydown', handleKeydown);
  }
  // Start auto-scroll if modal is already open
  if (props.isOpen) {
    startAutoScroll();
  }
});

onUnmounted(() => {
  if (typeof window !== 'undefined') {
    window.removeEventListener('keydown', handleKeydown);
  }
  stopAutoScroll();
});
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
  overflow-y: auto;
}

.modal-container {
  position: relative;
  background: white;
  border-radius: 16px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  max-width: 1200px;
  width: 100%;
  max-height: 90vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modal-close {
  position: absolute;
  top: 16px;
  right: 16px;
  background: rgba(0, 0, 0, 0.5);
  border: none;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
  transition: all 0.3s ease;
  color: white;
}

.modal-close:hover {
  background: rgba(0, 0, 0, 0.8);
  transform: rotate(90deg);
}

.modal-content {
  overflow-y: auto;
  flex: 1;
}

.product-modal-layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 40px;
  padding: 40px;
}

.product-modal-image {
  position: relative;
  border-radius: 12px;
  overflow: visible;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.image-gallery {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.main-image-container {
  position: relative;
  border-radius: 12px;
  overflow: hidden;
  background: #f8f9fa;
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.main-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: opacity 0.3s ease;
}

.gallery-nav {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(0, 0, 0, 0.6);
  border: none;
  border-radius: 50%;
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 5;
  transition: all 0.3s ease;
  color: white;
}

.gallery-nav:hover {
  background: rgba(0, 0, 0, 0.8);
  transform: translateY(-50%) scale(1.1);
}

.gallery-nav-prev {
  left: 16px;
}

.gallery-nav-next {
  right: 16px;
}

.image-counter {
  position: absolute;
  bottom: 16px;
  right: 16px;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  padding: 8px 16px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
  z-index: 5;
}

.thumbnail-strip {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding: 4px;
  scrollbar-width: thin;
  scrollbar-color: rgba(102, 126, 234, 0.3) transparent;
}

.thumbnail-strip::-webkit-scrollbar {
  height: 6px;
}

.thumbnail-strip::-webkit-scrollbar-track {
  background: transparent;
}

.thumbnail-strip::-webkit-scrollbar-thumb {
  background: rgba(102, 126, 234, 0.3);
  border-radius: 3px;
}

.thumbnail-strip::-webkit-scrollbar-thumb:hover {
  background: rgba(102, 126, 234, 0.5);
}

.thumbnail {
  flex-shrink: 0;
  width: 80px;
  height: 80px;
  border-radius: 8px;
  overflow: hidden;
  border: 3px solid transparent;
  cursor: pointer;
  transition: all 0.3s ease;
  background: #f8f9fa;
  padding: 0;
}

.thumbnail img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.thumbnail:hover {
  border-color: rgba(102, 126, 234, 0.5);
  transform: scale(1.05);
}

.thumbnail.active {
  border-color: #667eea;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.product-modal-details {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.product-modal-header {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.product-modal-title {
  font-size: 32px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0;
  line-height: 1.2;
}

.product-modal-category {
  display: inline-flex;
  align-items: center;
  padding: 6px 16px;
  background: var(--gradient);
  color: white;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
  width: fit-content;
}

.product-modal-price {
  display: flex;
  align-items: baseline;
  gap: 12px;
  padding: 16px 0;
  border-top: 2px solid #e9ecef;
  border-bottom: 2px solid #e9ecef;
}

.price-label {
  font-size: 18px;
  color: #6c757d;
  font-weight: 500;
}

.price-value {
  font-size: 36px;
  font-weight: 700;
  color: #667eea;
}

.product-modal-description {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.product-modal-description h3 {
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

.product-modal-description p {
  font-size: 16px;
  line-height: 1.6;
  color: #495057;
  margin: 0;
}

/* Specifications Section - Full Width */
.product-modal-specs-section {
  padding: 0 40px 40px 40px;
}

.specs-title {
  font-size: 24px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 24px 0;
  padding-bottom: 16px;
  border-bottom: 3px solid #1a1a1a;
}

.specs-table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  border: 1px solid #dee2e6;
}

.spec-row:nth-child(odd) {
  background-color: #f8f9fa;
}

.spec-row:nth-child(even) {
  background-color: white;
}

.spec-label {
  font-size: 15px;
  font-weight: 600;
  color: #1a1a1a;
  padding: 16px 24px;
  border-bottom: 1px solid #dee2e6;
  border-right: 1px solid #dee2e6;
  width: 35%;
  vertical-align: top;
}

.spec-value {
  font-size: 15px;
  color: #495057;
  padding: 16px 24px;
  border-bottom: 1px solid #dee2e6;
  vertical-align: top;
}

.spec-row:last-child .spec-label,
.spec-row:last-child .spec-value {
  border-bottom: none;
}

.product-modal-info {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 12px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.info-label {
  font-size: 14px;
  font-weight: 600;
  color: #6c757d;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-value {
  font-size: 16px;
  font-weight: 600;
  color: #1a1a1a;
}

.status-in {
  color: #28a745;
}

.status-low {
  color: #ffc107;
}

.status-out {
  color: #dc3545;
}

.product-modal-actions {
  display: flex;
  gap: 12px;
  margin-top: 8px;
}

.btn-primary {
  flex: 1;
  padding: 16px 24px;
  background: var(--gradient);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
}

.btn-primary:active {
  transform: translateY(0);
}

/* Modal Transitions */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-active .modal-container,
.modal-leave-active .modal-container {
  transition: transform 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: scale(0.9);
}

/* Responsive Design */
@media (max-width: 968px) {
  .product-modal-layout {
    grid-template-columns: 1fr;
    gap: 24px;
    padding: 24px;
  }
  
  .main-image-container {
    min-height: 300px;
  }
  
  .product-modal-title {
    font-size: 24px;
  }
  
  .price-value {
    font-size: 28px;
  }
  
  .gallery-nav {
    width: 40px;
    height: 40px;
  }
  
  .gallery-nav-prev {
    left: 12px;
  }
  
  .gallery-nav-next {
    right: 12px;
  }
  
  .thumbnail {
    width: 60px;
    height: 60px;
  }
}

@media (max-width: 640px) {
  .modal-overlay {
    padding: 0;
  }
  
  .modal-container {
    border-radius: 0;
    max-height: 100vh;
  }
  
  .product-modal-layout {
    padding: 16px;
    gap: 16px;
  }
  
  .modal-close {
    top: 12px;
    right: 12px;
    width: 36px;
    height: 36px;
  }
  
  .gallery-nav {
    width: 36px;
    height: 36px;
  }
  
  .gallery-nav-prev {
    left: 8px;
  }
  
  .gallery-nav-next {
    right: 8px;
  }
  
  .thumbnail {
    width: 50px;
    height: 50px;
  }
  
  .image-counter {
    font-size: 12px;
    padding: 6px 12px;
  }

  .product-modal-specs-section {
    padding: 0 16px 24px;
  }
}
</style>
