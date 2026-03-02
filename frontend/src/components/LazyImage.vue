<template>
  <div class="lazy-image-wrapper" :class="{ loaded: imageLoaded }">
    <img
      v-if="!imageLoaded"
      :src="placeholderSrc"
      :alt="alt"
      class="placeholder"
      :style="{ aspectRatio: aspectRatio }"
    />
    <img
      ref="imageRef"
      :src="imageSrc"
      :alt="alt"
      :class="['lazy-image', { visible: imageLoaded }]"
      @load="onImageLoad"
      @error="onImageError"
      loading="lazy"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

interface Props {
  src: string
  alt: string
  aspectRatio?: string
  placeholderColor?: string
}

const props = withDefaults(defineProps<Props>(), {
  aspectRatio: 'auto',
  placeholderColor: '#e5e7eb'
})

const imageRef = ref<HTMLImageElement | null>(null)
const imageLoaded = ref(false)
const imageSrc = ref('')
const placeholderSrc = ref(`data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='${props.placeholderColor}'/%3E%3C/svg%3E`)

let observer: IntersectionObserver | null = null

onMounted(() => {
  // Use Intersection Observer for lazy loading
  if ('IntersectionObserver' in window && imageRef.value) {
    observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting && !imageSrc.value) {
            imageSrc.value = props.src
            observer?.disconnect()
          }
        })
      },
      {
        rootMargin: '50px' // Start loading 50px before image enters viewport
      }
    )

    observer.observe(imageRef.value)
  } else {
    // Fallback for browsers without Intersection Observer
    imageSrc.value = props.src
  }
})

onUnmounted(() => {
  observer?.disconnect()
})

const onImageLoad = () => {
  imageLoaded.value = true
}

const onImageError = () => {
  // Fallback to placeholder on error
  imageSrc.value = placeholderSrc.value
  imageLoaded.value = true
}
</script>

<style scoped>
.lazy-image-wrapper {
  position: relative;
  overflow: hidden;
  background-color: #f3f4f6;
}

.placeholder {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: blur(10px);
  transform: scale(1.1);
}

.lazy-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0;
  transition: opacity 0.3s ease-in-out;
  position: absolute;
  top: 0;
  left: 0;
}

.lazy-image.visible {
  opacity: 1;
}

.lazy-image-wrapper.loaded .placeholder {
  display: none;
}
</style>
