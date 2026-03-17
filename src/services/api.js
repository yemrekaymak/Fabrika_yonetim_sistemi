/**
 * Backend API fetch wrapper. Tüm istekler buradan geçer.
 */
import { API_BASE_URL } from 'config/api';

function translateCommonAspNetMessage(msg) {
  const s = String(msg || '').trim();
  if (!s) return s;
  const lower = s.toLowerCase();
  // ModelState: "The Email field is required."
  const m1 = s.match(/^The\s+(.+?)\s+field\s+is\s+required\.\s*$/i);
  if (m1) {
    const field = m1[1];
    const map = {
      Email: 'E-posta',
      Password: 'Şifre',
      Ad: 'Ad',
      Soyad: 'Soyad',
    };
    const tr = map[field] || field;
    return `${tr} alanı zorunludur.`;
  }
  // ModelState: "The field X must be a string with a minimum length of Y and a maximum length of Z."
  const m2 = s.match(/^The\s+field\s+(.+?)\s+must\s+be\s+a\s+string\s+with\s+a\s+minimum\s+length\s+of\s+(\d+)\s+and\s+a\s+maximum\s+length\s+of\s+(\d+)\.\s*$/i);
  if (m2) {
    const field = m2[1];
    const min = m2[2];
    const max = m2[3];
    const map = { Email: 'E-posta', Password: 'Şifre', Ad: 'Ad', Soyad: 'Soyad' };
    const tr = map[field] || field;
    return `${tr} alanı ${min}-${max} karakter olmalıdır.`;
  }
  // ModelState: "The field X must be between A and B."
  const m3 = s.match(/^The\s+field\s+(.+?)\s+must\s+be\s+between\s+(.+?)\s+and\s+(.+?)\.\s*$/i);
  if (m3) {
    const field = m3[1];
    const a = m3[2];
    const b = m3[3];
    const map = { Email: 'E-posta', Password: 'Şifre', Ad: 'Ad', Soyad: 'Soyad' };
    const tr = map[field] || field;
    return `${tr} alanı ${a} ile ${b} arasında olmalıdır.`;
  }
  // ProblemDetails common titles (sometimes plain text)
  if (lower === 'bad request') return 'Geçersiz istek.';
  if (lower === 'unauthorized') return 'Yetkisiz işlem. Lütfen tekrar giriş yapın.';
  if (lower === 'forbidden') return 'Bu işlem için yetkiniz yok.';
  if (lower === 'not found') return 'Kayıt bulunamadı.';
  if (lower === 'internal server error') return 'Sunucu hatası oluştu. Lütfen tekrar deneyin.';
  if (lower.includes('an error occurred while processing your request')) return 'İşlem sırasında bir hata oluştu.';
  if (lower.includes('a possible object cycle was detected')) return 'Sunucu yanıtında beklenmeyen veri döngüsü tespit edildi.';
  // Fluent-ish: "The JSON value could not be converted..." etc. (basit)
  if (lower.includes('could not be converted')) return 'Gönderilen veri formatı hatalı.';
  if (lower.includes('one or more validation errors')) return 'Doğrulama hatası: Lütfen alanları kontrol edin.';
  if (lower.includes('invalid') && lower.includes('token')) return 'Oturum süresi dolmuş olabilir. Lütfen tekrar giriş yapın.';
  if (lower.includes('invalid username') || lower.includes('invalid password')) return 'E-posta veya şifre hatalı.';
  if (lower.includes('failed') && lower.includes('fetch')) return 'Sunucuya bağlanılamadı.';
  return s;
}

/** ASP.NET / backend validation mesajlarını düzgünce ayıklar */
function extractErrorMessage(body) {
  if (!body) return null;
  const raw = body.errors ?? body.Errors;
  if (raw && typeof raw === 'object') {
    const parts = Array.isArray(raw)
      ? raw
      : Object.entries(raw).map(([k, v]) => (Array.isArray(v) ? v.join(' ') : String(v ?? '')));
    const text = parts.flat().filter(Boolean).join(' ');
    if (text) return translateCommonAspNetMessage(text);
  }
  const msg = body.Mesaj ?? body.message ?? body.title ?? body.error ?? body.Message ?? body.Error;
  if (msg) return translateCommonAspNetMessage(typeof msg === 'string' ? msg : JSON.stringify(msg));
  return null;
}

/** * ÖNEMLİ: Token isminin login kısmındakiyle aynı olduğundan emin ol! 
 * Genelde 'token' veya 'fabrika_token' kullanılır.
 */
function getToken() {
  try {
    return (
      localStorage.getItem('fabrika_token') ||
      localStorage.getItem('token') ||
      sessionStorage.getItem('fabrika_token') ||
      sessionStorage.getItem('token')
    );
  } catch {
    return null;
  }
}

async function request(path, options = {}) {
  // URL'nin sonundaki çift slash hatalarını engellemek için temizlik
  const cleanPath = path.startsWith('/') ? path : `/${path}`;
  const url = path.startsWith('http') ? path : `${API_BASE_URL.replace(/\/$/, '')}${cleanPath}`;

  const headers = {
    'Accept': 'application/json', // Backend'e JSON istediğimizi net söyleyelim
    'Content-Type': 'application/json',
    ...options.headers,
  };

  const token = getToken();
  if (token) {
    // Bearer kelimesinden sonra boşluk olduğundan emin oluyoruz
    headers['Authorization'] = `Bearer ${token.trim()}`;
  }

  try {
    const res = await fetch(url, {
      ...options,
      headers,
      // mode belirtme: proxy (development) ile same-origin, aksi halde tarayıcı varsayılanı (cors) kullanılır
    });

    // 401 hatası gelirse kullanıcıyı login'e yönlendirmek iyi bir fikirdir
    if (res.status === 401) {
       console.warn("Yetki hatası! Token geçersiz veya süresi dolmuş olabilir.");
       // İsteğe bağlı: window.location.href = '/login'; 
    }

    if (!res.ok) {
      let body;
      const text = await res.text();
      try {
        body = text ? JSON.parse(text) : {};
      } catch {
        body = { message: text };
      }
      
      const message = extractErrorMessage(body) || `Hata kodu: ${res.status}`;
      const err = new Error(message);
      err.status = res.status;
      err.body = body;
      throw err;
    }

    // Yanıt boş değilse JSON parse et
    const contentType = res.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
        return await res.json();
    }
    return await res.text();

  } catch (networkErr) {
    if (networkErr.name === 'TypeError' || networkErr.message?.includes('fetch')) {
      const useProxy = !API_BASE_URL || API_BASE_URL.startsWith('http://localhost');
      const hint = useProxy
        ? ' Geliştirmede npm start ile çalıştırın ve .env dosyasında REACT_APP_API_BASE_URL tanımlamayın (proxy kullanılır).'
        : ' Backend (Render) erişilebilir mi kontrol edin.';
      throw new Error('Sunucuya bağlanılamadı.' + hint);
    }
    throw networkErr;
  }
}

export const api = {
  get: (path) => request(path, { method: 'GET' }),
  post: (path, body) => request(path, { method: 'POST', body: body ? JSON.stringify(body) : undefined }),
  put: (path, body) => request(path, { method: 'PUT', body: body ? JSON.stringify(body) : undefined }),
  patch: (path, body) => request(path, { method: 'PATCH', body: body ? JSON.stringify(body) : undefined }),
  delete: (path) => request(path, { method: 'DELETE' }),
};