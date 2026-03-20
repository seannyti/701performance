import { api } from './api'
import type { PagedResult } from '@/types/common.types'
import type { Order, OrderListItem, CreateOrderDto, UpdateOrderDto, FinanceStats } from '@/types/order.types'

export async function getOrders(page = 1, pageSize = 20, status?: string, search?: string): Promise<PagedResult<OrderListItem>> {
  const { data } = await api.get('/orders', { params: { page, pageSize, status, search } })
  return data
}

export async function getOrder(id: number): Promise<Order> {
  const { data } = await api.get(`/orders/${id}`)
  return data
}

export async function createOrder(dto: CreateOrderDto): Promise<Order> {
  const { data } = await api.post('/orders', dto)
  return data
}

export async function updateOrder(id: number, dto: UpdateOrderDto): Promise<Order> {
  const { data } = await api.put(`/orders/${id}`, dto)
  return data
}

export async function deleteOrder(id: number): Promise<void> {
  await api.delete(`/orders/${id}`)
}

export async function getFinanceStats(): Promise<FinanceStats> {
  const { data } = await api.get('/orders/finance/stats')
  return data
}
