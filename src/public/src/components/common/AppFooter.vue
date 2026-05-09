<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useSettingsStore } from '../../stores/settings'

const settings = useSettingsStore()

const siteName = computed(() => settings.get('site_name', 'PerformancePower'))
const logoNameParts = computed(() => {
  const name = siteName.value
  const accent = settings.get('site_name_accent', '')
  if (!accent || !name.includes(accent)) return { before: name, accent: '', after: '' }
  const idx = name.indexOf(accent)
  return { before: name.substring(0, idx), accent, after: name.substring(idx + accent.length) }
})

const phone = computed(() => settings.get('contact_phone'))
const email = computed(() => settings.get('contact_email'))
const address = computed(() => settings.get('contact_address'))
const facebook = computed(() => settings.get('social_facebook'))
const instagram = computed(() => settings.get('social_instagram'))
const youtube = computed(() => settings.get('social_youtube'))

const businessHours = computed(() => {
  try {
    const raw = settings.get('business_hours')
    if (!raw || raw === '{}') return null
    return JSON.parse(raw) as Record<string, { open: string; close: string; closed: boolean }>
  } catch { return null }
})

const hoursRows = computed(() => {
  if (!businessHours.value) return []
  return Object.entries(businessHours.value).map(([day, val]) => ({
    day,
    label: val.closed ? 'Closed' : `${val.open} – ${val.close}`,
    closed: val.closed,
  }))
})

const year = new Date().getFullYear()
</script>

<template>
  <footer class="footer">

    <!-- Pre-footer CTA strip -->
    <div class="footer__cta">
      <div class="container footer__cta-inner">
        <div class="footer__cta-text">
          <h3>Ready to Hit the Trail?</h3>
          <p>Browse our full inventory of ATVs, UTVs, dirt bikes, snowmobiles and more.</p>
        </div>
        <div class="footer__cta-actions">
          <RouterLink to="/inventory" class="cta-btn cta-btn--primary">Browse Inventory</RouterLink>
          <RouterLink to="/finance" class="cta-btn cta-btn--ghost">Get Financing</RouterLink>
        </div>
      </div>
    </div>

    <!-- Main footer body -->
    <div class="footer__body">
      <div class="container footer__grid">

        <!-- Brand column -->
        <div class="footer__brand">
          <span class="footer__logo">
            {{ logoNameParts.before }}<span class="accent">{{ logoNameParts.accent }}</span>{{ logoNameParts.after }}
          </span>
          <p class="footer__desc">
            Your local powersports destination. We carry the best ATVs, UTVs, dirt bikes, snowmobiles, and watercraft — plus financing, service, and parts.
          </p>
          <div class="footer__social">
            <a v-if="facebook" :href="facebook" target="_blank" rel="noopener" aria-label="Facebook" class="social-link">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor">
                <path d="M18 2h-3a5 5 0 00-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 011-1h3z"/>
              </svg>
              <span>Facebook</span>
            </a>
            <a v-if="instagram" :href="instagram" target="_blank" rel="noopener" aria-label="Instagram" class="social-link">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="2" y="2" width="20" height="20" rx="5"/>
                <path d="M16 11.37A4 4 0 1112.63 8 4 4 0 0116 11.37z"/>
                <line x1="17.5" y1="6.5" x2="17.51" y2="6.5"/>
              </svg>
              <span>Instagram</span>
            </a>
            <a v-if="youtube" :href="youtube" target="_blank" rel="noopener" aria-label="YouTube" class="social-link">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor">
                <path d="M22.54 6.42a2.78 2.78 0 00-1.95-1.97C18.88 4 12 4 12 4s-6.88 0-8.59.46A2.78 2.78 0 001.46 6.42 29 29 0 001 12a29 29 0 00.46 5.58A2.78 2.78 0 003.41 19.6C5.12 20 12 20 12 20s6.88 0 8.59-.46a2.78 2.78 0 001.95-1.95A29 29 0 0023 12a29 29 0 00-.46-5.58zM9.75 15.02V8.98L15.5 12l-5.75 3.02z"/>
              </svg>
              <span>YouTube</span>
            </a>
          </div>
        </div>

        <!-- Quick links -->
        <div class="footer__col">
          <h4 class="footer__col-title">Shop</h4>
          <ul class="footer__links">
            <li><RouterLink to="/inventory">All Inventory</RouterLink></li>
            <li><RouterLink to="/inventory?type=atv">ATVs</RouterLink></li>
            <li><RouterLink to="/inventory?type=utv">UTVs</RouterLink></li>
            <li><RouterLink to="/inventory?type=dirtbike">Dirt Bikes</RouterLink></li>
            <li><RouterLink to="/inventory?type=snowmobile">Snowmobiles</RouterLink></li>
            <li><RouterLink to="/inventory?type=pwc">Watercraft</RouterLink></li>
            <li><RouterLink to="/inventory?tab=gear">Apparel &amp; Gear</RouterLink></li>
            <li><RouterLink to="/finance">Financing</RouterLink></li>
          </ul>
        </div>

        <!-- Links -->
        <div class="footer__col">
          <h4 class="footer__col-title">Company</h4>
          <ul class="footer__links">
            <li><RouterLink to="/about">About Us</RouterLink></li>
            <li><RouterLink to="/contact">Contact Us</RouterLink></li>
          </ul>
        </div>

        <!-- Contact & hours -->
        <div class="footer__col">
          <h4 class="footer__col-title">Visit Us</h4>
          <ul class="footer__contact-list">
            <li v-if="phone">
              <span class="contact-icon">
                <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07A19.5 19.5 0 013.95 9.67a19.79 19.79 0 01-3.07-8.67A2 2 0 012.88 1h3a2 2 0 012 1.72c.127.96.361 1.903.7 2.81a2 2 0 01-.45 2.11L7.09 8.74a16 16 0 006.29 6.29l1.1-1.1a2 2 0 012.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0122 16.92z"/>
                </svg>
              </span>
              <a :href="`tel:${phone}`">{{ phone }}</a>
            </li>
            <li v-if="email">
              <span class="contact-icon">
                <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
                  <polyline points="22,6 12,13 2,6"/>
                </svg>
              </span>
              <a :href="`mailto:${email}`">{{ email }}</a>
            </li>
            <li v-if="address">
              <span class="contact-icon">
                <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/>
                  <circle cx="12" cy="10" r="3"/>
                </svg>
              </span>
              <span style="white-space: pre-line">{{ address }}</span>
            </li>
          </ul>

          <template v-if="hoursRows.length">
            <h4 class="footer__col-title mt">Hours</h4>
            <ul class="footer__hours">
              <li v-for="row in hoursRows" :key="row.day" :class="{ closed: row.closed }">
                <span class="hours-day">{{ row.day }}</span>
                <span class="hours-val">{{ row.label }}</span>
              </li>
            </ul>
          </template>
        </div>

      </div>
    </div>

    <!-- Bottom bar -->
    <div class="footer__bottom">
      <div class="container footer__bottom-inner">
        <p class="footer__copyright">&copy; {{ year }} {{ siteName }}. All rights reserved.</p>
        <div class="footer__bottom-links">
          <RouterLink to="/contact">Contact</RouterLink>
          <RouterLink to="/about">About</RouterLink>
          <RouterLink to="/tos">Terms of Service</RouterLink>
          <RouterLink to="/privacy">Privacy Policy</RouterLink>
          <a href="/portal/" class="footer__staff-link">Staff Portal</a>
        </div>
      </div>
    </div>

  </footer>
</template>

<style lang="scss" scoped>
@use '../../assets/styles/variables' as *;
@use '../../assets/styles/mixins' as *;

// ── Pre-footer CTA ──────────────────────────────────────────────────────────
.footer__cta {
  background: linear-gradient(135deg, #111 0%, #1a1a1a 100%);
  border-top: 3px solid $primary;
  border-bottom: 1px solid $border;
  padding: $spacing-2xl $spacing-lg;
}

.footer__cta-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $spacing-xl;
  flex-wrap: wrap;
}

.footer__cta-text {
  h3 {
    font-size: $font-size-2xl;
    font-weight: 800;
    margin-bottom: $spacing-xs;
  }
  p {
    color: $text-muted;
    font-size: $font-size-sm;
  }
}

.footer__cta-actions {
  display: flex;
  gap: $spacing-md;
  flex-shrink: 0;
  flex-wrap: wrap;
}

.cta-btn {
  padding: 0.75rem 1.75rem;
  border-radius: $radius-md;
  font-weight: 700;
  font-size: $font-size-sm;
  transition: all $transition-fast;
  white-space: nowrap;

  &--primary {
    background: $primary;
    color: #fff;
    &:hover { background: darken(#e53935, 8%); transform: translateY(-1px); }
  }
  &--ghost {
    border: 1px solid rgba(255,255,255,0.2);
    color: $text;
    &:hover { border-color: $primary; color: $primary; transform: translateY(-1px); }
  }
}

// ── Main body ───────────────────────────────────────────────────────────────
.footer__body {
  background: #0d0d0d;
  padding: $spacing-3xl 0 $spacing-2xl;
}

.footer__grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: $spacing-2xl;

  @media (min-width: $bp-sm) {
    grid-template-columns: 1fr 1fr;
  }

  @media (min-width: $bp-lg) {
    grid-template-columns: 2fr 1fr 1fr 1.5fr;
  }
}

// ── Brand column ────────────────────────────────────────────────────────────
.footer__logo {
  display: block;
  font-size: $font-size-xl;
  font-weight: 800;
  letter-spacing: -0.5px;
  margin-bottom: $spacing-md;
  .accent { color: $primary; }
}

.footer__desc {
  color: $text-muted;
  font-size: $font-size-sm;
  line-height: 1.7;
  margin-bottom: $spacing-lg;
  max-width: 320px;
}

.footer__social {
  display: flex;
  flex-direction: column;
  gap: $spacing-sm;
}

.social-link {
  display: inline-flex;
  align-items: center;
  gap: $spacing-sm;
  color: $text-muted;
  font-size: $font-size-sm;
  font-weight: 500;
  transition: color $transition-fast;
  width: fit-content;

  &:hover { color: $primary; }
}

// ── Nav columns ─────────────────────────────────────────────────────────────
.footer__col-title {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 2px;
  color: $text-muted;
  margin-bottom: $spacing-lg;

  &.mt { margin-top: $spacing-xl; }
}

.footer__links {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.625rem;

  a {
    color: $text-secondary;
    font-size: $font-size-sm;
    transition: color $transition-fast;
    &:hover { color: $primary; }
  }
}

// ── Contact list ────────────────────────────────────────────────────────────
.footer__contact-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: $spacing-md;

  li {
    display: flex;
    align-items: flex-start;
    gap: $spacing-sm;
    color: $text-secondary;
    font-size: $font-size-sm;
  }

  a {
    color: $text-secondary;
    transition: color $transition-fast;
    &:hover { color: $primary; }
  }
}

.contact-icon {
  color: $primary;
  flex-shrink: 0;
  margin-top: 2px;
  display: flex;
  align-items: center;
}

// ── Hours ───────────────────────────────────────────────────────────────────
.footer__hours {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.375rem;

  li {
    display: flex;
    justify-content: space-between;
    gap: $spacing-sm;
    font-size: 0.8rem;
    color: $text-secondary;
  }

  .closed { color: $text-muted; }

  .hours-day { font-weight: 500; }
  .hours-val { color: $text-muted; text-align: right; }
}

// ── Bottom bar ──────────────────────────────────────────────────────────────
.footer__bottom {
  background: #080808;
  border-top: 1px solid $border;
  padding: $spacing-lg;
}

.footer__bottom-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: $spacing-md;
}

.footer__copyright {
  color: $text-muted;
  font-size: $font-size-sm;
}

.footer__bottom-links {
  display: flex;
  align-items: center;
  gap: $spacing-lg;

  a {
    color: $text-muted;
    font-size: $font-size-sm;
    transition: color $transition-fast;
    &:hover { color: $text-secondary; }
  }
}

.footer__staff-link {
  font-size: 0.7rem;
  opacity: 0.4;
  transition: opacity $transition-fast !important;
  &:hover { opacity: 1 !important; }
}
</style>
