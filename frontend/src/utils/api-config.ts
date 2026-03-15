/**
 * API Configuration
 * Uses environment variables for different environments (dev/production)
 */

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226';

export const API_URL = `${BASE_URL}/api/v1`;

/**
 * Helper to construct full media URLs
 * @param path - Relative path from server (e.g., '/uploads/image.jpg')
 */
export const getMediaUrl = (path: string): string => {
  if (!path) return '';
  return path.startsWith('http') ? path : `${BASE_URL}${path}`;
};
