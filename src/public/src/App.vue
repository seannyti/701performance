<script setup lang="ts">
import { watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useSettingsStore } from './stores/settings'

const settings = useSettingsStore()
const route = useRoute()

function applySeo() {
  const title = settings.get('seo_title', 'PerformancePower Powersports')
  const desc = settings.get('seo_description', 'ATVs, UTVs, Dirt Bikes, Snowmobiles & More')

  const routeTitle = route.meta?.title as string | undefined
  document.title = routeTitle ? `${routeTitle} | ${title}` : title

  let metaDesc = document.querySelector<HTMLMetaElement>('meta[name="description"]')
  if (!metaDesc) {
    metaDesc = document.createElement('meta')
    metaDesc.setAttribute('name', 'description')
    document.head.appendChild(metaDesc)
  }
  metaDesc.setAttribute('content', desc)

  setMeta('og:title', document.title)
  setMeta('og:description', desc)
  setMeta('og:type', 'website')
}

function setMeta(property: string, content: string) {
  let el = document.querySelector<HTMLMetaElement>(`meta[property="${property}"]`)
  if (!el) {
    el = document.createElement('meta')
    el.setAttribute('property', property)
    document.head.appendChild(el)
  }
  el.setAttribute('content', content)
}

function applyBranding() {
  const accent = settings.get('theme_primary_color')
  if (accent) document.documentElement.style.setProperty('--color-primary', accent)

  const faviconUrl = settings.get('theme_favicon_url')
  if (faviconUrl) {
    let link = document.querySelector<HTMLLinkElement>('link[rel~="icon"]')
    if (!link) {
      link = document.createElement('link')
      link.rel = 'icon'
      document.head.appendChild(link)
    }
    link.href = faviconUrl
  }
}

onMounted(async () => {
  await settings.fetchSettings()
  applySeo()
  applyBranding()
})

watch(() => route.path, () => {
  if (settings.loaded) applySeo()
})
</script>

<template>
  <RouterView v-slot="{ Component }">
    <Transition name="page" mode="out-in">
      <component :is="Component" />
    </Transition>
  </RouterView>
</template>
