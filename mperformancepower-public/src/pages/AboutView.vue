<script setup lang="ts">
import { computed } from 'vue'
import { useSettings } from '@/composables/useSettings'

const { settings } = useSettings()

const about = computed(() => settings.value?.pages?.about)
const testimonials = computed(() => settings.value?.content?.testimonials ?? [])
const faqs = computed(() => settings.value?.content?.faqs ?? [])

const bodyParagraphs = computed(() => {
  const body = about.value?.body || ''
  if (!body) {
    return [
      "At M Performance Power, we believe the outdoors should be experienced at full throttle. Whether you're hitting the trails, tackling the track, or cruising the road, we have the machine for you.",
      "Our team is passionate about powersports and ready to help you find the perfect ride. Come visit us — or browse our online inventory and send us an inquiry today.",
    ]
  }
  return body.split(/\n\n+/).filter(p => p.trim())
})
</script>

<template>
  <div class="about container">
    <h1>{{ about?.title || 'About M Performance Power' }}</h1>
    <p class="about__lead">
      {{ about?.lead || "We're your local powersports experts — selling ATVs, UTVs, dirt bikes, motorcycles, snowmobiles, and more." }}
    </p>
    <div class="about__body">
      <p v-for="(para, i) in bodyParagraphs" :key="i">{{ para }}</p>
    </div>
    <RouterLink :to="about?.ctaLink || '/contact'" class="btn-primary">
      {{ about?.ctaText || 'Get In Touch' }}
    </RouterLink>

    <!-- Testimonials -->
    <section v-if="testimonials.length > 0" class="testimonials">
      <h2 class="testimonials__title">What Our Customers Say</h2>
      <div class="testimonials__grid">
        <div v-for="(t, i) in testimonials" :key="i" class="testimonial-card">
          <div class="testimonial-card__stars">
            <span v-for="s in 5" :key="s" :class="s <= t.rating ? 'star star--on' : 'star'">★</span>
          </div>
          <p class="testimonial-card__text">"{{ t.text }}"</p>
          <p class="testimonial-card__name">— {{ t.name }}</p>
        </div>
      </div>
    </section>

    <!-- FAQs -->
    <section v-if="faqs.length > 0" class="faqs">
      <h2 class="faqs__title">Frequently Asked Questions</h2>
      <div class="faqs__list">
        <details v-for="(f, i) in faqs" :key="i" class="faq-item">
          <summary class="faq-item__question">{{ f.question }}</summary>
          <p class="faq-item__answer">{{ f.answer }}</p>
        </details>
      </div>
    </section>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;

.about {
  padding: $space-2xl 0;
  max-width: 720px;

  h1 { font-size: 2.5rem; font-weight: 700; margin-bottom: $space-md; }

  &__lead {
    font-size: 1.2rem;
    color: $color-muted;
    margin-bottom: $space-xl;
  }

  &__body {
    display: flex;
    flex-direction: column;
    gap: $space-md;
    margin-bottom: $space-xl;
    color: $color-muted;
    line-height: 1.8;
  }
}

.btn-primary {
  display: inline-block;
  padding: $space-md $space-xl;
  background: var(--color-primary);
  color: #fff;
  font-weight: 600;
  border-radius: $border-radius;
  transition: background 0.2s;

  &:hover { background: var(--color-primary-dark); }
}

.testimonials {
  margin-top: $space-2xl;

  &__title {
    font-size: 1.75rem;
    font-weight: 700;
    margin-bottom: $space-xl;
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: $space-lg;
  }
}

.testimonial-card {
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: $border-radius;
  padding: $space-lg;
  display: flex;
  flex-direction: column;
  gap: $space-sm;

  &__stars {
    display: flex;
    gap: 2px;
  }

  &__text {
    color: $color-muted;
    font-size: 0.95rem;
    line-height: 1.6;
    flex: 1;
  }

  &__name {
    font-size: 0.875rem;
    font-weight: 600;
    color: $color-text;
  }
}

.star { color: #333; font-size: 1rem; }
.star--on { color: #f59e0b; }

.faqs {
  margin-top: $space-2xl;

  &__title {
    font-size: 1.75rem;
    font-weight: 700;
    margin-bottom: $space-xl;
  }

  &__list {
    display: flex;
    flex-direction: column;
    gap: $space-sm;
  }
}

.faq-item {
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: $border-radius;
  overflow: hidden;

  &__question {
    padding: $space-md $space-lg;
    font-weight: 600;
    cursor: pointer;
    user-select: none;
    list-style: none;

    &::-webkit-details-marker { display: none; }
    &::after { content: '+'; float: right; color: var(--color-primary); }
  }

  &[open] &__question::after { content: '−'; }

  &__answer {
    padding: 0 $space-lg $space-md;
    color: $color-muted;
    line-height: 1.7;
    font-size: 0.95rem;
  }
}
</style>
