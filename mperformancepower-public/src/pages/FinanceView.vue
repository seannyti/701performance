<script setup lang="ts">
import { useSettings } from '@/composables/useSettings'
const { settings } = useSettings()
</script>

<template>
  <div class="finance container">

    <div class="finance__hero">
      <h1>Financing Made Easy</h1>
      <p>We work with two trusted lenders to get you on the road — or trail — fast. Apply online in minutes and get a decision the same day.</p>
    </div>

    <div class="finance__cards">

      <!-- Synchrony -->
      <div class="finance-card">
        <div class="finance-card__logo-wrap">
          <span class="finance-card__name">Synchrony</span>
        </div>
        <div class="finance-card__body">
          <p>Synchrony offers competitive rates and flexible terms for powersports financing. Whether you're buying new or used, Synchrony makes it simple to get approved.</p>
          <ul class="finance-card__perks">
            <li>Fast online application</li>
            <li>Competitive interest rates</li>
            <li>Flexible repayment terms</li>
            <li>New &amp; used vehicles</li>
          </ul>
        </div>
        <a
          href="https://www.synchronycredit.com"
          target="_blank"
          rel="noopener noreferrer"
          class="btn-apply"
        >
          Apply with Synchrony
        </a>
      </div>

      <!-- Octane -->
      <div class="finance-card">
        <div class="finance-card__logo-wrap">
          <span class="finance-card__name">Octane</span>
        </div>
        <div class="finance-card__body">
          <p>Octane specializes in powersports lending. With a quick pre-qualification that won't affect your credit score, you'll know your options before you even step in the door.</p>
          <ul class="finance-card__perks">
            <li>Soft pull pre-qualification</li>
            <li>Decisions in seconds</li>
            <li>Powersports specialists</li>
            <li>New &amp; used vehicles</li>
          </ul>
        </div>
        <a
          href="https://www.octane.co/apply"
          target="_blank"
          rel="noopener noreferrer"
          class="btn-apply"
        >
          Apply with Octane
        </a>
      </div>

    </div>

    <div class="finance__note">
      <p>Not sure which lender is right for you? <RouterLink to="/contact">Contact us</RouterLink> and we'll help you find the best option for your situation.</p>
    </div>

  </div>
</template>

<style scoped>
.finance {
  padding: 60px 20px;
  max-width: 900px;
}

.finance__hero {
  text-align: center;
  margin-bottom: 52px;
}
.finance__hero h1 {
  font-size: 2.2rem;
  font-weight: 800;
  margin-bottom: 16px;
}
.finance__hero p {
  font-size: 1.05rem;
  color: #9a9a9a;
  max-width: 580px;
  margin: 0 auto;
  line-height: 1.7;
}

.finance__cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin-bottom: 40px;
}

@media (max-width: 680px) {
  .finance__cards { grid-template-columns: 1fr; }
}

.finance-card {
  background: #111;
  border: 1px solid #222;
  border-radius: 14px;
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: 20px;
  transition: border-color 0.2s;
}
.finance-card:hover { border-color: #e63946; }

.finance-card__logo-wrap {
  padding-bottom: 20px;
  border-bottom: 1px solid #1e1e1e;
}
.finance-card__name {
  font-size: 1.4rem;
  font-weight: 800;
  color: #f0f0f0;
  letter-spacing: -0.02em;
}

.finance-card__body {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.finance-card__body p {
  font-size: 0.9rem;
  color: #9a9a9a;
  line-height: 1.7;
}

.finance-card__perks {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.finance-card__perks li {
  font-size: 0.875rem;
  color: #ccc;
  display: flex;
  align-items: center;
  gap: 8px;
}
.finance-card__perks li::before {
  content: '✓';
  color: #e63946;
  font-weight: 700;
  flex-shrink: 0;
}

.btn-apply {
  display: block;
  text-align: center;
  background: #e63946;
  color: #fff;
  font-weight: 700;
  font-size: 0.95rem;
  padding: 14px 20px;
  border-radius: 8px;
  text-decoration: none;
  transition: background 0.2s;
}
.btn-apply:hover { background: #c1121f; }

.finance__note {
  text-align: center;
  font-size: 0.9rem;
  color: #666;
}
.finance__note a {
  color: #e63946;
  text-decoration: none;
}
.finance__note a:hover { text-decoration: underline; }
</style>
