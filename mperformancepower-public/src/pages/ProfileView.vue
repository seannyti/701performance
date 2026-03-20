<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import * as profileService from '@/services/profile.service'
import type { UserProfile, ProfileAppointment, ProfileOrder } from '@/services/profile.service'

const router = useRouter()
const auth = useAuthStore()

if (!auth.isAuthenticated) router.push('/login')


const apiBase = import.meta.env.VITE_API_URL.replace(/\/api$/, '')

const profile = ref<UserProfile | null>(null)
const appointments = ref<ProfileAppointment[]>([])
const orders = ref<ProfileOrder[]>([])

const infoForm = reactive({ firstName: '', lastName: '', phone: '' })
const infoSaving = ref(false)
const infoSuccess = ref('')
const infoError = ref('')

const pwForm = reactive({ currentPassword: '', newPassword: '', confirmPassword: '' })
const pwSaving = ref(false)
const pwSuccess = ref('')
const pwError = ref('')

const avatarUploading = ref(false)
const avatarError = ref('')
const avatarInput = ref<HTMLInputElement | null>(null)

onMounted(async () => {
  const [p, appts, ords] = await Promise.all([
    profileService.getProfile(),
    profileService.getMyAppointments(),
    profileService.getMyOrders(),
  ])
  profile.value = p
  appointments.value = appts
  orders.value = ords
  infoForm.firstName = p.firstName ?? ''
  infoForm.lastName = p.lastName ?? ''
  infoForm.phone = p.phone ?? ''
})

const avatarUrl = computed(() => {
  if (!profile.value?.avatarPath) return null
  return `${apiBase}/uploads/${profile.value.avatarPath}`
})

const initials = computed(() => {
  const f = profile.value?.firstName?.[0] ?? ''
  const l = profile.value?.lastName?.[0] ?? ''
  return (f + l).toUpperCase() || profile.value?.email?.[0]?.toUpperCase() || '?'
})

async function saveInfo() {
  infoSaving.value = true
  infoSuccess.value = ''
  infoError.value = ''
  try {
    await profileService.updateProfile(infoForm)
    profile.value!.firstName = infoForm.firstName || null
    profile.value!.lastName = infoForm.lastName || null
    profile.value!.phone = infoForm.phone || null
    infoSuccess.value = 'Profile updated.'
    setTimeout(() => (infoSuccess.value = ''), 3000)
  } catch (e: any) {
    infoError.value = e?.message ?? 'Failed to update.'
  } finally {
    infoSaving.value = false
  }
}

async function changePassword() {
  pwError.value = ''
  pwSuccess.value = ''
  if (pwForm.newPassword !== pwForm.confirmPassword) {
    pwError.value = 'New passwords do not match.'
    return
  }
  if (pwForm.newPassword.length < 8) {
    pwError.value = 'Password must be at least 8 characters.'
    return
  }
  pwSaving.value = true
  try {
    await profileService.changePassword({
      currentPassword: pwForm.currentPassword,
      newPassword: pwForm.newPassword,
    })
    pwSuccess.value = 'Password changed successfully.'
    pwForm.currentPassword = ''
    pwForm.newPassword = ''
    pwForm.confirmPassword = ''
    setTimeout(() => (pwSuccess.value = ''), 4000)
  } catch (e: any) {
    pwError.value = e?.message ?? 'Failed to change password.'
  } finally {
    pwSaving.value = false
  }
}

async function deleteAvatar() {
  avatarError.value = ''
  avatarUploading.value = true
  try {
    await profileService.deleteAvatar()
    profile.value!.avatarPath = null
  } catch {
    avatarError.value = 'Failed to remove photo.'
  } finally {
    avatarUploading.value = false
  }
}

async function onAvatarChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  avatarUploading.value = true
  avatarError.value = ''
  try {
    const result = await profileService.uploadAvatar(file)
    profile.value!.avatarPath = result.avatarPath
  } catch (e: any) {
    avatarError.value = e?.message ?? 'Upload failed.'
  } finally {
    avatarUploading.value = false
  }
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

function formatDateTime(iso: string) {
  return new Date(iso).toLocaleString('en-US', {
    month: 'short', day: 'numeric', year: 'numeric',
    hour: 'numeric', minute: '2-digit',
  })
}

function formatPrice(n: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)
}

const statusColor: Record<string, string> = {
  Pending: '#f4a261',
  Completed: '#2dc653',
  Delivered: '#3b82f6',
  Cancelled: '#ef4444',
  Scheduled: '#2dc653',
}
</script>

<template>
  <div class="profile-page">
    <div class="container">
      <div v-if="!profile" class="profile-loading">Loading your profile...</div>

      <template v-else>
        <!-- Profile hero -->
        <div class="profile-hero">
          <div class="avatar-wrap-outer">
            <div class="avatar-wrap" @click="avatarInput?.click()">
              <img v-if="avatarUrl" :src="avatarUrl" class="avatar-img" alt="Profile picture" />
              <div v-else class="avatar-placeholder">{{ initials }}</div>
              <div class="avatar-overlay">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zm17.71-10.21a1 1 0 0 0 0-1.41l-2.34-2.34a1 1 0 0 0-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"/>
                </svg>
              </div>
              <input ref="avatarInput" type="file" accept="image/jpeg,image/png,image/webp" style="display:none" @change="onAvatarChange" />
            </div>
            <button v-if="avatarUrl" class="avatar-delete-btn" title="Remove photo" @click.stop="deleteAvatar">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="currentColor">
                <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z"/>
              </svg>
            </button>
          </div>
          <div class="profile-hero__info">
            <h1>{{ profile.firstName || profile.email.split('@')[0] }} {{ profile.lastName }}</h1>
            <p class="profile-hero__email">{{ profile.email }}</p>
            <p class="profile-hero__since">Member since {{ formatDate(profile.createdAt) }}</p>
          </div>
          <span v-if="avatarUploading" class="avatar-status">Uploading...</span>
          <span v-if="avatarError" class="avatar-status avatar-status--error">{{ avatarError }}</span>
        </div>

        <div class="profile-grid">

          <!-- ── Personal Info ─────────────────────────────────── -->
          <section class="card">
            <h2 class="card__title">Personal Info</h2>
            <form class="form" @submit.prevent="saveInfo">
              <div class="form-row">
                <div class="field">
                  <label>First Name</label>
                  <input v-model="infoForm.firstName" type="text" placeholder="First name" />
                </div>
                <div class="field">
                  <label>Last Name</label>
                  <input v-model="infoForm.lastName" type="text" placeholder="Last name" />
                </div>
              </div>
              <div class="field">
                <label>Phone</label>
                <input v-model="infoForm.phone" type="tel" placeholder="(555) 000-0000" />
              </div>
              <div class="field">
                <label>Email</label>
                <input :value="profile.email" type="email" disabled class="input--disabled" />
              </div>
              <div v-if="infoSuccess" class="alert alert--success">{{ infoSuccess }}</div>
              <div v-if="infoError" class="alert alert--error">{{ infoError }}</div>
              <button type="submit" class="btn-primary" :disabled="infoSaving">
                {{ infoSaving ? 'Saving...' : 'Save Changes' }}
              </button>
            </form>
          </section>

          <!-- ── Change Password ───────────────────────────────── -->
          <section class="card">
            <h2 class="card__title">Change Password</h2>
            <form class="form" @submit.prevent="changePassword">
              <div class="field">
                <label>Current Password</label>
                <input v-model="pwForm.currentPassword" type="password" placeholder="••••••••" required />
              </div>
              <div class="field">
                <label>New Password</label>
                <input v-model="pwForm.newPassword" type="password" placeholder="Min 8 characters" required />
              </div>
              <div class="field">
                <label>Confirm New Password</label>
                <input v-model="pwForm.confirmPassword" type="password" placeholder="••••••••" required />
              </div>
              <div v-if="pwSuccess" class="alert alert--success">{{ pwSuccess }}</div>
              <div v-if="pwError" class="alert alert--error">{{ pwError }}</div>
              <button type="submit" class="btn-primary" :disabled="pwSaving">
                {{ pwSaving ? 'Updating...' : 'Update Password' }}
              </button>
            </form>
          </section>

        </div>

        <!-- ── Upcoming Appointments ─────────────────────────────── -->
        <section class="card card--full">
          <h2 class="card__title">Upcoming Appointments</h2>
          <div v-if="appointments.length === 0" class="empty-state">No upcoming appointments.</div>
          <div v-else class="appt-list">
            <div v-for="a in appointments" :key="a.id" class="appt-row">
              <div class="appt-row__icon">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M17 12h-5v5h5v-5zM16 1v2H8V1H6v2H5c-1.11 0-2 .89-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.11-.89-2-2-2h-1V1h-2zm3 18H5V8h14v11z"/>
                </svg>
              </div>
              <div class="appt-row__body">
                <div class="appt-row__title">{{ a.title }}</div>
                <div v-if="a.vehicleName" class="appt-row__vehicle">{{ a.vehicleName }}</div>
                <div class="appt-row__time">{{ formatDateTime(a.startTime) }} – {{ formatDateTime(a.endTime) }}</div>
              </div>
              <span class="status-badge" :style="{ background: statusColor[a.status] + '22', color: statusColor[a.status] }">
                {{ a.status }}
              </span>
            </div>
          </div>
        </section>

        <!-- ── My Orders ────────────────────────────────────────── -->
        <section class="card card--full">
          <h2 class="card__title">My Orders</h2>
          <div v-if="orders.length === 0" class="empty-state">No orders yet.</div>
          <div v-else class="order-list">
            <div v-for="o in orders" :key="o.id" class="order-row">
              <div class="order-row__main">
                <div class="order-row__vehicle">{{ o.vehicleName }}</div>
                <div class="order-row__meta">
                  <span>{{ formatDate(o.createdAt) }}</span>
                  <span class="dot">·</span>
                  <span>{{ o.paymentMethod }}</span>
                  <span class="dot">·</span>
                  <span class="order-row__price">{{ formatPrice(o.salePrice) }}</span>
                </div>
                <div v-if="o.trackingNumber" class="tracking-row">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M17.92 11.62C17.73 10.74 16.95 10 16 10h-1V7c0-2.76-2.24-5-5-5S5 4.24 5 7v3H4c-.95 0-1.73.74-1.92 1.62L1 17c-.24 1.1.57 2 1.7 2h14.6c1.13 0 1.94-.9 1.7-2l-1.08-5.38zM10 14c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm-3-7c0-1.66 1.34-3 3-3s3 1.34 3 3H7z"/>
                  </svg>
                  <span>Tracking: <strong>{{ o.trackingNumber }}</strong></span>
                </div>
              </div>
              <span class="status-badge" :style="{ background: statusColor[o.status] + '22', color: statusColor[o.status] }">
                {{ o.status }}
              </span>
            </div>
          </div>
        </section>

      </template>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.profile-page {
  min-height: calc(100vh - #{$header-height});
  background: $color-dark;
  padding: $space-xl $space-md;
}

.profile-loading {
  color: $color-muted;
  text-align: center;
  padding: $space-2xl;
}

// ── Profile hero ──────────────────────────────────────────────
.profile-hero {
  display: flex;
  align-items: center;
  gap: $space-lg;
  margin-bottom: $space-xl;
  flex-wrap: wrap;
}

.avatar-wrap-outer {
  position: relative;
  flex-shrink: 0;
  width: 88px;
  height: 88px;
}

.avatar-wrap {
  position: relative;
  width: 88px;
  height: 88px;
  border-radius: 50%;
  cursor: pointer;

  &:hover .avatar-overlay { opacity: 1; }
}

.avatar-delete-btn {
  position: absolute;
  bottom: 2px;
  right: 2px;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: var(--color-primary);
  border: 2px solid $color-dark;
  color: #fff;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s;
  z-index: 1;

  &:hover { background: var(--color-primary-dark); }
}

.avatar-img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid $color-border;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: $color-surface;
  border: 2px solid $color-border;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.75rem;
  font-weight: 800;
  color: $color-text;
}

.avatar-overlay {
  position: absolute;
  inset: 0;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  opacity: 0;
  transition: opacity 0.2s;
}

.profile-hero__info {
  h1 { font-size: 1.5rem; font-weight: 800; }
}

.profile-hero__email { font-size: 0.875rem; color: $color-muted; margin-top: 2px; }
.profile-hero__since { font-size: 0.8rem; color: #555; margin-top: 4px; }

.avatar-status {
  font-size: 0.8rem;
  color: $color-muted;

  &--error { color: var(--color-primary); }
}

// ── Grid ──────────────────────────────────────────────────────
.profile-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: $space-md;
  margin-bottom: $space-md;

  @media (max-width: #{$bp-md}) { grid-template-columns: 1fr; }
}

// ── Cards ─────────────────────────────────────────────────────
.card {
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: 12px;
  padding: $space-lg;

  &--full { margin-bottom: $space-md; }
}

.card__title {
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: $color-muted;
  margin-bottom: $space-lg;
}

// ── Forms ─────────────────────────────────────────────────────
.form {
  display: flex;
  flex-direction: column;
  gap: $space-md;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: $space-md;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;

  label {
    font-size: 0.75rem;
    font-weight: 600;
    color: $color-muted;
    text-transform: uppercase;
    letter-spacing: 0.04em;
  }

  input {
    background: $color-dark;
    border: 1px solid $color-border;
    border-radius: $border-radius;
    padding: $space-sm $space-md;
    color: $color-text;
    font-size: 0.9rem;
    transition: border-color 0.2s;

    &:focus { outline: none; border-color: var(--color-primary); }
    &::placeholder { color: #555; }
  }

  .input--disabled { opacity: 0.4; cursor: not-allowed; }
}

.alert {
  border-radius: $border-radius;
  padding: $space-sm $space-md;
  font-size: 0.875rem;

  &--success {
    background: rgba($color-success, 0.1);
    border: 1px solid $color-success;
    color: $color-success;
  }

  &--error {
    background: rgba(var(--color-primary-rgb), 0.1);
    border: 1px solid var(--color-primary);
    color: var(--color-primary);
  }
}

.btn-primary {
  align-self: flex-start;
  padding: $space-sm $space-lg;
  background: var(--color-primary);
  color: #fff;
  font-size: 0.9rem;
  font-weight: 600;
  border-radius: $border-radius;
  border: none;
  cursor: pointer;
  transition: background 0.2s;

  &:hover:not(:disabled) { background: var(--color-primary-dark); }
  &:disabled { opacity: 0.5; cursor: not-allowed; }
}

// ── Appointments ──────────────────────────────────────────────
.empty-state {
  color: $color-muted;
  font-size: 0.875rem;
  padding: $space-lg 0;
}

.appt-list { display: flex; flex-direction: column; gap: 12px; }

.appt-row {
  display: flex;
  align-items: flex-start;
  gap: $space-md;
  padding: $space-md;
  background: $color-dark;
  border: 1px solid $color-border;
  border-radius: $border-radius;
}

.appt-row__icon {
  color: var(--color-primary);
  flex-shrink: 0;
  margin-top: 2px;
}

.appt-row__body { flex: 1; }
.appt-row__title { font-size: 0.9rem; font-weight: 600; color: $color-text; }
.appt-row__vehicle { font-size: 0.8rem; color: var(--color-primary); margin-top: 2px; }
.appt-row__time { font-size: 0.775rem; color: $color-muted; margin-top: 4px; }

// ── Orders ────────────────────────────────────────────────────
.order-list { display: flex; flex-direction: column; gap: 12px; }

.order-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: $space-md;
  padding: $space-md;
  background: $color-dark;
  border: 1px solid $color-border;
  border-radius: $border-radius;
}

.order-row__vehicle { font-size: 0.95rem; font-weight: 600; color: $color-text; }
.order-row__meta {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
  color: $color-muted;
  margin-top: 4px;
}
.order-row__price { color: $color-text; font-weight: 600; }
.dot { color: #444; }

.tracking-row {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
  color: $color-muted;
  margin-top: 6px;

  strong { color: $color-text; font-weight: 600; }
  svg { color: $color-warning; flex-shrink: 0; }
}

.status-badge {
  flex-shrink: 0;
  font-size: 0.72rem;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 999px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
</style>
