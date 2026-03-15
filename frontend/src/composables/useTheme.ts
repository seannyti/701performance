import { watch, onMounted } from 'vue'
import { useSettings } from './useSettings'
import { logDebug } from '@/services/logger'

export function useTheme() {
  const { settings, getSetting, loading } = useSettings()

  const applyTheme = () => {
    // Don't apply theme if settings are still loading
    if (loading.value) {
      logDebug('Theme application skipped - settings still loading');
      return
    }

    logDebug('Applying theme', { settingsCount: Object.keys(settings.value).length });
    
    const root = document.documentElement
    const isDarkMode = root.classList.contains('dark-mode')
    const darkModeEnabled = getSetting('theme_dark_mode_enabled', 'false') === 'true'

    // === COLORS ===
    root.style.setProperty('--color-primary', getSetting('theme_primary_color', '#4b5563') || '#4b5563')
    root.style.setProperty('--color-secondary', getSetting('theme_secondary_color', '#6b7280') || '#6b7280')
    root.style.setProperty('--color-accent', getSetting('theme_accent_color', '#f59e0b') || '#f59e0b')
    root.style.setProperty('--color-success', getSetting('theme_success_color', '#10b981') || '#10b981')
    root.style.setProperty('--color-warning', getSetting('theme_warning_color', '#f59e0b') || '#f59e0b')
    root.style.setProperty('--color-danger', getSetting('theme_danger_color', '#ef4444') || '#ef4444')

    // Backgrounds - adapt for dark mode only if enabled and active
    if (darkModeEnabled && isDarkMode) {
      root.style.setProperty('--color-bg', '#0f172a')
      root.style.setProperty('--color-bg-secondary', '#1e293b')
      root.style.setProperty('--color-bg-muted', '#334155')
    } else {
      root.style.setProperty('--color-bg', getSetting('theme_bg_color', '#ffffff') || '#ffffff')
      root.style.setProperty('--color-bg-secondary', getSetting('theme_bg_secondary', '#f9fafb') || '#f9fafb')
      root.style.setProperty('--color-bg-muted', getSetting('theme_bg_muted', '#f3f4f6') || '#f3f4f6')
    }

    // Text - adapt for dark mode only if enabled and active
    if (darkModeEnabled && isDarkMode) {
      root.style.setProperty('--color-text-primary', '#f1f5f9')
      root.style.setProperty('--color-text-secondary', '#cbd5e1')
      root.style.setProperty('--color-text-muted', '#94a3b8')
    } else {
      root.style.setProperty('--color-text-primary', getSetting('theme_text_primary', '#111827') || '#111827')
      root.style.setProperty('--color-text-secondary', getSetting('theme_text_secondary', '#6b7280') || '#6b7280')
      root.style.setProperty('--color-text-muted', getSetting('theme_text_muted', '#9ca3af') || '#9ca3af')
    }

    // Borders - adapt for dark mode only if enabled and active
    if (darkModeEnabled && isDarkMode) {
      root.style.setProperty('--color-border', '#334155')
      root.style.setProperty('--color-border-accent', '#475569')
    } else {
      root.style.setProperty('--color-border', getSetting('theme_border_color', '#e5e7eb') || '#e5e7eb')
      root.style.setProperty('--color-border-accent', getSetting('theme_border_accent', '#d1d5db') || '#d1d5db')
    }

    // === TYPOGRAPHY ===
    root.style.setProperty('--font-heading', getSetting('theme_font_heading', "'Inter', system-ui, sans-serif"))
    root.style.setProperty('--font-body', getSetting('theme_font_body', "'Inter', system-ui, sans-serif"))
    root.style.setProperty('--font-size-base', getSetting('theme_font_size_base', '16') + 'px')
    root.style.setProperty('--font-size-h1', getSetting('theme_font_size_h1', '2.5') + 'rem')
    root.style.setProperty('--font-size-h2', getSetting('theme_font_size_h2', '2') + 'rem')
    root.style.setProperty('--font-size-h3', getSetting('theme_font_size_h3', '1.5') + 'rem')
    root.style.setProperty('--font-weight-heading', getSetting('theme_font_weight_heading', '700'))
    root.style.setProperty('--font-weight-body', getSetting('theme_font_weight_body', '400'))
    root.style.setProperty('--line-height-heading', getSetting('theme_line_height_heading', '1.2'))
    root.style.setProperty('--line-height-body', getSetting('theme_line_height_body', '1.6'))

    // === COMPONENTS ===
    // Buttons
    root.style.setProperty('--button-radius', getSetting('theme_button_radius', '6') + 'px')
    root.style.setProperty('--button-padding-y', getSetting('theme_button_padding_y', '0.75') + 'rem')
    root.style.setProperty('--button-padding-x', getSetting('theme_button_padding_x', '1.5') + 'rem')
    root.style.setProperty('--button-font-weight', getSetting('theme_button_font_weight', '600'))
    root.style.setProperty('--button-text-transform', getSetting('theme_button_text_transform', 'none'))

    // Cards
    root.style.setProperty('--card-radius', getSetting('theme_card_radius', '12') + 'px')
    root.style.setProperty('--card-padding', getSetting('theme_card_padding', '1.5') + 'rem')
    root.style.setProperty('--card-shadow', getSetting('theme_card_shadow', '0 1px 3px 0 rgb(0 0 0 / 0.1)'))

    // Inputs
    root.style.setProperty('--input-radius', getSetting('theme_input_radius', '6') + 'px')
    root.style.setProperty('--input-border-width', getSetting('theme_input_border_width', '1') + 'px')
    root.style.setProperty('--input-focus-color', getSetting('theme_input_focus_color', '#6366f1'))

    // === LAYOUT ===
    root.style.setProperty('--container-max-width', getSetting('theme_container_max_width', '1280') + 'px')
    root.style.setProperty('--container-padding', getSetting('theme_container_padding', '20') + 'px')
    root.style.setProperty('--section-padding-top', getSetting('theme_section_padding_top', '4') + 'rem')
    root.style.setProperty('--section-padding-bottom', getSetting('theme_section_padding_bottom', '4') + 'rem')
    root.style.setProperty('--element-gap', getSetting('theme_element_gap', '1.5') + 'rem')

    // === EFFECTS ===
    root.style.setProperty('--transition-duration', getSetting('theme_transition_duration', '200') + 'ms')
    root.style.setProperty('--transition-timing', getSetting('theme_transition_timing', 'ease'))
    root.style.setProperty('--hover-lift-amount', getSetting('theme_hover_lift_amount', '4') + 'px')
    root.style.setProperty('--hover-scale', getSetting('theme_hover_scale', '1.02'))
    root.style.setProperty('--hover-shadow', getSetting('theme_hover_shadow', '0 8px 20px 0 rgb(0 0 0 / 0.15)'))

    // === HEADER ===
    if (darkModeEnabled && isDarkMode) {
      root.style.setProperty('--header-bg', '#1e293b')
      root.style.setProperty('--header-text', '#f1f5f9')
      root.style.setProperty('--header-height', getSetting('theme_header_height', '72') + 'px')
      root.style.setProperty('--header-shadow', '0 1px 3px 0 rgb(0 0 0 / 0.5)')
    } else {
      root.style.setProperty('--header-bg', getSetting('theme_header_bg', '#ffffff') || '#ffffff')
      root.style.setProperty('--header-text', getSetting('theme_header_text', '#111827') || '#111827')
      root.style.setProperty('--header-height', getSetting('theme_header_height', '72') + 'px')
      root.style.setProperty('--header-shadow', getSetting('theme_header_shadow', '0 1px 3px 0 rgb(0 0 0 / 0.1)') || '0 1px 3px 0 rgb(0 0 0 / 0.1)')
    }

    // === FOOTER ===
    if (darkModeEnabled && isDarkMode) {
      root.style.setProperty('--footer-bg', '#0f172a')
      root.style.setProperty('--footer-text', '#94a3b8')
      root.style.setProperty('--footer-padding', getSetting('theme_footer_padding', '3') + 'rem')
    } else {
      root.style.setProperty('--footer-bg', getSetting('theme_footer_bg', '#1f2937') || '#1f2937')
      root.style.setProperty('--footer-text', getSetting('theme_footer_text', '#9ca3af') || '#9ca3af')
      root.style.setProperty('--footer-padding', getSetting('theme_footer_padding', '3') + 'rem')
    }

    // === GRADIENTS ===
    const gradientStart = getSetting('theme_gradient_start', '#6366f1')
    const gradientEnd = getSetting('theme_gradient_end', '#ec4899')
    const gradientDirection = getSetting('theme_gradient_direction', 'to right')
    const gradientOpacity = parseInt(getSetting('theme_gradient_opacity', '70')) / 100
    
    root.style.setProperty('--gradient-start', gradientStart)
    root.style.setProperty('--gradient-end', gradientEnd)
    root.style.setProperty('--gradient-direction', gradientDirection)
    root.style.setProperty('--gradient-opacity', gradientOpacity.toString())
    root.style.setProperty('--gradient', `linear-gradient(${gradientDirection}, ${gradientStart}, ${gradientEnd})`)

    root.style.setProperty('--backdrop-blur', getSetting('theme_backdrop_blur', '10') + 'px')
    root.style.setProperty('--modal-backdrop-opacity', getSetting('theme_modal_backdrop_opacity', '75') + '%')

    // === ADVANCED FEATURES ===
    const smoothScroll = getSetting('theme_smooth_scroll', 'true') === 'true'
    root.style.scrollBehavior = smoothScroll ? 'smooth' : 'auto'

    const glassMorphism = getSetting('theme_glass_morphism', 'false') === 'true'
    root.style.setProperty('--glass-enabled', glassMorphism ? '1' : '0')

    const animationsEnabled = getSetting('theme_animations_enabled', 'true') === 'true'
    root.style.setProperty('--animations-enabled', animationsEnabled ? '1' : '0')

    const gradientOverlays = getSetting('theme_gradient_overlays', 'false') === 'true'
    root.style.setProperty('--gradient-overlays-enabled', gradientOverlays ? '1' : '0')

    const parallaxEnabled = getSetting('theme_parallax_enabled', 'false') === 'true'
    root.style.setProperty('--parallax-enabled', parallaxEnabled ? '1' : '0')

    // === CORNER STYLES ===
    const cornerStyle = getSetting('theme_corner_style', 'rounded')
    let cornerRadius = '8px'
    switch (cornerStyle) {
      case 'sharp':
        cornerRadius = '0px'
        break
      case 'rounded':
        cornerRadius = '8px'
        break
      case 'extra-rounded':
        cornerRadius = '16px'
        break
      case 'pill':
        cornerRadius = '999px'
        break
    }
    root.style.setProperty('--global-corner-radius', cornerRadius)

    // === BUTTON STYLES ===
    const buttonStyle = getSetting('theme_button_style', 'solid')
    root.style.setProperty('--button-style', buttonStyle)

    // === IMAGE EFFECTS ===
    const imageHover = getSetting('theme_image_hover', 'zoom')
    root.style.setProperty('--image-hover-effect', imageHover)

    const imageBorderStyle = getSetting('theme_image_border_style', 'shadow')
    root.style.setProperty('--image-border-style', imageBorderStyle)

    // === BACKGROUND PATTERN ===
    const bgPattern = getSetting('theme_bg_pattern', 'none')
    root.style.setProperty('--bg-pattern', bgPattern)
    applyBackgroundPattern(bgPattern)

    // === TEXT EFFECTS ===
    const headingShadow = getSetting('theme_heading_shadow', 'none')
    let shadowValue = 'none'
    switch (headingShadow) {
      case 'subtle':
        shadowValue = '0 1px 2px rgba(0, 0, 0, 0.1)'
        break
      case 'strong':
        shadowValue = '0 2px 4px rgba(0, 0, 0, 0.3)'
        break
      case 'glow':
        shadowValue = `0 0 20px ${getSetting('theme_primary_color', '#6366f1')}`
        break
      case 'long':
        shadowValue = '2px 2px 0 rgba(0, 0, 0, 0.1), 4px 4px 0 rgba(0, 0, 0, 0.05)'
        break
    }
    root.style.setProperty('--heading-text-shadow', shadowValue)

    const letterSpacing = getSetting('theme_letter_spacing', 'normal')
    let spacingValue = '0'
    switch (letterSpacing) {
      case 'tight':
        spacingValue = '-0.025em'
        break
      case 'normal':
        spacingValue = '0'
        break
      case 'wide':
        spacingValue = '0.025em'
        break
      case 'wider':
        spacingValue = '0.05em'
        break
      case 'widest':
        spacingValue = '0.1em'
        break
    }
    root.style.setProperty('--heading-letter-spacing', spacingValue)

    // === CUSTOM CSS ===
    applyCustomCSS(getSetting('theme_custom_css', ''))

    logDebug('Theme applied successfully')
  }

  const applyBackgroundPattern = (pattern: string) => {
    const root = document.documentElement
    let patternCSS = 'none'

    switch (pattern) {
      case 'dots':
        patternCSS = 'radial-gradient(circle, #00000008 1px, transparent 1px)'
        root.style.backgroundSize = '20px 20px'
        break
      case 'grid':
        patternCSS = 'linear-gradient(#00000008 1px, transparent 1px), linear-gradient(90deg, #00000008 1px, transparent 1px)'
        root.style.backgroundSize = '20px 20px'
        break
      case 'diagonal':
        patternCSS = 'repeating-linear-gradient(45deg, transparent, transparent 10px, #00000005 10px, #00000005 20px)'
        break
      case 'circuit':
        patternCSS = 'linear-gradient(90deg, #00000008 1px, transparent 1px), linear-gradient(#00000008 1px, transparent 1px)'
        root.style.backgroundSize = '50px 50px'
        break
      case 'topography':
        patternCSS = 'radial-gradient(circle at 20% 50%, transparent 20%, #00000005 21%, #00000005 34%, transparent 35%, transparent), radial-gradient(circle at 60% 70%, transparent 20%, #00000005 21%, #00000005 34%, transparent 35%, transparent)'
        root.style.backgroundSize = '100px 100px'
        break
    }

    root.style.backgroundImage = patternCSS
  }

  const applyCustomCSS = (css: string) => {
    // Remove existing custom CSS
    const existingStyle = document.getElementById('custom-theme-css')
    if (existingStyle) {
      existingStyle.remove()
    }

    // Add new custom CSS
    if (css && css.trim()) {
      const style = document.createElement('style')
      style.id = 'custom-theme-css'
      style.textContent = css
      document.head.appendChild(style)
    }
  }

  // Watch for settings changes and reapply theme
  watch(settings, () => {
    logDebug('Settings changed, reapplying theme', { settingsCount: Object.keys(settings.value).length });
    applyTheme()
  }, { deep: true })

  // Watch for loading to complete, then apply theme
  watch(loading, (newLoading, oldLoading) => {
    if (oldLoading && !newLoading) {
      logDebug('Settings finished loading, applying theme', {
        totalSettings: Object.keys(settings.value).length,
        primaryColor: getSetting('theme_primary_color', 'NOT_SET'),
        bgColor: getSetting('theme_bg_color', 'NOT_SET')
      });
      applyTheme()
    }
  })

  // Apply theme on mount
  onMounted(() => {
    logDebug('useTheme mounted', { 
      loading: loading.value, 
      settingsCount: Object.keys(settings.value).length 
    });
    applyTheme()

    // Watch for dark mode class changes on document root
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.attributeName === 'class') {
          applyTheme()
        }
      })
    })

    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['class']
    })
  })

  return {
    applyTheme
  }
}
