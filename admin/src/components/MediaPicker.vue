<template>
  <div v-if="isOpen" class="media-picker-overlay" @click.self="close">
    <div class="media-picker-modal">
      <div class="modal-header">
        <h3>Select Media</h3>
        <button @click="close" class="btn-close">&times;</button>
      </div>

      <div class="modal-body">
        <!-- Search and Filters -->
        <div class="filters-row">
          <input
            v-model="searchQuery"
            @input="searchMedia"
            type="text"
            class="form-control"
            placeholder="Search media..."
          />
          <select v-model="filterMediaType" @change="loadMedia" class="form-control">
            <option :value="null">All Types</option>
            <option value="0">Images</option>
            <option value="1">Videos</option>
            <option value="2">Documents</option>
            <option value="3">Other</option>
          </select>
          <select v-model="filterSection" @change="loadMedia" class="form-control">
            <option :value="null">All Sections</option>
            <option v-for="section in sections" :key="section.id" :value="section.id">
              {{ section.name }}
            </option>
          </select>
        </div>

        <!-- Media Grid -->
        <div class="media-grid">
          <div
            v-for="file in mediaFiles"
            :key="file.id"
            class="media-item"
            @click="selectMedia(file)"
            :class="{ selected: selectedMediaId === file.id }"
          >
            <div class="media-thumbnail">
              <img
                v-if="file.mediaType === 'Image'"
                :src="getMediaUrl(file.thumbnailPath || file.filePath)"
                :alt="file.altText || file.fileName"
              />
              <div v-else class="media-icon">
                <span v-if="file.mediaType === 'Video'">🎥</span>
                <span v-else-if="file.mediaType === 'Document'">📄</span>
                <span v-else>📎</span>
              </div>
            </div>
            <div class="media-info">
              <div class="media-filename">{{ file.fileName }}</div>
            </div>
          </div>
        </div>

        <div v-if="!mediaFiles.length" class="empty-state">
          No media files found. <router-link to="/media">Upload files</router-link>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="pagination">
          <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="btn btn-sm"
          >
            ← Previous
          </button>
          <span class="page-info">Page {{ currentPage }} of {{ totalPages }}</span>
          <button
            @click="goToPage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="btn btn-sm"
          >
            Next →
          </button>
        </div>
      </div>

      <div class="modal-footer">
        <button @click="close" class="btn btn-secondary">Cancel</button>
        <button
          @click="confirmSelection"
          :disabled="!selectedMediaId"
          class="btn btn-primary"
        >
          Select
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { logError } from '@/services/logger'
import { API_URL, getMediaUrl } from '@/utils/api-config'

interface MediaFile {
  id: number
  fileName: string
  filePath: string
  thumbnailPath: string | null
  mediaType: string
  altText: string | null
}

interface Props {
  isOpen: boolean
  mediaType?: string | null // Filter by specific media type
}

const props = withDefaults(defineProps<Props>(), {
  mediaType: null
})

const emit = defineEmits<{
  close: []
  select: [mediaFile: MediaFile]
}>()

const mediaFiles = ref<MediaFile[]>([])
const selectedMediaId = ref<number | null>(null)
const searchQuery = ref('')
const filterMediaType = ref<number | null>(props.mediaType)
const filterSection = ref<number | null>(null)
const currentPage = ref(1)
const pageSize = ref(12)
const totalCount = ref(0)
const sections = ref<any[]>([])

let searchDebounceTimer: number

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

const loadMedia = async () => {
  try {
    const params = new URLSearchParams({
      page: currentPage.value.toString(),
      pageSize: pageSize.value.toString()
    })
    
    if (searchQuery.value) {
      params.append('search', searchQuery.value)
    }
    
    if (filterMediaType.value !== null) {
      params.append('mediaType', filterMediaType.value.toString())
    }
    
    if (filterSection.value !== null) {
      params.append('sectionId', filterSection.value.toString())
    }

    const token = localStorage.getItem('admin_token')
    const response = await fetch(
      `${API_URL}/admin/media?${params}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    )

    if (response.ok) {
      const data = await response.json()
      mediaFiles.value = data.files || []
      totalCount.value = data.totalCount || 0
    }
  } catch (error) {
    logError('Failed to load media files', error)
  }
}

const searchMedia = () => {
  clearTimeout(searchDebounceTimer)
  searchDebounceTimer = window.setTimeout(() => {
    currentPage.value = 1
    loadMedia()
  }, 300)
}

const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    loadMedia()
  }
}

const selectMedia = (file: MediaFile) => {
  selectedMediaId.value = file.id
}

const confirmSelection = () => {
  const selected = mediaFiles.value.find(f => f.id === selectedMediaId.value)
  if (selected) {
    emit('select', selected)
    close()
  }
}

const close = () => {
  emit('close')
  selectedMediaId.value = null
}

const loadSections = async () => {
  try {
    const token = localStorage.getItem('admin_token')
    const response = await fetch(`${API_URL}/admin/media/sections`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })
    
    if (response.ok) {
      sections.value = await response.json()
    }
  } catch (error) {
    logError('Failed to load sections', error)
  }
}

watch(() => props.isOpen, (newValue) => {
  if (newValue) {
    filterMediaType.value = props.mediaType
    loadSections()
    loadMedia()
  }
})
</script>

<style scoped>
.media-picker-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 1rem;
}

.media-picker-modal {
  background: white;
  border-radius: 8px;
  width: 100%;
  max-width: 900px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e0e0e0;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
}

.modal-body {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  padding: 1.5rem;
  border-top: 1px solid #e0e0e0;
}

.filters-row {
  display: flex;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.filters-row .form-control {
  flex: 1;
}

.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.media-item {
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s;
}

.media-item:hover {
  border-color: #007bff;
}

.media-item.selected {
  border-color: #007bff;
  box-shadow: 0 0 0 2px #007bff;
}

.media-thumbnail {
  aspect-ratio: 1;
  background: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.media-thumbnail img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.media-icon {
  font-size: 2.5rem;
}

.media-info {
  padding: 0.5rem;
}

.media-filename {
  font-size: 0.875rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: #666;
}

.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid #e0e0e0;
}

.page-info {
  font-size: 0.875rem;
  color: #666;
}

.btn-close {
  background: none;
  border: none;
  font-size: 2rem;
  cursor: pointer;
  color: #666;
  line-height: 1;
  padding: 0;
  width: 2rem;
  height: 2rem;
}

.btn-close:hover {
  color: #333;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 4px;
  border: 1px solid #ccc;
  background: white;
  cursor: pointer;
  font-size: 0.875rem;
}

.btn:hover:not(:disabled) {
  background: #f5f5f5;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-sm {
  padding: 0.25rem 0.75rem;
  font-size: 0.875rem;
}

.btn-primary {
  background: #007bff;
  color: white;
  border-color: #007bff;
}

.btn-primary:hover:not(:disabled) {
  background: #0056b3;
  border-color: #0056b3;
}

.btn-secondary {
  background: #6c757d;
  color: white;
  border-color: #6c757d;
}

.btn-secondary:hover {
  background: #5a6268;
  border-color: #5a6268;
}

.form-control {
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 0.875rem;
}

.form-control:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 2px rgba(0, 123, 255, 0.25);
}
</style>
