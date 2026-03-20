<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useInquiryStore } from '@/stores/inquiry.store'
import type { CreateInquiryDto } from '@/types/inquiry.types'

const props = defineProps<{ vehicleId?: number }>()

const store = useInquiryStore()

const firstName = ref('')
const lastName = ref('')

const form = reactive<CreateInquiryDto>({
  vehicleId: props.vehicleId,
  name: '',
  email: '',
  phone: '',
  message: '',
})

async function handleSubmit() {
  const name = [firstName.value.trim(), lastName.value.trim()].filter(Boolean).join(' ')
  await store.submit({ ...form, name })
}
</script>

<template>
  <div class="inquiry-form">
    <div v-if="store.submitted" class="inquiry-form__success">
      <h3>Thank you!</h3>
      <p>We've received your inquiry and will be in touch shortly.</p>
      <button class="btn-outline" @click="store.reset()">Send Another</button>
    </div>

    <form v-else @submit.prevent="handleSubmit">
      <h3 class="inquiry-form__title">Send Us a Message</h3>

      <div v-if="store.error" class="inquiry-form__error">{{ store.error }}</div>

      <div class="form-row">
        <div class="form-group">
          <label for="inq-fname">First Name *</label>
          <input id="inq-fname" v-model="firstName" type="text" required placeholder="First name" />
        </div>
        <div class="form-group">
          <label for="inq-lname">Last Name</label>
          <input id="inq-lname" v-model="lastName" type="text" placeholder="Last name" />
        </div>
      </div>

      <div class="form-group">
        <label for="inq-email">Email *</label>
        <input id="inq-email" v-model="form.email" type="email" required placeholder="you@example.com" />
      </div>

      <div class="form-group">
        <label for="inq-phone">Phone</label>
        <input id="inq-phone" v-model="form.phone" type="tel" placeholder="(555) 000-0000" />
      </div>

      <div class="form-group">
        <label for="inq-message">Message *</label>
        <textarea id="inq-message" v-model="form.message" required rows="4" placeholder="Tell us how we can help..."></textarea>
      </div>

      <button type="submit" class="btn-primary" :disabled="store.submitting">
        {{ store.submitting ? 'Sending...' : 'Send Message' }}
      </button>
    </form>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;

.inquiry-form {
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: $border-radius;
  padding: $space-lg;

  &__title {
    font-size: 1.1rem;
    font-weight: 600;
    margin-bottom: $space-lg;
  }

  &__error {
    background: rgba(var(--color-primary-rgb), 0.1);
    border: 1px solid var(--color-primary);
    border-radius: $border-radius;
    padding: $space-sm $space-md;
    color: var(--color-primary);
    margin-bottom: $space-md;
    font-size: 0.875rem;
  }

  &__success {
    text-align: center;
    padding: $space-xl;

    h3 { font-size: 1.2rem; margin-bottom: $space-sm; }
    p { color: $color-muted; margin-bottom: $space-lg; }
  }
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: $space-sm;

  .form-group { min-width: 0; }
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: $space-md;

  label {
    font-size: 0.8rem;
    font-weight: 600;
    color: $color-muted;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  input, textarea {
    background: $color-dark;
    border: 1px solid $color-border;
    border-radius: $border-radius;
    padding: $space-sm $space-md;
    color: $color-text;
    font-size: 0.9rem;
    transition: border-color 0.2s;

    &:focus {
      outline: none;
      border-color: var(--color-primary);
    }

    &::placeholder { color: $color-muted; }
  }

  textarea { resize: vertical; }
}

.btn-primary {
  width: 100%;
  padding: $space-md;
  background: var(--color-primary);
  color: #fff;
  font-size: 0.9rem;
  font-weight: 600;
  border-radius: $border-radius;
  transition: background 0.2s;
  border: none;
  cursor: pointer;

  &:hover:not(:disabled) { background: var(--color-primary-dark); }
  &:disabled { opacity: 0.5; cursor: not-allowed; }
}

.btn-outline {
  padding: $space-sm $space-lg;
  border: 1px solid $color-border;
  border-radius: $border-radius;
  color: $color-text;
  background: transparent;
  font-size: 0.875rem;
  cursor: pointer;
  transition: border-color 0.2s;

  &:hover { border-color: var(--color-primary); }
}
</style>
