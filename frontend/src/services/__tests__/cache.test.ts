import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { ApiCache } from '@/services/cache'

// We test the class directly (not the singleton) so each test gets a fresh instance

describe('ApiCache', () => {
  let cache: ApiCache

  beforeEach(() => {
    cache = new ApiCache()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  // ─── get / set ─────────────────────────────────────────────────────────────

  describe('set & get', () => {
    it('returns stored data before expiry', () => {
      cache.set('key1', { name: 'bike' })

      const result = cache.get<{ name: string }>('key1')

      expect(result).toEqual({ name: 'bike' })
    })

    it('returns null for unknown key', () => {
      expect(cache.get('missing')).toBeNull()
    })

    it('returns null after default TTL (5 minutes) expires', () => {
      cache.set('key2', 'data')

      vi.advanceTimersByTime(5 * 60 * 1000 + 1)

      expect(cache.get('key2')).toBeNull()
    })

    it('returns null after custom TTL expires', () => {
      cache.set('short', 'value', 1000) // 1 second TTL

      vi.advanceTimersByTime(1001)

      expect(cache.get('short')).toBeNull()
    })

    it('still returns data before custom TTL expires', () => {
      cache.set('short', 'value', 1000)

      vi.advanceTimersByTime(999)

      expect(cache.get('short')).toBe('value')
    })

    it('overwrites existing entry', () => {
      cache.set('key', 'first')
      cache.set('key', 'second')

      expect(cache.get('key')).toBe('second')
    })
  })

  // ─── has ───────────────────────────────────────────────────────────────────

  describe('has', () => {
    it('returns true for a valid unexpired entry', () => {
      cache.set('exists', 42)

      expect(cache.has('exists')).toBe(true)
    })

    it('returns false for a missing key', () => {
      expect(cache.has('nope')).toBe(false)
    })

    it('returns false after the entry expires', () => {
      cache.set('expires', 'soon', 500)

      vi.advanceTimersByTime(501)

      expect(cache.has('expires')).toBe(false)
    })
  })

  // ─── delete ────────────────────────────────────────────────────────────────

  describe('delete', () => {
    it('removes the specified key', () => {
      cache.set('toDelete', 'value')
      cache.delete('toDelete')

      expect(cache.get('toDelete')).toBeNull()
    })

    it('does not affect other keys', () => {
      cache.set('keep', 'value')
      cache.set('remove', 'other')

      cache.delete('remove')

      expect(cache.get('keep')).toBe('value')
    })

    it('does not throw when deleting a non-existent key', () => {
      expect(() => cache.delete('ghost')).not.toThrow()
    })
  })

  // ─── clear ─────────────────────────────────────────────────────────────────

  describe('clear', () => {
    it('removes all entries', () => {
      cache.set('a', 1)
      cache.set('b', 2)
      cache.set('c', 3)

      cache.clear()

      expect(cache.getStats().size).toBe(0)
    })

    it('returns null for any key after clear', () => {
      cache.set('key', 'value')
      cache.clear()

      expect(cache.get('key')).toBeNull()
    })
  })

  // ─── cleanup ───────────────────────────────────────────────────────────────

  describe('cleanup', () => {
    it('removes only expired entries', () => {
      cache.set('fresh', 'data', 60_000)   // expires in 60s
      cache.set('stale', 'data', 500)      // expires in 0.5s

      vi.advanceTimersByTime(501)
      cache.cleanup()

      expect(cache.has('fresh')).toBe(true)
      expect(cache.has('stale')).toBe(false)
    })

    it('does not remove valid entries', () => {
      cache.set('valid', 'data')
      cache.cleanup()

      expect(cache.has('valid')).toBe(true)
    })

    it('leaves cache empty when all entries are expired', () => {
      cache.set('a', 1, 100)
      cache.set('b', 2, 100)

      vi.advanceTimersByTime(101)
      cache.cleanup()

      expect(cache.getStats().size).toBe(0)
    })
  })

  // ─── getStats ──────────────────────────────────────────────────────────────

  describe('getStats', () => {
    it('reports correct size', () => {
      cache.set('x', 1)
      cache.set('y', 2)

      expect(cache.getStats().size).toBe(2)
    })

    it('lists all cache keys', () => {
      cache.set('alpha', 1)
      cache.set('beta', 2)

      const { keys } = cache.getStats()

      expect(keys).toContain('alpha')
      expect(keys).toContain('beta')
    })

    it('returns size 0 on empty cache', () => {
      expect(cache.getStats().size).toBe(0)
    })
  })
})
