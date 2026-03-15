import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

export const useThemeStore = defineStore('theme', () => {
  // State - Check localStorage first, then system preference
  const isDarkMode = ref<boolean>(
    localStorage.getItem('darkMode') === 'true' || 
    (localStorage.getItem('darkMode') === null && window.matchMedia('(prefers-color-scheme: dark)').matches)
  )

  // Watch for changes and persist to localStorage
  watch(isDarkMode, (newValue) => {
    localStorage.setItem('darkMode', String(newValue))
    applyDarkMode(newValue)
  })

  // Apply dark mode class to document
  const applyDarkMode = (isDark: boolean) => {
    if (isDark) {
      document.documentElement.classList.add('dark-mode')
    } else {
      document.documentElement.classList.remove('dark-mode')
    }
  }

  // Toggle dark mode
  const toggleDarkMode = () => {
    isDarkMode.value = !isDarkMode.value
  }

  // Set dark mode explicitly
  const setDarkMode = (value: boolean) => {
    isDarkMode.value = value
  }

  // Initialize on mount
  applyDarkMode(isDarkMode.value)

  return {
    isDarkMode,
    toggleDarkMode,
    setDarkMode
  }
})
