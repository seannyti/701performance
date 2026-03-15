import { createApp } from 'vue'
import { createPinia } from 'pinia'
import Toast, { type PluginOptions } from 'vue-toastification'
import 'vue-toastification/dist/index.css'
import App from './App.vue'
import router from './router'
import './style.css'
import { DEFAULT_TOAST_DURATION_MS } from './constants'
import { setRouterInstance } from './utils/apiClient'

const app = createApp(App)

// Toast notification options
const toastOptions: PluginOptions = {
  position: 'top-right',
  timeout: DEFAULT_TOAST_DURATION_MS,
  closeOnClick: true,
  pauseOnFocusLoss: true,
  pauseOnHover: true,
  draggable: true,
  draggablePercent: 0.6,
  showCloseButtonOnHover: false,
  hideProgressBar: false,
  closeButton: 'button',
  icon: true,
  rtl: false
}

app.use(createPinia())
app.use(router)
app.use(Toast, toastOptions)

// Initialize API client with router for 401 handling
setRouterInstance(router)

app.mount('#app')