/**
 * Admin logger service - uses shared logger from @powersports/shared-types
 */
import { createLogger, createLoggerFunctions } from '@powersports/shared-types';

// Create logger instance with environment detection
const logger = createLogger(import.meta.env.DEV);

// Export logger instance
export { logger };

// Export convenience functions
const { logDebug, logInfo, logWarn, logError, logApi, logPerf } = createLoggerFunctions(logger);
export { logDebug, logInfo, logWarn, logError, logApi, logPerf };

// Re-export types for backward compatibility
export { Logger, LogLevel } from '@powersports/shared-types';
