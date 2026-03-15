import { useToast as useToastification } from 'vue-toastification'
import { ERROR_TOAST_DURATION_MS } from '@/constants'

/**
 * Toast notification composable with consistent styling and behavior
 */
export function useToast() {
  const toast = useToastification()

  return {
    success: (message: string, title?: string) => {
      toast.success(title ? `${title}: ${message}` : message)
    },
    error: (message: string, title?: string) => {
      toast.error(title ? `${title}: ${message}` : message, {
        timeout: ERROR_TOAST_DURATION_MS
      })
    },
    warning: (message: string, title?: string) => {
      toast.warning(title ? `${title}: ${message}` : message)
    },
    info: (message: string, title?: string) => {
      toast.info(title ? `${title}: ${message}` : message)
    },
    // Convenience methods for common operations
    uploadSuccess: (count: number = 1) => {
      toast.success(count === 1 ? 'File uploaded successfully!' : `${count} files uploaded successfully!`)
    },
    uploadError: (message: string = 'Failed to upload file') => {
      toast.error(message, { timeout: ERROR_TOAST_DURATION_MS })
    },
    deleteSuccess: (itemName: string = 'Item') => {
      toast.success(`${itemName} deleted successfully!`)
    },
    deleteError: (message: string = 'Failed to delete item') => {
      toast.error(message, { timeout: ERROR_TOAST_DURATION_MS })
    },
    saveSuccess: (itemName: string = 'Changes') => {
      toast.success(`${itemName} saved successfully!`)
    },
    saveError: (message: string = 'Failed to save changes') => {
      toast.error(message, { timeout: ERROR_TOAST_DURATION_MS })
    },
    copySuccess: () => {
      toast.info('Copied to clipboard!')
    }
  }
}
