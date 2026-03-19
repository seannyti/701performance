import { ref, computed } from 'vue'
import { logError, logDebug } from '@/services/logger'
import { SETTINGS_POLL_INTERVAL_MS } from '@/constants'

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
      logDebug('Fetching settings from API...');
      const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226'
      logDebug('API URL', { url: API_URL });
      const response = await fetch(`${API_URL}/api/v1/settings`)
      
      if (!response.ok) {
        throw new Error('Failed to fetch settings')
      }

      const data: SiteSetting[] = await response.json()
      logDebug('Received settings from API', { count: data.length });
      
      // Convert array to key-value map
      const settingsMap: Record<string, string> = {}
      data.forEach(setting => {
        settingsMap[setting.key] = setting.value
      })
      
      settings.value = settingsMap
      logDebug('Settings loaded successfully', { 
        keysCount: Object.keys(settingsMap).length,
        samplePrimaryColor: settingsMap['theme_primary_color']
      });
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
    
    // Update only keys that actually changed — avoids replacing the whole
    // reactive object which would cause computed properties returning parsed
    // JSON arrays to get new references and trigger unnecessary re-renders
    let changed = false
    for (const [key, value] of Object.entries(settingsMap)) {
      if (settings.value[key] !== value) {
        settings.value[key] = value
        changed = true
      }
    }
    for (const key of Object.keys(settings.value)) {
      if (!(key in settingsMap)) {
        delete settings.value[key]
        changed = true
      }
    }
    if (changed) logDebug('Settings updated from server')
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
  }, SETTINGS_POLL_INTERVAL_MS)
  
  logDebug('Auto-refresh enabled: Settings will update every 30 seconds');
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

  const getSetting = (key: string, defaultValue: string = ''): string => {
    const value = settings.value[key]
    // Return default if value is undefined, null, or empty string
    return (value !== undefined && value !== null && value !== '') ? value : defaultValue
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
