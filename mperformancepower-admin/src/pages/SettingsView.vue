<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { useToast } from 'primevue/usetoast'
import AdminShell from '@/components/layout/AdminShell.vue'

const toast = useToast()
const API_URL = import.meta.env.VITE_API_URL as string

const activeTab = ref<'general' | 'pages' | 'content' | 'email' | 'legal' | 'theme' | 'advanced' | 'auth'>('general')
const saving = ref(false)

// ── Section data ──────────────────────────────────────────────────

const general = reactive({
  businessName: '',
  tagline: '',
  phone: '',
  email: '',
  address: '',
  hours: '',
  navLinks: [] as { name: string; to: string }[],
  socials: { facebook: '', instagram: '', tiktok: '', youtube: '' },
})

const pages = reactive({
  home: {
    heroTitle: '',
    heroSubtitle: '',
    ctaPrimaryText: '',
    ctaPrimaryLink: '',
    ctaSecondaryText: '',
    ctaSecondaryLink: '',
    featuredTitle: '',
  },
  about: { title: '', lead: '', body: '', ctaText: '', ctaLink: '' },
  contact: { title: '', intro: '' },
  seo: {
    homeTitle: '', homeDescription: '',
    aboutTitle: '', aboutDescription: '',
    contactTitle: '', contactDescription: '',
    inventoryTitle: '', inventoryDescription: '',
  },
})

const content = reactive({
  announcementBar: { enabled: false, text: '', bgColor: '#e63946', textColor: '#ffffff' },
  testimonials: [] as { name: string; text: string; rating: number }[],
  faqs: [] as { question: string; answer: string }[],
  brands: [] as { name: string; url: string }[],
})

const email = reactive({
  inquiryRecipients: [] as string[],
  orderRecipients: [] as string[],
  replyFrom: '',
  emailFooter: '',
  smtpHost: '',
  smtpPort: 587,
  smtpUser: '',
  smtpPass: '',
  smtpSsl: true,
})

const advanced = reactive({
  googleAnalyticsId: '',
  facebookPixelId: '',
  customHeadCode: '',
  maintenanceModeEnabled: false,
  maintenanceModeMessage: '',
})

const auth = reactive({
  enableRegistration: true,
  requireEmailVerification: false,
})

const legal = reactive({
  faq: '',
  shipping: '',
  privacy: '',
  terms: '',
})

// ── Theme ─────────────────────────────────────────────────────────
const PRESETS: Record<string, { label: string; primary: string; primaryDark: string; primaryRgb: string; description: string }> = {
  'red-stealth': {
    label: 'Red Stealth',
    primary: '#e63946', primaryDark: '#c1121f', primaryRgb: '230, 57, 70',
    description: 'Bold red accent on a deep black background — the default.',
  },
  'crimson': {
    label: 'Crimson',
    primary: '#DC143C', primaryDark: '#A50029', primaryRgb: '220, 20, 60',
    description: 'Deep, rich crimson — more intense than classic red.',
  },
  'steel': {
    label: 'Steel Metal',
    primary: '#9EABB8', primaryDark: '#7A8A9A', primaryRgb: '158, 171, 184',
    description: 'Cool steel-gray accent — clean metallic look.',
  },
  'chrome': {
    label: 'Chrome Silver',
    primary: '#C8CDD4', primaryDark: '#A8B0BA', primaryRgb: '200, 205, 212',
    description: 'Bright chrome-silver — high contrast metallic.',
  },
  'custom': {
    label: 'Custom',
    primary: '#e63946', primaryDark: '#c1121f', primaryRgb: '230, 57, 70',
    description: 'Pick your own accent color.',
  },
}

const theme = reactive({
  preset: 'red-stealth',
  primary: '#e63946',
  primaryDark: '#c1121f',
  primaryRgb: '230, 57, 70',
})

function applyPreset(key: string) {
  const p = PRESETS[key]
  if (!p || key === 'custom') { theme.preset = key; return }
  theme.preset = key
  theme.primary = p.primary
  theme.primaryDark = p.primaryDark
  theme.primaryRgb = p.primaryRgb
}

function hexToRgb(hex: string): string {
  const r = parseInt(hex.slice(1, 3), 16)
  const g = parseInt(hex.slice(3, 5), 16)
  const b = parseInt(hex.slice(5, 7), 16)
  return `${r}, ${g}, ${b}`
}

function onPrimaryChange(val: string) {
  theme.primaryRgb = hexToRgb(val)
  // Auto-darken: reduce each channel by ~18%
  const r = Math.max(0, Math.round(parseInt(val.slice(1,3),16) * 0.82))
  const g = Math.max(0, Math.round(parseInt(val.slice(3,5),16) * 0.82))
  const b = Math.max(0, Math.round(parseInt(val.slice(5,7),16) * 0.82))
  theme.primaryDark = `#${r.toString(16).padStart(2,'0')}${g.toString(16).padStart(2,'0')}${b.toString(16).padStart(2,'0')}`
}

// ── Email recipient chip inputs ───────────────────────────────────
const inquiryInput = ref('')
const orderInput = ref('')

function addRecipient(list: string[], inputRef: { value: string }) {
  const val = inputRef.value.trim()
  if (val && !list.includes(val)) list.push(val)
  inputRef.value = ''
}
function removeRecipient(list: string[], i: number) { list.splice(i, 1) }

// ── Brands ───────────────────────────────────────────────────────
const brandNameInput = ref('')
const brandUrlInput = ref('')
function addBrand() {
  const name = brandNameInput.value.trim()
  if (!name) return
  content.brands.push({ name, url: brandUrlInput.value.trim() })
  brandNameInput.value = ''
  brandUrlInput.value = ''
}
function removeBrand(i: number) { content.brands.splice(i, 1) }

// ── Testimonials ──────────────────────────────────────────────────
function addTestimonial() {
  content.testimonials.push({ name: '', text: '', rating: 5 })
}
function removeTestimonial(i: number) { content.testimonials.splice(i, 1) }

// ── FAQs ─────────────────────────────────────────────────────────
function addFaq() {
  content.faqs.push({ question: '', answer: '' })
}
function removeFaq(i: number) { content.faqs.splice(i, 1) }

// ── Nav Links ─────────────────────────────────────────────────────
const DEFAULT_NAV_LINKS = [
  { name: 'Home', to: '/' },
  { name: 'Inventory', to: '/inventory' },
  { name: 'About', to: '/about' },
  { name: 'Contact', to: '/contact' },
]

function addNavLink() {
  // If starting from scratch, pre-populate defaults first so user doesn't lose them
  if (general.navLinks.length === 0) {
    DEFAULT_NAV_LINKS.forEach(l => general.navLinks.push({ ...l }))
  }
  general.navLinks.push({ name: '', to: '/' })
}
function removeNavLink(i: number) { general.navLinks.splice(i, 1) }
function moveNavLink(from: number, to: number) {
  const arr = general.navLinks
  if (to < 0 || to >= arr.length) return
  const [item] = arr.splice(from, 1)
  arr.splice(to, 0, item)
}

// ── Load ─────────────────────────────────────────────────────────
onMounted(async () => {
  try {
    const res = await fetch(`${API_URL}/settings`)
    const data = await res.json()
    if (data.general) {
      Object.assign(general, data.general)
      if (!general.navLinks || general.navLinks.length === 0)
        general.navLinks = DEFAULT_NAV_LINKS.map(l => ({ ...l }))
    }
    if (data.pages) {
      Object.assign(pages.home, data.pages.home ?? {})
      Object.assign(pages.about, data.pages.about ?? {})
      Object.assign(pages.contact, data.pages.contact ?? {})
      Object.assign(pages.seo, data.pages.seo ?? {})
    }
    if (data.content) {
      Object.assign(content.announcementBar, data.content.announcementBar ?? {})
      if (data.content.testimonials) content.testimonials = data.content.testimonials
      if (data.content.faqs) content.faqs = data.content.faqs
      if (data.content.brands) content.brands = data.content.brands
    }
    if (data.email) {
      Object.assign(email, data.email)
      if (!email.inquiryRecipients) email.inquiryRecipients = []
      if (!email.orderRecipients) email.orderRecipients = []
    }
    if (data.advanced) Object.assign(advanced, data.advanced)
    if (data.auth) Object.assign(auth, data.auth)
    if (data.legal) Object.assign(legal, data.legal)
    if (data.theme) Object.assign(theme, data.theme)
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to load settings', life: 3000 })
  }
})

// ── Save ─────────────────────────────────────────────────────────
async function save() {
  // Auto-commit any partially typed recipient before saving
  if (inquiryInput.value.trim()) addRecipient(email.inquiryRecipients, inquiryInput as any)
  if (orderInput.value.trim()) addRecipient(email.orderRecipients, orderInput as any)
  saving.value = true
  const token = localStorage.getItem('mpp_token')
  const sectionMap: Record<string, unknown> = {
    general,
    pages,
    content,
    email,
    advanced,
    auth,
    legal,
    theme,
  }
  const section = activeTab.value
  try {
    const res = await fetch(`${API_URL}/settings/${section}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
      body: JSON.stringify(sectionMap[section]),
    })
    if (!res.ok) throw new Error()
    toast.add({ severity: 'success', summary: 'Saved!', detail: `${section} settings updated`, life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Save failed', life: 3000 })
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <AdminShell>
    <div class="settings-page">

      <!-- Top bar -->
      <div class="settings-header">
        <h1 class="settings-title">Site Settings</h1>
        <button class="save-btn" :disabled="saving" @click="save">
          {{ saving ? 'Saving...' : 'Save Changes' }}
        </button>
      </div>

      <!-- Tabs -->
      <div class="tab-bar">
        <button v-for="t in (['general','pages','content','email','legal','theme','advanced','auth'] as const)" :key="t"
          class="tab-btn" :class="{ 'tab-btn--active': activeTab === t }" @click="activeTab = t">
          {{ t.charAt(0).toUpperCase() + t.slice(1) }}
        </button>
      </div>

      <div class="settings-body">

        <!-- ── GENERAL ─────────────────────────────── -->
        <template v-if="activeTab === 'general'">
          <div class="section">
            <h2 class="section-title">Business Info</h2>
            <div class="form-grid">
              <div class="field">
                <label>
                  Business Name
                  <span class="tip" data-tip="Your dealership name — shown in the footer, browser tab, and outgoing emails.">ⓘ</span>
                </label>
                <input v-model="general.businessName" type="text" />
              </div>
              <div class="field">
                <label>
                  Tagline
                  <span class="tip" data-tip="A short slogan displayed under your name in the footer (e.g. 'Your Powersports Destination').">ⓘ</span>
                </label>
                <input v-model="general.tagline" type="text" />
              </div>
              <div class="field">
                <label>
                  Phone
                  <span class="tip" data-tip="Main contact number — shown on the Contact page and in the footer.">ⓘ</span>
                </label>
                <input v-model="general.phone" type="tel" />
              </div>
              <div class="field">
                <label>
                  Email
                  <span class="tip" data-tip="Public contact email — shown on the Contact page and in the footer.">ⓘ</span>
                </label>
                <input v-model="general.email" type="email" />
              </div>
              <div class="field field--full">
                <label>
                  Address
                  <span class="tip" data-tip="Physical store address — shown on the Contact page and in the footer.">ⓘ</span>
                </label>
                <input v-model="general.address" type="text" />
              </div>
              <div class="field field--full">
                <label>
                  Business Hours
                  <span class="tip" data-tip="Opening hours shown on the Contact page. Each line is a separate row (e.g. Mon–Fri: 9am – 6pm).">ⓘ</span>
                </label>
                <textarea v-model="general.hours" rows="4" placeholder="Mon–Fri: 9am – 6pm&#10;Sat: 9am – 4pm&#10;Sun: Closed" />
              </div>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">Social Links</h2>
            <div class="form-grid">
              <div class="field">
                <label>
                  Facebook URL
                  <span class="tip" data-tip="Full URL to your Facebook page. When set, a Facebook link appears in the footer.">ⓘ</span>
                </label>
                <input v-model="general.socials.facebook" type="url" placeholder="https://facebook.com/..." />
              </div>
              <div class="field">
                <label>
                  Instagram URL
                  <span class="tip" data-tip="Full URL to your Instagram profile. When set, an Instagram link appears in the footer.">ⓘ</span>
                </label>
                <input v-model="general.socials.instagram" type="url" placeholder="https://instagram.com/..." />
              </div>
              <div class="field">
                <label>
                  TikTok URL
                  <span class="tip" data-tip="Full URL to your TikTok profile. When set, a TikTok link appears in the footer.">ⓘ</span>
                </label>
                <input v-model="general.socials.tiktok" type="url" placeholder="https://tiktok.com/..." />
              </div>
              <div class="field">
                <label>
                  YouTube URL
                  <span class="tip" data-tip="Full URL to your YouTube channel. When set, a YouTube link appears in the footer.">ⓘ</span>
                </label>
                <input v-model="general.socials.youtube" type="url" placeholder="https://youtube.com/..." />
              </div>
            </div>
          </div>

          <div class="section">
            <div class="section-title-row">
              <h2 class="section-title">
                Navigation Links
                <span class="tip" data-tip="Links shown in the header navbar and footer Quick Links column. Drag the ↑↓ buttons to reorder.">ⓘ</span>
              </h2>
              <button class="add-btn" @click="addNavLink">+ Add Link</button>
            </div>
            <p class="section-note">These replace the default nav links (Home, Inventory, About, Contact). Leave empty to use defaults.</p>
            <div v-if="general.navLinks.length === 0" class="empty-list">Using defaults — add links above to override.</div>
            <div v-for="(link, i) in general.navLinks" :key="i" class="nav-link-row">
              <div class="nav-link-row__order">
                <button class="order-btn" :disabled="i === 0" @click="moveNavLink(i, i - 1)" title="Move up">↑</button>
                <button class="order-btn" :disabled="i === general.navLinks.length - 1" @click="moveNavLink(i, i + 1)" title="Move down">↓</button>
              </div>
              <input v-model="link.name" type="text" placeholder="Label (e.g. Home)" class="nav-link-row__name" />
              <input v-model="link.to" type="text" placeholder="Path (e.g. /inventory)" class="nav-link-row__path" />
              <button class="remove-btn" @click="removeNavLink(i)">Remove</button>
            </div>
          </div>
        </template>

        <!-- ── PAGES ──────────────────────────────── -->
        <template v-if="activeTab === 'pages'">
          <div class="section">
            <h2 class="section-title">Home Page</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label>
                  Hero Title
                  <span class="tip" data-tip="The large heading visitors see first on the home page. Wrap words in &lt;span&gt; tags to highlight them in red (e.g. Your &lt;span&gt;Powersports&lt;/span&gt; Destination).">ⓘ</span>
                </label>
                <input v-model="pages.home.heroTitle" type="text" placeholder="Your Powersports Destination" />
              </div>
              <div class="field field--full">
                <label>
                  Hero Subtitle
                  <span class="tip" data-tip="Smaller descriptive text shown below the hero heading.">ⓘ</span>
                </label>
                <input v-model="pages.home.heroSubtitle" type="text" />
              </div>
              <div class="field">
                <label>
                  Primary CTA Text
                  <span class="tip" data-tip="Label on the main action button in the hero section (e.g. 'Browse Inventory').">ⓘ</span>
                </label>
                <input v-model="pages.home.ctaPrimaryText" type="text" placeholder="Browse Inventory" />
              </div>
              <div class="field">
                <label>
                  Primary CTA Link
                  <span class="tip" data-tip="Page the primary button links to (e.g. /inventory).">ⓘ</span>
                </label>
                <input v-model="pages.home.ctaPrimaryLink" type="text" placeholder="/inventory" />
              </div>
              <div class="field">
                <label>
                  Secondary CTA Text
                  <span class="tip" data-tip="Label on the second button in the hero section (e.g. 'Contact Us').">ⓘ</span>
                </label>
                <input v-model="pages.home.ctaSecondaryText" type="text" placeholder="Contact Us" />
              </div>
              <div class="field">
                <label>
                  Secondary CTA Link
                  <span class="tip" data-tip="Page the secondary button links to (e.g. /contact).">ⓘ</span>
                </label>
                <input v-model="pages.home.ctaSecondaryLink" type="text" placeholder="/contact" />
              </div>
              <div class="field">
                <label>
                  Featured Section Title
                  <span class="tip" data-tip="Heading displayed above the featured vehicles grid on the home page.">ⓘ</span>
                </label>
                <input v-model="pages.home.featuredTitle" type="text" placeholder="Featured Vehicles" />
              </div>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">About Page</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label>
                  Page Title
                  <span class="tip" data-tip="Main heading at the top of the About page.">ⓘ</span>
                </label>
                <input v-model="pages.about.title" type="text" />
              </div>
              <div class="field field--full">
                <label>
                  Lead Text
                  <span class="tip" data-tip="Bold introductory paragraph shown directly below the page title.">ⓘ</span>
                </label>
                <textarea v-model="pages.about.lead" rows="2" />
              </div>
              <div class="field field--full">
                <label>
                  Body Text
                  <span class="tip" data-tip="Main content of the About page. Separate paragraphs with a blank line — each block becomes its own paragraph.">ⓘ</span>
                </label>
                <textarea v-model="pages.about.body" rows="6" />
              </div>
              <div class="field">
                <label>
                  CTA Button Text
                  <span class="tip" data-tip="Label on the call-to-action button at the bottom of the About page.">ⓘ</span>
                </label>
                <input v-model="pages.about.ctaText" type="text" />
              </div>
              <div class="field">
                <label>
                  CTA Button Link
                  <span class="tip" data-tip="Where the About page CTA button links to (e.g. /contact).">ⓘ</span>
                </label>
                <input v-model="pages.about.ctaLink" type="text" />
              </div>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">Contact Page</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label>
                  Page Title
                  <span class="tip" data-tip="Main heading shown at the top of the Contact page.">ⓘ</span>
                </label>
                <input v-model="pages.contact.title" type="text" />
              </div>
              <div class="field field--full">
                <label>
                  Intro Text
                  <span class="tip" data-tip="Short subheading below the Contact page title — tells visitors what to expect.">ⓘ</span>
                </label>
                <textarea v-model="pages.contact.intro" rows="3" />
              </div>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">SEO</h2>
            <p class="section-note">Meta titles and descriptions control how your pages appear in Google search results.</p>
            <div class="seo-grid">
              <template v-for="p in (['home','about','contact','inventory'] as const)" :key="p">
                <div class="seo-page">
                  <h3 class="seo-page-title">{{ p.charAt(0).toUpperCase() + p.slice(1) }}</h3>
                  <div class="field">
                    <label>
                      Meta Title
                      <span class="tip" data-tip="Appears in the browser tab and as the blue link title in Google search results.">ⓘ</span>
                    </label>
                    <input v-model="(pages.seo as Record<string, string>)[`${p}Title`]" type="text" />
                  </div>
                  <div class="field">
                    <label>
                      Meta Description
                      <span class="tip" data-tip="Short summary shown under the title in Google results. Keep under 160 characters.">ⓘ</span>
                    </label>
                    <textarea v-model="(pages.seo as Record<string, string>)[`${p}Description`]" rows="2" />
                  </div>
                </div>
              </template>
            </div>
          </div>
        </template>

        <!-- ── CONTENT ─────────────────────────────── -->
        <template v-if="activeTab === 'content'">
          <div class="section">
            <h2 class="section-title">Announcement Bar</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label class="toggle-label">
                  <span>
                    Enable Announcement Bar
                    <span class="tip" data-tip="When on, a colored banner appears at the very top of every public page.">ⓘ</span>
                  </span>
                  <div class="toggle" :class="{ 'toggle--on': content.announcementBar.enabled }"
                    @click="content.announcementBar.enabled = !content.announcementBar.enabled">
                    <div class="toggle__thumb" />
                  </div>
                </label>
              </div>
              <div class="field field--full">
                <label>
                  Message
                  <span class="tip" data-tip="The text displayed inside the announcement bar. Emojis are supported.">ⓘ</span>
                </label>
                <input v-model="content.announcementBar.text" type="text"
                  placeholder="🎉 Summer Sale — Up to 15% off select models!" />
              </div>
              <div class="field">
                <label>
                  Background Color
                  <span class="tip" data-tip="Background color of the announcement bar.">ⓘ</span>
                </label>
                <div class="color-row">
                  <input type="color" v-model="content.announcementBar.bgColor" class="color-picker" />
                  <input v-model="content.announcementBar.bgColor" type="text" class="color-text" />
                </div>
              </div>
              <div class="field">
                <label>
                  Text Color
                  <span class="tip" data-tip="Color of the text inside the announcement bar.">ⓘ</span>
                </label>
                <div class="color-row">
                  <input type="color" v-model="content.announcementBar.textColor" class="color-picker" />
                  <input v-model="content.announcementBar.textColor" type="text" class="color-text" />
                </div>
              </div>
              <div v-if="content.announcementBar.enabled" class="field field--full">
                <label>Preview</label>
                <div class="announcement-preview"
                  :style="{ background: content.announcementBar.bgColor, color: content.announcementBar.textColor }">
                  {{ content.announcementBar.text || 'Your announcement will appear here' }}
                </div>
              </div>
            </div>
          </div>

          <div class="section">
            <div class="section-title-row">
              <h2 class="section-title">
                Testimonials
                <span class="tip" data-tip="Customer reviews shown in a grid on the About page. Add as many as you like.">ⓘ</span>
              </h2>
              <button class="add-btn" @click="addTestimonial">+ Add</button>
            </div>
            <div v-if="content.testimonials.length === 0" class="empty-list">No testimonials yet.</div>
            <div v-for="(t, i) in content.testimonials" :key="i" class="list-card">
              <div class="list-card__top">
                <span class="list-card__num">#{{ i + 1 }}</span>
                <button class="remove-btn" @click="removeTestimonial(i)">Remove</button>
              </div>
              <div class="form-grid">
                <div class="field">
                  <label>
                    Customer Name
                    <span class="tip" data-tip="The customer's name shown below their review (e.g. John D.).">ⓘ</span>
                  </label>
                  <input v-model="t.name" type="text" placeholder="John D." />
                </div>
                <div class="field">
                  <label>
                    Rating
                    <span class="tip" data-tip="Star rating out of 5, displayed above the review text.">ⓘ</span>
                  </label>
                  <div class="star-row">
                    <button v-for="s in 5" :key="s" class="star" :class="{ 'star--on': s <= t.rating }"
                      @click="t.rating = s">★</button>
                  </div>
                </div>
                <div class="field field--full">
                  <label>
                    Review
                    <span class="tip" data-tip="The customer's review text, shown in quotes on the About page.">ⓘ</span>
                  </label>
                  <textarea v-model="t.text" rows="3" placeholder="Great experience!" />
                </div>
              </div>
            </div>
          </div>

          <div class="section">
            <div class="section-title-row">
              <h2 class="section-title">
                FAQs
                <span class="tip" data-tip="Frequently asked questions shown as an expandable list on the About page.">ⓘ</span>
              </h2>
              <button class="add-btn" @click="addFaq">+ Add</button>
            </div>
            <div v-if="content.faqs.length === 0" class="empty-list">No FAQs yet.</div>
            <div v-for="(f, i) in content.faqs" :key="i" class="list-card">
              <div class="list-card__top">
                <span class="list-card__num">FAQ #{{ i + 1 }}</span>
                <button class="remove-btn" @click="removeFaq(i)">Remove</button>
              </div>
              <div class="form-grid">
                <div class="field field--full">
                  <label>
                    Question
                    <span class="tip" data-tip="The question text shown in the collapsed FAQ row.">ⓘ</span>
                  </label>
                  <input v-model="f.question" type="text" placeholder="Do you offer financing?" />
                </div>
                <div class="field field--full">
                  <label>
                    Answer
                    <span class="tip" data-tip="The answer revealed when a visitor clicks the question.">ⓘ</span>
                  </label>
                  <textarea v-model="f.answer" rows="3" />
                </div>
              </div>
            </div>
          </div>

          <div class="section">
            <div class="section-title-row">
              <h2 class="section-title">
                Brands We Carry
                <span class="tip" data-tip="Brand names displayed in the strip on the homepage between the hero and featured vehicles.">ⓘ</span>
              </h2>
            </div>
            <div class="brand-input-row">
              <input v-model="brandNameInput" type="text" placeholder="Brand name (e.g. Vitacci)" @keydown.enter.prevent="addBrand" />
              <input v-model="brandUrlInput" type="url" placeholder="Website URL (optional)" @keydown.enter.prevent="addBrand" />
              <button class="add-btn" @click="addBrand">+ Add</button>
            </div>
            <div v-if="content.brands.length === 0" class="empty-list">No brands yet.</div>
            <div class="chip-list">
              <span v-for="(b, i) in content.brands" :key="i" class="chip">
                <i v-if="b.url" class="pi pi-link" style="font-size:0.7rem;opacity:0.6" />
                {{ b.name }}
                <button class="chip__remove" @click="removeBrand(i)">×</button>
              </span>
            </div>
          </div>
        </template>

        <!-- ── EMAIL ──────────────────────────────── -->
        <template v-if="activeTab === 'email'">
          <div class="section">
            <h2 class="section-title">Notification Recipients</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label>
                  Inquiry Notifications
                  <span class="tip" data-tip="These email addresses receive a notification every time someone submits the contact form. Press Enter or Tab after each address to add it.">ⓘ</span>
                </label>
                <div class="chips-input">
                  <span v-for="(r, i) in email.inquiryRecipients" :key="i" class="chip">
                    {{ r }} <button @click="removeRecipient(email.inquiryRecipients, i)">×</button>
                  </span>
                  <input v-model="inquiryInput" type="email" placeholder="email@example.com"
                    @keydown.enter.prevent="addRecipient(email.inquiryRecipients, inquiryInput as any)"
                    @keydown.tab.prevent="addRecipient(email.inquiryRecipients, inquiryInput as any)" />
                </div>
              </div>
              <div class="field field--full">
                <label>
                  Order Notifications
                  <span class="tip" data-tip="These email addresses receive a notification when a new order is placed. Press Enter or Tab after each address to add it.">ⓘ</span>
                </label>
                <div class="chips-input">
                  <span v-for="(r, i) in email.orderRecipients" :key="i" class="chip">
                    {{ r }} <button @click="removeRecipient(email.orderRecipients, i)">×</button>
                  </span>
                  <input v-model="orderInput" type="email" placeholder="email@example.com"
                    @keydown.enter.prevent="addRecipient(email.orderRecipients, orderInput as any)"
                    @keydown.tab.prevent="addRecipient(email.orderRecipients, orderInput as any)" />
                </div>
              </div>
              <div class="field">
                <label>
                  Reply-From Address
                  <span class="tip" data-tip="The 'From' email address on all outgoing automated emails (e.g. noreply@yourdomain.com).">ⓘ</span>
                </label>
                <input v-model="email.replyFrom" type="email" placeholder="noreply@yourdomain.com" />
              </div>
              <div class="field field--full">
                <label>
                  Email Footer Text
                  <span class="tip" data-tip="Text appended to the bottom of all automated emails — useful for unsubscribe info or legal disclaimers.">ⓘ</span>
                </label>
                <textarea v-model="email.emailFooter" rows="2" />
              </div>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">SMTP Configuration</h2>
            <p class="section-note">Leave blank to use the server's default mail transport.</p>
            <div class="form-grid">
              <div class="field">
                <label>
                  SMTP Host
                  <span class="tip" data-tip="Your mail server hostname (e.g. smtp.gmail.com or mail.yourdomain.com).">ⓘ</span>
                </label>
                <input v-model="email.smtpHost" type="text" placeholder="smtp.gmail.com" />
              </div>
              <div class="field">
                <label>
                  SMTP Port
                  <span class="tip" data-tip="Port used to connect to the mail server. Common values: 587 (TLS), 465 (SSL), 25 (plain).">ⓘ</span>
                </label>
                <input v-model.number="email.smtpPort" type="number" placeholder="587" />
              </div>
              <div class="field">
                <label>
                  Username
                  <span class="tip" data-tip="Login username for your SMTP server — usually your full email address.">ⓘ</span>
                </label>
                <input v-model="email.smtpUser" type="text" />
              </div>
              <div class="field">
                <label>
                  Password
                  <span class="tip" data-tip="Password or app-specific password for your SMTP account.">ⓘ</span>
                </label>
                <input v-model="email.smtpPass" type="password" />
              </div>
              <div class="field">
                <label class="toggle-label">
                  <span>
                    Use SSL/TLS
                    <span class="tip" data-tip="Enable for encrypted connections (recommended). Required on port 465; optional on 587.">ⓘ</span>
                  </span>
                  <div class="toggle" :class="{ 'toggle--on': email.smtpSsl }"
                    @click="email.smtpSsl = !email.smtpSsl">
                    <div class="toggle__thumb" />
                  </div>
                </label>
              </div>
            </div>
          </div>
        </template>

        <!-- ── ADVANCED ────────────────────────────── -->
        <template v-if="activeTab === 'advanced'">
          <div class="section">
            <h2 class="section-title">Analytics &amp; Tracking</h2>
            <div class="form-grid">
              <div class="field">
                <label>
                  Google Analytics ID
                  <span class="tip" data-tip="Your GA4 measurement ID (starts with G-). Enables visitor traffic tracking on the public site.">ⓘ</span>
                </label>
                <input v-model="advanced.googleAnalyticsId" type="text" placeholder="G-XXXXXXXXXX" />
              </div>
              <div class="field">
                <label>
                  Facebook Pixel ID
                  <span class="tip" data-tip="Your Meta/Facebook Pixel ID for tracking ad conversions and retargeting visitors.">ⓘ</span>
                </label>
                <input v-model="advanced.facebookPixelId" type="text" placeholder="1234567890" />
              </div>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">Custom Code</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label>
                  Custom Head Code
                  <span class="tip" data-tip="Raw HTML injected into the &lt;head&gt; of every public page — use for tag managers, custom fonts, or any third-party scripts.">ⓘ</span>
                </label>
                <p class="section-note" style="margin: 0 0 6px">Injected into the &lt;head&gt; of every public page.</p>
                <textarea v-model="advanced.customHeadCode" rows="8" class="mono"
                  placeholder="<!-- Google Tag Manager, custom meta tags, etc. -->" />
              </div>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">Maintenance Mode</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label class="toggle-label">
                  <span>
                    Enable Maintenance Mode
                    <span class="tip" data-tip="When on, all public site visitors see a maintenance message instead of the site. Admin access is unaffected.">ⓘ</span>
                  </span>
                  <div class="toggle" :class="{ 'toggle--on': advanced.maintenanceModeEnabled }"
                    @click="advanced.maintenanceModeEnabled = !advanced.maintenanceModeEnabled">
                    <div class="toggle__thumb" />
                  </div>
                </label>
              </div>
              <div v-if="advanced.maintenanceModeEnabled" class="field field--full">
                <label>
                  Maintenance Message
                  <span class="tip" data-tip="The message shown to visitors while the site is in maintenance mode.">ⓘ</span>
                </label>
                <textarea v-model="advanced.maintenanceModeMessage" rows="3" />
              </div>
            </div>
          </div>
        </template>

        <!-- ── THEME ──────────────────────────────── -->
        <template v-if="activeTab === 'theme'">
          <div class="section">
            <h2 class="section-title">Color Presets</h2>
            <p class="section-note">Pick a preset or choose Custom to set your own accent color. Changes apply to buttons, links, and highlighted elements across the public site.</p>
            <div class="preset-grid">
              <button
                v-for="(p, key) in PRESETS"
                :key="key"
                class="preset-card"
                :class="{ 'preset-card--active': theme.preset === key }"
                @click="applyPreset(key)"
              >
                <div class="preset-card__swatch" :style="{ background: p.primary }" />
                <div class="preset-card__info">
                  <span class="preset-card__name">{{ p.label }}</span>
                  <span class="preset-card__desc">{{ p.description }}</span>
                </div>
                <div v-if="theme.preset === key" class="preset-card__check">✓</div>
              </button>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">
              {{ theme.preset === 'custom' ? 'Custom Color' : 'Active Colors' }}
            </h2>
            <div class="form-grid">
              <div class="field">
                <label>
                  Accent Color
                  <span class="tip" data-tip="The main brand color used for buttons, links, and highlights across the public site.">ⓘ</span>
                </label>
                <div class="color-row">
                  <input type="color" v-model="theme.primary" class="color-picker"
                    :disabled="theme.preset !== 'custom'"
                    @input="onPrimaryChange(theme.primary)" />
                  <input v-model="theme.primary" type="text" class="color-text"
                    :disabled="theme.preset !== 'custom'"
                    @change="onPrimaryChange(theme.primary)" />
                </div>
              </div>
              <div class="field">
                <label>
                  Accent Dark (hover)
                  <span class="tip" data-tip="Darker shade of the accent color used on button hover states. Auto-calculated from the accent color.">ⓘ</span>
                </label>
                <div class="color-row">
                  <input type="color" v-model="theme.primaryDark" class="color-picker"
                    :disabled="theme.preset !== 'custom'" />
                  <input v-model="theme.primaryDark" type="text" class="color-text"
                    :disabled="theme.preset !== 'custom'" />
                </div>
              </div>
            </div>

            <!-- Live preview -->
            <div class="theme-preview">
              <div class="theme-preview__label">Preview</div>
              <div class="theme-preview__bar">
                <div class="theme-preview__btn"
                  :style="{ background: theme.primary }">
                  Browse Inventory
                </div>
                <div class="theme-preview__link" :style="{ color: theme.primary }">
                  View Details →
                </div>
                <div class="theme-preview__badge" :style="{ background: theme.primary }">
                  $12,999
                </div>
              </div>
            </div>
          </div>
        </template>

        <!-- ── LEGAL ──────────────────────────────── -->
        <template v-if="activeTab === 'legal'">
          <p class="section-note" style="margin-top:0">
            Enter the content for each legal page below. You can use basic HTML tags
            (<code>&lt;h2&gt;</code>, <code>&lt;p&gt;</code>, <code>&lt;ul&gt;</code>, <code>&lt;strong&gt;</code>, etc.).
            Leave a page blank to show a "coming soon" placeholder to visitors.
          </p>

          <div class="section">
            <h2 class="section-title">FAQ</h2>
            <p class="section-note">Shown at <strong>/faq</strong> — linked from the footer Customer Service column.</p>
            <div class="field field--full">
              <textarea v-model="legal.faq" rows="14" class="mono"
                placeholder="&lt;h2&gt;Do you offer financing?&lt;/h2&gt;&#10;&lt;p&gt;Yes! We work with several lenders...&lt;/p&gt;" />
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">Shipping &amp; Returns</h2>
            <p class="section-note">Shown at <strong>/shipping-returns</strong>.</p>
            <div class="field field--full">
              <textarea v-model="legal.shipping" rows="14" class="mono"
                placeholder="&lt;h2&gt;Shipping Policy&lt;/h2&gt;&#10;&lt;p&gt;We ship within...&lt;/p&gt;" />
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">Privacy Policy</h2>
            <p class="section-note">Shown at <strong>/privacy-policy</strong>.</p>
            <div class="field field--full">
              <textarea v-model="legal.privacy" rows="14" class="mono"
                placeholder="&lt;h2&gt;Information We Collect&lt;/h2&gt;&#10;&lt;p&gt;We collect...&lt;/p&gt;" />
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">Terms of Service</h2>
            <p class="section-note">Shown at <strong>/terms-of-service</strong>.</p>
            <div class="field field--full">
              <textarea v-model="legal.terms" rows="14" class="mono"
                placeholder="&lt;h2&gt;Acceptance of Terms&lt;/h2&gt;&#10;&lt;p&gt;By using this site...&lt;/p&gt;" />
            </div>
          </div>
        </template>

        <!-- ── AUTH ───────────────────────────────── -->
        <template v-if="activeTab === 'auth'">
          <div class="section">
            <h2 class="section-title">User Registration</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label class="toggle-label">
                  <span>
                    Allow Customer Registration
                    <span class="tip" data-tip="When off, the sign-up page returns an error and no new customer accounts can be created. Existing users are unaffected.">ⓘ</span>
                  </span>
                  <div class="toggle" :class="{ 'toggle--on': auth.enableRegistration }"
                    @click="auth.enableRegistration = !auth.enableRegistration">
                    <div class="toggle__thumb" />
                  </div>
                </label>
              </div>
            </div>
          </div>

          <div class="section">
            <h2 class="section-title">Email Verification</h2>
            <div class="form-grid">
              <div class="field field--full">
                <label class="toggle-label">
                  <span>
                    Require Email Verification on Sign-Up
                    <span class="tip" data-tip="When on, newly registered customers must click a verification link sent to their email before they can log in. Admins can force-verify any account from the Users page.">ⓘ</span>
                  </span>
                  <div class="toggle" :class="{ 'toggle--on': auth.requireEmailVerification }"
                    @click="auth.requireEmailVerification = !auth.requireEmailVerification">
                    <div class="toggle__thumb" />
                  </div>
                </label>
              </div>
              <div v-if="auth.requireEmailVerification" class="field field--full">
                <p class="section-note" style="margin:0;">
                  Verification emails are sent via the SMTP settings configured in the <strong>Email</strong> tab.
                  Make sure SMTP is set up before enabling this option.
                  You can manually verify any account from the <strong>Users</strong> page.
                </p>
              </div>
            </div>
          </div>
        </template>

      </div>
    </div>
  </AdminShell>
</template>

<style scoped>
.settings-page {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 120px);
  overflow: hidden;
}

.settings-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  flex-shrink: 0;
}
.settings-title { font-size: 1.4rem; font-weight: 700; }
.save-btn {
  padding: 9px 24px;
  background: #e63946;
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s;
}
.save-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.save-btn:not(:disabled):hover { opacity: 0.85; }

/* Tab bar */
.tab-bar {
  display: flex;
  gap: 2px;
  background: #111;
  border: 1px solid #222;
  border-radius: 10px 10px 0 0;
  padding: 6px 6px 0;
  flex-shrink: 0;
}
.tab-btn {
  padding: 10px 20px;
  background: transparent;
  border: none;
  color: #555;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  border-radius: 6px 6px 0 0;
  border-bottom: 2px solid transparent;
  transition: color 0.15s, border-color 0.15s;
}
.tab-btn:hover { color: #9a9a9a; }
.tab-btn--active { color: #f0f0f0; border-bottom-color: #e63946; background: #0d0d0d; }

/* Body */
.settings-body {
  flex: 1;
  overflow-y: auto;
  background: #0d0d0d;
  border: 1px solid #222;
  border-top: none;
  border-radius: 0 0 10px 10px;
  padding: 28px 32px;
  display: flex;
  flex-direction: column;
  gap: 32px;
}

/* Section */
.section {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding-bottom: 32px;
  border-bottom: 1px solid #1a1a1a;
}
.section:last-child { border-bottom: none; padding-bottom: 0; }
.section-title { font-size: 0.9rem; font-weight: 700; color: #f0f0f0; text-transform: uppercase; letter-spacing: 0.06em; display: flex; align-items: center; gap: 6px; }
.section-title-row { display: flex; align-items: center; justify-content: space-between; }
.section-note { font-size: 0.8rem; color: #555; margin-top: -8px; }

/* Grid */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}
.field { display: flex; flex-direction: column; gap: 6px; }
.field--full { grid-column: 1 / -1; }
.field label {
  font-size: 0.75rem;
  color: #777;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 5px;
}
.field-hint { font-size: 0.7rem; color: #444; font-weight: 400; }

input[type="text"],
input[type="email"],
input[type="tel"],
input[type="url"],
input[type="password"],
input[type="number"],
textarea {
  background: #111;
  border: 1px solid #2a2a2a;
  border-radius: 8px;
  padding: 9px 12px;
  color: #f0f0f0;
  font-size: 0.875rem;
  font-family: inherit;
  resize: vertical;
  transition: border-color 0.15s;
}
input:focus, textarea:focus { outline: none; border-color: #e63946; }
textarea.mono { font-family: 'Courier New', monospace; font-size: 0.8rem; }

/* Toggle */
.toggle-label { display: flex; align-items: center; justify-content: space-between; cursor: pointer; }
.toggle {
  width: 40px; height: 22px; border-radius: 999px; background: #2a2a2a;
  position: relative; cursor: pointer; transition: background 0.2s; flex-shrink: 0;
}
.toggle--on { background: #e63946; }
.toggle__thumb {
  position: absolute; top: 3px; left: 3px;
  width: 16px; height: 16px; border-radius: 50%; background: #fff;
  transition: transform 0.2s;
}
.toggle--on .toggle__thumb { transform: translateX(18px); }

/* Color picker */
.color-row { display: flex; align-items: center; gap: 8px; }
.color-picker { width: 40px; height: 36px; border-radius: 6px; border: 1px solid #2a2a2a; cursor: pointer; padding: 2px; background: #111; }
.color-text { flex: 1; }

/* Announcement preview */
.announcement-preview {
  padding: 10px 16px;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 600;
  text-align: center;
}

/* List cards (testimonials, faqs) */
.empty-list { font-size: 0.8rem; color: #444; text-align: center; padding: 16px 0; }
.list-card {
  background: #111;
  border: 1px solid #222;
  border-radius: 10px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.list-card__top { display: flex; align-items: center; justify-content: space-between; }
.list-card__num { font-size: 0.75rem; color: #555; font-weight: 600; }
.add-btn {
  padding: 6px 14px; background: transparent; border: 1px solid #2a2a2a; border-radius: 6px;
  color: #9a9a9a; font-size: 0.8rem; cursor: pointer; transition: border-color 0.15s, color 0.15s;
}
.add-btn:hover { border-color: #e63946; color: #e63946; }
.remove-btn {
  padding: 4px 10px; background: transparent; border: 1px solid #2a2a2a; border-radius: 5px;
  color: #555; font-size: 0.75rem; cursor: pointer; transition: border-color 0.15s, color 0.15s;
}
.remove-btn:hover { border-color: #ef4444; color: #ef4444; }

/* Stars */
.star-row { display: flex; gap: 4px; padding-top: 4px; }
.star { background: none; border: none; font-size: 1.4rem; color: #333; cursor: pointer; padding: 0; transition: color 0.1s; }
.star--on { color: #f59e0b; }

/* Email chips */
.chips-input {
  display: flex; flex-wrap: wrap; gap: 6px; align-items: center;
  background: #111; border: 1px solid #2a2a2a; border-radius: 8px;
  padding: 6px 10px; min-height: 42px; cursor: text;
}
.chips-input:focus-within { border-color: #e63946; }
.chip {
  display: flex; align-items: center; gap: 4px;
  background: #1e1e1e; border: 1px solid #333; border-radius: 999px;
  padding: 2px 10px 2px 12px; font-size: 0.8rem; color: #d0d0d0;
}
.chip button { background: none; border: none; color: #555; cursor: pointer; font-size: 1rem; line-height: 1; padding: 0; }
.chip button:hover { color: #ef4444; }
.chips-input input { background: none; border: none; outline: none; color: #f0f0f0; font-size: 0.875rem; flex: 1; min-width: 160px; }

/* Brand chip input */
.brand-input-row {
  display: flex; gap: 8px; align-items: center; margin-bottom: 12px; flex-wrap: wrap;
}
.brand-input-row input {
  flex: 1; min-width: 140px; background: #111; border: 1px solid #2a2a2a; border-radius: 8px;
  padding: 8px 12px; color: #f0f0f0; font-size: 0.875rem;
}
.brand-input-row input:focus { outline: none; border-color: #e63946; }
.chip-list { display: flex; flex-wrap: wrap; gap: 8px; }
.chip__remove { background: none; border: none; color: #555; cursor: pointer; font-size: 1rem; line-height: 1; padding: 0; }
.chip__remove:hover { color: #ef4444; }

/* SEO grid */
.seo-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.seo-page { display: flex; flex-direction: column; gap: 10px; background: #111; border: 1px solid #1e1e1e; border-radius: 8px; padding: 14px; }
.seo-page-title { font-size: 0.78rem; font-weight: 700; color: #777; text-transform: uppercase; letter-spacing: 0.05em; }

/* ── Tooltip ──────────────────────────────────── */
.tip {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  background: #2a2a2a;
  color: #666;
  font-size: 0.6rem;
  font-weight: 700;
  cursor: default;
  position: relative;
  flex-shrink: 0;
  user-select: none;
}
.tip:hover { background: #333; color: #aaa; }

.tip::after {
  content: attr(data-tip);
  position: absolute;
  bottom: calc(100% + 7px);
  left: 50%;
  transform: translateX(-50%);
  background: #1e1e1e;
  border: 1px solid #333;
  color: #ccc;
  font-size: 0.72rem;
  font-weight: 400;
  line-height: 1.5;
  padding: 7px 10px;
  border-radius: 6px;
  white-space: normal;
  width: 240px;
  pointer-events: none;
  opacity: 0;
  transition: opacity 0.15s;
  z-index: 100;
  box-shadow: 0 4px 16px rgba(0,0,0,0.4);
  text-transform: none;
  letter-spacing: 0;
}

.tip::before {
  content: '';
  position: absolute;
  bottom: calc(100% + 1px);
  left: 50%;
  transform: translateX(-50%);
  border: 5px solid transparent;
  border-top-color: #333;
  pointer-events: none;
  opacity: 0;
  transition: opacity 0.15s;
  z-index: 101;
}

.tip:hover::after,
.tip:hover::before {
  opacity: 1;
}

/* Nav link editor rows */
.nav-link-row {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #111;
  border: 1px solid #222;
  border-radius: 8px;
  padding: 10px 12px;
}
.nav-link-row__order { display: flex; flex-direction: column; gap: 2px; flex-shrink: 0; }
.order-btn {
  background: none; border: 1px solid #2a2a2a; border-radius: 4px;
  color: #555; font-size: 0.75rem; cursor: pointer; padding: 1px 5px;
  line-height: 1.4; transition: color 0.15s, border-color 0.15s;
}
.order-btn:not(:disabled):hover { border-color: #e63946; color: #e63946; }
.order-btn:disabled { opacity: 0.25; cursor: not-allowed; }
.nav-link-row__name {
  flex: 1; min-width: 0;
  background: #0d0d0d; border: 1px solid #2a2a2a; border-radius: 6px;
  padding: 7px 10px; color: #f0f0f0; font-size: 0.875rem; font-family: inherit;
}
.nav-link-row__path {
  flex: 2; min-width: 0;
  background: #0d0d0d; border: 1px solid #2a2a2a; border-radius: 6px;
  padding: 7px 10px; color: #9ca3af; font-size: 0.875rem; font-family: monospace;
}
.nav-link-row__name:focus,
.nav-link-row__path:focus { outline: none; border-color: #e63946; }

/* ── Theme presets ─────────────────────────────── */
.preset-grid {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.preset-card {
  display: flex;
  align-items: center;
  gap: 14px;
  background: #111;
  border: 1px solid #222;
  border-radius: 10px;
  padding: 14px 16px;
  cursor: pointer;
  text-align: left;
  transition: border-color 0.15s;
  position: relative;
}
.preset-card:hover { border-color: #444; }
.preset-card--active { border-color: #e63946; background: #110a0b; }
.preset-card__swatch {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  flex-shrink: 0;
  border: 1px solid rgba(255,255,255,0.08);
}
.preset-card__info { display: flex; flex-direction: column; gap: 2px; flex: 1; }
.preset-card__name { font-size: 0.875rem; font-weight: 700; color: #f0f0f0; }
.preset-card__desc { font-size: 0.75rem; color: #555; }
.preset-card__check {
  font-size: 0.9rem;
  color: #e63946;
  font-weight: 800;
  flex-shrink: 0;
}

/* Theme preview */
.theme-preview {
  margin-top: 8px;
  background: #111;
  border: 1px solid #1e1e1e;
  border-radius: 10px;
  padding: 16px 20px;
}
.theme-preview__label { font-size: 0.7rem; color: #444; font-weight: 600; text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 12px; }
.theme-preview__bar { display: flex; align-items: center; gap: 16px; flex-wrap: wrap; }
.theme-preview__btn {
  padding: 9px 20px;
  color: #fff;
  border-radius: 7px;
  font-size: 0.875rem;
  font-weight: 700;
  transition: background 0.2s;
}
.theme-preview__link { font-size: 0.9rem; font-weight: 600; }
.theme-preview__badge {
  color: #fff;
  font-size: 0.85rem;
  font-weight: 700;
  padding: 4px 12px;
  border-radius: 999px;
}
input:disabled { opacity: 0.4; cursor: not-allowed; }
</style>
