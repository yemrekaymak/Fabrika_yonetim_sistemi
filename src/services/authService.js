import { api } from './api';

const TOKEN_KEY = 'fabrika_token';
const USER_KEY = 'fabrika_user';

function storeToken(token, rememberMe) {
  if (!token) return;
  try {
    // Önce eski kalıntıları temizleyelim ki çakışma olmasın
    localStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    
    const storage = rememberMe ? localStorage : sessionStorage;
    storage.setItem(TOKEN_KEY, token);
    
    // api.js'in kolayca bulabilmesi için yedek bir 'token' ismiyle de kaydedebiliriz
    storage.setItem('token', token); 
  } catch {
    // storage hatalarını sessizce yut
  }
}

function storeUser(user, rememberMe) {
  if (!user) return;
  try {
    localStorage.removeItem(USER_KEY);
    sessionStorage.removeItem(USER_KEY);
    const storage = rememberMe ? localStorage : sessionStorage;
    storage.setItem(USER_KEY, JSON.stringify(user));
  } catch {
    // storage hatalarını sessizce yut
  }
}

export function getStoredUser() {
  try {
    const raw =
      localStorage.getItem(USER_KEY) ||
      sessionStorage.getItem(USER_KEY) ||
      localStorage.getItem('user') ||
      sessionStorage.getItem('user');
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

/** Swagger: POST /api/Auth/login, body UserLoginDto { email, password } */
export async function login(email, password, rememberMe) {
  const body = {
    email: (email || '').trim() || null,
    password: password || null,
  };

  try {
    const result = await api.post('/api/Auth/login', body);
    const token = result?.token || result?.accessToken || result?.data?.token;
    storeToken(token, rememberMe);
    storeUser(
      {
        id: result?.id ?? null,
        ad: result?.ad ?? null,
        soyad: result?.soyad ?? null,
        email: (result?.email ?? ((email || '').trim() || null)),
        rol: result?.rol ?? null,
      },
      rememberMe
    );
    return result;
  } catch (err) {
    const status = err.status;
    if (status === 401) {
      throw new Error('E-posta veya şifre hatalı. Lütfen tekrar deneyin.');
    }
    if (status === 404) {
      throw new Error('Giriş adresi bulunamadı. Sunucu adresini veya bağlantıyı kontrol edin.');
    }
    if (status === 503) {
      throw new Error('Sunucu şu an yanıt vermiyor. Birkaç saniye sonra tekrar deneyin.');
    }
    throw err;
  }
}

/** Swagger: POST /api/Auth/register, body UserRegisterDto { ad, soyad, email, password }. Token kaydedilmez; kullanıcı giriş ekranına yönlendirilir. */
export async function register(ad, soyad, email, password) {
  return api.post('/api/Auth/register', {
    ad: (ad || '').trim() || null,
    soyad: (soyad || '').trim() || null,
    email: (email || '').trim() || null,
    password: password || null,
  });
}

export function logout() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem('token');
  localStorage.removeItem(USER_KEY);
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
}