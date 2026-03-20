import DOMPurify from 'dompurify'

export function useSafeHtml() {
  function sanitize(raw: string): string {
    return DOMPurify.sanitize(raw, { USE_PROFILES: { html: true } })
  }
  return { sanitize }
}
