import { defineStore } from 'pinia'
import { ref } from 'vue'
import { submitInquiry } from '@/services/inquiry.service'
import type { CreateInquiryDto } from '@/types/inquiry.types'

export const useInquiryStore = defineStore('inquiry', () => {
  const submitting = ref(false)
  const submitted = ref(false)
  const error = ref<string | null>(null)

  async function submit(dto: CreateInquiryDto) {
    submitting.value = true
    error.value = null
    try {
      await submitInquiry(dto)
      submitted.value = true
    } catch (e: any) {
      error.value = e?.title ?? 'Failed to submit inquiry. Please try again.'
    } finally {
      submitting.value = false
    }
  }

  function reset() {
    submitted.value = false
    error.value = null
  }

  return { submitting, submitted, error, submit, reset }
})
