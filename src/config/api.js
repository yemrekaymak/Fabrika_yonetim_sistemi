/**
 * Backend API base URL.
 * - Geliştirme: boş = proxy kullanır.
 * - Production (Render): Build sırasında REACT_APP_API_BASE_URL = backend servisinin URL'i (örn. https://fabrika-backend.onrender.com)
 */
const explicit = process.env.REACT_APP_API_BASE_URL;
const defaultUrl = process.env.NODE_ENV === 'development'
  ? ''
  : 'https://fabrika-yonetim-sistemi.onrender.com';
export const API_BASE_URL = (explicit !== undefined && explicit !== '')
  ? String(explicit).replace(/\/$/, '')
  : defaultUrl;
