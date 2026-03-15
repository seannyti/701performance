/**
 * Structured logging service for the application.
 * Provides environment-aware logging with levels and context.
 */

// Type declaration for console (works in both browser and Node.js)
declare const console: {
  log: (...args: any[]) => void;
  info: (...args: any[]) => void;
  warn: (...args: any[]) => void;
  error: (...args: any[]) => void;
};

export enum LogLevel {
  Debug = 0,
  Info = 1,
  Warn = 2,
  Error = 3,
  None = 4
}

interface LogContext {
  [key: string]: any;
}

export class Logger {
  private currentLevel: LogLevel;
  private isDevelopment: boolean;

  constructor(isDevelopment: boolean = false) {
    this.isDevelopment = isDevelopment;
    this.currentLevel = this.isDevelopment ? LogLevel.Debug : LogLevel.Warn;
  }

  /**
   * Set the minimum log level. Logs below this level will be ignored.
   */
  setLevel(level: LogLevel): void {
    this.currentLevel = level;
  }

  /**
   * Log a debug message (only in development).
   */
  debug(message: string, context?: LogContext): void {
    if (this.shouldLog(LogLevel.Debug)) {
      console.log(`[DEBUG] ${message}`, context || '');
    }
  }

  /**
   * Log an informational message.
   */
  info(message: string, context?: LogContext): void {
    if (this.shouldLog(LogLevel.Info)) {
      console.info(`[INFO] ${message}`, context || '');
    }
  }

  /**
   * Log a warning message.
   */
  warn(message: string, context?: LogContext): void {
    if (this.shouldLog(LogLevel.Warn)) {
      console.warn(`[WARN] ${message}`, context || '');
    }
  }

  /**
   * Log an error message.
   */
  error(message: string, error?: Error | any, context?: LogContext): void {
    if (this.shouldLog(LogLevel.Error)) {
      console.error(`[ERROR] ${message}`, error || '', context || '');
    }
  }

  /**
   * Log API request/response information (development only).
   */
  api(method: string, url: string, status?: number, context?: LogContext): void {
    if (this.isDevelopment && this.shouldLog(LogLevel.Debug)) {
      const statusColor = status && status >= 400 ? '🔴' : '🟢';
      console.log(`[API] ${statusColor} ${method} ${url}`, status ? `(${status})` : '', context || '');
    }
  }

  /**
   * Performance logging (development only).
   */
  perf(label: string, duration: number): void {
    if (this.isDevelopment && this.shouldLog(LogLevel.Debug)) {
      console.log(`[PERF] ${label}: ${duration.toFixed(2)}ms`);
    }
  }

  private shouldLog(level: LogLevel): boolean {
    return level >= this.currentLevel;
  }
}

/**
 * Create a logger instance with environment detection
 */
export function createLogger(isDevelopment: boolean): Logger {
  return new Logger(isDevelopment);
}

/**
 * Create convenience functions bound to a logger instance
 */
export function createLoggerFunctions(logger: Logger) {
  return {
    logDebug: (message: string, context?: LogContext) => logger.debug(message, context),
    logInfo: (message: string, context?: LogContext) => logger.info(message, context),
    logWarn: (message: string, context?: LogContext) => logger.warn(message, context),
    logError: (message: string, error?: Error | any, context?: LogContext) => logger.error(message, error, context),
    logApi: (method: string, url: string, status?: number, context?: LogContext) => logger.api(method, url, status, context),
    logPerf: (label: string, duration: number) => logger.perf(label, duration)
  };
}
