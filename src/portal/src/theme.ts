import { definePreset } from '@primevue/themes'
import Aura from '@primevue/themes/aura'

export default definePreset(Aura, {
  primitive: {
    borderRadius: {
      none: '0',
      xs: '1px',
      sm: '2px',
      md: '2px',
      lg: '2px',
      xl: '2px',
    },
  },
})
