import { onMounted, onUnmounted, ref } from 'vue'

export function useParallax(speed: number = 0.5) {
  const parallaxOffset = ref(0)

  const handleScroll = () => {
    const scrolled = window.pageYOffset
    parallaxOffset.value = scrolled * speed
  }

  onMounted(() => {
    window.addEventListener('scroll', handleScroll, { passive: true })
  })

  onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll)
  })

  return {
    parallaxOffset
  }
}
