import React, { useState } from 'react';
import { Eye, EyeOff, ArrowLeft } from 'lucide-react';
import { register as apiRegister } from 'services/authService';

const RegisterScreen = ({ onSuccess, onBack }) => {
  const [mail, setMail] = useState('');
  const [isim, setIsim] = useState('');
  const [soyisim, setSoyisim] = useState('');
  const [sifre, setSifre] = useState('');
  const [sifreTekrar, setSifreTekrar] = useState('');
  const [showSifre, setShowSifre] = useState(false);
  const [showSifreTekrar, setShowSifreTekrar] = useState(false);
  const [hata, setHata] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setHata('');
    const emailTrim = (mail || '').trim();
    const adTrim = (isim || '').trim();
    const soyadTrim = (soyisim || '').trim();
    if (!emailTrim) {
      setHata('E-posta alanı zorunludur.');
      return;
    }
    if (!adTrim) {
      setHata('Ad alanı zorunludur.');
      return;
    }
    if (!soyadTrim) {
      setHata('Soyad alanı zorunludur.');
      return;
    }
    if (!sifre) {
      setHata('Şifre alanı zorunludur.');
      return;
    }
    if (sifre !== sifreTekrar) {
      setHata('Şifreler eşleşmiyor.');
      return;
    }
    if (sifre.length < 4) {
      setHata('Şifre en az 4 karakter olmalı.');
      return;
    }
    setLoading(true);
    try {
      await apiRegister(adTrim, soyadTrim, emailTrim, sifre);
      onSuccess();
    } catch (err) {
      setHata(err?.message || 'Hesap oluşturulamadı.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex h-screen bg-gray-900 items-center justify-center py-8 overflow-y-auto">
      <div className="bg-gray-800 p-8 rounded-lg shadow-lg w-96 border border-gray-700">
        <button
          type="button"
          onClick={onBack}
          className="flex items-center gap-2 mb-4 text-gray-400 hover:text-white text-sm"
        >
          <ArrowLeft size={16} /> Girişe dön
        </button>
        <h2 className="text-2xl font-bold text-white mb-6 text-center">Hesap oluştur</h2>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-gray-400 text-sm mb-1">E-posta</label>
            <input
              type="email"
              value={mail}
              onChange={(e) => setMail(e.target.value)}
              className="w-full p-2 rounded bg-gray-700 text-white border border-gray-600 focus:outline-none focus:border-blue-500"
              placeholder="ornek@firma.com"
              required
            />
          </div>
          <div>
            <label className="block text-gray-400 text-sm mb-1">İsim</label>
            <input
              type="text"
              value={isim}
              onChange={(e) => setIsim(e.target.value)}
              className="w-full p-2 rounded bg-gray-700 text-white border border-gray-600 focus:outline-none focus:border-blue-500"
              placeholder="Adınız"
              required
            />
          </div>
          <div>
            <label className="block text-gray-400 text-sm mb-1">Soyisim</label>
            <input
              type="text"
              value={soyisim}
              onChange={(e) => setSoyisim(e.target.value)}
              className="w-full p-2 rounded bg-gray-700 text-white border border-gray-600 focus:outline-none focus:border-blue-500"
              placeholder="Soyadınız"
              required
            />
          </div>
          <div>
            <label className="block text-gray-400 text-sm mb-1">Şifre</label>
            <div className="relative">
              <input
                type={showSifre ? 'text' : 'password'}
                value={sifre}
                onChange={(e) => setSifre(e.target.value)}
                className="w-full p-2 pr-10 rounded bg-gray-700 text-white border border-gray-600 focus:outline-none focus:border-blue-500"
                placeholder="••••••"
                required
              />
              <button
                type="button"
                onClick={() => setShowSifre((p) => !p)}
                className="absolute right-2 top-1/2 -translate-y-1/2 p-1 text-gray-400 hover:text-white rounded"
                title={showSifre ? 'Şifreyi gizle' : 'Şifreyi göster'}
              >
                {showSifre ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </div>
          <div>
            <label className="block text-gray-400 text-sm mb-1">Şifre tekrar</label>
            <div className="relative">
              <input
                type={showSifreTekrar ? 'text' : 'password'}
                value={sifreTekrar}
                onChange={(e) => setSifreTekrar(e.target.value)}
                className="w-full p-2 pr-10 rounded bg-gray-700 text-white border border-gray-600 focus:outline-none focus:border-blue-500"
                placeholder="••••••"
                required
              />
              <button
                type="button"
                onClick={() => setShowSifreTekrar((p) => !p)}
                className="absolute right-2 top-1/2 -translate-y-1/2 p-1 text-gray-400 hover:text-white rounded"
                title={showSifreTekrar ? 'Şifreyi gizle' : 'Şifreyi göster'}
              >
                {showSifreTekrar ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </div>
          {hata && (
            <p className="text-red-400 text-sm">{hata}</p>
          )}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-blue-900 disabled:cursor-not-allowed text-white font-bold py-2 px-4 rounded transition duration-200"
          >
            {loading ? 'Oluşturuluyor...' : 'Hesap oluştur'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default RegisterScreen;
