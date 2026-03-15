import { onMounted, onUnmounted, ref } from 'vue'

export function useScrollAnimation(threshold: number = 0.1) {
  const animatedElements = ref<Map<Element, boolean>>(new Map())

  const observerCallback = (entries: IntersectionObserverEntry[]) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add('animate-in')
        animatedElements.value.set(entry.target, true)
      }
    })
  }

  let observer: IntersectionObserver | null = null

  onMounted(() => {
    observer = new IntersectionObserver(observerCallback, {
      threshold,
      rootMargin: '50px'
    })

    // Observe elements with data-animate attribute
    const elements = document.querySelectorAll('[data-animate]')
    elements.forEach((el) => {
      observer?.observe(el)
    })
  })

  onUnmounted(() => {
    observer?.disconnect()
  })

  return {
    animatedElements
  }
}
