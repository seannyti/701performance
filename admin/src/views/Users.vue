?<template>
  <AdminLayout>
    <div class="users-page">
      <!-- Loading overlay -->
      <div v-if="isLoading && users.length === 0" class="loading-overlay">
        <div class="spinner spinner-lg"></div>
      </div>

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">User Management</h1>
          <p class="page-subtitle">{{ filteredUsers.length }} of {{ users.length }} users shown</p>
        </div>
        <button v-if="authStore.isSuperAdmin" class="btn" @click="openAddModal">
          ➕ Add User
        </button>
      </div>

      <!-- Filters bar -->
      <div class="filters-bar card">
        <input
          v-model="search"
          class="form-control search-input"
          placeholder="Search by name, email, or IP…"
          type="search"
        />

        <select v-model="roleFilter" class="form-control filter-select">
          <option value="">All roles</option>
          <option :value="UserRole.User">User</option>
          <option :value="UserRole.Admin">Admin</option>
          <option :value="UserRole.SuperAdmin">SuperAdmin</option>
        </select>

        <select v-model="statusFilter" class="form-control filter-select">
          <option value="">All statuses</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
        </select>

        <button
          class="btn btn-sm"
          :class="showInactive ? '' : 'btn-secondary'"
          @click="showInactive = !showInactive"
          :title="showInactive ? 'Hiding inactive only if status filter is set' : 'Toggle inactive visibility'"
        >
          {{ showInactive ? '👁 All users' : '👁 Show inactive' }}
        </button>

        <button class="btn btn-secondary btn-sm" :disabled="isLoading" @click="loadUsers">
          {{ isLoading ? '⟳ Loading…' : '⟳ Refresh' }}
        </button>
      </div>

      <!-- Error banner -->
      <div v-if="error" class="error-banner">
        ⚠ {{ error }}
        <button class="btn btn-sm" style="margin-left:auto" @click="loadUsers">Retry</button>
      </div>

      <!-- Table -->
      <div class="card">
        <div v-if="isLoading && users.length === 0" class="empty-state">
          <div class="skeleton-row" v-for="i in 5" :key="i" />
        </div>

        <table v-else-if="filteredUsers.length > 0" class="table">
          <thead>
            <tr>
              <th>User</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Online</th>
              <th>Last Login</th>
              <th>Last Login IP</th>
              <th style="text-align:right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in filteredUsers" :key="user.id" :class="{ 'row--inactive': !user.isActive }">
              <!-- User -->
              <td>
                <div class="user-cell">
                  <div class="avatar" :style="{ background: avatarColor(user) }">
                    {{ initials(user) }}
                  </div>
                  <div>
                    <div class="user-name">
                      {{ user.firstName }} {{ user.lastName }}
                      <span v-if="user.subscribeNewsletter" class="newsletter-dot" title="Subscribed to newsletter">●</span>
                    </div>
                    <div class="user-meta">
                      <span v-if="user.phone">{{ user.phone }}</span>
                      <span class="text-muted">Joined {{ formatDate(user.createdAt) }}</span>
                    </div>
                  </div>
                </div>
              </td>

              <!-- Email -->
              <td class="text-muted" style="font-size:0.875rem">
                {{ user.email }}
                <span v-if="user.isEmailVerified" class="verified-badge" title="Email verified">✓</span>
                <span v-else class="unverified-badge" title="Email not verified">!</span>
              </td>

              <!-- Role -->
              <td>
                <span class="role-badge" :class="roleBadgeClass(user.roleName)">
                  {{ user.roleName }}
                </span>
              </td>

              <!-- Status -->
              <td>
                <span class="status-badge" :class="user.isActive ? 'status-badge--active' : 'status-badge--inactive'">
                  {{ user.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>

              <!-- Online indicator -->
              <td>
                <span class="online-badge" :class="isOnline(user) ? 'online-badge--on' : 'online-badge--off'" :title="isOnline(user) ? 'Online now' : (user.lastSeenAt ? 'Last seen ' + timeAgo(user.lastSeenAt) : 'Never seen')">
                  <span class="online-dot"></span>
                  {{ isOnline(user) ? 'Online' : 'Offline' }}
                </span>
              </td>

              <!-- Last login -->
              <td class="text-muted" style="font-size:0.8rem">
                {{ user.lastLoginAt ? timeAgo(user.lastLoginAt) : 'Never' }}
              </td>

              <!-- IP -->
              <td>
                <span v-if="user.lastLoginIp" class="ip-badge" :title="user.lastLoginIp">
                  {{ formatIp(user.lastLoginIp) }}
                </span>
                <span v-else class="text-muted" style="font-size:0.8rem">—</span>
              </td>

              <!-- Actions -->
              <td style="text-align:right">
                <div class="action-buttons">
                  <!-- Edit info (Admin+) -->
                  <button
                    class="btn btn-sm btn-secondary"
                    @click="openEditModal(user)"
                    title="Edit user info"
                  >
                    ✏️ Edit
                  </button>

                  <!-- Reset password (SuperAdmin only) -->
                  <button
                    v-if="authStore.isSuperAdmin"
                    class="btn btn-sm btn-secondary"
                    @click="openResetPwModal(user)"
                    title="Reset password"
                  >
                    🔑
                  </button>

                  <!-- Toggle status (SuperAdmin only, not self) -->
                  <button
                    v-if="authStore.isSuperAdmin && user.id !== currentUserId"
                    class="btn btn-sm"
                    :class="user.isActive ? 'btn-secondary' : 'btn-success'"
                    :disabled="isActionLoading(user.id)"
                    @click="toggleStatus(user)"
                    :title="user.isActive ? 'Deactivate' : 'Activate'"
                  >
                    <span v-if="isActionLoading(user.id)" class="btn-spinner"></span>
                    <span v-else>{{ user.isActive ? '⏸' : '▶' }}</span>
                  </button>

                  <!-- Delete (SuperAdmin only, not self) -->
                  <button
                    v-if="authStore.isSuperAdmin && user.id !== currentUserId"
                    class="btn btn-sm btn-danger"
                    @click="confirmDelete(user)"
                    title="Delete permanently"
                  >
                    🗑
                  </button>

                  <span v-if="user.id === currentUserId" class="text-muted" style="font-size:0.8rem">(you)</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Empty state -->
        <div v-else class="empty-state">
          <p>No users match your filters.</p>
          <button class="btn btn-secondary btn-sm" @click="resetFilters">Clear filters</button>
        </div>
      </div>

      <!-- ─── Edit User Modal ────────────────────────────────────────────── -->
      <div v-if="editTarget" class="modal-overlay" @click.self="closeEditModal">
        <div class="modal">
          <div class="modal-header">
            <h2 class="card-title">Edit User</h2>
            <button class="modal-close" @click="closeEditModal">✕</button>
          </div>
          <div class="modal-body">
            <div v-if="editError" class="error-banner" style="margin-bottom:1rem">{{ editError }}</div>

            <!-- Profile fields -->
            <div class="form-section-title">Profile Information</div>

            <div class="form-row">
              <div class="form-group">
                <label class="form-label">First Name *</label>
                <input v-model="editForm.firstName" class="form-control" :class="{ error: editFormErrors.firstName }" />
                <span v-if="editFormErrors.firstName" class="field-error">{{ editFormErrors.firstName }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Last Name *</label>
                <input v-model="editForm.lastName" class="form-control" :class="{ error: editFormErrors.lastName }" />
                <span v-if="editFormErrors.lastName" class="field-error">{{ editFormErrors.lastName }}</span>
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Email *</label>
              <input v-model="editForm.email" class="form-control" :class="{ error: editFormErrors.email }" type="email" />
              <span v-if="editFormErrors.email" class="field-error">{{ editFormErrors.email }}</span>
            </div>

            <div class="form-group">
              <label class="form-label">Phone</label>
              <input v-model="editForm.phone" class="form-control" type="tel" placeholder="(555) 000-0000" />
            </div>

            <!-- Role (SuperAdmin only) -->
            <div v-if="authStore.isSuperAdmin && editTarget.id !== currentUserId" class="form-group">
              <label class="form-label">Role</label>
              <select v-model="editForm.role" class="form-control">
                <option :value="UserRole.User">User</option>
                <option :value="UserRole.Admin">Admin</option>
                <option :value="UserRole.SuperAdmin">SuperAdmin</option>
              </select>
            </div>

            <!-- Newsletter -->
            <div class="form-group form-group--check">
              <label class="check-label">
                <input type="checkbox" v-model="editForm.subscribeNewsletter" class="check-input" />
                <span>Subscribed to newsletter</span>
              </label>
            </div>

            <!-- Email Verified -->
            <div class="form-group form-group--check">
              <label class="check-label">
                <input type="checkbox" v-model="editForm.isEmailVerified" class="check-input" />
                <span>Email verified</span>
              </label>
              <p class="form-hint" style="margin-top: 0.25rem; margin-left: 1.5rem;">Check to manually verify this user's email address</p>
            </div>

            <!-- Metadata (read-only) -->
            <div class="form-section-title" style="margin-top:1.25rem">Account Info</div>
            <div class="meta-grid">
              <div class="meta-item">
                <span class="meta-label">User ID</span>
                <span class="meta-value">#{{ editTarget.id }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">Joined</span>
                <span class="meta-value">{{ formatDate(editTarget.createdAt) }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">Last Login</span>
                <span class="meta-value">{{ editTarget.lastLoginAt ? timeAgo(editTarget.lastLoginAt) : 'Never' }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">Last Login IP</span>
                <span class="meta-value ip-mono">{{ formatIp(editTarget.lastLoginIp) }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">Status</span>
                <span class="status-badge" :class="editTarget.isActive ? 'status-badge--active' : 'status-badge--inactive'">
                  {{ editTarget.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
              <div v-if="editTarget.subscribeNewsletter" class="meta-item">
                <span class="meta-label">Newsletter</span>
                <span class="meta-value" style="color:#059669">✓ Subscribed</span>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="closeEditModal">Cancel</button>
            <button class="btn" :disabled="isActionLoading('editUser')" @click="submitEditUser">
              <span v-if="isActionLoading('editUser')" class="btn-spinner"></span>
              {{ isActionLoading('editUser') ? 'Saving…' : 'Save Changes' }}
            </button>
          </div>
        </div>
      </div>

      <!-- ─── Reset Password Modal ───────────────────────────────────────── -->
      <div v-if="resetPwTarget" class="modal-overlay" @click.self="resetPwTarget = null">
        <div class="modal modal--narrow">
          <div class="modal-header">
            <h2 class="card-title">🔑 Reset Password</h2>
            <button class="modal-close" @click="resetPwTarget = null">✕</button>
          </div>
          <div class="modal-body">
            <div v-if="resetPwError" class="error-banner" style="margin-bottom:1rem">{{ resetPwError }}</div>
            <p style="margin-bottom:1rem; font-size:0.9rem; color:#374151">
              Setting a new password for <strong>{{ resetPwTarget.firstName }} {{ resetPwTarget.lastName }}</strong>.
              All active sessions will be revoked.
            </p>
            <div class="form-group">
              <label class="form-label">New Password *</label>
              <input
                v-model="resetPwForm.newPassword"
                class="form-control"
                :class="{ error: resetPwFormErrors.newPassword }"
                type="password"
                placeholder="Min 8 characters"
              />
              <span v-if="resetPwFormErrors.newPassword" class="field-error">{{ resetPwFormErrors.newPassword }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Confirm Password *</label>
              <input
                v-model="resetPwForm.confirmPassword"
                class="form-control"
                :class="{ error: resetPwFormErrors.confirmPassword }"
                type="password"
                placeholder="Confirm new password"
              />
              <span v-if="resetPwFormErrors.confirmPassword" class="field-error">{{ resetPwFormErrors.confirmPassword }}</span>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="resetPwTarget = null">Cancel</button>
            <button class="btn btn-danger" :disabled="isActionLoading('resetPw')" @click="submitResetPassword">
              <span v-if="isActionLoading('resetPw')" class="btn-spinner"></span>
              {{ isActionLoading('resetPw') ? 'Resetting…' : 'Reset Password' }}
            </button>
          </div>
        </div>
      </div>

      <!-- ─── Add User Modal ──────────────────────────────────────────────── -->
      <div v-if="showAddModal" class="modal-overlay" @click.self="closeAddModal">
        <div class="modal">
          <div class="modal-header">
            <h2 class="card-title">Add New User</h2>
            <button class="modal-close" @click="closeAddModal">✕</button>
          </div>
          <div class="modal-body">
            <div v-if="addError" class="error-banner" style="margin-bottom:1rem">{{ addError }}</div>

            <div class="form-row">
              <div class="form-group">
                <label class="form-label">First Name *</label>
                <input v-model="addForm.firstName" class="form-control" :class="{ error: addFormErrors.firstName }" placeholder="John" />
                <span v-if="addFormErrors.firstName" class="field-error">{{ addFormErrors.firstName }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Last Name *</label>
                <input v-model="addForm.lastName" class="form-control" :class="{ error: addFormErrors.lastName }" placeholder="Doe" />
                <span v-if="addFormErrors.lastName" class="field-error">{{ addFormErrors.lastName }}</span>
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Email *</label>
              <input v-model="addForm.email" class="form-control" :class="{ error: addFormErrors.email }" type="email" placeholder="john@example.com" />
              <span v-if="addFormErrors.email" class="field-error">{{ addFormErrors.email }}</span>
            </div>

            <div class="form-group">
              <label class="form-label">Phone</label>
              <input v-model="addForm.phone" class="form-control" type="tel" placeholder="(555) 000-0000" />
            </div>

            <div class="form-row">
              <div class="form-group">
                <label class="form-label">Password *</label>
                <input v-model="addForm.password" class="form-control" :class="{ error: addFormErrors.password }" type="password" placeholder="Min 8 characters" />
                <span v-if="addFormErrors.password" class="field-error">{{ addFormErrors.password }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Role</label>
                <select v-model="addForm.role" class="form-control">
                  <option :value="UserRole.User">User</option>
                  <option :value="UserRole.Admin">Admin</option>
                  <option :value="UserRole.SuperAdmin">SuperAdmin</option>
                </select>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="closeAddModal">Cancel</button>
            <button class="btn" :disabled="isActionLoading('addUser')" @click="submitAddUser">
              <span v-if="isActionLoading('addUser')" class="btn-spinner"></span>
              {{ isActionLoading('addUser') ? 'Creating…' : 'Create User' }}
            </button>
          </div>
        </div>
      </div>

      <!-- ─── Delete Confirm Modal ───────────────────────────────────────── -->
      <div v-if="deleteTarget" class="modal-overlay" @click.self="deleteTarget = null">
        <div class="modal modal--narrow">
          <div class="modal-header">
            <h2 class="card-title" style="color:#dc2626">⚠ Delete User</h2>
          </div>
          <div class="modal-body">
            <p>
              Permanently delete <strong>{{ deleteTarget.firstName }} {{ deleteTarget.lastName }}</strong>
              ({{ deleteTarget.email }})?
            </p>
            <p class="text-muted" style="margin-top:0.5rem; font-size:0.875rem">This action cannot be undone.</p>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="deleteTarget = null">Cancel</button>
            <button class="btn btn-danger" :disabled="deleteTarget && isActionLoading(deleteTarget.id)" @click="deleteUser">
              <span v-if="deleteTarget && isActionLoading(deleteTarget.id)" class="btn-spinner"></span>
              {{ deleteTarget && isActionLoading(deleteTarget.id) ? 'Deleting…' : 'Delete permanently' }}
            </button>
          </div>
        </div>
      </div>

    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import { useAuthStore } from '@/stores/auth'
import { useLoadingState } from '@/composables/useLoadingState'
import { UserRole } from '@/types'
import type { AdminUser, CreateUserRequest, UpdateUserInfoRequest } from '@/types'
import { logError, logDebug } from '@/services/logger'

import { API_URL } from '@/utils/api-config'

const authStore = useAuthStore()
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState()

// ─── State ───────────────────────────────────────────────────────────────────
const users        = ref<AdminUser[]>([])
const error        = ref<string | null>(null)
const search       = ref('')
const roleFilter   = ref<UserRole | ''>('')
const statusFilter = ref<'active' | 'inactive' | ''>('')
const showInactive = ref(true)

// ─── Edit user modal ──────────────────────────────────────────────────────────
const editTarget     = ref<AdminUser | null>(null)
const editError      = ref<string | null>(null)
const editFormErrors = ref<Record<string, string>>({})
const editForm = ref<UpdateUserInfoRequest & { role: UserRole }>({
  firstName: '', lastName: '', email: '', phone: '', subscribeNewsletter: false, isEmailVerified: false, role: UserRole.User
})

// ─── Reset password modal ──────────────────────────────────────────────────────
const resetPwTarget     = ref<AdminUser | null>(null)
const resetPwError      = ref<string | null>(null)
const resetPwFormErrors = ref<Record<string, string>>({})
const resetPwForm = ref({ newPassword: '', confirmPassword: '' })

// ─── Add user modal ────────────────────────────────────────────────────────────
const showAddModal  = ref(false)
const addError      = ref<string | null>(null)
const addFormErrors = ref<Record<string, string>>({})
const addForm = ref<CreateUserRequest>({
  firstName: '', lastName: '', email: '', password: '', phone: '', role: UserRole.User
})

// ─── Delete modal ─────────────────────────────────────────────────────────────
const deleteTarget  = ref<AdminUser | null>(null)

const currentUserId = computed(() => authStore.user?.id ?? -1)

// ─── Computed ────────────────────────────────────────────────────────────────
const filteredUsers = computed(() => {
  const q = search.value.toLowerCase()
  return users.value.filter(u => {
    if (!showInactive.value && !u.isActive) return false

    const matchSearch = !q ||
      `${u.firstName} ${u.lastName}`.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q) ||
      (u.lastLoginIp ?? '').includes(q) ||
      formatIp(u.lastLoginIp).includes(q)

    // Handle both string role names and numeric role values from API
    const matchRole = roleFilter.value === '' || 
      u.role === Number(roleFilter.value) ||
      (Number(roleFilter.value) === 0 && (u.role === 'User' || u.role === 0)) ||
      (Number(roleFilter.value) === 1 && (u.role === 'Admin' || u.role === 1)) ||
      (Number(roleFilter.value) === 2 && (u.role === 'SuperAdmin' || u.role === 2))
    const matchStatus =
      statusFilter.value === '' ||
      (statusFilter.value === 'active' && u.isActive) ||
      (statusFilter.value === 'inactive' && !u.isActive)

    return matchSearch && matchRole && matchStatus
  })
})

// ─── Data loading ─────────────────────────────────────────────────────────────
const loadUsers = async () => {
  await executeWithLoading(async () => {
    error.value = null
    try {
      const res = await fetch(`${API_URL}/admin/users`, {
        headers: { Authorization: `Bearer ${authStore.token}` }
      })
      if (!res.ok) throw new Error(`Server responded with ${res.status}`)
      users.value = await res.json()
      logDebug('Users loaded', { count: users.value.length })
    } catch (err) {
      logError('Failed to load users', err)
      error.value = 'Could not load users. Is the API running?'
    }
  })
}

// ─── Edit user ────────────────────────────────────────────────────────────────
const openEditModal = (user: AdminUser) => {
  editTarget.value = user
  editForm.value = {
    firstName: user.firstName,
    lastName:  user.lastName,
    email:     user.email,
    phone:     user.phone ?? '',
    subscribeNewsletter: user.subscribeNewsletter ?? false,
    isEmailVerified: user.isEmailVerified ?? false,
    role: user.role
  }
  editFormErrors.value = {}
  editError.value = null
}

const closeEditModal = () => { editTarget.value = null }

const validateEditForm = (): boolean => {
  const errs: Record<string, string> = {}
  if (!editForm.value.firstName.trim()) errs.firstName = 'Required'
  if (!editForm.value.lastName.trim())  errs.lastName  = 'Required'
  if (!editForm.value.email.trim())     errs.email     = 'Required'
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(editForm.value.email)) errs.email = 'Invalid email'
  editFormErrors.value = errs
  return Object.keys(errs).length === 0
}

const submitEditUser = async () => {
  if (!editTarget.value || !validateEditForm()) return
  await executeWithLoading(async () => {
    editError.value = null
    try {
      const userId = editTarget.value!.id

      // Save profile info
      const infoRes = await fetch(`${API_URL}/admin/users/${userId}/info`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${authStore.token}` },
        body: JSON.stringify({
          firstName: editForm.value.firstName,
          lastName:  editForm.value.lastName,
          email:     editForm.value.email,
          phone:     editForm.value.phone || null,
          subscribeNewsletter: editForm.value.subscribeNewsletter,
          isEmailVerified: editForm.value.isEmailVerified
        })
      })
      if (!infoRes.ok) {
        const data = await infoRes.json()
        throw new Error(data.message ?? 'Update failed')
      }

      // Save role if changed and SuperAdmin
      if (authStore.isSuperAdmin && editTarget.value!.id !== currentUserId.value && editForm.value.role !== editTarget.value!.role) {
        const roleRes = await fetch(`${API_URL}/admin/users/${userId}/role`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${authStore.token}` },
          body: JSON.stringify({ role: editForm.value.role })
        })
        if (!roleRes.ok) {
          const data = await roleRes.json()
          throw new Error(data.message ?? 'Role update failed')
        }
      }

      // Update local state
      const u = users.value.find(u => u.id === userId)
      if (u) {
        u.firstName = editForm.value.firstName
        u.lastName  = editForm.value.lastName
        u.email     = editForm.value.email
        u.phone     = editForm.value.phone || undefined
        u.subscribeNewsletter = editForm.value.subscribeNewsletter
        u.isEmailVerified = editForm.value.isEmailVerified
        if (authStore.isSuperAdmin && u.id !== currentUserId.value) {
          u.role     = editForm.value.role
          u.roleName = roleLabel(editForm.value.role)
        }
      }

      closeEditModal()
    } catch (err) {
      logError('Failed to edit user', err)
      editError.value = (err as Error).message
    }
  }, 'editUser')
}

// ─── Reset password ────────────────────────────────────────────────────────────
const openResetPwModal = (user: AdminUser) => {
  resetPwTarget.value = user
  resetPwForm.value   = { newPassword: '', confirmPassword: '' }
  resetPwFormErrors.value = {}
  resetPwError.value  = null
}

const validateResetPwForm = (): boolean => {
  const errs: Record<string, string> = {}
  if (!resetPwForm.value.newPassword)             errs.newPassword     = 'Required'
  else if (resetPwForm.value.newPassword.length < 8) errs.newPassword  = 'Minimum 8 characters'
  if (resetPwForm.value.confirmPassword !== resetPwForm.value.newPassword) errs.confirmPassword = 'Passwords do not match'
  resetPwFormErrors.value = errs
  return Object.keys(errs).length === 0
}

const submitResetPassword = async () => {
  if (!resetPwTarget.value || !validateResetPwForm()) return
  await executeWithLoading(async () => {
    resetPwError.value = null
    try {
      const res = await fetch(`${API_URL}/admin/users/${resetPwTarget.value!.id}/password`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${authStore.token}` },
        body: JSON.stringify({ newPassword: resetPwForm.value.newPassword })
      })
      if (!res.ok) {
        const data = await res.json()
        throw new Error(data.message ?? 'Password reset failed')
      }
      resetPwTarget.value = null
    } catch (err) {
      logError('Failed to reset password', err)
      resetPwError.value = (err as Error).message
    }
  }, 'resetPw')
}

// ─── Status toggle ────────────────────────────────────────────────────────────
const toggleStatus = async (user: AdminUser) => {
  await executeWithLoading(async () => {
    try {
      const res = await fetch(`${API_URL}/admin/users/${user.id}/status`, {
        method: 'PUT',
        headers: { Authorization: `Bearer ${authStore.token}` }
      })
      if (!res.ok) {
        const data = await res.json()
        throw new Error(data.message ?? 'Status update failed')
      }
      user.isActive = !user.isActive
    } catch (err) {
      logError('Failed to toggle status', err)
      error.value = (err as Error).message
    }
  }, user.id)
}

// ─── Add user ─────────────────────────────────────────────────────────────────
const openAddModal = () => {
  addForm.value = { firstName: '', lastName: '', email: '', password: '', phone: '', role: UserRole.User }
  addFormErrors.value = {}
  addError.value = null
  showAddModal.value = true
}

const closeAddModal = () => { showAddModal.value = false }

const validateAddForm = (): boolean => {
  const errs: Record<string, string> = {}
  if (!addForm.value.firstName.trim()) errs.firstName = 'Required'
  if (!addForm.value.lastName.trim())  errs.lastName  = 'Required'
  if (!addForm.value.email.trim())     errs.email     = 'Required'
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(addForm.value.email)) errs.email = 'Invalid email'
  if (!addForm.value.password)         errs.password  = 'Required'
  else if (addForm.value.password.length < 8) errs.password = 'Minimum 8 characters'
  addFormErrors.value = errs
  return Object.keys(errs).length === 0
}

const submitAddUser = async () => {
  if (!validateAddForm()) return
  await executeWithLoading(async () => {
    addError.value = null
    try {
      const res = await fetch(`${API_URL}/admin/users`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${authStore.token}` },
        body: JSON.stringify(addForm.value)
      })
      const data = await res.json()
      if (!res.ok) throw new Error(data.message ?? 'Create failed')
      users.value.unshift(data)
      closeAddModal()
    } catch (err) {
      logError('Failed to create user', err)
      addError.value = (err as Error).message
    }
  }, 'addUser')
}

// ─── Delete user ──────────────────────────────────────────────────────────────
const confirmDelete = (user: AdminUser) => { deleteTarget.value = user }

const deleteUser = async () => {
  if (!deleteTarget.value) return
  const userId = deleteTarget.value.id
  await executeWithLoading(async () => {
    try {
      const res = await fetch(`${API_URL}/admin/users/${userId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${authStore.token}` }
      })
      if (!res.ok) {
        const data = await res.json()
        throw new Error(data.message ?? 'Delete failed')
      }
      users.value = users.value.filter(u => u.id !== userId)
      deleteTarget.value = null
    } catch (err) {
      logError('Failed to delete user', err)
      error.value = (err as Error).message
      deleteTarget.value = null
    }
  }, userId)
}

// ─── Helpers ──────────────────────────────────────────────────────────────────
const resetFilters = () => {
  search.value = ''; roleFilter.value = ''; statusFilter.value = ''; showInactive.value = true
}

const initials = (u: AdminUser) =>
  `${u.firstName[0] ?? ''}${u.lastName[0] ?? ''}`.toUpperCase()

const AVATAR_COLORS = ['#7c3aed','#2563eb','#059669','#d97706','#dc2626','#0891b2','#db2777']
const avatarColor = (u: AdminUser) => AVATAR_COLORS[u.id % AVATAR_COLORS.length]

const roleBadgeClass = (roleName: string) => {
  if (roleName === 'SuperAdmin') return 'role-badge--superadmin'
  if (roleName === 'Admin')      return 'role-badge--admin'
  return 'role-badge--user'
}

const roleLabel = (role: UserRole) => {
  if (role === UserRole.SuperAdmin) return 'SuperAdmin'
  if (role === UserRole.Admin)      return 'Admin'
  return 'User'
}

const timeAgo = (dateStr: string): string => {
  const diff = Date.now() - new Date(dateStr).getTime()
  const m = Math.floor(diff / 60_000)
  if (m < 1)  return 'just now'
  if (m < 60) return `${m}m ago`
  const h = Math.floor(m / 60)
  if (h < 24) return `${h}h ago`
  const d = Math.floor(h / 24)
  return `${d}d ago`
}

const formatIp = (ip: string | undefined): string => {
  if (!ip) return '—'
  // Unwrap IPv4-mapped IPv6 (::ffff:1.2.3.4 → 1.2.3.4)
  const v4mapped = ip.match(/^::ffff:(.+)$/i)
  if (v4mapped) return `${v4mapped[1]} (IPv4)`
  return ip
}

const formatDate = (dateStr: string) =>
  new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(dateStr))

// Consider a user online if LastSeenAt is within the last 5 minutes
const isOnline = (user: AdminUser): boolean => {
  if (!user.lastSeenAt) return false
  return Date.now() - new Date(user.lastSeenAt).getTime() < 5 * 60_000
}

let refreshInterval: ReturnType<typeof setInterval>
onMounted(() => {
  loadUsers()
  refreshInterval = setInterval(loadUsers, 30_000)
})
onUnmounted(() => clearInterval(refreshInterval))
</script>

<style scoped>
.users-page { display: flex; flex-direction: column; gap: 1.25rem; }

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}

.page-title    { font-size: 1.75rem; font-weight: 800; color: #111827; }
.page-subtitle { color: #64748b; font-size: 0.9rem; margin-top: 0.15rem; }

/* Filters */
.filters-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  align-items: center;
}

.search-input  { flex: 1; min-width: 200px; }
.filter-select { width: 140px; }

/* Table row — inactive */
.row--inactive td { opacity: 0.6; }

/* Avatar */
.user-cell { display: flex; align-items: center; gap: 0.75rem; }

.avatar {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 50%;
  color: white;
  font-size: 0.75rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.user-name { font-weight: 600; font-size: 0.9rem; color: #1e293b; display: flex; align-items: center; gap: 0.35rem; }
.user-meta { font-size: 0.75rem; color: #94a3b8; display: flex; gap: 0.5rem; flex-wrap: wrap; margin-top: 0.1rem; }

.newsletter-dot { color: #059669; font-size: 0.65rem; }

/* Role badge */
.role-badge {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
  letter-spacing: 0.03em;
  text-transform: uppercase;
}

.role-badge--user       { background: #eff6ff; color: #2563eb; }
.role-badge--admin      { background: #fef3c7; color: #d97706; }
.role-badge--superadmin { background: #fee2e2; color: #dc2626; }

/* Status badge */
.status-badge {
  display: inline-block;
  padding: 0.15rem 0.55rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
}

.status-badge--active   { background: #d1fae5; color: #059669; }
.status-badge--inactive { background: #f1f5f9; color: #94a3b8; }

/* Online/offline badge */
.online-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  padding: 0.15rem 0.55rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
}
.online-badge--on  { background: #d1fae5; color: #059669; }
.online-badge--off { background: #f1f5f9; color: #94a3b8; }

.online-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  flex-shrink: 0;
}
.online-badge--on  .online-dot { background: #059669; animation: pulse-green 2s infinite; }
.online-badge--off .online-dot { background: #94a3b8; }

@keyframes pulse-green {
  0%, 100% { opacity: 1; }
  50%       { opacity: 0.4; }
}

/* IP badge */
.ip-badge {
  font-family: 'Courier New', monospace;
  font-size: 0.78rem;
  color: #475569;
  background: #f1f5f9;
  padding: 0.15rem 0.45rem;
  border-radius: 4px;
  white-space: nowrap;
}

/* Actions */
.action-buttons { display: flex; gap: 0.4rem; justify-content: flex-end; align-items: center; flex-wrap: wrap; }

.btn-success { background: #059669; }
.btn-success:hover { background: #047857; }

/* Skeletons */
.skeleton-row {
  height: 52px;
  margin: 2px 0;
  background: linear-gradient(90deg, #f1f5f9 25%, #e2e8f0 50%, #f1f5f9 75%);
  background-size: 200% 100%;
  animation: shimmer 1.4s infinite;
  border-radius: 4px;
}

@keyframes shimmer {
  0%   { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* Empty state */
.empty-state {
  padding: 3rem;
  text-align: center;
  color: #94a3b8;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

/* Error banner */
.error-banner {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.875rem 1.25rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 0.625rem;
  color: #dc2626;
  font-size: 0.9rem;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal {
  background: white;
  border-radius: 0.875rem;
  box-shadow: 0 20px 60px rgba(0,0,0,0.25);
  width: 100%;
  max-width: 520px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal--narrow { max-width: 400px; }

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #e5e7eb;
  position: sticky;
  top: 0;
  background: white;
  z-index: 1;
}

.modal-close {
  background: none;
  border: none;
  font-size: 1.1rem;
  cursor: pointer;
  color: #94a3b8;
  padding: 0.25rem;
  line-height: 1;
}
.modal-close:hover { color: #374151; }

.modal-body   { padding: 1.25rem 1.5rem; }
.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid #e5e7eb;
  background: #f9fafb;
  border-radius: 0 0 0.875rem 0.875rem;
  position: sticky;
  bottom: 0;
}

/* Form */
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }

@media (max-width: 480px) { .form-row { grid-template-columns: 1fr; } }

.form-section-title {
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: #94a3b8;
  margin-bottom: 0.75rem;
}

.form-group--check { margin-top: 0.25rem; }

.check-label {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  cursor: pointer;
  font-size: 0.9rem;
  color: #374151;
}

.check-input {
  width: 1rem;
  height: 1rem;
  accent-color: #7c3aed;
  cursor: pointer;
}

.field-error { font-size: 0.78rem; color: #dc2626; margin-top: 0.2rem; display: block; }

/* Metadata grid */
.meta-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 0.5rem;
  padding: 0.875rem 1rem;
}

.meta-item { display: flex; flex-direction: column; gap: 0.15rem; }
.meta-label { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.06em; color: #94a3b8; font-weight: 600; }
.meta-value { font-size: 0.875rem; color: #374151; font-weight: 500; }
.ip-mono    { font-family: 'Courier New', monospace; background: #f1f5f9; padding: 0.1rem 0.35rem; border-radius: 3px; }

/* Email verification badges */
.verified-badge, .unverified-badge {
  display: inline-block;
  font-size: 0.7rem;
  font-weight: 700;
  width: 1.15rem;
  height: 1.15rem;
  line-height: 1.15rem;
  text-align: center;
  border-radius: 50%;
  margin-left: 0.4rem;
  vertical-align: middle;
}

.verified-badge {
  background: #22c55e;
  color: white;
}

.unverified-badge {
  background: #f59e0b;
  color: white;
  font-size: 0.65rem;
}
</style>
