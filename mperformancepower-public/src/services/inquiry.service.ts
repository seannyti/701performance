import { api } from './api'
import type { CreateInquiryDto } from '@/types/inquiry.types'

export async function submitInquiry(dto: CreateInquiryDto): Promise<void> {
  await api.post('/inquiries', dto)
}
