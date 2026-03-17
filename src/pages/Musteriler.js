import React, { useState, useMemo } from 'react';
import { UserCircle, Pencil, Info, Search } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';

const Musteriler = ({ isDark, musteriList = [], onMusteriEkle, onMusteriBilgi }) => {
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [arama, setArama] = useState('');

  const kurumsalListe = useMemo(() => {
    const list = musteriList.filter((m) => m.tip === 'kurumsal');
    return filtrele(list, arama);
  }, [musteriList, arama]);

  const bireyselListe = useMemo(() => {
    const list = musteriList.filter((m) => m.tip === 'bireysel');
    return filtrele(list, arama);
  }, [musteriList, arama]);

  const inputCls = isDark
    ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-500 focus:border-blue-500 focus:ring-blue-500/30'
    : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400 focus:border-blue-400 focus:ring-blue-400/30';

  const renderKartlar = (liste) => {
    if (liste.length === 0) {
      return (
        <p className={`text-center py-6 ${textSub}`}>
          {arama.trim() ? 'Arama kriterine uygun müşteri yok.' : 'Bu grupta müşteri yok.'}
        </p>
      );
    }
    return liste.map((m) => (
      <div
        key={m.id}
        className={`flex items-center justify-between gap-3 p-5 border rounded-lg transition shadow-sm ${isDark ? 'bg-gray-700 border-gray-600' : 'bg-white border-gray-200'}`}
      >
        <div className="flex items-center gap-4 min-w-0">
          <div className={`w-12 h-12 rounded-full flex items-center justify-center flex-shrink-0 ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
            <UserCircle size={24} />
          </div>
          <div className="min-w-0">
            <div className={`font-bold ${textTitle}`}>
              {m.tip === 'kurumsal' ? (m.unvan || `${m.ad} ${m.soyad}`) : `${m.ad} ${m.soyad}`}
            </div>
            <div className={`text-sm ${textSub}`}>
              {m.tip === 'kurumsal' ? `${m.ad} ${m.soyad}` : m.email || m.telefon || '—'}
            </div>
          </div>
        </div>
        <button
          type="button"
          onClick={() => onMusteriBilgi?.(m)}
          className={`p-2 rounded-lg flex-shrink-0 transition-colors ${isDark ? 'text-blue-400 hover:bg-blue-900/40' : 'text-blue-600 hover:bg-blue-50'}`}
          title="Bilgileri ve geçmiş siparişleri görüntüle"
        >
          <Info size={22} />
        </button>
      </div>
    ));
  };

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${bgCard}`}>
      <div className={`flex flex-wrap justify-between items-center gap-4 mb-4 border-b pb-4 ${borderCol}`}>
        <h2 className={`text-xl font-bold ${textTitle}`}>Müşteriler</h2>
        <button
          type="button"
          onClick={onMusteriEkle}
          className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold bg-blue-600 hover:bg-blue-700 text-white transition-colors"
        >
          <Pencil size={18} /> Düzenle
        </button>
      </div>

      <div className="mb-6">
        <label htmlFor="musteri-ara" className={`block text-sm font-medium mb-1.5 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
          Müşteri ara
        </label>
        <div className="flex items-center gap-2 w-full">
          <input
            id="musteri-ara"
            type="search"
            value={arama}
            onChange={(e) => setArama(e.target.value)}
            placeholder="Ad, soyad, e-posta, telefon veya şirket adına göre ara..."
            className={`flex-1 min-w-0 px-4 py-2.5 rounded-lg border outline-none focus:ring-2 transition-colors ${inputCls}`}
            autoComplete="off"
          />
          <button
            type="button"
            className={`flex items-center gap-2 px-4 py-2.5 rounded-lg border transition-colors flex-shrink-0 ${isDark ? 'border-gray-600 bg-gray-700 text-gray-200 hover:bg-gray-600' : 'border-gray-300 bg-gray-100 text-gray-800 hover:bg-gray-200'}`}
            title="Ara"
          >
            <Search size={18} /> Ara
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-[1fr_1px_1fr] gap-0 lg:gap-8 items-stretch">
        <section className="min-w-0">
          <h3 className={`text-lg font-semibold mb-4 ${textTitle}`}>Kurumsal Müşteriler</h3>
          <div className="space-y-4">
            {renderKartlar(kurumsalListe)}
          </div>
        </section>
        <div
          className={`hidden lg:block w-px flex-shrink-0 self-stretch ${isDark ? 'bg-gray-600' : 'bg-gray-300'}`}
          aria-hidden
        />
        <section className="min-w-0">
          <h3 className={`text-lg font-semibold mb-4 ${textTitle}`}>Bireysel Müşteriler</h3>
          <div className="space-y-4">
            {renderKartlar(bireyselListe)}
          </div>
        </section>
      </div>
    </div>
  );
};

function filtrele(liste, arama) {
  const q = arama.trim().toLowerCase();
  if (!q) return liste;
  return liste.filter((m) => {
    const adSoyad = `${m.ad || ''} ${m.soyad || ''}`.toLowerCase();
    const unvan = (m.unvan || '').toLowerCase();
    const email = (m.email || '').toLowerCase();
    const tel = (m.telefon || '').replace(/\s/g, '');
    const idKod = (m.idKod || '').toLowerCase();
    return (
      adSoyad.includes(q) ||
      unvan.includes(q) ||
      email.includes(q) ||
      tel.includes(q) ||
      idKod.includes(q)
    );
  });
}

export default Musteriler;
