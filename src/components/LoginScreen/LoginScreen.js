import React, { useState } from 'react';
import { login as apiLogin } from 'services/authService';
import { Eye, EyeOff } from 'lucide-react';

const REMEMBER_KEY = 'fabrika_remember';

const LoginScreen = ({ onLogin, onGoToRegister, successMessage }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(() => typeof window !== 'undefined' && !!localStorage.getItem(REMEMBER_KEY));
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    const emailTrim = (email || '').trim();
    if (!emailTrim) {
      setError('E-posta alanı zorunludur.');
      return;
    }
    if (!password) {
      setError('Şifre alanı zorunludur.');
      return;
    }
    setLoading(true);
    try {
      await apiLogin(emailTrim, password, rememberMe);
      onLogin(rememberMe);
    } catch (err) {
      setError(err?.message || 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex h-screen bg-gray-900 items-center justify-center py-8 overflow-y-auto">
      <div className="bg-gray-800 p-8 rounded-lg shadow-lg w-96 border border-gray-700">
        <h2 className="text-2xl font-bold text-white mb-6 text-center">Giriş yap</h2>

        {successMessage && (
          <div className="mb-4 text-sm text-green-400 bg-green-900/40 border border-green-700 rounded px-3 py-2">
            {successMessage}
          </div>
        )}
        {error && (
          <div className="mb-4 text-sm text-red-400 bg-red-900/40 border border-red-700 rounded px-3 py-2">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="block text-gray-400 text-sm mb-2">E-posta</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full p-2 rounded bg-gray-700 text-white border border-gray-600 focus:outline-none focus:border-blue-500"
              placeholder="yonetici@firma.com"
              required
            />
          </div>
          <div className="mb-4">
            <label className="block text-gray-400 text-sm mb-2">Şifre</label>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full p-2 pr-10 rounded bg-gray-700 text-white border border-gray-600 focus:outline-none focus:border-blue-500"
                placeholder="••••••"
                required
              />
              <button
                type="button"
                onClick={() => setShowPassword((p) => !p)}
                className="absolute right-2 top-1/2 -translate-y-1/2 p-1 text-gray-400 hover:text-white rounded"
                title={showPassword ? 'Şifreyi gizle' : 'Şifreyi göster'}
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </div>
          <label className="flex items-center gap-2 mb-6 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
              className="w-4 h-4 rounded border-gray-500 bg-gray-700 text-blue-600 focus:ring-blue-500 focus:ring-offset-0 focus:ring-offset-gray-800"
            />
            <span className="text-gray-400 text-sm">Oturumu açık tut</span>
          </label>
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-blue-900 disabled:cursor-not-allowed text-white font-bold py-2 px-4 rounded transition duration-200"
          >
            {loading ? 'Giriş yapılıyor...' : 'Sisteme Giriş Yap'}
          </button>
        </form>

        <div className="mt-6 pt-6 border-t border-gray-600 text-center">
          <button
            type="button"
            onClick={onGoToRegister}
            className="text-gray-400 hover:text-white text-sm underline underline-offset-2"
          >
            Hesap oluştur
          </button>
        </div>
      </div>
    </div>
  );
};

export default LoginScreen;
