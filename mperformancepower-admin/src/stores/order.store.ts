import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as orderService from '@/services/order.service'
import type { Order, OrderListItem, CreateOrderDto, UpdateOrderDto, FinanceStats } from '@/types/order.types'

export const useOrderStore = defineStore('orders', () => {
  const orders = ref<OrderListItem[]>([])
  const selectedOrder = ref<Order | null>(null)
  const totalCount = ref(0)
  const totalPages = ref(0)
  const loading = ref(false)
  const saving = ref(false)
  const financeStats = ref<FinanceStats | null>(null)

  async function fetchOrders(page = 1, pageSize = 20, status?: string, search?: string) {
    loading.value = true
    try {
      const result = await orderService.getOrders(page, pageSize, status, search)
      orders.value = result.items
      totalCount.value = result.totalCount
      totalPages.value = result.totalPages
    } finally {
      loading.value = false
    }
  }

  async function fetchOrder(id: number) {
    loading.value = true
    try {
      selectedOrder.value = await orderService.getOrder(id)
    } finally {
      loading.value = false
    }
  }

  async function createOrder(dto: CreateOrderDto): Promise<Order> {
    saving.value = true
    try {
      const o = await orderService.createOrder(dto)
      await fetchOrders()
      return o
    } finally {
      saving.value = false
    }
  }

  async function updateOrder(id: number, dto: UpdateOrderDto): Promise<Order> {
    saving.value = true
    try {
      const o = await orderService.updateOrder(id, dto)
      const idx = orders.value.findIndex(x => x.id === id)
      if (idx !== -1) {
        orders.value[idx] = {
          ...orders.value[idx],
          status: o.status,
          salePrice: o.salePrice,
          customerName: o.customerName,
          customerEmail: o.customerEmail,
          paymentMethod: o.paymentMethod,
        }
      }
      if (selectedOrder.value?.id === id) selectedOrder.value = o
      return o
    } finally {
      saving.value = false
    }
  }

  async function deleteOrder(id: number) {
    await orderService.deleteOrder(id)
    orders.value = orders.value.filter(o => o.id !== id)
    totalCount.value--
  }

  async function fetchFinanceStats() {
    financeStats.value = await orderService.getFinanceStats()
  }

  return {
    orders, selectedOrder, totalCount, totalPages, loading, saving, financeStats,
    fetchOrders, fetchOrder, createOrder, updateOrder, deleteOrder, fetchFinanceStats
  }
})
