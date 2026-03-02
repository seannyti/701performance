import { ref, onUnmounted, computed } from 'vue'
import { logError, logDebug } from '@/services/logger'

interface SiteSetting {
  id: number
  key: string
  value: string
  displayName: string
}

const settings = ref<Record<string, string>>({})
const loading = ref(true)
const error = ref<string | null>(null)

// Fetch settings only once
let fetchPromise: Promise<void> | null = null
let pollingInterval: number | null = null
let isPollingStarted = false

const fetchSettings = async () => {
  if (fetchPromise) return fetchPromise

  fetchPromise = (async () => {
    try {
      const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226'
      const response = await fetch(`${API_URL}/api/v1/settings`)
      
      if (!response.ok) {
        throw new Error('Failed to fetch settings')
      }

      const data: SiteSetting[] = await response.json()
      
      // Convert array to key-value map
      const settingsMap: Record<string, string> = {}
      data.forEach(setting => {
        settingsMap[setting.key] = setting.value
      })
      
      settings.value = settingsMap
      error.value = null
    } catch (err: any) {
      logError('Error loading site settings', err);
      error.value = err.message
      // Set defaults if fetch fails
      settings.value = {
        site_name: 'Powersports Gear & Vehicles',
        site_tagline: 'Your Ultimate Powersports Destination',
        contact_email: 'info@powersports.com',
        contact_phone: '(555) 555-5555',
        contact_address: '123 Main St, City, State 12345',
        facebook_url: '',
        instagram_url: '',
        twitter_url: '',
        youtube_url: '',
        hero_title: 'Premium Powersports Vehicles & Gear',
        hero_subtitle: 'Discover our collection of ATVs, dirt bikes, UTVs, snowmobiles, and premium gear.',
        enable_maintenance_mode: 'false'
      }
    } finally {
      loading.value = false
    }
  })()

  return fetchPromise
}

// Refetch settings (bypasses cache for polling)
const refetchSettings = async () => {
  try {
    const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226'
    const response = await fetch(`${API_URL}/api/v1/settings`)
    
    if (!response.ok) {
      return // Silently fail during polling
    }

    const data: SiteSetting[] = await response.json()
    
    // Convert array to key-value map
    const settingsMap: Record<string, string> = {}
    data.forEach(setting => {
      settingsMap[setting.key] = setting.value
    })
    
    // Only update if settings actually changed
    const currentJson = JSON.stringify(settings.value)
    const newJson = JSON.stringify(settingsMap)
    
    if (currentJson !== newJson) {
      settings.value = settingsMap
      logDebug('Settings updated from server');
    }
  } catch (err) {
    // Silently fail during polling - don't want to spam console
  }
}

// Start polling for updates every 30 seconds
const startPolling = () => {
  if (isPollingStarted) return
  
  isPollingStarted = true
  pollingInterval = window.setInterval(() => {
    refetchSettings()
  }, 30000) // Check every 30 seconds
  
  console.log('✅ Auto-refresh enabled: Settings will update every 30 seconds')
}

// Stop polling
const stopPolling = () => {
  if (pollingInterval) {
    clearInterval(pollingInterval)
    pollingInterval = null
    isPollingStarted = false
  }
}

export function useSettings() {
  // Auto-fetch on first use
  if (Object.keys(settings.value).length === 0 && !fetchPromise) {
    fetchSettings().then(() => {
      // Start polling after initial fetch
      startPolling()
    })
  } else if (!isPollingStarted) {
    // If settings already loaded, start polling immediately
    startPolling()
  }

  // Cleanup on component unmount
  onUnmounted(() => {
    stopPolling()
  })

  const getSetting = (key: string, defaultValue: string = ''): string => {
    return settings.value[key] || defaultValue
  }

  const isMaintenanceMode = computed(() => {
    return settings.value['enable_maintenance_mode'] === 'true'
  })

  return {
    settings,
    loading,
    error,
    getSetting,
    isMaintenanceMode,
    fetchSettings,
    refetchSettings,
    startPolling,
    stopPolling
  }
}
