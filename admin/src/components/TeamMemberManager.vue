<template>
  <div class="team-manager">
    <div class="manager-header">
      <h4>Team Members</h4>
      <button @click="openAddModal" class="btn btn-primary btn-sm">
        <span class="icon">➕</span>
        Add Team Member
      </button>
    </div>

    <div class="team-grid">
      <div v-for="(member, index) in teamMembers" :key="index" class="team-card">
        <div class="team-image">
          <img v-if="member.imageUrl && member.imageUrl.trim() !== ''" :src="getMediaUrl(member.imageUrl)" :alt="member.name" />
          <div v-else class="image-placeholder">
            <span class="icon">👤</span>
          </div>
        </div>
        <div class="team-info">
          <h5 class="team-name">{{ member.name }}</h5>
          <p class="team-role">{{ member.role }}</p>
          <p class="team-bio">{{ member.bio }}</p>
        </div>
        <div class="team-actions">
          <button @click="openEditModal(index)" class="btn btn-sm btn-outline">
            ✏️ Edit
          </button>
          <button @click="deleteMember(index)" class="btn btn-sm btn-danger">
            🗑️ Delete
          </button>
        </div>
      </div>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ editIndex !== null ? 'Edit' : 'Add' }} Team Member</h3>
          <button @click="closeModal" class="modal-close">×</button>
        </div>
        
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Photo</label>
            <div class="photo-upload-wrapper">
              <div v-if="editForm.imageUrl && editForm.imageUrl.trim() !== ''" class="current-image-preview">
                <img :src="getMediaUrl(editForm.imageUrl)" alt="Current" class="mini-preview" />
                <button @click="editForm.imageUrl = ''" class="btn btn-sm btn-danger" type="button">
                  🗑️ Remove Photo
                </button>
              </div>
              <button @click="showMediaPicker = true" class="btn btn-secondary" type="button">
                {{ editForm.imageUrl ? 'Change Photo' : 'Select from Library' }}
              </button>
              <p class="form-hint">Select a photo from the Media Library</p>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label required">Name</label>
            <input 
              v-model="editForm.name" 
              type="text" 
              class="form-control"
              placeholder="e.g., John Smith"
            />
          </div>

          <div class="form-group">
            <label class="form-label required">Role/Position</label>
            <input 
              v-model="editForm.role" 
              type="text" 
              class="form-control"
              placeholder="e.g., Sales Manager"
            />
          </div>

          <div class="form-group">
            <label class="form-label required">Bio</label>
            <textarea 
              v-model="editForm.bio" 
              class="form-control"
              rows="3"
              placeholder="Brief description of the team member..."
            ></textarea>
          </div>
        </div>

        <div class="modal-footer">
          <button @click="closeModal" class="btn btn-outline">Cancel</button>
          <button @click="saveMember" class="btn btn-primary" :disabled="!isFormValid">
            💾 Save
          </button>
        </div>
      </div>
    </div>

    <!-- Media Picker -->
    <MediaPicker 
      :is-open="showMediaPicker"
      @close="showMediaPicker = false"
      @select="handleMediaSelection"
      media-type="Image"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import MediaPicker from './MediaPicker.vue';
import { logError } from '@/services/logger';
import { getMediaUrl } from '@/utils/api-config';

interface TeamMember {
  name: string;
  role: string;
  bio: string;
  imageUrl: string;
}

interface Props {
  modelValue: string;
}

interface Emits {
  (e: 'update:modelValue', value: string): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const teamMembers = ref<TeamMember[]>([]);
const showModal = ref(false);
const showMediaPicker = ref(false);
const editIndex = ref<number | null>(null);

const editForm = ref<TeamMember>({
  name: '',
  role: '',
  bio: '',
  imageUrl: ''
});

// Parse JSON from prop
watch(() => props.modelValue, (newValue) => {
  try {
    const parsed = JSON.parse(newValue || '[]');
    teamMembers.value = Array.isArray(parsed) ? parsed : [];
  } catch (error) {
    logError('Error loading team members', error);
    teamMembers.value = [];
  }
}, { immediate: true });

// Emit changes
const emitChanges = () => {
  emit('update:modelValue', JSON.stringify(teamMembers.value));
};

const isFormValid = computed(() => {
  return editForm.value.name.trim() && 
         editForm.value.role.trim() && 
         editForm.value.bio.trim();
});

const openAddModal = () => {
  editIndex.value = null;
  editForm.value = { name: '', role: '', bio: '', imageUrl: '' };
  showModal.value = true;
};

const openEditModal = (index: number) => {
  editIndex.value = index;
  editForm.value = { ...teamMembers.value[index] };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  editIndex.value = null;
  editForm.value = { name: '', role: '', bio: '', imageUrl: '' };
};

const saveMember = () => {
  if (!isFormValid.value) return;

  if (editIndex.value !== null) {
    teamMembers.value[editIndex.value] = { ...editForm.value };
  } else {
    teamMembers.value.push({ ...editForm.value });
  }

  emitChanges();
  closeModal();
};

const deleteMember = (index: number) => {
  if (confirm('Are you sure you want to delete this team member?')) {
    teamMembers.value.splice(index, 1);
    emitChanges();
  }
};

const handleMediaSelection = (mediaFile: any) => {
  editForm.value.imageUrl = mediaFile.filePath;
  showMediaPicker.value = false;
};
</script>

<style scoped>
.team-manager {
  margin-bottom: 2rem;
}

.manager-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.manager-header h4 {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
}

.team-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
}

.team-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.team-image {
  width: 100%;
  aspect-ratio: 1;
  border-radius: 8px;
  overflow: hidden;
  background: #f3f4f6;
}

.team-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 4rem;
  color: #9ca3af;
}

.team-info {
  flex: 1;
}

.team-name {
  margin: 0 0 0.25rem 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
}

.team-role {
  margin: 0 0 0.5rem 0;
  font-size: 0.875rem;
  color: #6b7280;
  font-weight: 500;
}

.team-bio {
  margin: 0;
  font-size: 0.875rem;
  color: #4b5563;
  line-height: 1.5;
}

.team-actions {
  display: flex;
  gap: 0.5rem;
}

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

.modal-content {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
}

.modal-close {
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
}

.modal-close:hover {
  color: #1f2937;
}

.modal-body {
  padding: 1.5rem;
  overflow-y: auto;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1.5rem;
  border-top: 1px solid #e5e7eb;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group:last-child {
  margin-bottom: 0;
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
  padding: 0.625rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  transition: all 0.2s;
}

.form-control:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.form-hint {
  font-size: 0.75rem;
  color: #6b7280;
  margin: 0.375rem 0 0 0;
}

.photo-upload-wrapper {
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1rem;
  background-color: #f9fafb;
}

.current-image-preview {
  margin-top: 1rem;
  padding: 1rem;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 6px;
}

.current-image-preview .form-hint {
  margin-bottom: 0.5rem;
}

.mini-preview {
  max-width: 150px;
  max-height: 150px;
  border-radius: 6px;
  display: block;
  margin: 0.5rem 0;
  border: 1px solid #d1d5db;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
  font-size: 0.875rem;
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
}

.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.813rem;
}

.btn-primary {
  background: #4f46e5;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #4338ca;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-outline {
  background: white;
  color: #4f46e5;
  border: 1px solid #4f46e5;
}

.btn-outline:hover {
  background: #eef2ff;
}

.btn-danger {
  background: #ef4444;
  color: white;
}

.btn-danger:hover {
  background: #dc2626;
}

.icon {
  display: inline-block;
}
</style>
