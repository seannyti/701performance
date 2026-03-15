<template>
  <AdminLayout>
    <div class="orders-page">
      <div class="page-header">
        <div class="header-content">
          <h1>Orders Management</h1>
          <p>Manage manual and offline orders</p>
        </div>
        <div class="stats-row">
          <div class="stat-card">
            <div class="stat-label">Total Orders</div>
            <div class="stat-value">{{ stats.totalOrders }}</div>
          </div>
          <div class="stat-card pending">
            <div class="stat-label">Pending</div>
            <div class="stat-value">{{ stats.pendingOrders }}</div>
          </div>
          <div class="stat-card processing">
            <div class="stat-label">Processing</div>
            <div class="stat-value">{{ stats.processingOrders }}</div>
          </div>
          <div class="stat-card shipped">
            <div class="stat-label">Shipped</div>
            <div class="stat-value">{{ stats.shippedOrders }}</div>
          </div>
          <div class="stat-card revenue">
            <div class="stat-label">Total Revenue</div>
            <div class="stat-value">${{ formatCurrency(stats.totalRevenue) }}</div>
          </div>
        </div>
      </div>

      <div class="actions-bar">
        <button class="btn btn-primary" @click="openCreateOrderModal">Create Order</button>
      </div>

      <div class="filters">
        <div class="filter-group">
          <label>Search:</label>
          <input type="text" v-model="filters.search" @input="debounceLoad" placeholder="Order #, name, email, phone..." />
        </div>
        <div class="filter-group">
          <label>Order Status:</label>
          <select v-model="filters.orderStatus" @change="loadOrders">
            <option value="">All</option>
            <option value="Pending">Pending</option>
            <option value="Processing">Processing</option>
            <option value="Shipped">Shipped</option>
            <option value="Delivered">Delivered</option>
            <option value="Cancelled">Cancelled</option>
            <option value="Refunded">Refunded</option>
          </select>
        </div>
        <div class="filter-group">
          <label>Payment Status:</label>
          <select v-model="filters.paymentStatus" @change="loadOrders">
            <option value="">All</option>
            <option value="Pending">Pending</option>
            <option value="Received">Received</option>
            <option value="PartiallyPaid">Partially Paid</option>
            <option value="Failed">Failed</option>
            <option value="Refunded">Refunded</option>
          </select>
        </div>
        <div class="filter-group">
          <label>From Date:</label>
          <input type="date" v-model="filters.startDate" @change="loadOrders" />
        </div>
        <div class="filter-group">
          <label>To Date:</label>
          <input type="date" v-model="filters.endDate" @change="loadOrders" />
        </div>
        <button class="btn-clear" @click="clearFilters">Clear Filters</button>
      </div>

      <div class="loading-container">
        <div v-if="isLoading" class="loading-overlay">
          <div class="spinner"></div>
          <p>Loading orders...</p>
        </div>
        
        <div v-if="orders.length === 0 && !isLoading" class="empty-state">
          <p>No orders found</p>
        </div>

        <div v-if="!isLoading && orders.length > 0" class="orders-table">
        <table>
          <thead>
            <tr>
              <th>Order #</th>
              <th>Customer</th>
              <th>Items</th>
              <th>Total</th>
              <th>Order Status</th>
              <th>Payment Status</th>
              <th>Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="order in orders" :key="order.id">
              <td><strong>{{ order.orderNumber }}</strong></td>
              <td>
                <div>{{ order.customerName }}</div>
                <div class="text-muted">{{ order.customerEmail }}</div>
              </td>
              <td>{{ order.itemCount }} item(s)</td>
              <td><strong>${{ formatCurrency(order.totalAmount) }}</strong></td>
              <td>
                <span class="status-badge" :class="getOrderStatusClass(order.orderStatus)">
                  {{ formatEnumValue(order.orderStatus) }}
                </span>
              </td>
              <td>
                <span class="status-badge" :class="getPaymentStatusClass(order.paymentStatus)">
                  {{ formatEnumValue(order.paymentStatus) }}
                </span>
              </td>
              <td>{{ formatDate(order.createdAt) }}</td>
              <td>
                <button class="btn-view" @click="viewOrder(order.id)">View</button>
                <button class="btn-delete" @click="confirmDeleteOrder(order.id)" :disabled="isActionLoading(order.id)">
                  <span v-if="isActionLoading(order.id)" class="btn-spinner"></span>
                  {{ isActionLoading(order.id) ? 'Deleting...' : 'Delete' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <div v-if="pagination.totalPages > 1" class="pagination">
          <button @click="changePage(page - 1)" :disabled="page === 1">Previous</button>
          <span>Page {{ page }} of {{ pagination.totalPages }}</span>
          <button @click="changePage(page + 1)" :disabled="page === pagination.totalPages">Next</button>
        </div>
      </div>
      </div>

      <!-- View/Edit Order Modal -->
      <div v-if="selectedOrder" class="modal" @click.self="closeModal">
        <div class="modal-content large">
          <div class="modal-header">
            <h2>Order Details - {{ selectedOrder.orderNumber }}</h2>
            <button class="close-btn" @click="closeModal">×</button>
          </div>
          <div class="modal-body">
            <div class="order-sections">
              <!-- Customer Information -->
              <div class="section">
                <h3>Customer Information</h3>
                <div class="form-grid">
                  <div class="form-group">
                    <label>Name:</label>
                    <input type="text" v-model="editForm.customerName" />
                  </div>
                  <div class="form-group">
                    <label>Email:</label>
                    <input type="email" v-model="editForm.customerEmail" />
                  </div>
                  <div class="form-group">
                    <label>Phone:</label>
                    <input type="tel" v-model="editForm.customerPhone" />
                  </div>
                </div>
              </div>

              <!-- Shipping Address -->
              <div class="section">
                <h3>Shipping Address</h3>
                <div class="form-grid">
                  <div class="form-group full-width">
                    <label>Address:</label>
                    <input type="text" v-model="editForm.shippingAddress" />
                  </div>
                  <div class="form-group">
                    <label>City:</label>
                    <input type="text" v-model="editForm.shippingCity" />
                  </div>
                  <div class="form-group">
                    <label>State:</label>
                    <input type="text" v-model="editForm.shippingState" />
                  </div>
                  <div class="form-group">
                    <label>Zip Code:</label>
                    <input type="text" v-model="editForm.shippingZipCode" />
                  </div>
                </div>
              </div>

              <!-- Order Items -->
              <div class="section">
                <h3>Order Items</h3>
                <table class="items-table">
                  <thead>
                    <tr>
                      <th>Product</th>
                      <th>SKU</th>
                      <th>Quantity</th>
                      <th>Unit Price</th>
                      <th>Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="item in selectedOrder.items" :key="item.id">
                      <td>{{ item.productName }}</td>
                      <td>{{ item.productSku || '-' }}</td>
                      <td>{{ item.quantity }}</td>
                      <td>${{ formatCurrency(item.unitPrice) }}</td>
                      <td>${{ formatCurrency(item.totalPrice) }}</td>
                    </tr>
                  </tbody>
                  <tfoot>
                    <tr>
                      <td colspan="4" align="right"><strong>Subtotal:</strong></td>
                      <td><strong>${{ formatCurrency(selectedOrder.subtotal) }}</strong></td>
                    </tr>
                    <tr>
                      <td colspan="4" align="right">Tax:</td>
                      <td>${{ formatCurrency(selectedOrder.taxAmount) }}</td>
                    </tr>
                    <tr>
                      <td colspan="4" align="right">Shipping:</td>
                      <td>${{ formatCurrency(selectedOrder.shippingCost) }}</td>
                    </tr>
                    <tr class="total-row">
                      <td colspan="4" align="right"><strong>Total:</strong></td>
                      <td><strong>${{ formatCurrency(selectedOrder.totalAmount) }}</strong></td>
                    </tr>
                  </tfoot>
                </table>
              </div>

              <!-- Order Status -->
              <div class="section">
                <h3>Order Status</h3>
                <div class="form-grid">
                  <div class="form-group">
                    <label>Order Status:</label>
                    <select v-model="editForm.orderStatus">
                      <option value="Pending">Pending</option>
                      <option value="Processing">Processing</option>
                      <option value="Shipped">Shipped</option>
                      <option value="Delivered">Delivered</option>
                      <option value="Cancelled">Cancelled</option>
                      <option value="Refunded">Refunded</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>Shipping Carrier:</label>
                    <input type="text" v-model="editForm.shippingCarrier" placeholder="UPS, FedEx, USPS..." />
                  </div>
                  <div class="form-group">
                    <label>Tracking Number:</label>
                    <input type="text" v-model="editForm.trackingNumber" />
                  </div>
                  <div class="form-group">
                    <label>Shipped Date:</label>
                    <input type="datetime-local" v-model="editForm.shippedDate" />
                  </div>
                </div>
              </div>

              <!-- Payment Information -->
              <div class="section">
                <h3>Payment Information</h3>
                <div class="form-grid">
                  <div class="form-group">
                    <label>Payment Method:</label>
                    <select v-model="editForm.paymentMethod">
                      <option value="Cash">Cash</option>
                      <option value="Check">Check</option>
                      <option value="BankTransfer">Bank Transfer</option>
                      <option value="CreditCardPhone">Credit Card (Phone)</option>
                      <option value="Financing">Financing</option>
                      <option value="Other">Other</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>Payment Status:</label>
                    <select v-model="editForm.paymentStatus">
                      <option value="Pending">Pending</option>
                      <option value="Received">Received</option>
                      <option value="PartiallyPaid">Partially Paid</option>
                      <option value="Failed">Failed</option>
                      <option value="Refunded">Refunded</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>Payment Received Date:</label>
                    <input type="datetime-local" v-model="editForm.paymentReceivedDate" />
                  </div>
                  <div class="form-group full-width">
                    <label>Payment Notes:</label>
                    <textarea v-model="editForm.paymentNotes" rows="2"></textarea>
                  </div>
                </div>
              </div>

              <!-- Notes -->
              <div class="section">
                <h3>Notes</h3>
                <div class="form-grid">
                  <div class="form-group full-width">
                    <label>Customer Notes:</label>
                    <textarea v-model="editForm.customerNotes" rows="3" readonly></textarea>
                  </div>
                  <div class="form-group full-width">
                    <label>Admin Notes:</label>
                    <textarea v-model="editForm.adminNotes" rows="3"></textarea>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="closeModal">Cancel</button>
            <button class="btn btn-primary" @click="saveOrder" :disabled="isActionLoading('save')">
              <span v-if="isActionLoading('save')" class="btn-spinner"></span>
              {{ isActionLoading('save') ? 'Saving...' : 'Save Changes' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Create Order Modal -->
      <div v-if="showCreateModal" class="modal" @click.self="closeCreateModal">
        <div class="modal-content large">
          <div class="modal-header">
            <h2>Create New Order</h2>
            <button class="close-btn" @click="closeCreateModal">×</button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="createOrder">
              <!-- Customer Information -->
              <div class="form-section">
                <h3>Customer Information</h3>
                <div class="form-row">
                  <div class="form-group">
                    <label>Customer Name <span class="required">*</span></label>
                    <input type="text" v-model="createForm.customerName" required />
                  </div>
                  <div class="form-group">
                    <label>Email <span class="required">*</span></label>
                    <input type="email" v-model="createForm.customerEmail" required />
                  </div>
                  <div class="form-group">
                    <label>Phone <span class="required">*</span></label>
                    <input type="tel" v-model="createForm.customerPhone" required />
                  </div>
                </div>
              </div>

              <!-- Shipping Address -->
              <div class="form-section">
                <h3>Shipping Address</h3>
                <div class="form-row">
                  <div class="form-group full-width">
                    <label>Street Address <span class="required">*</span></label>
                    <input type="text" v-model="createForm.shippingAddress" required />
                  </div>
                </div>
                <div class="form-row">
                  <div class="form-group">
                    <label>City <span class="required">*</span></label>
                    <input type="text" v-model="createForm.shippingCity" required />
                  </div>
                  <div class="form-group">
                    <label>State <span class="required">*</span></label>
                    <input type="text" v-model="createForm.shippingState" required />
                  </div>
                  <div class="form-group">
                    <label>ZIP Code <span class="required">*</span></label>
                    <input type="text" v-model="createForm.shippingZipCode" required />
                  </div>
                </div>
              </div>

              <!-- Product Selection -->
              <div class="form-section">
                <h3>Products</h3>
                <div class="product-search">
                  <input 
                    type="text" 
                    v-model="productSearch" 
                    @input="searchProducts" 
                    placeholder="Search products by name or SKU..."
                  />
                  <div v-if="searchResults.length > 0" class="search-results">
                    <div 
                      v-for="product in searchResults" 
                      :key="product.id" 
                      class="search-result-item"
                      @click="addProductToOrder(product)"
                    >
                      <div class="product-info">
                        <strong>{{ product.name }}</strong>
                        <span class="product-sku">{{ product.sku }}</span>
                      </div>
                      <div class="product-price">${{ formatCurrency(product.price) }}</div>
                    </div>
                  </div>
                </div>

                <!-- Order Items Table -->
                <div v-if="createForm.items.length > 0" class="order-items-table">
                  <table>
                    <thead>
                      <tr>
                        <th>Product</th>
                        <th>SKU</th>
                        <th>Quantity</th>
                        <th>Unit Price</th>
                        <th>Total</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(item, index) in createForm.items" :key="index">
                        <td>{{ item.productName }}</td>
                        <td>{{ item.productSku }}</td>
                        <td>
                          <input 
                            type="number" 
                            v-model.number="item.quantity" 
                            min="1" 
                            class="quantity-input"
                            @input="calculateOrderTotal"
                          />
                        </td>
                        <td>
                          <input 
                            type="number" 
                            v-model.number="item.unitPrice" 
                            min="0.01" 
                            step="0.01"
                            class="price-input"
                            @input="calculateOrderTotal"
                          />
                        </td>
                        <td>${{ formatCurrency(item.quantity * item.unitPrice) }}</td>
                        <td>
                          <button type="button" @click="removeItem(index)" class="btn-delete">
                            Remove
                          </button>
                        </td>
                      </tr>
                    </tbody>
                    <tfoot>
                      <tr>
                        <td colspan="4" class="text-right"><strong>Subtotal:</strong></td>
                        <td colspan="2">${{ formatCurrency(orderSubtotal) }}</td>
                      </tr>
                      <tr>
                        <td colspan="4" class="text-right">
                          <strong>Tax:</strong>
                          <input 
                            type="number" 
                            v-model.number="createForm.taxAmount" 
                            min="0" 
                            step="0.01"
                            class="inline-input"
                            @input="calculateOrderTotal"
                          />
                        </td>
                        <td colspan="2">${{ formatCurrency(createForm.taxAmount) }}</td>
                      </tr>
                      <tr>
                        <td colspan="4" class="text-right">
                          <strong>Shipping:</strong>
                          <input 
                            type="number" 
                            v-model.number="createForm.shippingCost" 
                            min="0" 
                            step="0.01"
                            class="inline-input"
                            @input="calculateOrderTotal"
                          />
                        </td>
                        <td colspan="2">${{ formatCurrency(createForm.shippingCost) }}</td>
                      </tr>
                      <tr class="total-row">
                        <td colspan="4" class="text-right"><strong>Total:</strong></td>
                        <td colspan="2"><strong>${{ formatCurrency(orderTotal) }}</strong></td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
                <p v-else class="text-muted">No products added yet. Search and select products above.</p>
              </div>

              <!-- Payment Information -->
              <div class="form-section">
                <h3>Payment Information</h3>
                <div class="form-row">
                  <div class="form-group">
                    <label>Payment Method <span class="required">*</span></label>
                    <select v-model="createForm.paymentMethod" required>
                      <option value="">Select...</option>
                      <option value="CreditCard">Credit Card</option>
                      <option value="DebitCard">Debit Card</option>
                      <option value="Cash">Cash</option>
                      <option value="Check">Check</option>
                      <option value="BankTransfer">Bank Transfer</option>
                      <option value="Other">Other</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>Payment Status</label>
                    <select v-model="createForm.paymentStatus">
                      <option value="Pending">Pending</option>
                      <option value="Received">Received</option>
                      <option value="PartiallyPaid">Partially Paid</option>
                      <option value="Failed">Failed</option>
                    </select>
                  </div>
                </div>
                <div class="form-row">
                  <div class="form-group full-width">
                    <label>Payment Notes</label>
                    <textarea v-model="createForm.paymentNotes" rows="2"></textarea>
                  </div>
                </div>
              </div>

              <!-- Notes -->
              <div class="form-section">
                <h3>Notes</h3>
                <div class="form-row">
                  <div class="form-group full-width">
                    <label>Customer Notes</label>
                    <textarea v-model="createForm.customerNotes" rows="2" placeholder="Notes from customer..."></textarea>
                  </div>
                </div>
                <div class="form-row">
                  <div class="form-group full-width">
                    <label>Admin Notes</label>
                    <textarea v-model="createForm.adminNotes" rows="2" placeholder="Internal notes..."></textarea>
                  </div>
                </div>
              </div>

              <!-- Actions -->
              <div class="modal-actions">
                <button type="button" class="btn btn-secondary" @click="closeCreateModal">
                  Cancel
                </button>
                <button type="submit" class="btn btn-primary" :disabled="isActionLoading('createOrder') || createForm.items.length === 0">
                  {{ isActionLoading('createOrder') ? 'Creating...' : 'Create Order' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import AdminLayout from '@/components/AdminLayout.vue';
import { useToast } from '@/composables/useToast';
import { useLoadingState } from '@/composables/useLoadingState';
import { logError } from '@/services/logger';
import { API_URL } from '@/utils/api-config';

interface Order {
  id: number;
  orderNumber: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  totalAmount: number;
  orderStatus: string;
  paymentStatus: string;
  paymentMethod: string;
  createdAt: string;
  itemCount: number;
  shippedDate?: string;
  trackingNumber?: string;
}

interface OrderDetails extends Order {
  userId?: number;
  shippingAddress: string;
  shippingCity: string;
  shippingState: string;
  shippingZipCode: string;
  shippingCountry: string;
  items: OrderItem[];
  subtotal: number;
  taxAmount: number;
  shippingCost: number;
  paymentReceivedDate?: string;
  paymentNotes?: string;
  shippingCarrier?: string;
  deliveredDate?: string;
  customerNotes?: string;
  adminNotes?: string;
  updatedAt: string;
}

interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productSku?: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

const toast = useToast();
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState();

const orders = ref<Order[]>([]);
const selectedOrder = ref<OrderDetails | null>(null);
const showCreateModal = ref(false);
const page = ref(1);
const pagination = reactive({
  totalPages: 1,
  totalCount: 0
});

const stats = reactive({
  totalOrders: 0,
  pendingOrders: 0,
  processingOrders: 0,
  shippedOrders: 0,
  deliveredOrders: 0,
  totalRevenue: 0,
  pendingPayments: 0
});

const filters = reactive({
  search: '',
  orderStatus: '',
  paymentStatus: '',
  startDate: '',
  endDate: ''
});

const editForm = reactive({
  customerName: '',
  customerEmail: '',
  customerPhone: '',
  shippingAddress: '',
  shippingCity: '',
  shippingState: '',
  shippingZipCode: '',
  orderStatus: '',
  paymentStatus: '',
  paymentMethod: '',
  paymentReceivedDate: '',
  paymentNotes: '',
  shippingCarrier: '',
  trackingNumber: '',
  shippedDate: '',
  deliveredDate: '',
  customerNotes: '',
  adminNotes: ''
});

// Create Order Form State
interface CreateOrderItem {
  productId: number;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
}

interface Product {
  id: number;
  name: string;
  sku: string;
  price: number;
}

const createForm = reactive({
  customerName: '',
  customerEmail: '',
  customerPhone: '',
  shippingAddress: '',
  shippingCity: '',
  shippingState: '',
  shippingZipCode: '',
  items: [] as CreateOrderItem[],
  taxAmount: 0,
  shippingCost: 0,
  paymentMethod: '',
  paymentStatus: 'Pending',
  paymentNotes: '',
  customerNotes: '',
  adminNotes: ''
});

const productSearch = ref('');
const searchResults = ref<Product[]>([]);
let searchDebounceTimer: any;

const orderSubtotal = ref(0);
const orderTotal = ref(0);

let debounceTimer: any;

const debounceLoad = () => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadOrders();
  }, 500);
};

const loadStats = async () => {
  try {
    const response = await fetch(`${API_URL}/admin/orders/stats`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('admin_token')}`
      }
    });
    const data = await response.json();
    Object.assign(stats, data);
  } catch (error) {
    logError('Failed to load stats', error);
  }
};

const loadOrders = async () => {
  await executeWithLoading(async () => {
    try {
      const params = new URLSearchParams({
        page: page.value.toString(),
        pageSize: '50'
      });

      if (filters.search) params.append('search', filters.search);
      if (filters.orderStatus) params.append('orderStatus', filters.orderStatus);
      if (filters.paymentStatus) params.append('paymentStatus', filters.paymentStatus);
      if (filters.startDate) params.append('startDate', filters.startDate);
      if (filters.endDate) params.append('endDate', filters.endDate);

      const response = await fetch(`${API_URL}/admin/orders?${params}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('admin_token')}`
        }
      });
      const data = await response.json();
      orders.value = data.orders;
      pagination.totalPages = data.totalPages;
      pagination.totalCount = data.totalCount;
    } catch (error) {
      logError('Failed to load orders', error);
    }
  });
};

const viewOrder = async (id: number) => {
  try {
    const response = await fetch(`${API_URL}/admin/orders/${id}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('admin_token')}`
      }
    });
    selectedOrder.value = await response.json();
    
    // Populate edit form
    editForm.customerName = selectedOrder.value!.customerName;
    editForm.customerEmail = selectedOrder.value!.customerEmail;
    editForm.customerPhone = selectedOrder.value!.customerPhone;
    editForm.shippingAddress = selectedOrder.value!.shippingAddress;
    editForm.shippingCity = selectedOrder.value!.shippingCity;
    editForm.shippingState = selectedOrder.value!.shippingState;
    editForm.shippingZipCode = selectedOrder.value!.shippingZipCode;
    editForm.orderStatus = selectedOrder.value!.orderStatus;
    editForm.paymentStatus = selectedOrder.value!.paymentStatus;
    editForm.paymentMethod = selectedOrder.value!.paymentMethod;
    editForm.paymentReceivedDate = formatDateTimeLocal(selectedOrder.value!.paymentReceivedDate);
    editForm.paymentNotes = selectedOrder.value!.paymentNotes || '';
    editForm.shippingCarrier = selectedOrder.value!.shippingCarrier || '';
    editForm.trackingNumber = selectedOrder.value!.trackingNumber || '';
    editForm.shippedDate = formatDateTimeLocal(selectedOrder.value!.shippedDate);
    editForm.deliveredDate = formatDateTimeLocal(selectedOrder.value!.deliveredDate);
    editForm.customerNotes = selectedOrder.value!.customerNotes || '';
    editForm.adminNotes = selectedOrder.value!.adminNotes || '';
  } catch (error) {
    logError('Failed to load order details', error);
  }
};

const saveOrder = async () => {
  if (!selectedOrder.value) return;
  
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/orders/${selectedOrder.value!.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('admin_token')}`
        },
        body: JSON.stringify({
          customerName: editForm.customerName,
          customerEmail: editForm.customerEmail,
          customerPhone: editForm.customerPhone,
          shippingAddress: editForm.shippingAddress,
          shippingCity: editForm.shippingCity,
          shippingState: editForm.shippingState,
          shippingZipCode: editForm.shippingZipCode,
          orderStatus: editForm.orderStatus,
          paymentStatus: editForm.paymentStatus,
          paymentMethod: editForm.paymentMethod,
          paymentReceivedDate: editForm.paymentReceivedDate || null,
          paymentNotes: editForm.paymentNotes,
          shippingCarrier: editForm.shippingCarrier,
          trackingNumber: editForm.trackingNumber,
          shippedDate: editForm.shippedDate || null,
          deliveredDate: editForm.deliveredDate || null,
          customerNotes: editForm.customerNotes,
          adminNotes: editForm.adminNotes
        })
      });

      if (response.ok) {
        closeModal();
        loadOrders();
        loadStats();
      }
    } catch (error) {
      logError('Failed to save order', error);
    }
  }, 'save');
};

const confirmDeleteOrder = async (id: number) => {
  if (!confirm('Are you sure you want to delete this order? This action cannot be undone.')) {
    return;
  }
  
  await executeWithLoading(async () => {
    try {
      const response = await fetch(`${API_URL}/admin/orders/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('admin_token')}`
        }
      });

      if (response.ok) {
        loadOrders();
        loadStats();
      }
    } catch (error) {
      logError('Failed to delete order', error);
    }
  }, id);
};

const changePage = (newPage: number) => {
  page.value = newPage;
  loadOrders();
};

const clearFilters = () => {
  filters.search = '';
  filters.orderStatus = '';
  filters.paymentStatus = '';
  filters.startDate = '';
  filters.endDate = '';
  loadOrders();
};

const closeModal = () => {
  selectedOrder.value = null;
};

const openCreateOrderModal = () => {
  resetCreateForm();
  showCreateModal.value = true;
};

const closeCreateModal = () => {
  showCreateModal.value = false;
  resetCreateForm();
};

const resetCreateForm = () => {
  createForm.customerName = '';
  createForm.customerEmail = '';
  createForm.customerPhone = '';
  createForm.shippingAddress = '';
  createForm.shippingCity = '';
  createForm.shippingState = '';
  createForm.shippingZipCode = '';
  createForm.items = [];
  createForm.taxAmount = 0;
  createForm.shippingCost = 0;
  createForm.paymentMethod = '';
  createForm.paymentStatus = 'Pending';
  createForm.paymentNotes = '';
  createForm.customerNotes = '';
  createForm.adminNotes = '';
  productSearch.value = '';
  searchResults.value = [];
  orderSubtotal.value = 0;
  orderTotal.value = 0;
};

const searchProducts = async () => {
  clearTimeout(searchDebounceTimer);
  
  if (productSearch.value.length < 2) {
    searchResults.value = [];
    return;
  }

  searchDebounceTimer = setTimeout(async () => {
    try {
      const response = await fetch(
        `${API_URL}/products?search=${encodeURIComponent(productSearch.value)}&pageSize=10`,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('admin_token')}`
          }
        }
      );
      const data = await response.json();
      searchResults.value = data.products || [];
    } catch (error) {
      logError('Failed to search products', error);
      searchResults.value = [];
    }
  }, 300);
};

const addProductToOrder = (product: Product) => {
  // Check if product already exists in order
  const existingItem = createForm.items.find(item => item.productId === product.id);
  
  if (existingItem) {
    existingItem.quantity++;
  } else {
    createForm.items.push({
      productId: product.id,
      productName: product.name,
      productSku: product.sku,
      quantity: 1,
      unitPrice: product.price
    });
  }
  
  // Clear search
  productSearch.value = '';
  searchResults.value = [];
  
  calculateOrderTotal();
};

const removeItem = (index: number) => {
  createForm.items.splice(index, 1);
  calculateOrderTotal();
};

const calculateOrderTotal = () => {
  orderSubtotal.value = createForm.items.reduce((sum, item) => {
    return sum + (item.quantity * item.unitPrice);
  }, 0);
  
  orderTotal.value = orderSubtotal.value + createForm.taxAmount + createForm.shippingCost;
};

const createOrder = async () => {
  if (createForm.items.length === 0) {
    toast.warning('Please add at least one product to the order');
    return;
  }

  await executeWithLoading(async () => {
    try {
      const orderData = {
        customerName: createForm.customerName,
        customerEmail: createForm.customerEmail,
        customerPhone: createForm.customerPhone,
        shippingAddress: createForm.shippingAddress,
        shippingCity: createForm.shippingCity,
        shippingState: createForm.shippingState,
        shippingZipCode: createForm.shippingZipCode,
        shippingCountry: 'USA',
        items: createForm.items.map(item => ({
          productId: item.productId,
          quantity: item.quantity,
          unitPriceOverride: item.unitPrice
        })),
        taxAmount: createForm.taxAmount,
        shippingCost: createForm.shippingCost,
        paymentMethod: createForm.paymentMethod,
        paymentStatus: createForm.paymentStatus,
        paymentNotes: createForm.paymentNotes,
        customerNotes: createForm.customerNotes,
        adminNotes: createForm.adminNotes
      };

      const response = await fetch(`${API_URL}/admin/orders`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('admin_token')}`
        },
        body: JSON.stringify(orderData)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to create order');
      }

      const result = await response.json();
      toast.success(`Order created successfully! Order Number: ${result.orderNumber}`);
      
      closeCreateModal();
      loadOrders();
      loadStats();
    } catch (error: any) {
      logError('Failed to create order', error);
      toast.error(`Failed to create order: ${error.message}`);
    }
  }, 'createOrder');
};

const formatCurrency = (value: number) => {
  return value.toFixed(2);
};

const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
};

const formatDateTimeLocal = (dateString?: string) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toISOString().slice(0, 16);
};

const formatEnumValue = (value: string) => {
  return value.replace(/([A-Z])/g, ' $1').trim();
};

const getOrderStatusClass = (status: string) => {
  const statusMap: Record<string, string> = {
    'Pending': 'status-pending',
    'Processing': 'status-processing',
    'Shipped': 'status-shipped',
    'Delivered': 'status-delivered',
    'Cancelled': 'status-cancelled',
    'Refunded': 'status-refunded'
  };
  return statusMap[status] || '';
};

const getPaymentStatusClass = (status: string) => {
  const statusMap: Record<string, string> = {
    'Pending': 'payment-pending',
    'Received': 'payment-received',
    'PartiallyPaid': 'payment-partial',
    'Failed': 'payment-failed',
    'Refunded': 'payment-refunded'
  };
  return statusMap[status] || '';
};

onMounted(() => {
  loadStats();
  loadOrders();
});
</script>

<style scoped>
.orders-page {
  padding: 2rem;
}

.page-header {
  margin-bottom: 2rem;
}

.header-content h1 {
  margin: 0 0 0.5rem 0;
  font-size: 2rem;
}

.header-content p {
  margin: 0;
  color: #666;
}

.stats-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  margin-top: 1.5rem;
}

.stat-card {
  background: white;
  padding: 1rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.stat-card.pending {
  border-left: 4px solid #ff9800;
}

.stat-card.processing {
  border-left: 4px solid #2196f3;
}

.stat-card.shipped {
  border-left: 4px solid #9c27b0;
}

.stat-card.revenue {
  border-left: 4px solid #4caf50;
}

.stat-label {
  font-size: 0.875rem;
  color: #666;
  margin-bottom: 0.5rem;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: bold;
}

.actions-bar {
  margin-bottom: 1rem;
}

.filters {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  background: white;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.filter-group label {
  font-size: 0.875rem;
  color: #666;
}

.filter-group input,
.filter-group select {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.btn-clear {
  align-self: flex-end;
  padding: 0.5rem 1rem;
  background: #f5f5f5;
  border: 1px solid #ddd;
  border-radius: 4px;
  cursor: pointer;
}

.loading,
.empty-state {
  text-align: center;
  padding: 2rem;
  background: white;
  border-radius: 8px;
}

.orders-table {
  background: white;
  border-radius: 8px;
  overflow: hidden;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background: #f5f5f5;
}

th, td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid #e0e0e0;
}

.text-muted {
  color: #666;
  font-size: 0.875rem;
}

.status-badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.status-pending {
  background: #fff3e0;
  color: #f57c00;
}

.status-processing {
  background: #e3f2fd;
  color: #1976d2;
}

.status-shipped {
  background: #f3e5f5;
  color: #7b1fa2;
}

.status-delivered {
  background: #e8f5e9;
  color: #388e3c;
}

.status-cancelled {
  background: #ffebee;
  color: #c62828;
}

.status-refunded {
  background: #fce4ec;
  color: #c2185b;
}

.payment-pending {  background: #fff3e0;
  color: #f57c00;
}

.payment-received {
  background: #e8f5e9;
  color: #388e3c;
}

.payment-partial {
  background: #fff9c4;
  color: #f57f17;
}

.payment-failed {
  background: #ffebee;
  color: #c62828;
}

.payment-refunded {
  background: #fce4ec;
  color: #c2185b;
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

.btn-delete {
  background: #f44336;
  color: white;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border-top: 1px solid #e0e0e0;
}

.pagination button {
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: white;
  cursor: pointer;
}

.pagination button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

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
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-content.large {
  max-width: 900px;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e0e0e0;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 2rem;
  cursor: pointer;
  color: #666;
}

.modal-body {
  padding: 1.5rem;
}

.order-sections {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.section h3 {
  margin: 0 0 1rem 0;
  font-size: 1.125rem;
  color: #333;
  border-bottom: 2px solid #f5f5f5;
  padding-bottom: 0.5rem;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.form-group.full-width {
  grid-column: 1 / -1;
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #666;
}

.form-group input,
.form-group select,
.form-group textarea {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.875rem;
}

.form-group input[readonly],
.form-group textarea[readonly] {
  background: #f5f5f5;
}

.items-table {
  width: 100%;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
}

.items-table thead {
  background: #f5f5f5;
}

.items-table th,
.items-table td {
  padding: 0.75rem;
  text-align: left;
  border-bottom: 1px solid #e0e0e0;
}

.items-table tfoot {
  background: #fafafa;
  font-weight: 600;
}

.total-row {
  background: #f5f5f5;
  font-size: 1.125rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1.5rem;
  border-top: 1px solid #e0e0e0;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 600;
}

.btn-primary {
  background: #2196f3;
  color: white;
}

.btn-secondary {
  background: #f5f5f5;
  color: #333;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.text-center {
  text-align: center;
}

/* Create Order Form Styles */
.form-section {
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.form-section:last-of-type {
  border-bottom: none;
}

.form-section h3 {
  font-size: 1.125rem;
  font-weight: 600;
  margin-bottom: 1rem;
  color: #1f2937;
}

.form-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group.full-width {
  grid-column: 1 / -1;
}

.form-group label {
  font-weight: 500;
  margin-bottom: 0.5rem;
  color: #374151;
  font-size: 0.875rem;
}

.required {
  color: #dc2626;
}

.form-group input,
.form-group select,
.form-group textarea {
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.product-search {
  position: relative;
  margin-bottom: 1rem;
}

.product-search input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
}

.search-results {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  margin-top: 0.25rem;
  max-height: 300px;
  overflow-y: auto;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
  z-index: 10;
}

.search-result-item {
  padding: 0.75rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  border-bottom: 1px solid #f3f4f6;
}

.search-result-item:hover {
  background-color: #f9fafb;
}

.search-result-item:last-child {
  border-bottom: none;
}

.product-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.product-sku {
  font-size: 0.75rem;
  color: #6b7280;
}

.product-price {
  font-weight: 600;
  color: #4f46e5;
}

.order-items-table {
  margin-top: 1rem;
}

.order-items-table table {
  width: 100%;
  border-collapse: collapse;
}

.order-items-table th {
  background-color: #f9fafb;
  padding: 0.75rem;
  text-align: left;
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
  border-bottom: 2px solid #e5e7eb;
}

.order-items-table td {
  padding: 0.75rem;
  border-bottom: 1px solid #e5e7eb;
  font-size: 0.875rem;
}

.order-items-table tfoot td {
  font-weight: 500;
  background-color: #f9fafb;
}

.order-items-table .total-row td {
  padding-top: 1rem;
  font-size: 1rem;
  color: #1f2937;
}

.quantity-input,
.price-input {
  width: 80px;
  padding: 0.375rem;
  border: 1px solid #d1d5db;
  border-radius: 0.25rem;
  font-size: 0.875rem;
}

.inline-input {
  width: 80px;
  padding: 0.25rem 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 0.25rem;
  font-size: 0.875rem;
  margin-left: 0.5rem;
}

.text-right {
  text-align: right;
}

.text-muted {
  color: #6b7280;
  font-size: 0.875rem;
  font-style: italic;
  padding: 1rem;
  text-align: center;
}

.btn-delete {
  padding: 0.375rem 0.75rem;
  background-color: #dc2626;
  color: white;
  border: none;
  border-radius: 0.25rem;
  font-size: 0.75rem;
  cursor: pointer;
}

.btn-delete:hover {
  background-color: #b91c1c;
}

.modal-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid #e5e7eb;
}
</style>
