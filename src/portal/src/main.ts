import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import PortalTheme from './theme'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import 'primeicons/primeicons.css'
import { usePortalAuthStore } from './stores/auth'
import App from './App.vue'
import router from './router'

const app = createApp(App)
const pinia = createPinia()
app.use(pinia)

// Restore session before router navigation guards run
const auth = usePortalAuthStore()
await auth.fetchMe()

app.use(router)
document.documentElement.classList.add('p-dark')
app.use(PrimeVue, {
  theme: {
    preset: PortalTheme,
    options: {
      darkModeSelector: '.p-dark',
      cssLayer: false
    }
  }
})
app.use(ToastService)
app.use(ConfirmationService)

// Click-outside directive for dropdowns
app.directive('click-outside', {
  mounted(el, binding) {
    el._clickOutside = (e: MouseEvent) => { if (!el.contains(e.target as Node)) binding.value() }
    document.addEventListener('mousedown', el._clickOutside)
  },
  unmounted(el) { document.removeEventListener('mousedown', el._clickOutside) },
})

app.mount('#app')
