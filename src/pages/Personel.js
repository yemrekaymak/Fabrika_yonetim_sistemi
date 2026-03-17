import React, { useState, useMemo } from 'react';
import { Users, Pencil, Info, Search } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import ThemeDropdown from 'components/ThemeDropdown';
import { POZISYON_OPTIONS } from 'constants/departmanPozisyon';

const Personel = ({ isDark, personelList = [], onPersonelEkle, onPersonelBilgi }) => {
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [arama, setArama] = useState('');
  const [pozisyonFiltre, setPozisyonFiltre] = useState('');

  const filtrelenenListe = useMemo(() => {
    let liste = personelList;
    if (pozisyonFiltre) {
      liste = liste.filter((p) => (p.pozisyon || '') === pozisyonFiltre);
    }
    const q = arama.trim().toLowerCase();
    if (!q) return liste;
    return liste.filter((p) => {
      const adSoyad = `${p.firstName || ''} ${p.lastName || ''}`.toLowerCase();
      const pozisyon = (p.pozisyon || '').toLowerCase();
      const telefon = (p.telefon || '').replace(/\s/g, '');
      const tc = (p.tcKimlikNo || '').toLowerCase();
      return (
        adSoyad.includes(q) ||
        pozisyon.includes(q) ||
        telefon.includes(q) ||
        tc.includes(q)
      );
    });
  }, [personelList, arama, pozisyonFiltre]);

  const inputCls = isDark
    ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-500 focus:border-blue-500 focus:ring-blue-500/30'
    : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400 focus:border-blue-400 focus:ring-blue-400/30';

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${bgCard}`}>
      <div className={`flex flex-wrap justify-between items-center gap-4 mb-4 border-b pb-4 ${borderCol}`}>
        <h2 className={`text-xl font-bold ${textTitle}`}>Personel Listesi</h2>
        <button
          type="button"
          onClick={onPersonelEkle}
          className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold bg-blue-600 hover:bg-blue-700 text-white transition-colors"
        >
          <Pencil size={18} /> Düzenle
        </button>
      </div>
      <div className="mb-6 flex flex-col sm:flex-row sm:items-end gap-4 sm:gap-6">
        <div className="flex-1 min-w-0">
          <label htmlFor="personel-ara" className={`block text-sm font-medium mb-1.5 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
            Personel ara
          </label>
          <div className="relative">
            <Search
              size={20}
              className={`absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none ${isDark ? 'text-gray-500' : 'text-gray-400'}`}
            />
            <input
              id="personel-ara"
              type="search"
              value={arama}
              onChange={(e) => setArama(e.target.value)}
              placeholder="Ad, soyad veya pozisyona göre ara..."
              className={`w-full pl-10 pr-4 py-2.5 rounded-lg border outline-none focus:ring-2 transition-colors ${inputCls}`}
              autoComplete="off"
            />
          </div>
        </div>
        <div className="w-full sm:w-52 flex-shrink-0">
          <ThemeDropdown
            label="Pozisyona göre filtrele"
            options={[{ id: '', ad: 'Tüm pozisyonlar' }, ...POZISYON_OPTIONS]}
            value={pozisyonFiltre}
            onChange={setPozisyonFiltre}
            renderLabel={(o) => o.ad}
            placeholder="Tüm pozisyonlar"
            isDark={isDark}
          />
        </div>
      </div>
      {(arama.trim() || pozisyonFiltre) && (
        <p className={`mb-4 text-sm ${textSub}`}>
          {filtrelenenListe.length} personel listeleniyor
        </p>
      )}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filtrelenenListe.length === 0 ? (
          <p className={`col-span-full text-center py-8 ${textSub}`}>
            {arama.trim() || pozisyonFiltre ? 'Arama veya filtreye uygun personel bulunamadı.' : 'Listede personel yok.'}
          </p>
        ) : (
          filtrelenenListe.map((p) => (
          <div
            key={p.id}
            className={`flex items-center justify-between gap-3 p-5 border rounded-lg transition shadow-sm ${isDark ? 'bg-gray-700 border-gray-600' : 'bg-white border-gray-200'}`}
          >
            <div className="flex items-center gap-4 min-w-0">
              <div className={`w-12 h-12 rounded-full flex items-center justify-center flex-shrink-0 ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                <Users size={24} />
              </div>
              <div className="min-w-0">
                <div className={`font-bold ${textTitle}`}>{p.firstName} {p.lastName}</div>
                <div className={`text-sm ${textSub}`}>{p.pozisyon || '—'}</div>
              </div>
            </div>
            <button
              type="button"
              onClick={() => onPersonelBilgi?.(p)}
              className={`p-2 rounded-lg flex-shrink-0 transition-colors ${isDark ? 'text-blue-400 hover:bg-blue-900/40' : 'text-blue-600 hover:bg-blue-50'}`}
              title="Bilgileri görüntüle"
            >
              <Info size={22} />
            </button>
          </div>
          ))
        )}
      </div>
    </div>
  );
};

export default Personel;
