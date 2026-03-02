import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { Logger, LogLevel } from '@/services/logger'

describe('Logger', () => {
  let logger: Logger
  let debugSpy: ReturnType<typeof vi.spyOn>
  let infoSpy: ReturnType<typeof vi.spyOn>
  let warnSpy: ReturnType<typeof vi.spyOn>
  let errorSpy: ReturnType<typeof vi.spyOn>

  beforeEach(() => {
    logger = new Logger()
    debugSpy = vi.spyOn(console, 'log').mockImplementation(() => {})
    infoSpy = vi.spyOn(console, 'info').mockImplementation(() => {})
    warnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {})
    errorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  // ─── Warn level (simulates production default) ────────────────────────────

  describe('Warn level suppression', () => {
    beforeEach(() => {
      // Production default: only Warn and above are logged
      logger.setLevel(LogLevel.Warn)
    })

    it('suppresses debug messages', () => {
      logger.debug('should not appear')

      expect(debugSpy).not.toHaveBeenCalled()
    })

    it('suppresses info messages', () => {
      logger.info('should not appear')

      expect(infoSpy).not.toHaveBeenCalled()
    })

    it('outputs warn messages', () => {
      logger.warn('this should appear')

      expect(warnSpy).toHaveBeenCalledOnce()
    })

    it('outputs error messages', () => {
      logger.error('this should appear')

      expect(errorSpy).toHaveBeenCalledOnce()
    })
  })

  // ─── setLevel ─────────────────────────────────────────────────────────────

  describe('setLevel', () => {
    it('Debug level allows all messages through', () => {
      logger.setLevel(LogLevel.Debug)

      logger.debug('d')
      logger.info('i')
      logger.warn('w')
      logger.error('e')

      expect(debugSpy).toHaveBeenCalledOnce()
      expect(infoSpy).toHaveBeenCalledOnce()
      expect(warnSpy).toHaveBeenCalledOnce()
      expect(errorSpy).toHaveBeenCalledOnce()
    })

    it('Info level suppresses debug only', () => {
      logger.setLevel(LogLevel.Info)

      logger.debug('d')
      logger.info('i')
      logger.warn('w')

      expect(debugSpy).not.toHaveBeenCalled()
      expect(infoSpy).toHaveBeenCalledOnce()
      expect(warnSpy).toHaveBeenCalledOnce()
    })

    it('Error level suppresses debug, info, and warn', () => {
      logger.setLevel(LogLevel.Error)

      logger.debug('d')
      logger.info('i')
      logger.warn('w')
      logger.error('e')

      expect(debugSpy).not.toHaveBeenCalled()
      expect(infoSpy).not.toHaveBeenCalled()
      expect(warnSpy).not.toHaveBeenCalled()
      expect(errorSpy).toHaveBeenCalledOnce()
    })

    it('None level suppresses all messages', () => {
      logger.setLevel(LogLevel.None)

      logger.debug('d')
      logger.info('i')
      logger.warn('w')
      logger.error('e')

      expect(debugSpy).not.toHaveBeenCalled()
      expect(infoSpy).not.toHaveBeenCalled()
      expect(warnSpy).not.toHaveBeenCalled()
      expect(errorSpy).not.toHaveBeenCalled()
    })
  })

  // ─── Message format ───────────────────────────────────────────────────────

  describe('message format', () => {
    beforeEach(() => {
      logger.setLevel(LogLevel.Debug)
    })

    it('debug message includes [DEBUG] prefix', () => {
      logger.debug('test message')

      expect(debugSpy).toHaveBeenCalledWith('[DEBUG] test message', '')
    })

    it('info message includes [INFO] prefix', () => {
      logger.info('test message')

      expect(infoSpy).toHaveBeenCalledWith('[INFO] test message', '')
    })

    it('warn message includes [WARN] prefix', () => {
      logger.warn('test message')

      expect(warnSpy).toHaveBeenCalledWith('[WARN] test message', '')
    })

    it('error message includes [ERROR] prefix', () => {
      logger.error('test message')

      expect(errorSpy).toHaveBeenCalledWith('[ERROR] test message', '', '')
    })

    it('passes context object to console when provided', () => {
      logger.warn('with context', { userId: 42 })

      expect(warnSpy).toHaveBeenCalledWith('[WARN] with context', { userId: 42 })
    })
  })
})
