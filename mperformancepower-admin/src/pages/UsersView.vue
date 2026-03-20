<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import AdminShell from '@/components/layout/AdminShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Paginator from 'primevue/paginator'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import type { AdminUser, AdminUpdateUserDto } from '@/types/user.types'
import * as usersService from '@/services/users.service'

const toast = useToast()

const users = ref<AdminUser[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = 20
const loading = ref(false)
const searchQuery = ref('')
const roleFilter = ref<string | null>(null)

const editOpen = ref(false)
const editUser = ref<AdminUser | null>(null)
const editForm = reactive<AdminUpdateUserDto>({ firstName: '', lastName: '', email: '', phone: '', role: '' })
const editSaving = ref(false)

const pwResetOpen = ref(false)

const deleteTarget = ref<AdminUser | null>(null)
const deleteOpen = ref(false)

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize))
let searchTimer: number

const roleOptions = [
  { label: 'Admin', value: 'Admin' },
  { label: 'Customer', value: 'Customer' },
]

const roleFilterOptions = [
  { label: 'Admin', value: 'Admin' },
  { label: 'Customer', value: 'Customer' },
]

async function load() {
  loading.value = true
  try {
    const result = await usersService.getUsers(page.value, pageSize, searchQuery.value || undefined, roleFilter.value ?? undefined)
    users.value = result.items
    totalCount.value = result.totalCount
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to load users', life: 3000 })
  } finally {
    loading.value = false
  }
}

function onSearch() {
  clearTimeout(searchTimer)
  searchTimer = window.setTimeout(() => { page.value = 1; load() }, 300)
}

function onRoleFilter() { page.value = 1; load() }

function onPage(e: { page: number }) {
  page.value = e.page + 1
  load()
}

onMounted(load)

function openEdit(user: AdminUser) {
  editUser.value = user
  Object.assign(editForm, {
    firstName: user.firstName ?? '',
    lastName: user.lastName ?? '',
    email: user.email,
    phone: user.phone ?? '',
    role: user.role,
  })
  editOpen.value = true
}

async function saveEdit() {
  if (!editUser.value) return
  editSaving.value = true
  try {
    const updated = await usersService.updateUser(editUser.value.id, editForm)
    const idx = users.value.findIndex(u => u.id === updated.id)
    if (idx !== -1) users.value[idx] = updated
    toast.add({ severity: 'success', summary: 'User updated', life: 2500 })
    editOpen.value = false
  } catch (e: any) {
    toast.add({ severity: 'error', summary: 'Update failed', detail: e?.response?.data?.message ?? 'Please try again.', life: 3500 })
  } finally {
    editSaving.value = false
  }
}

async function forceVerify(user: AdminUser) {
  try {
    await usersService.forceVerifyUser(user.id)
    user.emailVerified = true
    toast.add({ severity: 'success', summary: 'Email verified', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed', life: 3000 })
  }
}

async function toggleActive(user: AdminUser) {
  try {
    const result = await usersService.toggleUserActive(user.id)
    user.isActive = result.isActive
    toast.add({ severity: 'success', summary: user.isActive ? 'User activated' : 'User deactivated', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed', life: 3000 })
  }
}

async function resetPassword(user: AdminUser) {
  try {
    await usersService.resetUserPassword(user.id)
    pwResetOpen.value = true
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to reset password', life: 3000 })
  }
}

function openDelete(user: AdminUser) {
  deleteTarget.value = user
  deleteOpen.value = true
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  try {
    await usersService.deleteUser(deleteTarget.value.id)
    users.value = users.value.filter(u => u.id !== deleteTarget.value!.id)
    totalCount.value--
    toast.add({ severity: 'success', summary: 'User deleted', life: 2500 })
    deleteOpen.value = false
  } catch (e: any) {
    toast.add({ severity: 'error', summary: 'Delete failed', detail: e?.response?.data?.message ?? '', life: 3500 })
  }
}

function fullName(u: AdminUser): string {
  return [u.firstName, u.lastName].filter(Boolean).join(' ') || '—'
}
</script>

<template>
  <AdminShell>
    <div class="users-page">

      <div class="page-header">
        <div>
          <h1>Users <span class="count-badge">{{ totalCount }}</span></h1>
          <p>Manage admin and customer accounts.</p>
        </div>
      </div>

      <!-- Filters -->
      <div class="filter-bar">
        <IconField style="flex:1; max-width:340px;">
          <InputIcon class="pi pi-search" />
          <InputText v-model="searchQuery" placeholder="Search by name or email…" style="width:100%" @input="onSearch" />
        </IconField>
        <Select
          v-model="roleFilter"
          :options="roleFilterOptions"
          option-label="label"
          option-value="value"
          placeholder="All Roles"
          show-clear
          style="min-width:140px"
          @change="onRoleFilter"
        />
      </div>

      <!-- Table -->
      <DataTable :value="users" :loading="loading">
        <template #empty>No users found.</template>
        <Column header="Name">
          <template #body="{ data }">
            <span :style="{ opacity: data.isActive ? 1 : 0.4 }">{{ fullName(data) }}</span>
          </template>
        </Column>
        <Column header="Email">
          <template #body="{ data }">
            <span class="email-cell" :style="{ opacity: data.isActive ? 1 : 0.4 }">{{ data.email }}</span>
          </template>
        </Column>
        <Column field="phone" header="Phone">
          <template #body="{ data }">
            <span :style="{ opacity: data.isActive ? 1 : 0.4 }">{{ data.phone || '—' }}</span>
          </template>
        </Column>
        <Column header="Role">
          <template #body="{ data }">
            <Tag :value="data.role" :severity="data.role === 'Admin' ? 'danger' : 'success'" />
          </template>
        </Column>
        <Column header="Verified">
          <template #body="{ data }">
            <Tag :value="data.emailVerified ? 'Verified' : 'Unverified'" :severity="data.emailVerified ? 'success' : 'warn'" />
          </template>
        </Column>
        <Column header="Status">
          <template #body="{ data }">
            <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'info' : 'secondary'" />
          </template>
        </Column>
        <Column header="Joined">
          <template #body="{ data }">
            <span class="date-cell">{{ new Date(data.createdAt).toLocaleDateString() }}</span>
          </template>
        </Column>
        <Column header="" style="width:160px;">
          <template #body="{ data }">
            <div class="action-btns">
              <Button icon="pi pi-pencil" severity="secondary" text rounded v-tooltip.top="'Edit'" @click="openEdit(data)" />
              <Button v-if="!data.emailVerified" icon="pi pi-check-circle" severity="success" text rounded v-tooltip.top="'Force verify'" @click="forceVerify(data)" />
              <Button :icon="data.isActive ? 'pi pi-ban' : 'pi pi-check'" severity="secondary" text rounded v-tooltip.top="data.isActive ? 'Deactivate' : 'Activate'" @click="toggleActive(data)" />
              <Button icon="pi pi-key" severity="warn" text rounded v-tooltip.top="'Reset password'" @click="resetPassword(data)" />
              <Button icon="pi pi-trash" severity="danger" text rounded v-tooltip.top="'Delete'" @click="openDelete(data)" />
            </div>
          </template>
        </Column>
      </DataTable>

      <!-- Pagination -->
      <Paginator
        v-if="totalPages > 1"
        :rows="pageSize"
        :total-records="totalCount"
        :first="(page - 1) * pageSize"
        @page="onPage"
      />

    </div>

    <!-- Edit Dialog -->
    <Dialog v-model:visible="editOpen" header="Edit User" :modal="true" :style="{ width: '480px' }">
      <div class="dialog-form">
        <div class="form-row">
          <div class="field">
            <label>First Name</label>
            <InputText v-model="editForm.firstName" style="width:100%" />
          </div>
          <div class="field">
            <label>Last Name</label>
            <InputText v-model="editForm.lastName" style="width:100%" />
          </div>
        </div>
        <div class="field">
          <label>Email</label>
          <InputText v-model="editForm.email" type="email" style="width:100%" />
        </div>
        <div class="field">
          <label>Phone</label>
          <InputText v-model="editForm.phone" type="tel" style="width:100%" />
        </div>
        <div class="field">
          <label>Role</label>
          <Select v-model="editForm.role" :options="roleOptions" option-label="label" option-value="value" style="width:100%" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="editOpen = false" />
        <Button label="Save Changes" :loading="editSaving" @click="saveEdit" />
      </template>
    </Dialog>

    <!-- Password Reset Dialog -->
    <Dialog v-model:visible="pwResetOpen" header="Password Reset" :modal="true" :style="{ width: '380px' }">
      <div class="dialog-form">
        <p style="font-size:0.875rem; color:#9a9a9a; line-height:1.6;">
          A temporary password has been sent to the user's email address. Ask them to log in and change it immediately.
        </p>
      </div>
      <template #footer>
        <Button label="Done" @click="pwResetOpen = false" />
      </template>
    </Dialog>

    <!-- Delete Confirm Dialog -->
    <Dialog v-model:visible="deleteOpen" header="Delete User" :modal="true" :style="{ width: '380px' }">
      <div class="dialog-form">
        <p style="font-size:0.875rem; color:#9a9a9a;">
          Are you sure you want to delete <strong style="color:#f0f0f0;">{{ deleteTarget?.email }}</strong>? This cannot be undone.
        </p>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="deleteOpen = false" />
        <Button label="Delete" severity="danger" @click="confirmDelete" />
      </template>
    </Dialog>

  </AdminShell>
</template>

<style scoped>
.users-page { display: flex; flex-direction: column; gap: 20px; }

.page-header { display: flex; align-items: center; justify-content: space-between; }
.page-header h1 { font-size: 1.5rem; font-weight: 700; display: flex; align-items: center; gap: 10px; }
.page-header p { font-size: 0.875rem; color: #9a9a9a; margin-top: 4px; }

.count-badge {
  font-size: 0.8rem; color: #555; background: #1a1a1a;
  padding: 3px 10px; border-radius: 999px; font-weight: 400;
}

.filter-bar {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
  background: #111;
  border: 1px solid #222;
  border-radius: 10px;
  padding: 14px 18px;
}

.email-cell { font-size: 0.8rem; color: #9a9a9a; }
.date-cell { font-size: 0.75rem; color: #555; white-space: nowrap; }
.action-btns { display: flex; gap: 2px; }

.dialog-form { display: flex; flex-direction: column; gap: 14px; padding-top: 8px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }
.field { display: flex; flex-direction: column; gap: 6px; }
.field label { font-size: 0.75rem; font-weight: 600; color: #777; text-transform: uppercase; letter-spacing: 0.04em; }
</style>
