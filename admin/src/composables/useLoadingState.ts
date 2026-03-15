import { ref } from 'vue'

/**
 * Composable for managing loading states in admin views
 * Provides consistent loading indicators and button state management
 */
export function useLoadingState() {
  // Global loading state (e.g., for initial data fetch)
  const isLoading = ref(false)
  
  // Action-specific loading state (tracks which item is being processed)
  const actionLoading = ref<number | string | null>(null)

  /**
   * Execute an async action with loading state management
   * @param action - Async function to execute
   * @param itemId - Optional ID to track which item is loading
   */
  const executeWithLoading = async <T>(
    action: () => Promise<T>,
    itemId?: number | string
  ): Promise<T> => {
    try {
      if (itemId !== undefined) {
        actionLoading.value = itemId
      } else {
        isLoading.value = true
      }
      return await action()
    } finally {
      if (itemId !== undefined) {
        actionLoading.value = null
      } else {
        isLoading.value = false
      }
    }
  }

  /**
   * Check if a specific item is currently loading
   */
  const isActionLoading = (itemId: number | string): boolean => {
    return actionLoading.value === itemId
  }

  /**
   * Check if any action is currently in progress
   */
  const hasActiveAction = (): boolean => {
    return actionLoading.value !== null
  }

  /**
   * Reset all loading states
   */
  const resetLoading = () => {
    isLoading.value = false
    actionLoading.value = null
  }

  return {
    isLoading,
    actionLoading,
    executeWithLoading,
    isActionLoading,
    hasActiveAction,
    resetLoading
  }
}
