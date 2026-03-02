<template>
  <footer class="footer">
    <div class="footer-container">
      <!-- Unified Section Columns -->
      <div class="footer-content">
        <!-- Company Info Section -->
        <div class="footer-section">
          <h3 class="footer-title">{{ getSetting('site_name', 'Powersports Gear & Vehicles') }}</h3>
          <p class="footer-description">
            {{ getSetting('site_tagline', 'Your Ultimate Powersports Destination') }}
          </p>
          <div class="logo-placeholder">🏍️</div>
        </div>

        <!-- Quick Links Section -->
        <div class="footer-section">
          <h4 class="footer-subtitle">Quick Links</h4>
          <ul class="footer-links">
            <li>
              <router-link to="/" class="footer-link">Home</router-link>
            </li>
            <li>
              <router-link to="/products" class="footer-link">All Products</router-link>
            </li>
            <li>
              <router-link to="/about" class="footer-link">About Us</router-link>
            </li>
            <li>
              <router-link to="/contact" class="footer-link">Contact</router-link>
            </li>
          </ul>
        </div>

        <!-- Customer Service Section -->
        <div class="footer-section">
          <h4 class="footer-subtitle">Customer Service</h4>
          <ul class="footer-links">
            <li>
              <router-link to="/faq" class="footer-link">FAQ</router-link>
            </li>
            <li>
              <router-link to="/shipping-returns" class="footer-link">Shipping & Returns</router-link>
            </li>
            <li>
              <router-link to="/privacy-policy" class="footer-link">Privacy Policy</router-link>
            </li>
            <li>
              <router-link to="/terms-of-service" class="footer-link">Terms of Service</router-link>
            </li>
          </ul>
        </div>

        <!-- Contact Info Section -->
        <div class="footer-section">
          <h4 class="footer-subtitle">Contact Info</h4>
          <div class="contact-info">
            <p class="contact-item" v-if="getSetting('contact_phone')">
              <span class="contact-icon">📞</span>
              {{ getSetting('contact_phone', '(555) 123-4567') }}
            </p>
            <p class="contact-item" v-if="getSetting('contact_email')">
              <span class="contact-icon">✉️</span>
              {{ getSetting('contact_email', 'info@powersportsshowcase.com') }}
            </p>
            <p class="contact-item" v-if="getSetting('contact_address')">
              <span class="contact-icon">📍</span>
              {{ getSetting('contact_address', '123 Adventure Way, Motorville, MV 12345') }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Bottom Bar -->
    <div class="footer-bottom">
      <div class="footer-container">
        <div class="bottom-content">
          <p class="copyright">
            © {{ currentYear }} {{ getSetting('site_name', 'Powersports Gear & Vehicles') }}. All rights reserved.
          </p>
          <div class="social-links">
            <a v-if="getSetting('facebook_url')" :href="getSetting('facebook_url')" target="_blank" class="social-link" aria-label="Facebook">📘</a>
            <a v-if="getSetting('instagram_url')" :href="getSetting('instagram_url')" target="_blank" class="social-link" aria-label="Instagram">📷</a>
            <a v-if="getSetting('youtube_url')" :href="getSetting('youtube_url')" target="_blank" class="social-link" aria-label="YouTube">📺</a>
            <a v-if="getSetting('twitter_url')" :href="getSetting('twitter_url')" target="_blank" class="social-link" aria-label="Twitter">🐦</a>
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useSettings } from '@/composables/useSettings';

// Get current year for copyright
const currentYear = computed(() => new Date().getFullYear());
const { getSetting } = useSettings();
</script>

<style scoped>
.footer {
  background-color: #1a1a1a;
  color: white;
  margin-top: auto;
}

.footer-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

/* Content Grid */
.footer-content {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  gap: 2rem;
  align-items: start;
}

.footer-section {
  /* Each section contains its heading and content together */
}

.footer-title {
  color: #ff6b35;
  font-size: 1.3rem;
  margin: 0 0 1rem 0;
  font-weight: bold;
}

.footer-subtitle {
  color: white;
  font-size: 1.1rem;
  margin: 0 0 1rem 0;
  font-weight: 600;
}

.footer-description {
  color: #cccccc;
  line-height: 1.5;
  margin-bottom: 0.75rem;
  font-size: 0.9rem;
}

.logo-placeholder {
  font-size: 1.75rem;
  margin-top: 0.5rem;
}

.footer-links {
  list-style: none;
  padding: 0;
  margin: 0;
}

.footer-links li {
  margin-bottom: 0.5rem;
}

.footer-link {
  color: #cccccc;
  text-decoration: none;
  transition: color 0.3s ease;
  font-size: 0.95rem;
}

.footer-link:hover {
  color: #ff6b35;
}

.contact-info {
  color: #cccccc;
}

.contact-item {
  display: flex;
  align-items: center;
  margin-bottom: 0.6rem;
  line-height: 1.4;
  font-size: 0.9rem;
}

.contact-icon {
  margin-right: 0.5rem;
  font-size: 1rem;
  width: 1.25rem;
  display: inline-block;
}

.footer-bottom {
  border-top: 1px solid #333;
  padding: 1rem 0 0 0;
  margin-top: 1.5rem;
}

.bottom-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.copyright {
  color: #888;
  margin: 0;
  font-size: 0.875rem;
}

.social-links {
  display: flex;
  gap: 1rem;
}

.social-link {
  display: inline-block;
  font-size: 1.3rem;
  text-decoration: none;
  transition: transform 0.3s ease;
}

.social-link:hover {
  transform: translateY(-2px);
}

/* Desktop layout */
@media (min-width: 769px) {
  .footer-content {
    grid-template-columns: 2fr 1fr 1fr 1fr;
  }
}

/* Tablet and mobile responsive */
@media (max-width: 768px) {
  .footer-content {
    grid-template-columns: 1fr;
    gap: 2rem;
  }

  .footer-container {
    padding: 1.5rem 1rem;
  }

  .bottom-content {
    flex-direction: column;
    text-align: center;
  }

  .social-links {
    justify-content: center;
  }

  .footer-title {
    font-size: 1.2rem;
  }

  .footer-subtitle {
    font-size: 1.05rem;
  }

  .contact-item {
    font-size: 0.85rem;
  }

  /* Center align mobile sections */
  .footer-section {
    text-align: center;
  }
  
  .contact-item {
    justify-content: center;
  }
}
</style>