import { api } from './api'
import type { PagedResult } from '@/types/common.types'
import type { Inquiry, InquiryStats } from '@/types/inquiry.types'

export async function getInquiries(
  page = 1,
  pageSize = 20,
  status?: string,
  search?: string,
  from?: string,
  to?: string
): Promise<PagedResult<Inquiry>> {
  const { data } = await api.get('/inquiries', {
    params: { page, pageSize, status, search, from, to }
  })
  return data
}

export async function getInquiry(id: number): Promise<Inquiry> {
  const { data } = await api.get(`/inquiries/${id}`)
  return data
}

export async function updateStatus(id: number, status: string): Promise<Inquiry> {
  const { data } = await api.put(`/inquiries/${id}/status`, { status })
  return data
}

export async function getStats(): Promise<InquiryStats> {
  const { data } = await api.get('/inquiries/stats')
  return data
}

export async function deleteInquiry(id: number): Promise<void> {
  await api.delete(`/inquiries/${id}`)
}
