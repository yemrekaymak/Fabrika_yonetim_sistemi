import React, { useState } from 'react';
import { ArrowLeft, Activity, Plus, Trash2, Search } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import { useToast } from 'contexts/ToastContext';
import { createMakine, deleteMakine } from 'services';

const inputCls = (isDark) =>
  `w-full p-3 rounded-lg border bg-transparent ${isDark ? 'border-gray-600 text-white placeholder-gray-500' : 'border-gray-300 text-gray-900 placeholder-gray-400'}`;

const MakineEkle = ({ isDark, makineList = [], onRefresh, onBack }) => {
  const { toast } = useToast();
  const [submitting, setSubmitting] = useState(false);
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [view, setView] = useState('menu'); // 'menu' | 'ekle' | 'sil'
  const [ad, setAd] = useState('');
  const [detay, setDetay] = useState('');
  const [silArama, setSilArama] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    const adTrim = ad.trim();
    if (!adTrim) return;
    setSubmitting(true);
    try {
      await createMakine({
        ad: adTrim,
        detay: detay.trim() || null,
      });
      setAd('');
      setDetay('');
      setView('menu');
      toast('Makine eklendi');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'Makine eklenemedi');
    } finally {
      setSubmitting(false);
    }
  };

  const handleRemove = async (id, makineAd) => {
    if (!window.confirm(`"${makineAd}" makinesini listeden çıkarmak istiyor musunuz? Emin misiniz?`)) return;
    try {
      await deleteMakine(id);
      toast('Makine listeden çıkarıldı');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'Makine çıkarılamadı');
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
            Makine Düzenle
          </h2>
          <div className="flex flex-col sm:flex-row justify-center items-center gap-4 min-h-[280px]">
            <button
              type="button"
              onClick={() => setView('ekle')}
              className="flex items-center gap-3 px-8 py-4 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-semibold text-lg shadow-lg transition-colors"
            >
              <Plus size={28} /> Makine Ekle
            </button>
            <button
              type="button"
              onClick={() => setView('sil')}
              className="flex items-center gap-3 px-8 py-4 rounded-xl border-2 border-red-500/80 text-red-500 hover:bg-red-500/10 font-semibold text-lg transition-colors"
            >
              <Trash2 size={28} /> Makine Sil
            </button>
          </div>
        </>
      )}

      {view === 'ekle' && (
        <div className="max-w-xl">
          <h2 className={`text-xl font-bold mb-6 border-b pb-2 ${textTitle} ${borderCol}`}>Yeni Makine Ekle</h2>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Makine Adı *</label>
              <input
                type="text"
                value={ad}
                onChange={(e) => setAd(e.target.value)}
                placeholder="Örn: CNC Pres - 01"
                className={inputCls(isDark)}
                required
              />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Detay (Opsiyonel)</label>
              <input
                type="text"
                value={detay}
                onChange={(e) => setDetay(e.target.value)}
                placeholder="Örn: Sıcaklık: 85°C"
                className={inputCls(isDark)}
              />
            </div>
            <div className="flex gap-3 pt-2">
              <button
                type="submit"
                disabled={submitting}
                className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors disabled:opacity-50"
              >
                <Activity size={18} /> {submitting ? 'Kaydediliyor...' : 'Kaydet'}
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
      )}

      {view === 'sil' && (() => {
        const arama = (silArama || '').trim().toLowerCase();
        const filtrele = (m) => {
          if (!arama) return true;
          const metin = [m.ad, m.detay, m.idKod, m.id].filter(Boolean).join(' ').toLowerCase();
          return metin.includes(arama);
        };
        const listelenen = makineList.filter(filtrele);

        return (
          <div className="w-full max-w-2xl">
            <h2 className={`text-xl font-bold mb-4 border-b pb-2 ${textTitle} ${borderCol}`}>
              Mevcut Makineler
            </h2>
            <div className="flex items-center gap-2 mb-6 w-full">
              <input
                type="text"
                value={silArama}
                onChange={(e) => setSilArama(e.target.value)}
                placeholder="Makine adı, detay veya ID ile ara..."
                className={inputCls(isDark) + ' flex-1 min-w-0'}
              />
              <button
                type="button"
                className={`flex items-center gap-2 px-4 py-2.5 rounded-lg border transition-colors ${isDark ? 'border-gray-600 bg-gray-700 text-gray-200 hover:bg-gray-600' : 'border-gray-300 bg-gray-100 text-gray-800 hover:bg-gray-200'}`}
                title="Ara"
              >
                <Search size={18} /> Ara
              </button>
            </div>
            <div className="space-y-3">
              {listelenen.length === 0 ? (
                <p className={`text-sm ${textSub}`}>{arama ? 'Arama kriterine uygun makine yok.' : 'Listede makine yok.'}</p>
              ) : (
                listelenen.map((m) => (
                  <div
                    key={m.id}
                    className={`flex items-center justify-between gap-4 p-4 border rounded-lg ${isDark ? 'bg-gray-700/50 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <div className={`w-10 h-10 rounded-full flex items-center justify-center shrink-0 ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                        <Activity size={20} />
                      </div>
                      <div className="min-w-0">
                        <div className={`font-semibold truncate ${textTitle}`}>{m.ad}</div>
                        <div className={`text-sm ${textSub}`}>{m.detay || (m.idKod ? `ID: ${m.idKod}` : '—')}</div>
                      </div>
                    </div>
                    <button
                      type="button"
                      onClick={() => handleRemove(m.id, m.ad)}
                      className={`p-2 rounded-lg transition-colors shrink-0 ${isDark ? 'text-red-400 hover:bg-red-900/30' : 'text-red-600 hover:bg-red-50'}`}
                      title="Listeden çıkar"
                    >
                      <Trash2 size={18} />
                    </button>
                  </div>
                ))
              )}
            </div>
          </div>
        );
      })()}
    </div>
  );
};

export default MakineEkle;
