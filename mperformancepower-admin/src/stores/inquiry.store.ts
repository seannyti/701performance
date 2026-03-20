import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import * as inquiryService from '@/services/inquiry.service'
import type { Inquiry, InquiryStats, InquiryFilters } from '@/types/inquiry.types'

export const useInquiryStore = defineStore('inquiries', () => {
  const inquiries = ref<Inquiry[]>([])
  const totalCount = ref(0)
  const totalPages = ref(0)
  const loading = ref(false)
  const stats = ref<InquiryStats>({ total: 0, new: 0, inProgress: 0, resolved: 0, avgResponseMinutes: null })

  const filters = reactive<InquiryFilters>({
    status: '',
    search: '',
    from: '',
    to: '',
  })

  async function fetchInquiries(page = 1, pageSize = 20) {
    loading.value = true
    try {
      const result = await inquiryService.getInquiries(
        page, pageSize,
        filters.status || undefined,
        filters.search || undefined,
        filters.from || undefined,
        filters.to || undefined
      )
      inquiries.value = result.items
      totalCount.value = result.totalCount
      totalPages.value = result.totalPages
    } finally {
      loading.value = false
    }
  }

  async function fetchStats() {
    stats.value = await inquiryService.getStats()
  }

  async function updateStatus(id: number, status: string) {
    const updated = await inquiryService.updateStatus(id, status)
    const idx = inquiries.value.findIndex(i => i.id === id)
    if (idx !== -1) inquiries.value[idx] = updated
    await fetchStats()
    return updated
  }

  function addInquiry(inq: Inquiry) {
    inquiries.value.unshift(inq)
    stats.value.total++
    stats.value.new++
  }

  function clearFilters() {
    filters.status = ''
    filters.search = ''
    filters.from = ''
    filters.to = ''
  }

  return {
    inquiries, totalCount, totalPages, loading, stats, filters,
    fetchInquiries, fetchStats, updateStatus, addInquiry, clearFilters
  }
})
