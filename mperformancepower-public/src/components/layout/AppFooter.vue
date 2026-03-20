<script setup lang="ts">
import { computed } from 'vue'
import { useSettings } from '@/composables/useSettings'

const year = new Date().getFullYear()
const { settings } = useSettings()

const g = computed(() => settings.value?.general)
const businessName = computed(() => g.value?.businessName || 'M Performance Power')
const tagline = computed(() => g.value?.tagline || 'Your Powersports Destination')
const phone = computed(() => g.value?.phone || '701-989-1494')
const email = computed(() => g.value?.email || 'contact@minotperformancepowersports.com')
const address = computed(() => g.value?.address || '6210 HWY 2 E Minot, ND 58701')
const socials = computed(() => g.value?.socials)

const defaultNavLinks = [
  { name: 'Home', to: '/' },
  { name: 'Inventory', to: '/inventory' },
  { name: 'About', to: '/about' },
  { name: 'Contact', to: '/contact' },
]
const navLinks = computed(() => {
  const links = g.value?.navLinks?.filter((l: { name: string; to: string }) => l.name?.trim() && l.to?.trim())
  return links && links.length > 0 ? links : defaultNavLinks
})
</script>

<template>
  <footer class="footer">
    <div class="container footer__grid">
      <div class="footer__col footer__col--brand">
        <div class="footer__logo">{{ businessName }}</div>
        <p class="footer__tagline">{{ tagline }}</p>
        <div v-if="socials" class="footer__socials">
          <a v-if="socials.facebook" :href="socials.facebook" target="_blank" rel="noopener" class="footer__social-link">Facebook</a>
          <a v-if="socials.instagram" :href="socials.instagram" target="_blank" rel="noopener" class="footer__social-link">Instagram</a>
          <a v-if="socials.tiktok" :href="socials.tiktok" target="_blank" rel="noopener" class="footer__social-link">TikTok</a>
          <a v-if="socials.youtube" :href="socials.youtube" target="_blank" rel="noopener" class="footer__social-link">YouTube</a>
        </div>
      </div>

      <div class="footer__col">
        <h4 class="footer__heading">Quick Links</h4>
        <nav class="footer__nav">
          <RouterLink
            v-for="link in navLinks"
            :key="link.to"
            :to="link.to"
          >{{ link.name }}</RouterLink>
        </nav>
      </div>

      <div class="footer__col">
        <h4 class="footer__heading">Customer Service</h4>
        <nav class="footer__nav">
          <RouterLink to="/faq">FAQ</RouterLink>
          <RouterLink to="/shipping-returns">Shipping &amp; Returns</RouterLink>
          <RouterLink to="/privacy-policy">Privacy Policy</RouterLink>
          <RouterLink to="/terms-of-service">Terms of Service</RouterLink>
        </nav>
      </div>

      <div class="footer__col">
        <h4 class="footer__heading">Contact Info</h4>
        <div class="footer__contact">
          <a :href="`tel:+1${phone.replace(/\D/g, '')}`">{{ phone }}</a>
          <a :href="`mailto:${email}`">{{ email }}</a>
          <p>{{ address }}</p>
        </div>
      </div>
    </div>

    <div class="footer__bottom">
      <div class="container">
        <p>&copy; {{ year }} {{ businessName }}. All rights reserved.</p>
      </div>
    </div>
  </footer>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.footer {
  background: #1e2535;
  color: #9ca3af;
  padding-top: 4rem;

  &__grid {
    display: grid;
    grid-template-columns: 1.5fr 1fr 1fr 1.5fr;
    gap: 3rem;
    padding-bottom: 3rem;

    @media (max-width: 1024px) {
      grid-template-columns: 1fr 1fr;
      gap: 2rem;
    }

    @media (max-width: 560px) {
      grid-template-columns: 1fr;
      gap: 2rem;
    }
  }

  &__logo {
    font-size: 1.2rem;
    font-weight: 800;
    color: var(--color-primary);
    margin-bottom: 0.75rem;
    line-height: 1.3;
  }

  &__tagline {
    font-size: 0.9rem;
    line-height: 1.7;
    margin-bottom: 1rem;
  }

  &__socials {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    margin-top: 0.5rem;
  }

  &__social-link {
    font-size: 0.8rem;
    color: #9ca3af;
    transition: color 0.15s;
    &:hover { color: var(--color-primary); }
  }

  &__heading {
    font-size: 1rem;
    font-weight: 700;
    color: $color-text;
    margin-bottom: 1.25rem;
  }

  &__nav {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;

    a {
      font-size: 0.9rem;
      color: #9ca3af;
      transition: color 0.15s;
      &:hover { color: var(--color-primary); }
    }
  }

  &__contact {
    display: flex;
    flex-direction: column;
    gap: 0.875rem;
    font-size: 0.9rem;

    a, p {
      display: flex;
      align-items: flex-start;
      gap: 0.5rem;
      color: #9ca3af;
      line-height: 1.5;
      transition: color 0.15s;
    }

    a:hover { color: var(--color-primary); }
  }

  &__contact-icon {
    flex-shrink: 0;
    margin-top: 1px;
  }

  &__bottom {
    border-top: 1px solid rgba(255, 255, 255, 0.08);
    padding: 1.25rem 0;
    text-align: center;
    font-size: 0.85rem;
  }
}
</style>
