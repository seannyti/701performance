<template>
  <AdminLayout>
    <div class="backup-page">
      <!-- Loading overlay -->
      <div v-if="isLoading && backupList.length === 0" class="loading-overlay">
        <div class="spinner spinner-lg"></div>
      </div>

      <div class="page-header">
        <h1 class="page-title">💾 Backup & Restore</h1>
        <p class="page-description">Manage database backups, configure automatic backups, and restore from previous backups.</p>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading && backupList.length === 0" class="loading-state">
        <div class="spinner"></div>
        <div>Loading backup data...</div>
      </div>

      <!-- Main Content -->
      <div v-else>
        <!-- Backup Overview Stats -->
        <div class="backup-stats-grid">
          <div class="stat-card">
            <div class="stat-icon">💾</div>
            <div class="stat-content">
              <div class="stat-value">{{ backupList.length }}</div>
              <div class="stat-label">Total Backups</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">📦</div>
            <div class="stat-content">
              <div class="stat-value">{{ latestBackupDate }}</div>
              <div class="stat-label">Last Backup</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">📊</div>
            <div class="stat-content">
              <div class="stat-value">{{ totalBackupSize }}</div>
              <div class="stat-label">Total Size</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">🤖</div>
            <div class="stat-content">
              <div class="stat-value">{{ autoBackupEnabled ? 'Enabled' : 'Disabled' }}</div>
              <div class="stat-label">Auto Backup (1 AM)</div>
            </div>
          </div>
        </div>

        <!-- Manual Backup Section -->
        <div class="content-section">
          <h3 class="section-header">📦 Manual Backup</h3>
          <p class="section-description">Create an on-demand backup of your entire database including settings, users, categories, and products.</p>
          
          <div class="backup-manual-actions">
            <button 
              @click="createManualBackup" 
              class="btn btn-primary btn-large"
              :disabled="isActionLoading('createBackup')"
            >
              <span v-if="isActionLoading('createBackup')" class="btn-spinner"></span>
              <span v-else class="icon">📦</span>
              {{ isActionLoading('createBackup') ? 'Creating Backup...' : 'Create Manual Backup' }}
            </button>
            
            <button 
              @click="refreshBackupList" 
              class="btn btn-secondary"
              :disabled="isActionLoading('loadBackups')"
            >
              <span v-if="isActionLoading('loadBackups')" class="btn-spinner"></span>
              <span v-else class="icon">🔄</span>
              {{ isActionLoading('loadBackups') ? 'Refreshing...' : 'Refresh List' }}
            </button>
          </div>

          <div v-if="backupStatus" class="backup-status-message" :class="backupStatus.type">
            <span class="icon">{{ backupStatus.type === 'success' ? '✅' : backupStatus.type === 'error' ? '❌' : 'ℹ️' }}</span>
            {{ backupStatus.message }}
          </div>
        </div>

        <!-- Auto Backup Configuration -->
        <div class="content-section">
          <div class="section-header-collapsible" @click="autoBackupExpanded = !autoBackupExpanded">
            <h3 class="section-header">🤖 Automatic Backup</h3>
            <span class="collapse-icon">{{ autoBackupExpanded ? '▼' : '▶' }}</span>
          </div>
          
          <div v-show="autoBackupExpanded" class="collapsible-content">
            <p class="section-description">Configure automatic daily backups at 1:00 AM. Auto backups are stored separately from manual backups.</p>
            
            <div class="form-group">
              <label class="form-label">
                <input 
                  type="checkbox" 
                  v-model="autoBackupEnabled" 
                  @change="toggleAutoBackup"
                  class="form-checkbox"
                />
                Enable Automatic Daily Backup (1:00 AM)
              </label>
              <p class="form-hint">When enabled, the system will automatically create a backup every night at 1:00 AM server time.</p>
            </div>

            <div v-if="autoBackupEnabled" class="auto-backup-info">
              <div class="info-row">
                <span class="info-label">Next Scheduled Backup:</span>
                <span class="info-value">{{ nextAutoBackupTime }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Retention Period:</span>
                <span class="info-value">Keep last {{ autoBackupRetention }} backups</span>
              </div>
              <div class="form-group">
                <label class="form-label">Number of Auto Backups to Keep</label>
                <input 
                  type="number" 
                  v-model.number="autoBackupRetention" 
                  @change="saveAutoBackupRetention"
                  min="1" 
                  max="30"
                  class="form-input"
                  style="width: 150px;"
                />
                <p class="form-hint">Older auto backups will be automatically deleted when this limit is reached.</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Available Backups List -->
        <div class="content-section">
          <h3 class="section-header">📋 Available Backups</h3>
          <p class="section-description">View, download, or restore from your available backups. Manual and automatic backups are listed separately.</p>
          
          <!-- Loading State -->
          <div v-if="loadingBackups" class="loading-state-inline">
            <div class="spinner-sm"></div>
            <span>Loading backups...</span>
          </div>

          <!-- No Backups State -->
          <div v-else-if="backupList.length === 0" class="empty-state">
            <div class="empty-icon">📦</div>
            <h4>No Backups Available</h4>
            <p>Create your first backup using the button above.</p>
          </div>

          <!-- Backups List -->
          <div v-else class="backups-list">
            <!-- Manual Backups -->
            <div v-if="manualBackups.length > 0" class="backup-group">
              <h4 class="backup-group-title">Manual Backups ({{ manualBackups.length }})</h4>
              <div class="backup-item" v-for="backup in manualBackups" :key="backup.fileName">
                <div class="backup-item-icon">👤</div>
                <div class="backup-item-info">
                  <div class="backup-item-name">{{ backup.fileName }}</div>
                  <div class="backup-item-meta">
                    <span>{{ formatDate(backup.createdAt) }}</span>
                    <span class="meta-separator">•</span>
                    <span>{{ formatFileSize(backup.size) }}</span>
                    <span class="meta-separator">•</span>
                    <span>{{ backup.recordCount }} records</span>
                  </div>
                </div>
                <div class="backup-item-actions">
                  <button 
                    @click="downloadBackup(backup.fileName)" 
                    class="btn btn-sm btn-secondary"
                    :disabled="isActionLoading(`download-${backup.fileName}`)"
                    title="Download this backup"
                  >
                    <span v-if="isActionLoading(`download-${backup.fileName}`)" class="btn-spinner"></span>
                    <span v-else class="icon">⬇️</span>
                    Download
                  </button>
                  <button 
                    @click="restoreBackup(backup.fileName)" 
                    class="btn btn-sm btn-warning"
                    :disabled="isActionLoading(`restore-${backup.fileName}`)"
                    title="Restore from this backup"
                  >
                    <span v-if="isActionLoading(`restore-${backup.fileName}`)" class="btn-spinner"></span>
                    <span v-else class="icon">🔄</span>
                    Restore
                  </button>
                  <button 
                    @click="deleteBackup(backup.fileName)" 
                    class="btn btn-sm btn-danger"
                    :disabled="isActionLoading(`delete-${backup.fileName}`)"
                    title="Delete this backup"
                  >
                    <span v-if="isActionLoading(`delete-${backup.fileName}`)" class="btn-spinner"></span>
                    <span v-else class="icon">🗑️</span>
                    Delete
                  </button>
                </div>
              </div>
            </div>

            <!-- Auto Backups -->
            <div v-if="autoBackups.length > 0" class="backup-group">
              <h4 class="backup-group-title">Automatic Backups ({{ autoBackups.length }})</h4>
              <div class="backup-item" v-for="backup in autoBackups" :key="backup.fileName">
                <div class="backup-item-icon">🤖</div>
                <div class="backup-item-info">
                  <div class="backup-item-name">{{ backup.fileName }}</div>
                  <div class="backup-item-meta">
                    <span>{{ formatDate(backup.createdAt) }}</span>
                    <span class="meta-separator">•</span>
                    <span>{{ formatFileSize(backup.size) }}</span>
                    <span class="meta-separator">•</span>
                    <span>{{ backup.recordCount }} records</span>
                  </div>
                </div>
                <div class="backup-item-actions">
                  <button 
                    @click="downloadBackup(backup.fileName)" 
                    class="btn btn-sm btn-secondary"
                    :disabled="isActionLoading(`download-${backup.fileName}`)"
                    title="Download this backup"
                  >
                    <span v-if="isActionLoading(`download-${backup.fileName}`)" class="btn-spinner"></span>
                    <span v-else class="icon">⬇️</span>
                    Download
                  </button>
                  <button 
                    @click="restoreBackup(backup.fileName)" 
                    class="btn btn-sm btn-warning"
                    :disabled="isActionLoading(`restore-${backup.fileName}`)"
                    title="Restore from this backup"
                  >
                    <span v-if="isActionLoading(`restore-${backup.fileName}`)" class="btn-spinner"></span>
                    <span v-else class="icon">🔄</span>
                    Restore
                  </button>
                  <button 
                    @click="deleteBackup(backup.fileName)" 
                    class="btn btn-sm btn-danger"
                    :disabled="isActionLoading(`delete-${backup.fileName}`)"
                    title="Delete this backup"
                  >
                    <span v-if="isActionLoading(`delete-${backup.fileName}`)" class="btn-spinner"></span>
                    <span v-else class="icon">🗑️</span>
                    Delete
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Important Notes -->
        <div class="content-section backup-notes">
          <h3 class="section-header">⚠️ Important Information</h3>
          <div class="note-grid">
            <div class="note-card">
              <div class="note-icon">💾</div>
              <h4>Backup Contents</h4>
              <p>Backups include all database content: settings, users, categories, products, and images. Media files in <code>wwwroot/uploads</code> are NOT included.</p>
            </div>
            <div class="note-card">
              <div class="note-icon">🔄</div>
              <h4>Restoring Backups</h4>
              <p>Restoring will <strong>replace</strong> all current data with the backup data. This action cannot be undone. Always create a current backup before restoring.</p>
            </div>
            <div class="note-card">
              <div class="note-icon">📁</div>
              <h4>Storage Location</h4>
              <p>Backups are stored on the server in the <code>backups</code> folder. Manual backups can also be downloaded to your computer for safe keeping.</p>
            </div>
            <div class="note-card">
              <div class="note-icon">🤖</div>
              <h4>Auto Backup</h4>
              <p>Automatic backups run at 1:00 AM server time. The system keeps the most recent backups based on your retention setting and automatically deletes older ones.</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import { useAuthStore } from '@/stores/auth'
import { logError } from '@/services/logger'
import { useToast } from '@/composables/useToast'
import { useLoadingState } from '@/composables/useLoadingState'
import { SUCCESS_MESSAGE_DURATION_MS } from '@/constants'
import { API_URL } from '@/utils/api-config'

const authStore = useAuthStore()
const toast = useToast()
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState()

// State
const backupStatus = ref<{ type: 'success' | 'error' | 'info'; message: string } | null>(null)
const backupList = ref<any[]>([])
const autoBackupEnabled = ref(false)
const autoBackupRetention = ref(7)
const nextAutoBackupTime = ref('Calculating...')
const autoBackupExpanded = ref(true)

// Computed properties
const manualBackups = computed(() => backupList.value.filter(b => b.type === 'manual'))
const autoBackups = computed(() => backupList.value.filter(b => b.type === 'auto'))

const latestBackupDate = computed(() => {
  if (backupList.value.length === 0) return 'Never'
  const latest = backupList.value[0]
  return formatDate(latest.createdAt)
})

const totalBackupSize = computed(() => {
  const total = backupList.value.reduce((sum, b) => sum + (b.size || 0), 0)
  return formatFileSize(total)
})

// Functions
const createManualBackup = async () => {
  if (!confirm('⚠️ Create a new manual backup? This will be saved on the server and can be downloaded or restored later.')) {
    return
  }

  await executeWithLoading(async () => {
    backupStatus.value = { type: 'info', message: 'Creating backup...' }

    try {
      const response = await fetch(`${API_URL}/admin/backup/create`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${authStore.token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ type: 'manual' })
      })

      if (!response.ok) {
        throw new Error('Failed to create backup')
      }

      const result = await response.json()
      backupStatus.value = { type: 'success', message: `✅ Backup created successfully: ${result.fileName}` }
      toast.success('Manual backup created successfully!')

      // Refresh the backup list
      await refreshBackupList()

      // Clear status after 5 seconds
      setTimeout(() => {
        backupStatus.value = null
      }, SUCCESS_MESSAGE_DURATION_MS)
    } catch (err: any) {
      logError('Failed to create backup', err)
      backupStatus.value = { type: 'error', message: '❌ Failed to create backup. Please try again.' }
      toast.error('Failed to create backup')
    }
  }, 'createBackup')
}

const refreshBackupList = async () => {
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/backup/list`, {
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        }
      })

      if (!response.ok) {
        throw new Error('Failed to load backups')
      }

      const result = await response.json()
      backupList.value = result.backups || []
      
      // Update next auto backup time
      if (result.nextAutoBackup) {
        nextAutoBackupTime.value = formatDate(result.nextAutoBackup)
      }
    } catch (err: any) {
      logError('Failed to load backups', err)
      toast.error('Failed to load backup list')
    }
  }, 'loadBackups')
}

const downloadBackup = async (fileName: string) => {
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/backup/download/${encodeURIComponent(fileName)}`, {
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        }
      })

      if (!response.ok) {
        throw new Error('Failed to download backup')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = fileName
      document.body.appendChild(a)
      a.click()
      window.URL.revokeObjectURL(url)
      document.body.removeChild(a)
      
      toast.success('Backup downloaded successfully!')
    } catch (err: any) {
      logError('Failed to download backup', err)
      toast.error('Failed to download backup')
    }
  }, `download-${fileName}`)
}

const restoreBackup = async (fileName: string) => {
  if (!confirm(`⚠️ WARNING: Restoring will REPLACE all current data with data from this backup!\n\nBackup: ${fileName}\n\nThis action cannot be undone. Are you absolutely sure?`)) {
    return
  }

  if (!confirm('⚠️ FINAL CONFIRMATION: This will overwrite ALL your current data. Continue?')) {
    return
  }

  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/backup/restore`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${authStore.token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ fileName })
      })

      if (!response.ok) {
        const errorData = await response.json()
        throw new Error(errorData.message || 'Failed to restore backup')
      }

      await response.json()
      toast.success('Backup restored successfully! Reloading...')
      
      // Reload after restore
      setTimeout(() => {
        window.location.reload()
      }, 1500)
    } catch (err: any) {
      logError('Failed to restore backup', err)
      toast.error(`Failed to restore backup: ${err.message}`)
    }
  }, `restore-${fileName}`)
}

const deleteBackup = async (fileName: string) => {
  if (!confirm(`⚠️ Delete backup: ${fileName}?\n\nThis action cannot be undone.`)) {
    return
  }

  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/backup/delete/${encodeURIComponent(fileName)}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        }
      })

      if (!response.ok) {
        throw new Error('Failed to delete backup')
      }

      toast.success('Backup deleted successfully!')
      await refreshBackupList()
    } catch (err: any) {
      logError('Failed to delete backup', err)
      toast.error('Failed to delete backup')
    }
  }, `delete-${fileName}`)
}

const toggleAutoBackup = async () => {
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/backup/auto-backup/toggle`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${authStore.token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ enabled: autoBackupEnabled.value })
      })

      if (!response.ok) {
        throw new Error('Failed to toggle auto backup')
      }

      toast.success(`Auto backup ${autoBackupEnabled.value ? 'enabled' : 'disabled'}`)
      await refreshBackupList()
    } catch (err: any) {
      logError('Failed to toggle auto backup', err)
      toast.error('Failed to update auto backup setting')
      // Revert the toggle
      autoBackupEnabled.value = !autoBackupEnabled.value
    }
  }, 'toggleAutoBackup')
}

const saveAutoBackupRetention = async () => {
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/backup/auto-backup/retention`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${authStore.token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ retention: autoBackupRetention.value })
      })

      if (!response.ok) {
        throw new Error('Failed to save retention setting')
      }

      toast.success('Retention setting saved')
    } catch (err: any) {
      logError('Failed to save retention setting', err)
      toast.error('Failed to save retention setting')
    }
  }, 'saveRetention')
}

// Helper functions
const formatDate = (dateString: string) => {
  if (!dateString) return 'N/A'
  const date = new Date(dateString)
  return date.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
}

// Load backup settings
const loadBackupSettings = async () => {
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/backup/settings`, {
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        }
      })

      if (response.ok) {
        const result = await response.json()
        autoBackupEnabled.value = result.enabled || false
        autoBackupRetention.value = result.retention || 7
      }
    } catch (err: any) {
      logError('Failed to load backup settings', err)
    }
  })
}

// Initialize
onMounted(async () => {
  try {
    await loadBackupSettings()
    await refreshBackupList()
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.backup-page {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 2rem;
}

.page-title {
  font-size: 2rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 0.5rem 0;
}

.page-description {
  font-size: 1rem;
  color: #6b7280;
  margin: 0;
}

.loading-state {
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

.backup-stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: linear-gradient(135deg, #f9fafb 0%, #ffffff 100%);
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: all 0.3s ease;
}

.stat-card:hover {
  border-color: #4f46e5;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.15);
}

.stat-icon {
  font-size: 2.5rem;
  line-height: 1;
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1f2937;
  line-height: 1.2;
}

.stat-label {
  font-size: 0.875rem;
  color: #6b7280;
  margin-top: 0.25rem;
}

.content-section {
  background: #ffffff;
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
  margin-bottom: 2rem;
}

.section-header {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 0.75rem 0;
}

.section-description {
  color: #6b7280;
  margin: 0 0 1.5rem 0;
  line-height: 1.6;
}

.section-header-collapsible {
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  user-select: none;
  padding-bottom: 0.75rem;
}

.section-header-collapsible:hover .section-header {
  color: #4f46e5;
}

.collapse-icon {
  font-size: 1.25rem;
  color: #6b7280;
  transition: transform 0.2s;
}

.collapsible-content {
  padding-top: 0.75rem;
}

.backup-manual-actions {
  display: flex;
  gap: 1rem;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1rem;
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
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}

.btn-secondary {
  background: #e5e7eb;
  color: #1f2937;
}

.btn-secondary:hover:not(:disabled) {
  background: #d1d5db;
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

.btn-large {
  padding: 1rem 2rem;
  font-size: 1.125rem;
  font-weight: 600;
}

.btn-sm {
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
}

.btn-sm .icon {
  font-size: 1rem;
}

.backup-status-message {
  padding: 1rem 1.25rem;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 500;
  margin-top: 1rem;
}

.backup-status-message.success {
  background: #d1fae5;
  color: #065f46;
  border: 1px solid #6ee7b7;
}

.backup-status-message.error {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.backup-status-message.info {
  background: #dbeafe;
  color: #1e40af;
  border: 1px solid #93c5fd;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
  color: #1f2937;
  margin-bottom: 0.5rem;
  cursor: pointer;
}

.form-checkbox {
  width: 20px;
  height: 20px;
  cursor: pointer;
}

.form-hint {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0.5rem 0 0 0;
}

.form-input {
  padding: 0.75rem;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.auto-backup-info {
  background: #f9fafb;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  padding: 1.5rem;
  margin-top: 1rem;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 0.75rem 0;
  border-bottom: 1px solid #e5e7eb;
}

.info-row:last-child {
  border-bottom: none;
}

.info-label {
  font-weight: 500;
  color: #6b7280;
}

.info-value {
  font-weight: 600;
  color: #1f2937;
}

.loading-state-inline {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1.5rem;
  background: #f9fafb;
  border-radius: 8px;
  color: #6b7280;
}

.spinner-sm {
  width: 20px;
  height: 20px;
  border: 2px solid #e5e7eb;
  border-top-color: #4f46e5;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  background: #f9fafb;
  border: 2px dashed #d1d5db;
  border-radius: 12px;
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.empty-state h4 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 0.5rem 0;
}

.empty-state p {
  color: #6b7280;
  margin: 0;
}

.backup-group {
  margin-bottom: 2rem;
}

.backup-group:last-child {
  margin-bottom: 0;
}

.backup-group-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 1rem 0;
  padding-bottom: 0.5rem;
  border-bottom: 2px solid #e5e7eb;
}

.backup-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.25rem;
  background: #ffffff;
  border: 2px solid #e5e7eb;
  border-radius: 10px;
  margin-bottom: 1rem;
  transition: all 0.2s ease;
}

.backup-item:hover {
  border-color: #4f46e5;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.1);
}

.backup-item-icon {
  font-size: 2rem;
  line-height: 1;
  flex-shrink: 0;
}

.backup-item-info {
  flex: 1;
  min-width: 0;
}

.backup-item-name {
  font-weight: 600;
  color: #1f2937;
  font-size: 1rem;
  margin-bottom: 0.25rem;
  word-break: break-all;
}

.backup-item-meta {
  font-size: 0.875rem;
  color: #6b7280;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.meta-separator {
  color: #d1d5db;
}

.backup-item-actions {
  display: flex;
  gap: 0.5rem;
  flex-shrink: 0;
  flex-wrap: wrap;
}

.backup-notes {
  background: #fffbeb;
  border: 2px solid #fbbf24;
  border-radius: 12px;
  padding: 2rem;
}

.note-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-top: 1.5rem;
}

.note-card {
  background: #ffffff;
  border: 1px solid #fde68a;
  border-radius: 8px;
  padding: 1.25rem;
}

.note-icon {
  font-size: 2rem;
  margin-bottom: 0.75rem;
}

.note-card h4 {
  font-size: 1rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 0.5rem 0;
}

.note-card p {
  font-size: 0.875rem;
  color: #6b7280;
  line-height: 1.6;
  margin: 0;
}

.note-card code {
  background: #fef3c7;
  padding: 0.125rem 0.375rem;
  border-radius: 3px;
  font-family: 'Courier New', monospace;
  font-size: 0.813rem;
  color: #92400e;
}
</style>
