<template>
  <AdminLayout>
    <div>
      <!-- Loading overlay -->
      <div v-if="isLoading && sections.length === 0" class="loading-overlay">
        <div class="spinner spinner-lg"></div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-6">
        <h1 class="card-title" style="color: #333 !important;">Media Library</h1>
        <button @click="showCreateSectionModal = true" class="btn" style="color: white !important;">
          ➕ Create New Section
        </button>
      </div>

      <!-- Sections List -->
      <div v-for="section in sections" :key="section.id" class="section-card mb-4">
        <div class="card">
          <div 
            class="card-header section-header"
            @click="toggleSection(section.id)"
            style="color: white !important; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;"
          >
            <div class="section-info" style="color: white !important;">
              <h3 class="section-title" style="color: white !important;">
                <span class="toggle-icon" style="color: white !important;">{{ expandedSections.includes(section.id) ? '▼' : '▶' }}</span>
                <span style="color: white !important;">{{ section.name }}</span>
                <span v-if="section.isSystem" class="badge badge-info ms-2">System</span>
                <span v-if="section.categoryName" class="badge badge-secondary ms-2">{{ section.categoryName }}</span>
              </h3>
              <p v-if="section.description" class="section-description" style="color: rgba(255, 255, 255, 0.95) !important;">{{ section.description }}</p>
              <div class="section-meta" style="color: rgba(255, 255, 255, 0.85) !important;">
                {{ section.mediaCount }} file(s)
              </div>
            </div>
            <div class="section-actions" @click.stop>
              <button
                @click="openUploadModal(section.id)"
                class="btn btn-sm"
                style="color: white !important; background: rgba(255, 255, 255, 0.2) !important; border: 1px solid rgba(255, 255, 255, 0.3) !important;"
              >
                ⬆ Upload Files
              </button>
              <button
                v-if="!section.isSystem"
                @click="openEditSectionModal(section)"
                class="btn btn-sm ms-2"
                style="color: white !important; background: rgba(255, 255, 255, 0.2) !important; border: 1px solid rgba(255, 255, 255, 0.3) !important;"
              >
                ✏️ Edit Section
              </button>
              <button
                v-if="!section.isSystem"
                @click="confirmDeleteSection(section)"
                :disabled="isActionLoading(section.id)"
                class="btn btn-sm btn-danger ms-2"
                style="color: white !important; background: rgba(220, 53, 69, 0.9) !important;"
              >
                <span v-if="isActionLoading(section.id)" class="btn-spinner"></span>
                🗑️ Delete Section
              </button>
            </div>
          </div>

          <!-- Section Content (Files) -->
          <div v-if="expandedSections.includes(section.id)" class="card-body">
            <div v-if="loadingSections.includes(section.id)" class="text-center py-4" style="color: #333 !important;">
              Loading files...
            </div>
            <div v-else-if="sectionFiles[section.id]?.length" class="media-grid">
              <div
                v-for="file in sectionFiles[section.id]"
                :key="file.id"
                class="media-item"
                @click="selectMedia(file)"
                :class="{ selected: selectedMedia?.id === file.id }"
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
                <div class="media-info" style="color: #333 !important;">
                  <div class="media-filename" style="color: #333 !important;">{{ file.fileName }}</div>
                  <div class="media-meta" style="color: #666 !important;">
                    {{ formatFileSize(file.fileSize) }}
                    <span v-if="file.width && file.height">- {{ file.width }}x{{ file.height }}</span>
                  </div>
                </div>
              </div>
            </div>
            <div v-else class="text-center text-muted py-4" style="color: #6c757d !important;">
              No files in this section. Click "Upload Files" to add some.
            </div>
          </div>
        </div>
      </div>

      <div v-if="!sections.length" class="text-center text-muted py-5" style="color: #6c757d !important;">
        No sections found. Create your first section to organize media files.
      </div>

      <!-- Upload Modal -->
      <div
        v-if="showUploadModal"
        class="modal-overlay"
        @click.self="closeUploadModal"
      >
        <div class="modal-dialog">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center" style="background: white !important;">
              <h3 class="card-title" style="color: #333 !important;">Upload Files to {{ currentSectionName }}</h3>
              <button @click="closeUploadModal" class="btn-close" style="color: #333 !important;">&times;</button>
            </div>
            <div class="card-body">
              <div
                class="upload-dropzone"
                @click="triggerFileInput"
                @drop.prevent="handleDrop"
                @dragover.prevent="isDragging = true"
                @dragleave.prevent="isDragging = false"
                :class="{ dragging: isDragging }"
              >
                <input
                  ref="fileInput"
                  type="file"
                  @change="handleFileSelect"
                  multiple
                  accept="image/*,video/*,.pdf,.doc,.docx"
                  style="display: none;"
                />
                <div class="upload-content" style="color: #333 !important;">
                  <div class="upload-icon">📁</div>
                  <p style="color: #333 !important;">Drag & drop files here or click to browse</p>
                  <p class="text-muted" style="color: #6c757d !important;">Supported: Images, Videos, Documents</p>
                </div>
              </div>

              <!-- Selected Files List -->
              <div v-if="uploadFiles.length" class="mt-4">
                <h4 style="color: #333 !important;">Selected Files ({{ uploadFiles.length }})</h4>
                <div class="upload-files-list">
                  <div
                    v-for="(file, index) in uploadFiles"
                    :key="index"
                    class="upload-file-item"
                    style="color: #333 !important;"
                  >
                    <div>
                      <strong style="color: #333 !important;">{{ file.file.name }}</strong>
                      <div class="text-muted" style="color: #6c757d !important;">{{ formatFileSize(file.file.size) }}</div>
                    </div>
                    <div>
                      <button
                        @click="removeUploadFile(index)"
                        class="btn btn-sm btn-danger"
                        style="color: white !important;"
                      >
                        Remove
                      </button>
                    </div>
                  </div>
                </div>

                <!-- Metadata Form -->
                <div class="mt-4">
                  <div class="form-group mb-3">
                    <label style="color: #333 !important;">Alt Text</label>
                    <input
                      v-model="uploadMetadata.altText"
                      type="text"
                      class="form-control"
                      placeholder="Describe the image for accessibility..."
                      style="color: #333 !important; background: white !important;"
                    />
                  </div>
                  <div class="form-group mb-3">
                    <label style="color: #333 !important;">Caption</label>
                    <input
                      v-model="uploadMetadata.caption"
                      type="text"
                      class="form-control"
                      placeholder="Caption or description..."
                      style="color: #333 !important; background: white !important;"
                    />
                  </div>
                  <div class="form-group mb-3">
                    <label style="color: #333 !important;">Tags (comma-separated)</label>
                    <input
                      v-model="uploadMetadata.tags"
                      type="text"
                      class="form-control"
                      placeholder="e.g., product, banner, hero"
                      style="color: #333 !important; background: white !important;"
                    />
                  </div>
                </div>

                <div class="d-flex gap-2">
                  <button
                    @click="uploadFilesToLibrary"
                    :disabled="isActionLoading('upload')"
                    class="btn flex-1"
                    style="color: white !important;"
                  >
                    <span v-if="isActionLoading('upload')" class="btn-spinner"></span>
                    {{ isActionLoading('upload') ? 'Uploading...' : `Upload ${uploadFiles.length} File(s)` }}
                  </button>
                  <button @click="closeUploadModal" class="btn btn-secondary" style="color: white !important;">
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Create Section Modal -->
      <div
        v-if="showCreateSectionModal"
        class="modal-overlay"
        @click.self="showCreateSectionModal = false"
      >
        <div class="modal-dialog">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center" style="background: white !important;">
              <h3 class="card-title" style="color: #333 !important;">Create New Section</h3>
              <button @click="showCreateSectionModal = false" class="btn-close" style="color: #333 !important;">&times;</button>
            </div>
            <div class="card-body">
              <div class="form-group mb-3">
                <label style="color: #333 !important;">Section Name *</label>
                <input
                  v-model="newSection.name"
                  type="text"
                  class="form-control"
                  placeholder="e.g., Promotions, Events, Seasonal"
                  style="color: #333 !important; background: white !important;"
                />
              </div>
              <div class="form-group mb-3">
                <label style="color: #333 !important;">Description</label>
                <textarea
                  v-model="newSection.description"
                  class="form-control"
                  rows="3"
                  placeholder="Optional description for this section..."
                  style="color: #333 !important; background: white !important;"
                ></textarea>
              </div>
              <div class="d-flex gap-2">
                <button
                  @click="createSection"
                  :disabled="!newSection.name.trim() || isActionLoading('createSection')"
                  class="btn flex-1"
                  style="color: white !important;"
                >
                  <span v-if="isActionLoading('createSection')" class="btn-spinner"></span>
                  {{ isActionLoading('createSection') ? 'Creating...' : 'Create Section' }}
                </button>
                <button @click="showCreateSectionModal = false" class="btn btn-secondary" style="color: white !important;">
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Edit Section Modal -->
      <div
        v-if="showEditSectionModal"
        class="modal-overlay"
        @click.self="showEditSectionModal = false"
      >
        <div class="modal-dialog">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center" style="background: white !important;">
              <h3 class="card-title" style="color: #333 !important;">Edit Section</h3>
              <button @click="showEditSectionModal = false" class="btn-close" style="color: #333 !important;">&times;</button>
            </div>
            <div class="card-body">
              <div class="form-group mb-3">
                <label style="color: #333 !important;">Section Name *</label>
                <input
                  v-model="editSectionForm.name"
                  type="text"
                  class="form-control"
                  placeholder="e.g., Promotions, Events, Seasonal"
                  style="color: #333 !important; background: white !important;"
                />
              </div>
              <div class="form-group mb-3">
                <label style="color: #333 !important;">Description</label>
                <textarea
                  v-model="editSectionForm.description"
                  class="form-control"
                  rows="3"
                  placeholder="Optional description for this section..."
                  style="color: #333 !important; background: white !important;"
                ></textarea>
              </div>
              <div class="d-flex gap-2">
                <button
                  @click="renameSection"
                  :disabled="!editSectionForm.name.trim() || isActionLoading('editSection')"
                  class="btn flex-1"
                  style="color: white !important;"
                >
                  <span v-if="isActionLoading('editSection')" class="btn-spinner"></span>
                  {{ isActionLoading('editSection') ? 'Saving...' : 'Save Changes' }}
                </button>
                <button @click="showEditSectionModal = false" class="btn btn-secondary" style="color: white !important;">
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Media Detail Panel -->
      <div v-if="selectedMedia" class="media-detail-panel">
        <div class="card">
          <div class="card-header d-flex justify-content-between align-items-center" style="background: white !important;">
            <h3 class="card-title" style="color: #333 !important;">Media Details</h3>
            <button @click="selectedMedia = null" class="btn-close" style="color: #333 !important;">&times;</button>
          </div>
          <div class="card-body">
            <!-- Preview -->
            <div class="media-preview mb-4">
              <img
                v-if="selectedMedia.mediaType === 'Image'"
                :src="getMediaUrl(selectedMedia.filePath)"
                :alt="selectedMedia.altText || selectedMedia.fileName"
                style="max-width: 100%; border-radius: 8px;"
              />
              <div v-else class="media-icon-large">
                <span v-if="selectedMedia.mediaType === 'Video'">🎥</span>
                <span v-else-if="selectedMedia.mediaType === 'Document'">📄</span>
                <span v-else>📎</span>
              </div>
            </div>

            <!-- Info -->
            <div class="info-group">
              <label style="color: #333 !important;">Filename</label>
              <div style="color: #333 !important;">{{ selectedMedia.fileName }}</div>
            </div>
            <div class="info-group">
              <label style="color: #333 !important;">File Path</label>
              <div class="text-muted" style="word-break: break-all; color: #6c757d !important;">{{ selectedMedia.filePath }}</div>
            </div>
            <div class="info-group">
              <label style="color: #333 !important;">Type</label>
              <div>
                <span class="badge badge-primary">{{ selectedMedia.mediaType }}</span>
              </div>
            </div>
            <div class="info-group">
              <label style="color: #333 !important;">Size</label>
              <div style="color: #333 !important;">{{ formatFileSize(selectedMedia.fileSize) }}</div>
            </div>
            <div v-if="selectedMedia.width && selectedMedia.height" class="info-group">
              <label style="color: #333 !important;">Dimensions</label>
              <div style="color: #333 !important;">{{ selectedMedia.width }} x {{ selectedMedia.height }} px</div>
            </div>

            <!-- Editable Metadata -->
            <hr />
            <div class="form-group mb-3">
              <label style="color: #333 !important;">Alt Text</label>
              <input
                v-model="editMetadata.altText"
                type="text"
                class="form-control"
                style="color: #333 !important; background: white !important;"
              />
            </div>
            <div class="form-group mb-3">
              <label style="color: #333 !important;">Caption</label>
              <input
                v-model="editMetadata.caption"
                type="text"
                class="form-control"
                style="color: #333 !important; background: white !important;"
              />
            </div>
            <div class="form-group mb-3">
              <label style="color: #333 !important;">Tags</label>
              <input
                v-model="editMetadata.tags"
                type="text"
                class="form-control"
                style="color: #333 !important; background: white !important;"
              />
            </div>

            <!-- Actions -->
            <div class="d-flex gap-2">
              <button 
                @click="updateMediaMetadata" 
                :disabled="isActionLoading('updateMetadata')"
                class="btn flex-1" 
                style="color: white !important;"
              >
                <span v-if="isActionLoading('updateMetadata')" class="btn-spinner"></span>
                {{ isActionLoading('updateMetadata') ? 'Saving...' : 'Save Changes' }}
              </button>
              <button @click="copyFileUrl" class="btn btn-secondary" style="color: white !important;">
                📋 Copy URL
              </button>
              <button 
                @click="deleteMediaFile(selectedMedia.id)" 
                :disabled="selectedMedia && isActionLoading(selectedMedia.id)"
                class="btn btn-danger" 
                style="color: white !important;"
              >
                <span v-if="selectedMedia && isActionLoading(selectedMedia.id)" class="btn-spinner"></span>
                🗑️ Delete
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Confirm Modal -->
    <div v-if="confirmModal.show" class="modal-overlay" @click.self="closeConfirmModal">
      <div class="modal-dialog" style="max-width: 480px;">
        <div class="card">
          <div class="card-header d-flex justify-content-between align-items-center" style="background: white !important;">
            <h3 class="card-title" style="color: #333 !important;">{{ confirmModal.title }}</h3>
            <button @click="closeConfirmModal" class="btn-close" style="color: #333 !important;">&times;</button>
          </div>
          <div class="card-body">
            <p style="color: #333; margin-bottom: 1.5rem;">{{ confirmModal.message }}</p>
            <div class="d-flex justify-content-end gap-2">
              <button @click="closeConfirmModal" class="btn btn-secondary">Cancel</button>
              <button @click="executeConfirmModal" :class="confirmModal.dangerous ? 'btn btn-danger' : 'btn btn-primary'">Confirm</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import { useToast } from '@/composables/useToast'
import { useLoadingState } from '@/composables/useLoadingState'
import { logError } from '@/services/logger'
import { getMediaUrl } from '@/utils/api-config'
import { apiGet, apiPost, apiPut, apiDelete, apiClient } from '@/utils/apiClient'

const toast = useToast()
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState()

interface MediaFile {
  id: number
  fileName: string
  storedFileName: string
  filePath: string
  thumbnailPath: string | null
  mimeType: string
  fileSize: number
  mediaType: string
  width: number | null
  height: number | null
  altText: string | null
  caption: string | null
  tags: string | null
  uploadedAt: string
  updatedAt: string | null
  uploadedBy: string | null
}

interface Section {
  id: number
  name: string
  description: string | null
  isSystem: boolean
  mediaCount: number
  categoryName: string | null
}

interface UploadFile {
  file: File
}

const confirmModal = reactive({ show: false, title: '', message: '', dangerous: false, onConfirm: null as (() => void) | null })
const showConfirmModal = (title: string, message: string, onConfirm: () => void, dangerous = false) => {
  Object.assign(confirmModal, { show: true, title, message, dangerous, onConfirm })
}
const closeConfirmModal = () => { confirmModal.show = false; confirmModal.onConfirm = null }
const executeConfirmModal = () => { confirmModal.onConfirm?.(); closeConfirmModal() }

const sections = ref<Section[]>([])
const expandedSections = ref<number[]>([])
const sectionFiles = ref<Record<number, MediaFile[]>>({})
const loadingSections = ref<number[]>([])
const selectedMedia = ref<MediaFile | null>(null)
const showUploadModal = ref(false)
const showCreateSectionModal = ref(false)
const isDragging = ref(false)
const uploadFiles = ref<UploadFile[]>([])
const fileInput = ref<HTMLInputElement | null>(null)
const currentUploadSectionId = ref<number | null>(null)

const uploadMetadata = ref({
  altText: '',
  caption: '',
  tags: ''
})

const editMetadata = ref({
  altText: '',
  caption: '',
  tags: ''
})

const newSection = ref({
  name: '',
  description: ''
})

const showEditSectionModal = ref(false)
const editingSection = ref<Section | null>(null)
const editSectionForm = ref({ name: '', description: '' })

const currentSectionName = computed(() => {
  const section = sections.value.find(s => s.id === currentUploadSectionId.value)
  return section?.name || 'Unknown Section'
})

const loadSections = async () => {
  await executeWithLoading(async () => {
    try {
      sections.value = await apiGet<Section[]>('/admin/media/sections')
      // Auto-expand first section
      if (sections.value.length > 0 && expandedSections.value.length === 0) {
        expandedSections.value.push(sections.value[0].id)
        loadSectionFiles(sections.value[0].id)
      }
    } catch (error) {
      logError('Failed to load sections', error)
    }
  })
}

const loadSectionFiles = async (sectionId: number) => {
  if (loadingSections.value.includes(sectionId)) return

  loadingSections.value.push(sectionId)

  try {
    const data = await apiGet<{ files: MediaFile[]; total: number }>(`/admin/media?sectionId=${sectionId}&pageSize=100`)
    sectionFiles.value[sectionId] = data.files || []
    if (data.total > 100) {
      toast.warning(`Section has ${data.total} files; only the first 100 are shown.`)
    }
  } catch (error) {
    logError(`Failed to load files for section ${sectionId}`, error)
  } finally {
    loadingSections.value = loadingSections.value.filter(id => id !== sectionId)
  }
}

const toggleSection = (sectionId: number) => {
  const index = expandedSections.value.indexOf(sectionId)
  if (index > -1) {
    expandedSections.value.splice(index, 1)
  } else {
    expandedSections.value.push(sectionId)
    loadSectionFiles(sectionId)
  }
}

const openUploadModal = (sectionId: number) => {
  currentUploadSectionId.value = sectionId
  showUploadModal.value = true
}

const closeUploadModal = () => {
  showUploadModal.value = false
  uploadFiles.value = []
  uploadMetadata.value = { altText: '', caption: '', tags: '' }
  currentUploadSectionId.value = null
}

const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files) {
    Array.from(target.files).forEach(file => {
      uploadFiles.value.push({ file })
    })
    target.value = ''
  }
}

const handleDrop = (event: DragEvent) => {
  isDragging.value = false
  if (event.dataTransfer?.files) {
    Array.from(event.dataTransfer.files).forEach(file => {
      uploadFiles.value.push({ file })
    })
  }
}

const removeUploadFile = (index: number) => {
  uploadFiles.value.splice(index, 1)
}

const uploadFilesToLibrary = async () => {
  if (!currentUploadSectionId.value) return

  await executeWithLoading(async () => {
    try {
      const failedFiles: string[] = []

      for (const uploadFile of uploadFiles.value) {
        const formData = new FormData()
        formData.append('file', uploadFile.file)
        formData.append('altText', uploadMetadata.value.altText)
        formData.append('caption', uploadMetadata.value.caption)
        formData.append('tags', uploadMetadata.value.tags)
        formData.append('sectionId', currentUploadSectionId.value!.toString())

        const response = await apiClient('/admin/media/upload', { method: 'POST', body: formData })
        if (!response.ok) {
          const errorText = await response.text()
          logError('Failed to upload file: ' + uploadFile.file.name, errorText)
          failedFiles.push(uploadFile.file.name)
        }
      }

      if (failedFiles.length > 0) {
        toast.error(`Failed to upload ${failedFiles.length} file(s): ${failedFiles.join(', ')}`)
      } else {
        const uploadedSectionId = currentUploadSectionId.value!
        toast.uploadSuccess(uploadFiles.value.length)
        closeUploadModal()
        loadSectionFiles(uploadedSectionId)
        loadSections()
      }
    } catch (error) {
      logError('Failed to upload files', error)
      toast.error('An error occurred while uploading files')
    }
  }, 'upload')
}

const createSection = async () => {
  if (!newSection.value.name.trim()) return

  await executeWithLoading(async () => {
    try {
      await apiPost('/admin/media/sections', {
        name: newSection.value.name.trim(),
        description: newSection.value.description.trim() || null,
        displayOrder: 100
      })
      toast.success('Section created successfully!')
      newSection.value = { name: '', description: '' }
      showCreateSectionModal.value = false
      loadSections()
    } catch (error: any) {
      logError('Failed to create section', error)
      toast.error(`Failed to create section: ${error.message || 'Unknown error'}`)
    }
  }, 'createSection')
}

const openEditSectionModal = (section: Section) => {
  editingSection.value = section
  editSectionForm.value = { name: section.name, description: section.description || '' }
  showEditSectionModal.value = true
}

const renameSection = async () => {
  if (!editingSection.value || !editSectionForm.value.name.trim()) return

  await executeWithLoading(async () => {
    try {
      await apiPut(`/admin/media/sections/${editingSection.value!.id}`, {
        name: editSectionForm.value.name.trim(),
        description: editSectionForm.value.description.trim() || null
      })
      toast.success('Section updated successfully!')
      showEditSectionModal.value = false
      editingSection.value = null
      loadSections()
    } catch (error: any) {
      logError('Failed to update section', error)
      toast.error(`Failed to update section: ${error.message || 'Unknown error'}`)
    }
  }, 'editSection')
}

const confirmDeleteSection = (section: Section) => {
  const fileCount = section.mediaCount
  const msg = fileCount > 0
    ? `Are you sure you want to delete "${section.name}" and all ${fileCount} file(s) in it? This will permanently delete the files from the server and cannot be undone.`
    : `Are you sure you want to delete "${section.name}"?`
  showConfirmModal('Delete Section', msg, async () => {
    await executeWithLoading(async () => {
    try {
      await apiDelete(`/admin/media/sections/${section.id}`)
      toast.deleteSuccess(`Section '${section.name}'`)
      expandedSections.value = expandedSections.value.filter(id => id !== section.id)
      delete sectionFiles.value[section.id]
      loadSections()
    } catch (error: any) {
      logError('Failed to delete section', error)
      toast.error(`Failed to delete section: ${error.message || 'Unknown error'}`)
    }
    }, section.id)
  }, true)
}

const selectMedia = (file: MediaFile) => {
  selectedMedia.value = file
  editMetadata.value = {
    altText: file.altText || '',
    caption: file.caption || '',
    tags: file.tags || ''
  }
}

const updateMediaMetadata = async () => {
  if (!selectedMedia.value) return

  await executeWithLoading(async () => {
    try {
      await apiPut(`/admin/media/${selectedMedia.value!.id}`, {
        altText: editMetadata.value.altText,
        caption: editMetadata.value.caption,
        tags: editMetadata.value.tags
      })
      toast.saveSuccess('Metadata')
      selectedMedia.value!.altText = editMetadata.value.altText
      selectedMedia.value!.caption = editMetadata.value.caption
      selectedMedia.value!.tags = editMetadata.value.tags
    } catch (error) {
      logError('Failed to update metadata', error)
      toast.saveError('Failed to update metadata')
    }
  }, 'updateMetadata')
}

const deleteMediaFile = (id: number) => {
  showConfirmModal('Delete File', 'Are you sure you want to delete this file? This action cannot be undone.', async () => {
    await executeWithLoading(async () => {
      try {
        await apiDelete(`/admin/media/${id}`)
        toast.deleteSuccess('File')
        selectedMedia.value = null
        loadSections()
        expandedSections.value.forEach(sid => loadSectionFiles(sid))
      } catch (error: any) {
        logError('Failed to delete file', error)
        toast.error(`Failed to delete file: ${error.message || 'Unknown error'}`)
      }
    }, id)
  }, true)
}

const copyFileUrl = () => {
  if (!selectedMedia.value) return
  
  const url = getMediaUrl(selectedMedia.value.filePath)
  navigator.clipboard.writeText(url).then(() => {
    toast.copySuccess()
  })
}

const formatFileSize = (bytes: number): string => {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

onMounted(() => {
  loadSections()
})
</script>

<style scoped>
.section-card {
  transition: all 0.2s ease;
}

.section-header {
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white !important;
  border: none;
  transition: all 0.2s ease;
}

.section-header:hover {
  background: linear-gradient(135deg, #5568d3 0%, #6a3f91 100%);
}

.section-info {
  flex: 1;
  color: white !important;
}

.section-title {
  margin: 0 0 0.5rem 0;
  font-size: 1.5rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: white !important;
}

.toggle-icon {
  font-size: 1rem;
  display: inline-block;
  width: 1.2rem;
  color: white !important;
}

.section-description {
  margin: 0 0 0.5rem 0;
  font-size: 0.95rem;
  color: rgba(255, 255, 255, 0.95) !important;
}

.section-meta {
  font-size: 0.875rem;
  color: rgba(255, 255, 255, 0.85) !important;
}

.section-actions {
  display: flex;
  gap: 0.5rem;
}

.section-actions .btn {
  background: rgba(255, 255, 255, 0.2);
  color: white !important;
  border: 1px solid rgba(255, 255, 255, 0.3);
  font-weight: 600;
}

.section-actions .btn:hover {
  background: rgba(255, 255, 255, 0.3);
  border-color: rgba(255, 255, 255, 0.5);
}

.section-actions .btn-danger {
  background: rgba(220, 53, 69, 0.9);
  border-color: rgba(220, 53, 69, 1);
}

.section-actions .btn-danger:hover {
  background: rgba(200, 35, 51, 1);
}

.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 1rem;
  padding: 1rem 0;
}

.media-item {
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s ease;
  background: white;
}

.media-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  border-color: #667eea;
}

.media-item.selected {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.2);
}

.media-thumbnail {
  width: 100%;
  height: 150px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f5f5;
  overflow: hidden;
}

.media-thumbnail img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.media-icon {
  font-size: 3rem;
}

.media-info {
  padding: 0.75rem;
  background: white;
}

.media-filename {
  font-weight: 500;
  font-size: 0.875rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: #333 !important;
}

.media-meta {
  font-size: 0.75rem;
  color: #666 !important;
  margin-top: 0.25rem;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 1rem;
}

.modal-dialog {
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
}

.upload-dropzone {
  border: 2px dashed #ccc;
  border-radius: 8px;
  padding: 3rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
  background: #fafafa;
  color: #333;
}

.upload-dropzone:hover,
.upload-dropzone.dragging {
  border-color: #667eea;
  background: #f0f4ff;
}

.upload-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.upload-content {
  color: #333;
}

.upload-content p {
  color: #333;
  margin: 0.5rem 0;
}

.upload-content .text-muted {
  color: #6c757d !important;
}

.upload-files-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.upload-file-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background: #f5f5f5;
  border-radius: 4px;
  color: #333;
}

.upload-file-item strong {
  color: #333;
}

.upload-file-item .text-muted {
  color: #6c757d !important;
}

.media-detail-panel {
  position: fixed;
  top: 0;
  right: 0;
  width: 400px;
  height: 100vh;
  background: white;
  box-shadow: -4px 0 16px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  overflow-y: auto;
}

.media-preview {
  text-align: center;
}

.media-icon-large {
  font-size: 6rem;
  padding: 2rem;
}

.info-group {
  margin-bottom: 1rem;
}

.info-group label {
  font-weight: 600;
  font-size: 0.875rem;
  color: #666;
  display: block;
  margin-bottom: 0.25rem;
}

.badge {
  display: inline-block;
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  font-weight: 600;
  border-radius: 4px;
}

.badge-info {
  background: #17a2b8;
  color: white;
}

.badge-secondary {
  background: #6c757d;
  color: white;
}

.badge-primary {
  background: #667eea;
  color: white;
}

.flex-1 {
  flex: 1;
}

.gap-2 {
  gap: 0.5rem;
}

.d-flex {
  display: flex;
}

.ms-2 {
  margin-left: 0.5rem;
}

.mb-3 {
  margin-bottom: 1rem;
}

.mb-4 {
  margin-bottom: 1.5rem;
}

.mb-6 {
  margin-bottom: 3rem;
}

.mt-4 {
  margin-top: 1.5rem;
}

.py-4 {
  padding-top: 1.5rem;
  padding-bottom: 1.5rem;
}

.py-5 {
  padding-top: 3rem;
  padding-bottom: 3rem;
}

.text-center {
  text-align: center;
  color: #333;
}

.text-muted {
  color: #6c757d !important;
}

.form-control {
  display: block;
  width: 100%;
  padding: 0.5rem 0.75rem;
  font-size: 1rem;
  border: 1px solid #ced4da;
  border-radius: 0.25rem;
  color: #333;
  background: white;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 0.25rem;
  background: #667eea;
  color: white !important;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn:hover {
  background: #5568d3;
  color: white !important;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.875rem;
}

.btn-secondary {
  background: #6c757d;
  color: white !important;
}

.btn-secondary:hover {
  background: #5a6268;
  color: white !important;
}

.btn-danger {
  background: #dc3545;
  color: white !important;
}

.btn-danger:hover {
  background: #c82333;
  color: white !important;
}

.btn-close {
  background: transparent;
  border: none;
  font-size: 2rem;
  line-height: 0.5;
  cursor: pointer;
  color: inherit;
  padding: 0;
}

.card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.card-header {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #e0e0e0;
  background: #f8f9fa;
  color: #333;
}

.card-body {
  padding: 1.5rem;
  color: #333;
}

.card-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  color: #333 !important;
}

/* General text visibility */
h1, h2, h3, h4, h5, h6 {
  color: #333 !important;
}

p, div, span, strong, label {
  color: inherit;
}

.section-header h3,
.section-header p,
.section-header div,
.section-header span {
  color: white !important;
}

@media (max-width: 600px) {
  .section-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
    padding: 1rem;
  }

  .section-title {
    font-size: 1.2rem;
  }

  .section-actions {
    width: 100%;
    flex-wrap: wrap;
  }

  .section-actions .btn {
    flex: 1 1 auto;
    text-align: center;
    font-size: 0.8rem;
    padding: 0.4rem 0.6rem;
  }
}
</style>
