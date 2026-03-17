import React, { useState } from 'react';
import { ArrowLeft, UserPlus, UserCircle, Trash2, Check, Square, Search } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import { useToast } from 'contexts/ToastContext';
import { createMusteri, deleteMusteri } from 'services';

const inputCls = (isDark) =>
  `w-full p-3 rounded-lg border bg-transparent ${isDark ? 'border-gray-600 text-white placeholder-gray-500' : 'border-gray-300 text-gray-900 placeholder-gray-400'}`;

const MusteriEkle = ({ isDark, musteriList = [], onRefresh, onBack }) => {
  const { toast } = useToast();
  const [submitting, setSubmitting] = useState(false);
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [view, setView] = useState('menu'); // 'menu' | 'ekle' | 'cikar'
  const [ad, setAd] = useState('');
  const [soyad, setSoyad] = useState('');
  const [email, setEmail] = useState('');
  const [telefon, setTelefon] = useState('');
  const [tip, setTip] = useState('');
  const [unvan, setUnvan] = useState('');
  const [cikarArama, setCikarArama] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!ad.trim() || !soyad.trim()) return;
    if (!tip) {
      alert('Lütfen müşteri tipi seçin.');
      return;
    }
    setSubmitting(true);
    try {
      const yeni = {
        ad: ad.trim(),
        soyad: soyad.trim(),
        email: email.trim() || null,
        telefon: telefon.trim() || null,
        tip,
        unvan: tip === 'kurumsal' ? (unvan.trim() || null) : null,
      };
      await createMusteri(yeni);
      setAd('');
      setSoyad('');
      setEmail('');
      setTelefon('');
      setTip('bireysel');
      setUnvan('');
      setView('menu');
      toast('Müşteri eklendi');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'Müşteri eklenemedi');
    } finally {
      setSubmitting(false);
    }
  };

  const handleRemove = async (id, adSoyad) => {
    if (!window.confirm(`${adSoyad || 'Bu müşteriyi'} listeden çıkarmak istiyor musunuz?`)) return;
    try {
      await deleteMusteri(id);
      toast('Müşteri listeden çıkarıldı');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'Müşteri çıkarılamadı');
    }
  };

  const linkCls = `flex items-center gap-2 text-sm font-medium ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`;

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${bgCard}`}>
      <div className="flex flex-wrap items-center gap-4 mb-6">
        <button type="button" onClick={onBack} className={linkCls}>
          <ArrowLeft size={18} /> Listeye dön
        </button>
        {view !== 'menu' && (
          <button type="button" onClick={() => setView('menu')} className={linkCls}>
            <ArrowLeft size={18} /> Geri
          </button>
        )}
      </div>

      {view === 'menu' && (
        <>
          <h2 className={`text-xl font-bold mb-8 border-b pb-2 text-center ${textTitle} ${borderCol}`}>
            Müşteri Düzenle
          </h2>
          <div className="flex flex-col sm:flex-row justify-center items-center gap-4 min-h-[280px]">
            <button
              type="button"
              onClick={() => setView('ekle')}
              className="flex items-center gap-3 px-8 py-4 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-semibold text-lg shadow-lg transition-colors"
            >
              <UserPlus size={28} /> Müşteri Ekle
            </button>
            <button
              type="button"
              onClick={() => setView('cikar')}
              className="flex items-center gap-3 px-8 py-4 rounded-xl border-2 border-red-500/80 text-red-500 hover:bg-red-500/10 font-semibold text-lg transition-colors"
            >
              <Trash2 size={28} /> Müşteri Çıkar
            </button>
          </div>
        </>
      )}

      {view === 'ekle' && (
        <div className="flex justify-start">
          <div className="w-full max-w-md">
            <h2 className={`text-xl font-bold mb-6 border-b pb-2 text-left ${textTitle} ${borderCol}`}>
              Yeni Müşteri Ekle
            </h2>
            <form onSubmit={handleSubmit} className="space-y-5">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Ad *</label>
            <input
              type="text"
              value={ad}
              onChange={(e) => setAd(e.target.value)}
              placeholder="Örn: Ahmet"
              className={inputCls(isDark)}
              required
            />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Soyad *</label>
            <input
              type="text"
              value={soyad}
              onChange={(e) => setSoyad(e.target.value)}
              placeholder="Örn: Yılmaz"
              className={inputCls(isDark)}
              required
            />
          </div>
        </div>

        <div>
          <label className={`block text-sm font-medium mb-2 ${textSub}`}>E-posta</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ornek@mail.com"
            className={inputCls(isDark)}
          />
        </div>

        <div>
          <label className={`block text-sm font-medium mb-2 ${textSub}`}>Telefon</label>
          <input
            type="tel"
            value={telefon}
            onChange={(e) => setTelefon(e.target.value)}
            placeholder="Örn: 0532 111 2233"
            className={inputCls(isDark)}
          />
        </div>

        <div>
          <label className={`block text-sm font-medium mb-2 ${textSub}`}>Müşteri tipi *</label>
          <div className={`p-4 rounded-lg border ${isDark ? 'border-gray-600 bg-gray-700/30' : 'border-gray-300 bg-gray-50'}`}>
            <div className="grid gap-3 grid-cols-2">
              <button
                type="button"
                onClick={() => setTip(tip === 'kurumsal' ? '' : 'kurumsal')}
                className={`flex items-center gap-2 rounded-lg px-3 py-2 border text-left transition-all ${
                  tip === 'kurumsal'
                    ? 'bg-[#2A334B] border-blue-400 text-white shadow-[0_0_0_1px_rgba(77,144,254,0.4)]'
                    : isDark
                      ? 'bg-transparent border-gray-600 text-gray-200 hover:border-gray-500'
                      : 'bg-transparent border-gray-200 text-gray-700 hover:border-gray-300'
                }`}
              >
                {tip === 'kurumsal' ? (
                  <span className="flex items-center justify-center w-5 h-5 rounded bg-blue-500 shrink-0">
                    <Check size={14} className="text-white" strokeWidth={2.5} />
                  </span>
                ) : (
                  <Square size={18} className={`shrink-0 ${isDark ? 'text-gray-400' : 'text-gray-500'}`} strokeWidth={1.5} />
                )}
                <span className="text-sm font-medium">Şirket (Kurumsal)</span>
              </button>
              <button
                type="button"
                onClick={() => setTip(tip === 'bireysel' ? '' : 'bireysel')}
                className={`flex items-center gap-2 rounded-lg px-3 py-2 border text-left transition-all ${
                  tip === 'bireysel'
                    ? 'bg-[#2A334B] border-blue-400 text-white shadow-[0_0_0_1px_rgba(77,144,254,0.4)]'
                    : isDark
                      ? 'bg-transparent border-gray-600 text-gray-200 hover:border-gray-500'
                      : 'bg-transparent border-gray-200 text-gray-700 hover:border-gray-300'
                }`}
              >
                {tip === 'bireysel' ? (
                  <span className="flex items-center justify-center w-5 h-5 rounded bg-blue-500 shrink-0">
                    <Check size={14} className="text-white" strokeWidth={2.5} />
                  </span>
                ) : (
                  <Square size={18} className={`shrink-0 ${isDark ? 'text-gray-400' : 'text-gray-500'}`} strokeWidth={1.5} />
                )}
                <span className="text-sm font-medium">Bireysel</span>
              </button>
            </div>
          </div>
        </div>

        {tip === 'kurumsal' && (
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Şirket unvanı</label>
            <input
              type="text"
              value={unvan}
              onChange={(e) => setUnvan(e.target.value)}
              placeholder="Örn: ABC Otomotiv A.Ş."
              className={inputCls(isDark)}
            />
          </div>
        )}

        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={submitting}
            className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors disabled:opacity-50"
          >
            <UserPlus size={18} /> {submitting ? 'Kaydediliyor...' : 'Kaydet'}
          </button>
          <button
            type="button"
            onClick={() => setView('menu')}
            className={`px-5 py-2.5 font-semibold rounded-lg border transition-colors ${isDark ? 'border-gray-600 text-gray-300 hover:bg-gray-700' : 'border-gray-300 text-gray-700 hover:bg-gray-100'}`}
          >
            İptal
          </button>
        </div>
            </form>
          </div>
        </div>
      )}

      {view === 'cikar' && (() => {
        const arama = (cikarArama || '').trim().toLowerCase();
        const filtrele = (m) => {
          if (!arama) return true;
          const metin = [m.ad, m.soyad, m.unvan, m.idKod, m.email, m.telefon].filter(Boolean).join(' ').toLowerCase();
          return metin.includes(arama);
        };
        const kurumsalListe = musteriList.filter((m) => m.tip === 'kurumsal' && filtrele(m));
        const bireyselListe = musteriList.filter((m) => m.tip === 'bireysel' && filtrele(m));

        const MusteriKarti = ({ m }) => (
          <div
            key={m.id}
            className={`flex items-center justify-between gap-4 p-4 border rounded-lg ${isDark ? 'bg-gray-700/50 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
          >
            <div className="flex items-center gap-3 min-w-0">
              <div className={`w-10 h-10 rounded-full flex items-center justify-center shrink-0 ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                <UserCircle size={20} />
              </div>
              <div className="min-w-0">
                <div className={`font-semibold truncate ${textTitle}`}>
                  {m.tip === 'kurumsal' ? (m.unvan || `${m.ad} ${m.soyad}`) : `${m.ad} ${m.soyad}`}
                </div>
                <div className={`text-sm ${textSub}`}>{m.tip === 'kurumsal' ? 'Kurumsal' : 'Bireysel'}</div>
              </div>
            </div>
            <button
              type="button"
              onClick={() => handleRemove(m.id, m.tip === 'kurumsal' ? m.unvan : `${m.ad} ${m.soyad}`)}
              className={`p-2 rounded-lg transition-colors shrink-0 ${isDark ? 'text-red-400 hover:bg-red-900/30' : 'text-red-600 hover:bg-red-50'}`}
              title="Listeden çıkar"
            >
              <Trash2 size={18} />
            </button>
          </div>
        );

        return (
          <div className="w-full max-w-4xl">
            <h2 className={`text-xl font-bold mb-4 border-b pb-2 ${textTitle} ${borderCol}`}>
              Mevcut Müşteriler
            </h2>
            <div className="flex items-center gap-2 mb-6 w-full">
              <input
                type="text"
                value={cikarArama}
                onChange={(e) => setCikarArama(e.target.value)}
                placeholder="Ad, unvan, e-posta..."
                className={inputCls(isDark) + ' flex-1 min-w-0'}
              />
              <button
                type="button"
                className={`flex items-center gap-2 px-4 py-2.5 rounded-lg border transition-colors flex-shrink-0 ${isDark ? 'border-gray-600 bg-gray-700 text-gray-200 hover:bg-gray-600' : 'border-gray-300 bg-gray-100 text-gray-800 hover:bg-gray-200'}`}
                title="Ara"
              >
                <Search size={18} /> Ara
              </button>
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <section>
                <h3 className={`text-lg font-semibold mb-3 ${textTitle}`}>Kurumsal Müşteriler</h3>
                <div className="space-y-3">
                  {kurumsalListe.length === 0 ? (
                    <p className={`text-sm ${textSub}`}>Kurumsal müşteri yok.</p>
                  ) : (
                    kurumsalListe.map((m) => <MusteriKarti key={m.id} m={m} />)
                  )}
                </div>
              </section>
              <section>
                <h3 className={`text-lg font-semibold mb-3 ${textTitle}`}>Bireysel Müşteriler</h3>
                <div className="space-y-3">
                  {bireyselListe.length === 0 ? (
                    <p className={`text-sm ${textSub}`}>Bireysel müşteri yok.</p>
                  ) : (
                    bireyselListe.map((m) => <MusteriKarti key={m.id} m={m} />)
                  )}
                </div>
              </section>
            </div>
          </div>
        );
      })()}
    </div>
  );
};

export default MusteriEkle;
