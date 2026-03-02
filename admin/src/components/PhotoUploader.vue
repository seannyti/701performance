<template>
  <div class="photo-uploader">
    <!-- Upload Area -->
    <div 
      v-if="canUpload"
      class="upload-zone" 
      :class="{ dragging: isDragging }"
      @drop.prevent="handleDrop"
      @dragover.prevent="isDragging = true"
      @dragleave="isDragging = false"
      @click="triggerFileInput"
    >
      <input 
        ref="fileInput"
        type="file" 
        :accept="acceptedTypes"
        :multiple="maxFiles > 1"
        @change="handleFileSelect"
        style="display: none;"
      />
      
      <div class="upload-icon">📸</div>
      <p class="upload-text">
        <strong>Click to upload</strong> or drag and drop
      </p>
      <p class="upload-hint">
        {{ maxFiles > 1 ? `Up to ${maxFiles} images` : 'Single image only' }} • 
        JPG, PNG, WebP • Max 10MB
      </p>
    </div>

    <!-- Uploaded Images -->
    <div v-if="images.length > 0" class="images-grid">
      <div 
        v-for="image in images" 
        :key="image.fileName"
        class="image-item"
      >
        <img :src="`http://localhost:5226/uploads/${entityType}s/${entityId}/thumb_${image.fileName}`" :alt="image.fileName" />
        
        <div class="image-overlay">
          <button 
            v-if="maxFiles > 1 && !image.isDefault"
            @click="setAsMain(image.fileName)" 
            class="btn-icon"
            title="Set as main image"
          >
            ⭐
          </button>
          <button 
            @click="deleteImage(image.fileName)" 
            class="btn-icon btn-danger"
            title="Delete image"
          >
            🗑️
          </button>
        </div>

        <div v-if="image.isDefault" class="main-badge">Main</div>
      </div>
    </div>

    <!-- Upload Progress -->
    <div v-if="uploading" class="upload-progress">
      <div class="progress-bar">
        <div class="progress-fill" :style="{ width: `${uploadProgress}%` }"></div>
      </div>
      <p class="progress-text">Uploading... {{ uploadProgress }}%</p>
    </div>

    <!-- Error Message -->
    <div v-if="error" class="error-message">
      {{ error }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useAuthStore } from '@/stores/auth'

interface UploadedImage {
  fileName: string
  fileSize: number
  uploadDate: string
  isDefault: boolean
}

interface Props {
  entityType: 'product' | 'category'
  entityId: number | null
  maxFiles?: number
}

const props = withDefaults(defineProps<Props>(), {
  maxFiles: 10
})

const emit = defineEmits<{
  (e: 'updated'): void
}>()

const authStore = useAuthStore()
const API_URL = 'http://localhost:5226/api/v1'

const images = ref<UploadedImage[]>([])
const fileInput = ref<HTMLInputElement | null>(null)
const isDragging = ref(false)
const uploading = ref(false)
const uploadProgress = ref(0)
const error = ref('')

const acceptedTypes = 'image/jpeg,image/png,image/webp'

const canUpload = computed(() => {
  if (!props.entityId) return false
  if (props.maxFiles === 1) return images.value.length === 0
  return images.value.length < props.maxFiles
})

// Load existing images
const loadImages = async () => {
  if (!props.entityId) return
  
  try {
    const response = await fetch(`${API_URL}/photos/${props.entityType}/${props.entityId}`, {
      headers: { 'Authorization': `Bearer ${authStore.token}` }
    })
    
    if (response.ok) {
      const data = await response.json()
      images.value = data.files || []
    }
  } catch (err: any) {
    console.error('Failed to load images:', err)
  }
}

// Watch for entityId changes and reload
watch(() => props.entityId, (newId) => {
  if (newId) {
    loadImages()
  }
}, { immediate: true })

// Trigger file input
const triggerFileInput = () => {
  fileInput.value?.click()
}

// Handle file selection
const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files) {
    uploadFiles(Array.from(target.files))
  }
}

// Handle drag and drop
const handleDrop = (event: DragEvent) => {
  isDragging.value = false
  if (event.dataTransfer?.files) {
    uploadFiles(Array.from(event.dataTransfer.files))
  }
}

// Validate and upload files
const uploadFiles = async (files: File[]) => {
  if (!props.entityId) {
    error.value = 'Please save the item first before uploading images'
    return
  }

  error.value = ''
  
  // Check file count
  const remainingSlots = props.maxFiles - images.value.length
  if (files.length > remainingSlots) {
    error.value = `You can only upload ${remainingSlots} more image(s)`
    return
  }

  // Validate each file
  for (const file of files) {
    if (file.size > 10 * 1024 * 1024) {
      error.value = `${file.name} is too large. Max size is 10MB`
      return
    }
    
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      error.value = `${file.name} is not a supported image type`
      return
    }
  }

  // Upload files
  uploading.value = true
  uploadProgress.value = 0

  try {
    for (let i = 0; i < files.length; i++) {
      const file = files[i]
      const formData = new FormData()
      formData.append('file', file)
      formData.append('entityType', props.entityType)
      formData.append('entityId', props.entityId.toString())

      console.log('Uploading:', {
        fileName: file.name,
        fileSize: file.size,
        fileType: file.type,
        entityType: props.entityType,
        entityId: props.entityId,
        hasToken: !!authStore.token
      })

      const response = await fetch(`${API_URL}/photos/upload`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        },
        body: formData
      })

      if (!response.ok) {
        const errorText = await response.text()
        console.error('Upload error response:', errorText)
        let errorMessage = 'Upload failed'
        try {
          const errorData = JSON.parse(errorText)
          errorMessage = errorData.message || errorData.title || 'Upload failed'
        } catch {
          errorMessage = errorText || 'Upload failed'
        }
        throw new Error(errorMessage)
      }

      uploadProgress.value = Math.round(((i + 1) / files.length) * 100)
    }

    await loadImages()
    emit('updated')
    
    // Reset file input
    if (fileInput.value) {
      fileInput.value.value = ''
    }
  } catch (err: any) {
    error.value = err.message || 'Failed to upload image'
  } finally {
    uploading.value = false
    uploadProgress.value = 0
  }
}

// Set image as main
const setAsMain = async (fileName: string) => {
  if (!props.entityId) return
  
  try {
    const response = await fetch(`${API_URL}/photos/${props.entityType}/${props.entityId}/${fileName}/default`, {
      method: 'PATCH',
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      }
    })

    if (!response.ok) throw new Error('Failed to set main image')

    await loadImages()
    emit('updated')
  } catch (err: any) {
    error.value = err.message || 'Failed to set main image'
  }
}

// Delete image
const deleteImage = async (fileName: string) => {
  if (!props.entityId) return
  if (!confirm('Are you sure you want to delete this image?')) return

  try {
    const response = await fetch(`${API_URL}/photos/${props.entityType}/${props.entityId}/${fileName}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      }
    })

    if (!response.ok) throw new Error('Failed to delete image')

    await loadImages()
    emit('updated')
  } catch (err: any) {
    error.value = err.message || 'Failed to delete image'
  }
}

// Expose loadImages for parent component
defineExpose({ loadImages })
</script>

<style scoped>
.photo-uploader {
  width: 100%;
}

.upload-zone {
  border: 2px dashed #d1d5db;
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
  background: #f9fafb;
}

.upload-zone:hover {
  border-color: #4f46e5;
  background: #f3f4f6;
}

.upload-zone.dragging {
  border-color: #4f46e5;
  background: #eef2ff;
}

.upload-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.upload-text {
  margin: 0;
  color: #374151;
  font-size: 0.938rem;
}

.upload-hint {
  margin: 0.5rem 0 0 0;
  color: #6b7280;
  font-size: 0.813rem;
}

.images-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
  margin-top: 1.5rem;
}

.image-item {
  position: relative;
  aspect-ratio: 1;
  border-radius: 8px;
  overflow: hidden;
  border: 2px solid #e5e7eb;
  transition: all 0.2s;
}

.image-item:hover {
  border-color: #4f46e5;
}

.image-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-overlay {
  position: absolute;
  top: 0;
  right: 0;
  padding: 0.5rem;
  display: flex;
  gap: 0.5rem;
  opacity: 0;
  transition: opacity 0.2s;
}

.image-item:hover .image-overlay {
  opacity: 1;
}

.btn-icon {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  border: none;
  background: rgba(255, 255, 255, 0.95);
  cursor: pointer;
  font-size: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.btn-icon:hover {
  transform: scale(1.1);
}

.btn-icon.btn-danger:hover {
  background: #fee2e2;
}

.main-badge {
  position: absolute;
  bottom: 0.5rem;
  left: 0.5rem;
  background: #4f46e5;
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.688rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.upload-progress {
  margin-top: 1rem;
}

.progress-bar {
  height: 8px;
  background: #e5e7eb;
  border-radius: 9999px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: #4f46e5;
  transition: width 0.3s;
}

.progress-text {
  margin: 0.5rem 0 0 0;
  text-align: center;
  color: #6b7280;
  font-size: 0.813rem;
}

.error-message {
  margin-top: 1rem;
  padding: 0.75rem 1rem;
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
  border-radius: 8px;
  font-size: 0.875rem;
}
</style>
