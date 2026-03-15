import axios from 'axios';
import type { Product } from '@/types';
import { logApi, logError } from './logger';
import { apiCache } from './cache';
import { API_TIMEOUT_MS } from '@/constants';

// API base URL - configurable for different environments
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226';

// Create axios instance with default configuration
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT_MS,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for logging (development only)
if (import.meta.env.DEV) {
  apiClient.interceptors.request.use(
    (config) => {
      logApi(config.method?.toUpperCase() || 'GET', config.url || '');
      return config;
    },
    (error) => Promise.reject(error)
  );
}

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    // Fix image URLs for products
    if (response.data && Array.isArray(response.data)) {
      response.data = response.data.map((item: any) => {
        if (item.imageUrl && item.imageUrl.startsWith('/uploads')) {
          item.imageUrl = `${API_BASE_URL}${item.imageUrl}`
        }
        return item
      })
    } else if (response.data && response.data.imageUrl && response.data.imageUrl.startsWith('/uploads')) {
      response.data.imageUrl = `${API_BASE_URL}${response.data.imageUrl}`
    }
    return response
  },
  (error) => {
    logError('API request failed', error, { 
      status: error.response?.status,
      data: error.response?.data 
    });
    return Promise.reject(error);
  }
);

export const productService = {
  /**
   * Get all products (cached for 5 minutes)
   */
  async getAllProducts(): Promise<Product[]> {
    const cacheKey = 'products:all';
    const cached = apiCache.get<Product[]>(cacheKey);
    
    if (cached) {
      return cached;
    }

    try {
      const response = await apiClient.get<Product[]>('/api/v1/products');
      apiCache.set(cacheKey, response.data); // Cache for default 5 min
      return response.data;
    } catch (error) {
      logError('Failed to fetch products', error);
      throw new Error('Failed to fetch products');
    }
  },

  /**
   * Get a specific product by ID (cached for 5 minutes)
   */
  async getProductById(id: number): Promise<Product | null> {
    const cacheKey = `product:${id}`;
    const cached = apiCache.get<Product>(cacheKey);
    
    if (cached) {
      return cached;
    }

    try {
      const response = await apiClient.get<Product>(`/api/v1/products/${id}`);
      apiCache.set(cacheKey, response.data);
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null;
      }
      logError(`Failed to fetch product ${id}`, error);
      throw new Error(`Failed to fetch product ${id}`);
    }
  },

  /**
   * Get products by category (cached for 5 minutes)
   */
  async getProductsByCategory(category: string): Promise<Product[]> {
    const cacheKey = `products:category:${category}`;
    const cached = apiCache.get<Product[]>(cacheKey);
    
    if (cached) {
      return cached;
    }

    try {
      const response = await apiClient.get<Product[]>(`/api/v1/products/category/${category}`);
      apiCache.set(cacheKey, response.data);
      return response.data;
    } catch (error) {
      logError(`Failed to fetch products for category ${category}`, error);
      throw new Error(`Failed to fetch products for category ${category}`);
    }
  },

  /**
   * Get featured products for home page (cached for 5 minutes)
   */
  async getFeaturedProducts(): Promise<Product[]> {
    const cacheKey = 'products:featured';
    const cached = apiCache.get<Product[]>(cacheKey);
    
    if (cached) {
      return cached;
    }

    try {
      const response = await apiClient.get<Product[]>('/api/v1/products/featured');
      apiCache.set(cacheKey, response.data);
      return response.data;
    } catch (error) {
      logError('Failed to fetch featured products', error);
      throw new Error('Failed to fetch featured products');
    }
  },
};

// Category service
export const categoryService = {
  /**
   * Get all active categories (cached for 10 minutes)
   */
  async getAllCategories(): Promise<any[]> {
    const cacheKey = 'categories:all';
    const cached = apiCache.get<any[]>(cacheKey);
    
    if (cached) {
      return cached;
    }

    try {
      const response = await apiClient.get('/api/v1/categories');
      apiCache.set(cacheKey, response.data, 10 * 60 * 1000); // Cache for 10 min
      return response.data;
    } catch (error) {
      logError('Failed to fetch categories', error);
      throw new Error('Failed to fetch categories');
    }
  },
};

// Health check for API connectivity
export const healthService = {
  async checkHealth(): Promise<boolean> {
    try {
      const response = await apiClient.get('/api/v1/health');
      return response.status === 200;
    } catch (error) {
      logError('Health check failed', error);
      return false;
    }
  },
};

export default apiClient;