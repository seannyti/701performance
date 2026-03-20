import { ref, readonly } from 'vue'

const API_URL = import.meta.env.VITE_API_URL as string

export interface NavLink {
  name: string
  to: string
}

interface GeneralSettings {
  businessName: string
  tagline: string
  phone: string
  email: string
  address: string
  hours: string
  navLinks: NavLink[]
  socials: { facebook: string; instagram: string; tiktok: string; youtube: string }
}

interface PagesSettings {
  home: {
    heroTitle: string; heroSubtitle: string
    ctaPrimaryText: string; ctaPrimaryLink: string
    ctaSecondaryText: string; ctaSecondaryLink: string
    featuredTitle: string
  }
  about: { title: string; lead: string; body: string; ctaText: string; ctaLink: string }
  contact: { title: string; intro: string }
  seo: Record<string, string>
}

interface ContentSettings {
  announcementBar: { enabled: boolean; text: string; bgColor: string; textColor: string }
  testimonials: { name: string; text: string; rating: number }[]
  faqs: { question: string; answer: string }[]
  brands: { name: string; url: string }[]
}

interface LegalSettings {
  faq: string
  shipping: string
  privacy: string
  terms: string
}

export interface ThemeSettings {
  preset: string
  primary: string
  primaryDark: string
  primaryRgb: string
}

interface AdvancedSettings {
  maintenanceModeEnabled: boolean
  maintenanceModeMessage: string
  googleAnalyticsId: string
  facebookPixelId: string
  customHeadCode: string
}

interface AllSettings {
  general: GeneralSettings
  pages: PagesSettings
  content: ContentSettings
  legal: LegalSettings
  theme: ThemeSettings
  advanced: AdvancedSettings
}

const settings = ref<AllSettings | null>(null)
const loaded = ref(false)

export async function loadSettings() {
  if (loaded.value) return
  try {
    const res = await fetch(`${API_URL}/settings`)
    settings.value = await res.json()
    loaded.value = true
  } catch {
    // use defaults if API unavailable
  }
}

export function useSettings() {
  return { settings: readonly(settings), loaded: readonly(loaded) }
}
