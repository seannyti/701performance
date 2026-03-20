<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useOrderStore } from '@/stores/order.store'
import AdminShell from '@/components/layout/AdminShell.vue'
import type { CreateOrderDto } from '@/types/order.types'
import * as vehicleService from '@/services/vehicle.service'
import type { VehicleListItem } from '@/types/vehicle.types'

const router = useRouter()
const route = useRoute()
const orderStore = useOrderStore()

const isEdit = computed(() => !!route.params.id)
const orderId = computed(() => isEdit.value ? Number(route.params.id) : null)

const API_URL = import.meta.env.VITE_API_URL as string

// Pre-fill from Convert to Order flow
const prefillInquiryId = route.query.inquiryId ? Number(route.query.inquiryId) : null
const prefillVehicleId = route.query.vehicleId ? Number(route.query.vehicleId) : null
const prefillCustomerName = (route.query.customerName as string) || ''
const prefillCustomerEmail = (route.query.customerEmail as string) || ''
const prefillCustomerPhone = (route.query.customerPhone as string) || ''

// Split prefill name into first/last
const prefillParts = prefillCustomerName.trim().split(' ')
const prefillFirst = prefillParts[0] ?? ''
const prefillLast = prefillParts.slice(1).join(' ')

const saving = ref(false)
const error = ref('')

// ── Vehicle search ────────────────────────────────────────────────
const vehicleSearch = ref('')
const vehicleResults = ref<VehicleListItem[]>([])
const selectedVehicleName = ref('')
let vehicleSearchTimeout: number | null = null

async function searchVehicles() {
  if (!vehicleSearch.value.trim()) { vehicleResults.value = []; return }
  if (vehicleSearchTimeout) clearTimeout(vehicleSearchTimeout)
  vehicleSearchTimeout = setTimeout(async () => {
    const result = await vehicleService.getVehicles(1, 10, undefined, vehicleSearch.value)
    vehicleResults.value = result.items
  }, 300)
}

function selectVehicle(v: VehicleListItem) {
  form.value.vehicleId = v.id
  selectedVehicleName.value = `${v.year} ${v.make} ${v.model}`
  vehicleSearch.value = ''
  vehicleResults.value = []
}

function clearVehicle() {
  form.value.vehicleId = 0
  selectedVehicleName.value = ''
}

// ── Customer search ───────────────────────────────────────────────
interface UserResult { id: number; firstName: string | null; lastName: string | null; email: string; phone: string | null }
const userQuery = ref('')
const userResults = ref<UserResult[]>([])
const userSearchOpen = ref(false)
const selectedUser = ref<UserResult | null>(null)
let userSearchTimer: number

async function searchUsers(q: string) {
  if (!q.trim()) { userResults.value = []; return }
  const token = localStorage.getItem('mpp_token')
  const res = await fetch(`${API_URL}/users/search?q=${encodeURIComponent(q)}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  userResults.value = await res.json()
  userSearchOpen.value = true
}

function onUserInput() {
  clearTimeout(userSearchTimer)
  selectedUser.value = null
  form.value.userId = null
  userSearchTimer = window.setTimeout(() => searchUsers(userQuery.value), 250)
}

function userFullName(u: UserResult): string {
  return [u.firstName, u.lastName].filter(Boolean).join(' ') || u.email
}

function pickUser(u: UserResult) {
  selectedUser.value = u
  form.value.userId = u.id
  customerFirstName.value = u.firstName ?? ''
  customerLastName.value = u.lastName ?? ''
  form.value.customerEmail = u.email
  form.value.customerPhone = u.phone ?? ''
  userQuery.value = userFullName(u)
  userResults.value = []
  userSearchOpen.value = false
}

function clearUser() {
  selectedUser.value = null
  form.value.userId = null
  customerFirstName.value = ''
  customerLastName.value = ''
  userQuery.value = ''
  userResults.value = []
}

// ── Customer name split fields ────────────────────────────────────
const customerFirstName = ref(prefillFirst)
const customerLastName = ref(prefillLast)

// ── Form ─────────────────────────────────────────────────────────
const form = ref<CreateOrderDto & { userId?: number | null }>({
  vehicleId: prefillVehicleId ?? 0,
  inquiryId: prefillInquiryId ?? null,
  customerName: prefillCustomerName,
  customerEmail: prefillCustomerEmail,
  customerPhone: prefillCustomerPhone,
  salePrice: 0,
  paymentMethod: 'Cash',
  downPayment: null,
  loanAmount: null,
  loanTermMonths: null,
  apr: null,
  lenderName: null,
  status: 'Pending',
  notes: null,
  userId: null,
})

const isFinanced = computed(() => form.value.paymentMethod === 'Financed')

onMounted(async () => {
  if (isEdit.value && orderId.value) {
    await orderStore.fetchOrder(orderId.value)
    const o = orderStore.selectedOrder!
    const parts = o.customerName.trim().split(' ')
    customerFirstName.value = parts[0] ?? ''
    customerLastName.value = parts.slice(1).join(' ')
    form.value = {
      vehicleId: o.vehicleId,
      inquiryId: o.inquiryId ?? null,
      customerName: o.customerName,
      customerEmail: o.customerEmail,
      customerPhone: o.customerPhone,
      salePrice: o.salePrice,
      paymentMethod: o.paymentMethod,
      downPayment: o.downPayment ?? null,
      loanAmount: o.loanAmount ?? null,
      loanTermMonths: o.loanTermMonths ?? null,
      apr: o.apr ?? null,
      lenderName: o.lenderName ?? null,
      status: o.status,
      notes: o.notes ?? null,
      userId: null,
    }
    selectedVehicleName.value = o.vehicleName
  } else if (prefillVehicleId) {
    const r = await vehicleService.getVehicle(prefillVehicleId)
    selectedVehicleName.value = `${r.year} ${r.make} ${r.model}`
  }
})

watch(() => form.value.paymentMethod, (m) => {
  if (m !== 'Financed') {
    form.value.downPayment = null
    form.value.loanAmount = null
    form.value.loanTermMonths = null
    form.value.apr = null
    form.value.lenderName = null
  }
})

const { setTimeout } = window

async function submit() {
  error.value = ''
  if (!form.value.vehicleId) { error.value = 'Please select a vehicle.'; return }
  if (!customerFirstName.value.trim()) { error.value = 'First name is required.'; return }
  if (!form.value.customerEmail.trim()) { error.value = 'Customer email is required.'; return }
  if (!form.value.salePrice || form.value.salePrice <= 0) { error.value = 'Sale price must be greater than 0.'; return }

  form.value.customerName = [customerFirstName.value.trim(), customerLastName.value.trim()].filter(Boolean).join(' ')

  saving.value = true
  try {
    if (isEdit.value && orderId.value) {
      const { vehicleId, inquiryId, userId, ...updateDto } = form.value
      await orderStore.updateOrder(orderId.value, updateDto)
    } else {
      await orderStore.createOrder(form.value)
    }
    router.push('/sales/orders')
  } catch (e: any) {
    error.value = e?.response?.data?.message ?? e?.message ?? 'An error occurred. Please try again.'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <AdminShell>
    <div class="order-edit">
      <div class="order-edit__header">
        <button class="back-btn" @click="router.push('/sales/orders')">
          <i class="pi pi-arrow-left" /> Back to Orders
        </button>
        <h2>{{ isEdit ? 'Edit Order' : 'New Order' }}</h2>
      </div>

      <form @submit.prevent="submit" class="order-form">
        <!-- Vehicle selection -->
        <div class="form-section">
          <h3 class="form-section__title">Vehicle</h3>

          <div v-if="selectedVehicleName" class="selected-vehicle">
            <i class="pi pi-car" />
            <span>{{ selectedVehicleName }}</span>
            <button type="button" class="clear-btn" @click="clearVehicle">
              <i class="pi pi-times" />
            </button>
          </div>

          <div v-else class="vehicle-search">
            <div class="form-group">
              <label>Search Vehicle</label>
              <input v-model="vehicleSearch" @input="searchVehicles" type="text"
                placeholder="Search by make, model, year..." class="form-input" />
            </div>
            <div v-if="vehicleResults.length > 0" class="vehicle-dropdown">
              <div v-for="v in vehicleResults" :key="v.id" class="vehicle-option" @click="selectVehicle(v)">
                <span>{{ v.year }} {{ v.make }} {{ v.model }}</span>
                <span class="vehicle-option__price">
                  {{ new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(v.price) }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Customer info -->
        <div class="form-section">
          <h3 class="form-section__title">Customer Information</h3>

          <!-- Registered customer search -->
          <div class="form-group">
            <label>Registered Customer <span class="form-hint">(optional — search to link account)</span></label>
            <div class="customer-search">
              <div class="customer-search__input-wrap">
                <i class="pi pi-search customer-search__icon" />
                <input v-model="userQuery" type="text" class="form-input customer-search__input"
                  placeholder="Search by name or email…"
                  @input="onUserInput"
                  @focus="userQuery && (userSearchOpen = true)"
                  @blur="setTimeout(() => userSearchOpen = false, 150)" />
                <button v-if="selectedUser" type="button" class="customer-search__clear" @click="clearUser" title="Clear">
                  <i class="pi pi-times" />
                </button>
              </div>
              <div v-if="selectedUser" class="customer-linked">
                <i class="pi pi-link" /> Linked to registered account
              </div>
              <div v-if="userSearchOpen && userResults.length > 0" class="customer-dropdown">
                <button v-for="u in userResults" :key="u.id" type="button"
                  class="customer-dropdown__item" @mousedown.prevent="pickUser(u)">
                  <span class="customer-dropdown__name">{{ userFullName(u) }}</span>
                  <span class="customer-dropdown__email">{{ u.email }}</span>
                </button>
              </div>
              <div v-else-if="userSearchOpen && userQuery.length > 0 && userResults.length === 0" class="customer-dropdown">
                <div class="customer-dropdown__empty">No registered customers found</div>
              </div>
            </div>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label>First Name <span class="required">*</span></label>
              <input v-model="customerFirstName" type="text" class="form-input" placeholder="John" />
            </div>
            <div class="form-group">
              <label>Last Name</label>
              <input v-model="customerLastName" type="text" class="form-input" placeholder="Doe" />
            </div>
            <div class="form-group">
              <label>Email <span class="required">*</span></label>
              <input v-model="form.customerEmail" type="email" class="form-input" placeholder="john@example.com" />
            </div>
            <div class="form-group">
              <label>Phone</label>
              <input v-model="form.customerPhone" type="text" class="form-input" placeholder="(555) 123-4567" />
            </div>
          </div>
        </div>

        <!-- Sale details -->
        <div class="form-section">
          <h3 class="form-section__title">Sale Details</h3>
          <div class="form-grid">
            <div class="form-group">
              <label>Sale Price <span class="required">*</span></label>
              <input v-model.number="form.salePrice" type="number" min="0" step="0.01" class="form-input" placeholder="0.00" />
            </div>
            <div class="form-group">
              <label>Payment Method</label>
              <select v-model="form.paymentMethod" class="form-select">
                <option value="Cash">Cash</option>
                <option value="Financed">Financed</option>
                <option value="TradeIn">Trade-In</option>
              </select>
            </div>
            <div class="form-group">
              <label>Status</label>
              <select v-model="form.status" class="form-select">
                <option value="Pending">Pending</option>
                <option value="Completed">Completed</option>
                <option value="Delivered">Delivered</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
          </div>
        </div>

        <!-- Finance details -->
        <div class="form-section" v-if="isFinanced">
          <h3 class="form-section__title">Finance Details</h3>
          <div class="form-grid">
            <div class="form-group">
              <label>Down Payment</label>
              <input v-model.number="form.downPayment" type="number" min="0" step="0.01" class="form-input" placeholder="0.00" />
            </div>
            <div class="form-group">
              <label>Loan Amount</label>
              <input v-model.number="form.loanAmount" type="number" min="0" step="0.01" class="form-input" placeholder="0.00" />
            </div>
            <div class="form-group">
              <label>Loan Term (months)</label>
              <input v-model.number="form.loanTermMonths" type="number" min="1" class="form-input" placeholder="e.g. 60" />
            </div>
            <div class="form-group">
              <label>APR (%)</label>
              <input v-model.number="form.apr" type="number" min="0" step="0.001" class="form-input" placeholder="e.g. 6.99" />
            </div>
            <div class="form-group">
              <label>Lender Name</label>
              <input v-model="form.lenderName" type="text" class="form-input" placeholder="e.g. TD Auto Finance" />
            </div>
          </div>
        </div>

        <!-- Notes -->
        <div class="form-section">
          <h3 class="form-section__title">Notes</h3>
          <div class="form-group">
            <textarea v-model="form.notes" class="form-input form-textarea" placeholder="Internal notes..." rows="4" />
          </div>
        </div>

        <!-- Linked inquiry -->
        <div class="form-section" v-if="form.inquiryId">
          <div class="linked-inquiry">
            <i class="pi pi-link" />
            <span>Linked to Inquiry #{{ form.inquiryId }}</span>
          </div>
        </div>

        <div class="form-error" v-if="error">{{ error }}</div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="router.push('/sales/orders')">Cancel</button>
          <button type="submit" class="btn-primary" :disabled="saving">
            <i class="pi pi-spinner pi-spin" v-if="saving" />
            {{ isEdit ? 'Save Changes' : 'Create Order' }}
          </button>
        </div>
      </form>
    </div>
  </AdminShell>
</template>

<style scoped>
.order-edit { display: flex; flex-direction: column; gap: 24px; max-width: 800px; }
.order-edit__header { display: flex; flex-direction: column; gap: 8px; }

.back-btn {
  display: inline-flex; align-items: center; gap: 6px;
  background: transparent; border: none; color: #777;
  font-size: 0.825rem; cursor: pointer; padding: 0;
}
.back-btn:hover { color: #f0f0f0; }
.order-edit__header h2 { font-size: 1.5rem; font-weight: 700; }

.order-form { display: flex; flex-direction: column; gap: 20px; }

.form-section {
  background: #111; border: 1px solid #222;
  border-radius: 10px; padding: 20px;
  display: flex; flex-direction: column; gap: 16px;
}
.form-section__title {
  font-size: 0.875rem; font-weight: 700; color: #9a9a9a;
  text-transform: uppercase; letter-spacing: 0.04em; margin: 0;
}

.form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 14px; }

.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group label { font-size: 0.8rem; color: #9a9a9a; font-weight: 500; }
.required { color: #e63946; }
.form-hint { color: #555; font-size: 0.7rem; font-weight: 400; }

.form-input {
  background: #0d0d0d; border: 1px solid #2a2a2a; border-radius: 6px;
  padding: 9px 12px; color: #f0f0f0; font-size: 0.875rem;
  transition: border-color 0.2s; width: 100%; box-sizing: border-box;
}
.form-input:focus { outline: none; border-color: #e63946; }
.form-select {
  background: #0d0d0d; border: 1px solid #2a2a2a; border-radius: 6px;
  padding: 9px 12px; color: #f0f0f0; font-size: 0.875rem; cursor: pointer; width: 100%;
}
.form-select:focus { outline: none; border-color: #e63946; }
.form-textarea { resize: vertical; }

/* Vehicle selection */
.selected-vehicle {
  display: flex; align-items: center; gap: 10px;
  padding: 12px 16px;
  background: color-mix(in srgb, #e63946 10%, transparent);
  border: 1px solid color-mix(in srgb, #e63946 30%, transparent);
  border-radius: 8px; font-size: 0.9rem; color: #f0f0f0;
}
.selected-vehicle i { color: #e63946; }
.clear-btn { margin-left: auto; background: transparent; border: none; color: #777; cursor: pointer; font-size: 0.85rem; }
.clear-btn:hover { color: #f0f0f0; }

.vehicle-search { display: flex; flex-direction: column; gap: 8px; position: relative; }
.vehicle-dropdown {
  background: #1a1a1a; border: 1px solid #2a2a2a;
  border-radius: 8px; overflow: hidden; max-height: 240px; overflow-y: auto;
}
.vehicle-option {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 14px; cursor: pointer; font-size: 0.875rem; border-bottom: 1px solid #222;
}
.vehicle-option:last-child { border-bottom: none; }
.vehicle-option:hover { background: #222; }
.vehicle-option__price { color: #777; font-size: 0.8rem; }

/* Customer search */
.customer-search { display: flex; flex-direction: column; gap: 6px; position: relative; }
.customer-search__input-wrap { position: relative; display: flex; align-items: center; }
.customer-search__icon { position: absolute; left: 10px; color: #555; font-size: 0.8rem; pointer-events: none; }
.customer-search__input { padding-left: 30px !important; }
.customer-search__clear {
  position: absolute; right: 8px;
  background: none; border: none; color: #555; cursor: pointer; font-size: 0.8rem;
}
.customer-search__clear:hover { color: #f0f0f0; }
.customer-linked {
  display: flex; align-items: center; gap: 6px;
  font-size: 0.75rem; color: #4ade80;
  padding: 4px 8px; background: rgba(74,222,128,0.08);
  border: 1px solid rgba(74,222,128,0.2); border-radius: 6px;
  width: fit-content;
}
.customer-linked i { font-size: 0.75rem; }
.customer-dropdown {
  background: #1a1a1a; border: 1px solid #2a2a2a;
  border-radius: 8px; overflow: hidden; max-height: 200px; overflow-y: auto;
  position: absolute; top: calc(100% + 4px); left: 0; right: 0; z-index: 50;
}
.customer-dropdown__item {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 14px; cursor: pointer; background: none; border: none;
  width: 100%; text-align: left; border-bottom: 1px solid #222;
}
.customer-dropdown__item:last-child { border-bottom: none; }
.customer-dropdown__item:hover { background: #222; }
.customer-dropdown__name { color: #f0f0f0; font-size: 0.875rem; }
.customer-dropdown__email { color: #555; font-size: 0.75rem; }
.customer-dropdown__empty { padding: 12px 14px; color: #555; font-size: 0.8rem; text-align: center; }

/* Linked inquiry */
.linked-inquiry { display: flex; align-items: center; gap: 8px; font-size: 0.8rem; color: #777; }
.linked-inquiry i { color: #3b82f6; }

/* Error & Actions */
.form-error {
  background: color-mix(in srgb, #ef4444 15%, transparent);
  border: 1px solid color-mix(in srgb, #ef4444 40%, transparent);
  border-radius: 8px; padding: 12px 16px; color: #ef4444; font-size: 0.875rem;
}
.form-actions { display: flex; justify-content: flex-end; gap: 12px; }

.btn-primary {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 24px; background: #e63946; color: #fff; border: none;
  border-radius: 6px; font-size: 0.875rem; font-weight: 600; cursor: pointer; transition: opacity 0.2s;
}
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-primary:not(:disabled):hover { opacity: 0.85; }

.btn-secondary {
  padding: 10px 20px; background: transparent; border: 1px solid #2a2a2a;
  border-radius: 6px; color: #9a9a9a; font-size: 0.875rem; cursor: pointer;
}
.btn-secondary:hover { border-color: #555; color: #f0f0f0; }
</style>
