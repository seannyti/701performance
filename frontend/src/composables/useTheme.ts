import { onMounted } from 'vue'
import * as signalR from '@microsoft/signalr'
import { logDebug } from '@/services/logger'

const API_URL = import.meta.env.VITE_API_URL || ''
const HUB_URL = `${API_URL}/hubs/chat`

// Module-level theme state — shared across all useTheme() calls
let themeConnection: signalR.HubConnection | null = null
let currentTheme: Record<string, string> = {}

export function useTheme() {
  const applyTheme = (theme?: Record<string, string>) => {
    const t = theme ?? currentTheme
    const root = document.documentElement

    // === COLORS ===
    root.style.setProperty('--color-primary', t.primaryColor ?? '#CC0000')
    root.style.setProperty('--color-secondary', t.secondaryColor ?? '#9A9A9A')
    root.style.setProperty('--color-accent', t.accentColor ?? '#FF3333')
    root.style.setProperty('--color-success', t.successColor ?? '#22c55e')
    root.style.setProperty('--color-warning', t.warningColor ?? '#f59e0b')
    root.style.setProperty('--color-danger', t.dangerColor ?? '#ef4444')

    // Backgrounds
    root.style.setProperty('--color-bg', t.bgColor ?? '#0A0A0A')
    root.style.setProperty('--color-bg-secondary', t.bgSecondary ?? '#141414')
    root.style.setProperty('--color-bg-muted', t.bgMuted ?? '#1C1C1C')

    // Text
    root.style.setProperty('--color-text-primary', t.textPrimary ?? '#FFFFFF')
    root.style.setProperty('--color-text-secondary', t.textSecondary ?? '#C8C8C8')
    root.style.setProperty('--color-text-muted', t.textMuted ?? '#7A7A7A')

    // Borders
    root.style.setProperty('--color-border', t.borderColor ?? '#252525')
    root.style.setProperty('--color-border-accent', t.borderAccent ?? '#CC0000')

    // === TYPOGRAPHY ===
    root.style.setProperty('--font-heading', t.fontHeading ?? "'Rajdhani', 'Inter', system-ui, sans-serif")
    root.style.setProperty('--font-body', t.fontBody ?? "'Inter', system-ui, sans-serif")
    root.style.setProperty('--font-size-base', (t.fontSizeBase ?? '16') + 'px')
    root.style.setProperty('--font-size-h1', (t.fontSizeH1 ?? '3') + 'rem')
    root.style.setProperty('--font-size-h2', (t.fontSizeH2 ?? '2.25') + 'rem')
    root.style.setProperty('--font-size-h3', (t.fontSizeH3 ?? '1.6') + 'rem')
    root.style.setProperty('--font-weight-heading', t.fontWeightHeading ?? '800')
    root.style.setProperty('--font-weight-body', t.fontWeightBody ?? '400')
    root.style.setProperty('--line-height-heading', t.lineHeightHeading ?? '1.1')
    root.style.setProperty('--line-height-body', t.lineHeightBody ?? '1.6')

    // === COMPONENTS ===
    root.style.setProperty('--button-radius', (t.buttonRadius ?? '3') + 'px')
    root.style.setProperty('--button-padding-y', (t.buttonPaddingY ?? '0.8') + 'rem')
    root.style.setProperty('--button-padding-x', (t.buttonPaddingX ?? '2') + 'rem')
    root.style.setProperty('--button-font-weight', t.buttonFontWeight ?? '700')
    root.style.setProperty('--button-text-transform', t.buttonTextTransform ?? 'uppercase')
    root.style.setProperty('--card-radius', (t.cardRadius ?? '6') + 'px')
    root.style.setProperty('--card-padding', (t.cardPadding ?? '1.5') + 'rem')
    root.style.setProperty('--card-shadow', t.cardShadow ?? '0 4px 24px rgba(0,0,0,0.7), inset 0 1px 0 rgba(255,255,255,0.04)')
    root.style.setProperty('--input-radius', (t.inputRadius ?? '3') + 'px')
    root.style.setProperty('--input-border-width', (t.inputBorderWidth ?? '1') + 'px')
    root.style.setProperty('--input-focus-color', t.inputFocusColor ?? '#CC0000')

    // === LAYOUT ===
    root.style.setProperty('--container-max-width', (t.containerMaxWidth ?? '1280') + 'px')
    root.style.setProperty('--container-padding', (t.containerPadding ?? '20') + 'px')
    root.style.setProperty('--section-padding-top', (t.sectionPaddingTop ?? '4') + 'rem')
    root.style.setProperty('--section-padding-bottom', (t.sectionPaddingBottom ?? '4') + 'rem')
    root.style.setProperty('--element-gap', (t.elementGap ?? '1.5') + 'rem')

    // === EFFECTS ===
    root.style.setProperty('--transition-duration', (t.transitionDuration ?? '200') + 'ms')
    root.style.setProperty('--transition-timing', t.transitionTiming ?? 'ease')
    root.style.setProperty('--hover-lift-amount', (t.hoverLiftAmount ?? '6') + 'px')
    root.style.setProperty('--hover-scale', t.hoverScale ?? '1.02')
    root.style.setProperty('--hover-shadow', t.hoverShadow ?? '0 8px 40px rgba(204,0,0,0.35), 0 0 0 1px rgba(204,0,0,0.2)')

    // === HEADER ===
    root.style.setProperty('--header-bg', t.headerBg ?? '#080808')
    root.style.setProperty('--header-text', t.headerText ?? '#FFFFFF')
    root.style.setProperty('--header-height', (t.headerHeight ?? '72') + 'px')
    root.style.setProperty('--header-shadow', t.headerShadow ?? '0 1px 0 rgba(204,0,0,0.3), 0 4px 24px rgba(0,0,0,0.85)')

    // === FOOTER ===
    root.style.setProperty('--footer-bg', t.footerBg ?? '#050505')
    root.style.setProperty('--footer-text', t.footerText ?? '#AAAAAA')
    root.style.setProperty('--footer-padding', (t.footerPadding ?? '3') + 'rem')

    // === GRADIENTS ===
    const gradStart = t.gradientStart ?? '#CC0000'
    const gradEnd = t.gradientEnd ?? '#660000'
    const gradDir = t.gradientDirection ?? '135deg'
    const gradOpacity = (parseInt(t.gradientOpacity ?? '70') / 100).toString()
    root.style.setProperty('--gradient-start', gradStart)
    root.style.setProperty('--gradient-end', gradEnd)
    root.style.setProperty('--gradient-direction', gradDir)
    root.style.setProperty('--gradient-opacity', gradOpacity)
    root.style.setProperty('--gradient', `linear-gradient(${gradDir}, ${gradStart}, ${gradEnd})`)
    root.style.setProperty('--backdrop-blur', (t.backdropBlur ?? '10') + 'px')
    root.style.setProperty('--modal-backdrop-opacity', (t.modalBackdropOpacity ?? '75') + '%')

    // === CORNER STYLE ===
    const cornerMap: Record<string, string> = {
      sharp: '0px', rounded: '8px', 'extra-rounded': '16px', pill: '999px'
    }
    root.style.setProperty('--global-corner-radius', cornerMap[t.cornerStyle ?? 'rounded'] ?? '8px')

    // === BUTTON STYLE ===
    root.style.setProperty('--button-style', t.buttonStyle ?? 'solid')

    // === HEADING EFFECTS ===
    const shadowMap: Record<string, string> = {
      subtle: '0 1px 2px rgba(0,0,0,0.1)',
      strong: '0 2px 4px rgba(0,0,0,0.3)',
      glow: `0 0 20px ${t.primaryColor ?? '#CC0000'}`,
      long: '2px 2px 0 rgba(0,0,0,0.1), 4px 4px 0 rgba(0,0,0,0.05)',
      none: 'none'
    }
    root.style.setProperty('--heading-text-shadow', shadowMap[t.headingShadow ?? 'glow'] ?? 'none')

    const spacingMap: Record<string, string> = {
      tight: '-0.025em', normal: '0', wide: '0.025em', wider: '0.05em', widest: '0.1em'
    }
    root.style.setProperty('--heading-letter-spacing', spacingMap[t.letterSpacing ?? 'wide'] ?? '0.025em')

    // === ANIMATIONS / EFFECTS ===
    root.style.setProperty('--animations-enabled', (t.animationsEnabled ?? 'true') === 'true' ? '1' : '0')
    root.style.setProperty('--glass-enabled', (t.glassMorphism ?? 'false') === 'true' ? '1' : '0')
    root.style.setProperty('--gradient-overlays-enabled', (t.gradientOverlays ?? 'false') === 'true' ? '1' : '0')
    root.style.setProperty('--parallax-enabled', (t.parallaxEnabled ?? 'false') === 'true' ? '1' : '0')
    root.style.setProperty('--image-hover-effect', t.imageHover ?? 'zoom')
    root.style.setProperty('--image-border-style', t.imageBorderStyle ?? 'shadow')
    root.style.scrollBehavior = (t.smoothScroll ?? 'true') === 'true' ? 'smooth' : 'auto'

    // === CUSTOM CSS ===
    applyCustomCSS(t.customCss ?? '')

    logDebug('Theme applied', { preset: t.presetName ?? 'custom' })
  }

  const applyCustomCSS = (css: string) => {
    const existing = document.getElementById('custom-theme-css')
    if (existing) existing.remove()
    if (css?.trim()) {
      const style = document.createElement('style')
      style.id = 'custom-theme-css'
      style.textContent = css
      document.head.appendChild(style)
    }
  }

  // Converts the legacy flat key/value format (theme_primary_color etc.) to the new camelCase format
  const normalizeLegacyTheme = (raw: Record<string, unknown>): Record<string, string> => {
    // If it already has camelCase keys (new format), return as-is
    if ('primaryColor' in raw) return raw as Record<string, string>

    // Map legacy snake_case keys to camelCase
    const MAP: Record<string, string> = {
      theme_primary_color: 'primaryColor',
      theme_secondary_color: 'secondaryColor',
      theme_accent_color: 'accentColor',
      theme_success_color: 'successColor',
      theme_warning_color: 'warningColor',
      theme_danger_color: 'dangerColor',
      theme_bg_color: 'bgColor',
      theme_bg_secondary: 'bgSecondary',
      theme_bg_muted: 'bgMuted',
      theme_text_primary: 'textPrimary',
      theme_text_secondary: 'textSecondary',
      theme_text_muted: 'textMuted',
      theme_border_color: 'borderColor',
      theme_border_accent: 'borderAccent',
      theme_font_heading: 'fontHeading',
      theme_font_body: 'fontBody',
      theme_font_size_base: 'fontSizeBase',
      theme_font_size_h1: 'fontSizeH1',
      theme_font_size_h2: 'fontSizeH2',
      theme_font_size_h3: 'fontSizeH3',
      theme_font_weight_heading: 'fontWeightHeading',
      theme_font_weight_body: 'fontWeightBody',
      theme_line_height_heading: 'lineHeightHeading',
      theme_line_height_body: 'lineHeightBody',
      theme_button_radius: 'buttonRadius',
      theme_button_padding_y: 'buttonPaddingY',
      theme_button_padding_x: 'buttonPaddingX',
      theme_button_font_weight: 'buttonFontWeight',
      theme_button_text_transform: 'buttonTextTransform',
      theme_card_radius: 'cardRadius',
      theme_card_padding: 'cardPadding',
      theme_card_shadow: 'cardShadow',
      theme_input_radius: 'inputRadius',
      theme_input_border_width: 'inputBorderWidth',
      theme_input_focus_color: 'inputFocusColor',
      theme_container_max_width: 'containerMaxWidth',
      theme_container_padding: 'containerPadding',
      theme_section_padding_top: 'sectionPaddingTop',
      theme_section_padding_bottom: 'sectionPaddingBottom',
      theme_element_gap: 'elementGap',
      theme_transition_duration: 'transitionDuration',
      theme_transition_timing: 'transitionTiming',
      theme_hover_lift_amount: 'hoverLiftAmount',
      theme_hover_scale: 'hoverScale',
      theme_hover_shadow: 'hoverShadow',
      theme_header_bg: 'headerBg',
      theme_header_text: 'headerText',
      theme_header_height: 'headerHeight',
      theme_header_shadow: 'headerShadow',
      theme_footer_bg: 'footerBg',
      theme_footer_text: 'footerText',
      theme_footer_padding: 'footerPadding',
      theme_gradient_start: 'gradientStart',
      theme_gradient_end: 'gradientEnd',
      theme_gradient_direction: 'gradientDirection',
      theme_gradient_opacity: 'gradientOpacity',
      theme_backdrop_blur: 'backdropBlur',
      theme_modal_backdrop_opacity: 'modalBackdropOpacity',
      theme_corner_style: 'cornerStyle',
      theme_button_style: 'buttonStyle',
      theme_heading_shadow: 'headingShadow',
      theme_letter_spacing: 'letterSpacing',
      theme_animations_enabled: 'animationsEnabled',
      theme_glass_morphism: 'glassMorphism',
      theme_gradient_overlays: 'gradientOverlays',
      theme_parallax_enabled: 'parallaxEnabled',
      theme_smooth_scroll: 'smoothScroll',
      theme_image_hover: 'imageHover',
      theme_image_border_style: 'imageBorderStyle',
      theme_custom_css: 'customCss',
      theme_preset_active: 'presetName',
    }
    const out: Record<string, string> = {}
    for (const [k, v] of Object.entries(raw)) {
      const mapped = MAP[k]
      if (mapped) out[mapped] = String(v)
    }
    return out
  }

  const loadTheme = async () => {
    try {
      const res = await fetch(`${API_URL}/api/v1/theme`)
      if (!res.ok) return
      const raw = await res.json()
      currentTheme = normalizeLegacyTheme(raw as Record<string, unknown>)
      applyTheme(currentTheme)
    } catch (err) {
      logDebug('Failed to load theme from API, using defaults')
      applyTheme()
    }
  }

  const connectSignalR = () => {
    if (themeConnection) return // already connected

    themeConnection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    themeConnection.on('ThemeUpdated', (theme: Record<string, unknown>) => {
      logDebug('ThemeUpdated received via SignalR')
      currentTheme = normalizeLegacyTheme(theme)
      applyTheme(currentTheme)
    })

    themeConnection.start().catch(err => {
      logDebug('SignalR theme connection failed', err)
    })
  }

  onMounted(() => {
    loadTheme()
    connectSignalR()
  })

  return { applyTheme }
}
