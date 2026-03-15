<template>
  <AdminLayout>
    <div class="inquiries-page">
      <div class="page-header">
        <div class="header-content">
          <h1>Contact Inquiries</h1>
          <p>Manage customer contact form submissions</p>
        </div>
        <div class="stats-row">
          <div class="stat-card">
            <div class="stat-label">Total</div>
            <div class="stat-value">{{ stats.totalSubmissions }}</div>
          </div>
          <div class="stat-card new">
            <div class="stat-label">New</div>
            <div class="stat-value">{{ stats.newSubmissions }}</div>
          </div>
          <div class="stat-card progress">
            <div class="stat-label">In Progress</div>
            <div class="stat-value">{{ stats.inProgressSubmissions }}</div>
          </div>
          <div class="stat-card resolved">
            <div class="stat-label">Resolved</div>
            <div class="stat-value">{{ stats.resolvedSubmissions }}</div>
          </div>
          <div class="stat-card">
            <div class="stat-label">Avg Response</div>
            <div class="stat-value">{{ formatMinutes(stats.avgResponseTimeMinutes) }}</div>
          </div>
        </div>
      </div>

      <div class="filters">
        <div class="filter-group">
          <label>Status:</label>
          <select v-model="filters.status" @change="loadSubmissions">
            <option value="">All</option>
            <option value="New">New</option>
            <option value="Read">Read</option>
            <option value="InProgress">In Progress</option>
            <option value="Replied">Replied</option>
            <option value="Resolved">Resolved</option>
            <option value="Archived">Archived</option>
          </select>
        </div>
        <div class="filter-group">
          <label>From Date:</label>
          <input type="date" v-model="filters.fromDate" @change="loadSubmissions" />
        </div>
        <div class="filter-group">
          <label>To Date:</label>
          <input type="date" v-model="filters.toDate" @change="loadSubmissions" />
        </div>
        <button class="btn-clear" @click="clearFilters">Clear Filters</button>
      </div>

      <div class="loading-container">
        <div v-if="isLoading" class="loading-overlay">
          <div class="spinner"></div>
          <p>Loading inquiries...</p>
        </div>
        
        <div v-if="submissions.length === 0 && !isLoading" class="empty-state">
          <p>No inquiries found</p>
        </div>

        <div v-if="!isLoading && submissions.length > 0" class="submissions-table">
        <table>
          <thead>
            <tr>
              <th>Status</th>
              <th>Name</th>
              <th>Email</th>
              <th>Subject</th>
              <th>Received</th>
              <th>Assigned To</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="submission in submissions" :key="submission.id" :class="{ 'unread': submission.status === 'New' }">
              <td>
                <span class="status-badge" :class="getStatusClass(submission.status)">
                  {{ formatStatus(submission.status) }}
                </span>
              </td>
              <td>{{ submission.name }}</td>
              <td>{{ submission.email }}</td>
              <td>{{ submission.subject || '(No subject)' }}</td>
              <td>{{ formatDate(submission.createdAt) }}</td>
              <td>{{ submission.assignedToUser?.fullName || '-' }}</td>
              <td>
                <button class="btn-view" @click="viewSubmission(submission)">View</button>
                <button class="btn-delete" @click="deleteSubmission(submission.id)" :disabled="isActionLoading(submission.id)">
                  <span v-if="isActionLoading(submission.id)" class="btn-spinner"></span>
                  {{ isActionLoading(submission.id) ? 'Deleting...' : 'Delete' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      </div>

      <!-- View/Edit Modal -->
      <div v-if="selectedSubmission" class="modal" @click.self="closeModal">
        <div class="modal-content">
          <div class="modal-header">
            <h2>Contact Inquiry Details</h2>
            <button class="close-btn" @click="closeModal">&times;</button>
          </div>
          <div class="modal-body">
            <div class="detail-group">
              <label>Name:</label>
              <div>{{ selectedSubmission.name }}</div>
            </div>
            <div class="detail-group">
              <label>Email:</label>
              <div>
                <a :href="`mailto:${selectedSubmission.email}`">{{ selectedSubmission.email }}</a>
              </div>
            </div>
            <div class="detail-group">
              <label>Subject:</label>
              <div>{{ selectedSubmission.subject || '(No subject)' }}</div>
            </div>
            <div class="detail-group">
              <label>Message:</label>
              <div class="message-box">{{ selectedSubmission.message }}</div>
            </div>
            <div class="detail-group">
              <label>Received:</label>
              <div>{{ formatDate(selectedSubmission.createdAt) }}</div>
            </div>
            <div class="detail-group" v-if="selectedSubmission.readAt">
              <label>Read At:</label>
              <div>{{ formatDate(selectedSubmission.readAt) }}</div>
            </div>

            <hr />

            <div class="detail-group">
              <label>Status:</label>
              <select v-model="editData.status">
                <option value="New">New</option>
                <option value="Read">Read</option>
                <option value="InProgress">In Progress</option>
                <option value="Replied">Replied</option>
                <option value="Resolved">Resolved</option>
                <option value="Archived">Archived</option>
              </select>
            </div>

            <div class="detail-group">
              <label>Assign To:</label>
              <select v-model="editData.assignedToUserId">
                <option :value="null">Unassigned</option>
                <option v-for="user in users" :key="user.id" :value="user.id">
                  {{ user.fullName }}
                </option>
              </select>
            </div>

            <div class="detail-group">
              <label>Admin Notes:</label>
              <textarea v-model="editData.adminNotes" rows="4" placeholder="Add internal notes..."></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="closeModal">Cancel</button>
            <button class="btn-save" @click="saveSubmission" :disabled="isActionLoading('save')">
              <span v-if="isActionLoading('save')" class="btn-spinner"></span>
              {{ isActionLoading('save') ? 'Saving...' : 'Save Changes' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import AdminLayout from '../components/AdminLayout.vue';
import { useAuthStore } from '../stores/auth';
import { useToast } from '@/composables/useToast';
import { useLoadingState } from '@/composables/useLoadingState';
import { logError } from '@/services/logger';
import { API_URL } from '@/utils/api-config';

const router = useRouter();
const authStore = useAuthStore();
const toast = useToast();
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState();

interface ContactSubmission {
  id: number;
  name: string;
  email: string;
  subject: string | null;
  message: string;
  status: string;
  adminNotes: string | null;
  createdAt: string;
  readAt: string | null;
  assignedToUserId: number | null;
  assignedToUser: { id: number; fullName: string } | null;
}

interface User {
  id: number;
  fullName: string;
}

interface Stats {
  totalSubmissions: number;
  newSubmissions: number;
  inProgressSubmissions: number;
  resolvedSubmissions: number;
  avgResponseTimeMinutes: number;
}

const submissions = ref<ContactSubmission[]>([]);
const users = ref<User[]>([]);
const selectedSubmission = ref<ContactSubmission | null>(null);
const editData = ref({
  status: '',
  adminNotes: '',
  assignedToUserId: null as number | null
});

const filters = ref({
  status: '',
  fromDate: '',
  toDate: ''
});

const stats = ref<Stats>({
  totalSubmissions: 0,
  newSubmissions: 0,
  inProgressSubmissions: 0,
  resolvedSubmissions: 0,
  avgResponseTimeMinutes: 0
});

onMounted(() => {
  loadSubmissions();
  loadStats();
  loadUsers();
});

async function loadSubmissions() {
  await executeWithLoading(async () => {
    try {
      const params = new URLSearchParams();
      if (filters.value.status) params.append('status', filters.value.status);
      if (filters.value.fromDate) params.append('fromDate', filters.value.fromDate);
      if (filters.value.toDate) params.append('toDate', filters.value.toDate);

      const response = await fetch(`${API_URL}/admin/contact-submissions?${params}`, {
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        }
      });

      if (!response.ok) throw new Error('Failed to load submissions');
      
      const data = await response.json();
      submissions.value = data;
    } catch (error) {
      logError('Error loading submissions', error);
      toast.error('Failed to load contact inquiries. Please try again.');
    }
  });
}

async function loadStats() {
  try {
    const response = await fetch(`${API_URL}/admin/contact-submissions/stats`, {
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      }
    });

    if (!response.ok) throw new Error('Failed to load stats');
    
    const data = await response.json();
    stats.value = data;
  } catch (error) {
    logError('Error loading stats', error);
  }
}

async function loadUsers() {
  try {
    const response = await fetch(`${API_URL}/admin/users`, {
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      }
    });

    if (!response.ok) throw new Error('Failed to load users');
    
    const data = await response.json();
    users.value = data.users;
  } catch (error) {
    logError('Error loading users', error);
  }
}

function viewSubmission(submission: ContactSubmission) {
  selectedSubmission.value = submission;
  editData.value = {
    status: submission.status,
    adminNotes: submission.adminNotes || '',
    assignedToUserId: submission.assignedToUserId
  };
}

function closeModal() {
  selectedSubmission.value = null;
  editData.value = {
    status: '',
    adminNotes: '',
    assignedToUserId: null
  };
}

async function saveSubmission() {
  if (!selectedSubmission.value) return;

  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/contact-submissions/${selectedSubmission.value.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authStore.token}`
        },
        body: JSON.stringify({
          status: editData.value.status,
          adminNotes: editData.value.adminNotes,
          assignedToUserId: editData.value.assignedToUserId
        })
      });

      if (!response.ok) throw new Error('Failed to update submission');

      toast.success('Contact inquiry updated successfully!');
      closeModal();
      await loadSubmissions();
      await loadStats();
    } catch (error) {
      logError('Error saving submission', error);
      toast.error('Failed to update contact inquiry. Please try again.');
    }
  }, 'save');
}

async function deleteSubmission(id: number) {
  if (!confirm('Are you sure you want to delete this inquiry? This action cannot be undone.')) {
    return;
  }

  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/contact-submissions/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        }
      });

      if (!response.ok) throw new Error('Failed to delete submission');

      toast.deleteSuccess('Contact inquiry');
      await loadSubmissions();
      await loadStats();
    } catch (error) {
      logError('Error deleting submission', error);
      toast.deleteError('Failed to delete contact inquiry. Please try again.');
    }
  }, id);
}

function clearFilters() {
  filters.value = {
    status: '',
    fromDate: '',
    toDate: ''
  };
  loadSubmissions();
}

function formatDate(dateString: string) {
  const date = new Date(dateString);
  return date.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit'
  });
}

function formatStatus(status: string) {
  return status.replace(/([A-Z])/g, ' $1').trim();
}

function getStatusClass(status: string) {
  return status.toLowerCase().replace(/\s+/g, '-');
}

function formatMinutes(minutes: number) {
  if (!minutes) return '-';
  if (minutes < 60) return `${Math.round(minutes)}m`;
  const hours = Math.floor(minutes / 60);
  const mins = Math.round(minutes % 60);
  return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
}
</script>

<style scoped>
.inquiries-page {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 2rem;
}

.header-content h1 {
  font-size: 2rem;
  font-weight: 600;
  color: #1a1a1a;
  margin-bottom: 0.5rem;
}

.header-content p {
  color: #666;
  font-size: 1rem;
}

.stats-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  margin-top: 1.5rem;
}

.stat-card {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1rem;
  text-align: center;
}

.stat-card.new {
  border-left: 4px solid #2196f3;
}

.stat-card.progress {
  border-left: 4px solid #ff9800;
}

.stat-card.resolved {
  border-left: 4px solid #4caf50;
}

.stat-label {
  font-size: 0.875rem;
  color: #666;
  margin-bottom: 0.5rem;
}

.stat-value {
  font-size: 1.75rem;
  font-weight: 600;
  color: #1a1a1a;
}

.filters {
  display: flex;
  gap: 1rem;
  align-items: end;
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-group label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #333;
}

.filter-group select,
.filter-group input {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.875rem;
}

.btn-clear {
  padding: 0.5rem 1rem;
  background: #f5f5f5;
  border: 1px solid #ddd;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
}

.btn-clear:hover {
  background: #e0e0e0;
}

.loading,
.empty-state {
  text-align: center;
  padding: 3rem;
  color: #666;
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
}

.submissions-table {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background: #f5f5f5;
  border-bottom: 2px solid #e0e0e0;
}

th {
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: #333;
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

td {
  padding: 1rem;
  border-bottom: 1px solid #e0e0e0;
  font-size: 0.875rem;
}

tr:hover {
  background: #f9f9f9;
}

tr.unread {
  background: #e3f2fd;
}

tr.unread:hover {
  background: #d1e9f6;
}

.status-badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-badge.new {
  background: #e3f2fd;
  color: #1976d2;
}

.status-badge.read {
  background: #f3e5f5;
  color: #7b1fa2;
}

.status-badge.inprogress,
.status-badge.in-progress {
  background: #fff3e0;
  color: #f57c00;
}

.status-badge.replied {
  background: #e8f5e9;
  color: #388e3c;
}

.status-badge.resolved {
  background: #e8f5e9;
  color: #2e7d32;
}

.status-badge.archived {
  background: #f5f5f5;
  color: #666;
}

.btn-view,
.btn-delete {
  padding: 0.375rem 0.75rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
  margin-right: 0.5rem;
}

.btn-view {
  background: #2196f3;
  color: white;
}

.btn-view:hover {
  background: #1976d2;
}

.btn-delete {
  background: #f44336;
  color: white;
}

.btn-delete:hover {
  background: #d32f2f;
}

/* Modal Styles */
.modal {
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
  border-radius: 8px;
  max-width: 700px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e0e0e0;
}

.modal-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

.close-btn {
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

.close-btn:hover {
  color: #333;
}

.modal-body {
  padding: 1.5rem;
}

.detail-group {
  margin-bottom: 1.5rem;
}

.detail-group label {
  display: block;
  font-weight: 600;
  color: #333;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
}

.detail-group > div {
  color: #666;
}

.message-box {
  background: #f9f9f9;
  padding: 1rem;
  border-radius: 4px;
  white-space: pre-wrap;
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid #e0e0e0;
}

.detail-group select,
.detail-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.875rem;
  font-family: inherit;
}

.detail-group textarea {
  resize: vertical;
}

hr {
  margin: 1.5rem 0;
  border: none;
  border-top: 1px solid #e0e0e0;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1.5rem;
  border-top: 1px solid #e0e0e0;
}

.btn-cancel,
.btn-save {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
}

.btn-cancel {
  background: #f5f5f5;
  color: #333;
}

.btn-cancel:hover {
  background: #e0e0e0;
}

.btn-save {
  background: #2196f3;
  color: white;
}

.btn-save:hover:not(:disabled) {
  background: #1976d2;
}

.btn-save:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
