import React, { useState, useMemo } from 'react';
import { Pencil, Sparkles, Search, RefreshCw } from 'lucide-react';
import { DepoBarChart } from 'charts';
import { getThemeClasses } from 'utils/theme';

const Stok = ({ isDark, stokList, stokLoading, onRefresh, onDuzenle }) => {
  const { bgCard, textTitle, textSub } = getThemeClasses(isDark);
  const [arama, setArama] = useState('');
  const list = Array.isArray(stokList) ? stokList : [];

  const inputCls = isDark
    ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-500 focus:border-blue-500 focus:ring-blue-500/30'
    : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400 focus:border-blue-400 focus:ring-blue-400/30';

  const filtrelenmisStok = useMemo(() => {
    const q = (arama || '').trim().toLowerCase();
    if (!q) return list;
    return list.filter((row) => {
      const kod = (row.kod || '').toLowerCase();
      const ad = (row.ad || '').toLowerCase();
      const miktar = (row.miktar || '').toLowerCase();
      return kod.includes(q) || ad.includes(q) || miktar.includes(q);
    });
  }, [arama, list]);

  return (
    <div className="space-y-6">
      <div className={`p-6 rounded-xl shadow-sm border transition-colors duration-300 ${bgCard}`}>
        <h2 className={`text-lg font-bold mb-4 ${textTitle}`}>Depo Doluluk Oranları</h2>
        <DepoBarChart
          isDark={isDark}
          data={Array.isArray(stokList) ? stokList.map((s) => ({ malzeme: s.ad, miktar: s.miktarSayi })) : []}
        />
      </div>

      <div className={`p-6 rounded-xl shadow-sm border overflow-hidden transition-colors duration-300 ${bgCard}`}>
        <div className="flex flex-wrap justify-between items-center gap-4 mb-4">
          <h2 className={`text-lg font-bold ${textTitle}`}>Depo Stok Durumu</h2>
          <div className="flex items-center gap-2 flex-wrap">
            {stokLoading && (
              <span className={`text-sm ${textSub}`}>Yükleniyor...</span>
            )}
            {onRefresh && (
              <button
                type="button"
                onClick={onRefresh}
                disabled={stokLoading}
                className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold border transition-colors ${isDark ? 'border-gray-600 text-gray-300 hover:bg-gray-700' : 'border-gray-300 text-gray-700 hover:bg-gray-100'} disabled:opacity-50`}
                title="Listeyi yenile"
              >
                <RefreshCw size={18} /> Yenile
              </button>
            )}
            <button
              type="button"
              onClick={() => {}}
              className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold border transition-colors ${
                isDark
                  ? 'border-purple-500/60 text-purple-400 hover:bg-purple-500/20'
                  : 'border-purple-400 text-purple-700 hover:bg-purple-50'
              }`}
              title="Stok analizleri modülü bağlanacak"
            >
              <Sparkles size={18} /> AI Analiz
            </button>
            <button
              type="button"
              onClick={onDuzenle}
              className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold bg-blue-600 hover:bg-blue-700 text-white transition-colors"
            >
              <Pencil size={18} /> Düzenle
            </button>
          </div>
        </div>
        <div className="flex items-center gap-2 w-full mb-6">
          <input
            type="text"
            value={arama}
            onChange={(e) => setArama(e.target.value)}
            placeholder="Ürün kodu, ad veya miktara göre ara..."
            className={`flex-1 min-w-0 px-4 py-2.5 rounded-lg border outline-none focus:ring-2 transition-colors ${inputCls}`}
          />
          <button
            type="button"
            className={`flex items-center gap-2 px-4 py-2.5 rounded-lg border transition-colors flex-shrink-0 ${isDark ? 'border-gray-600 bg-gray-700 text-gray-200 hover:bg-gray-600' : 'border-gray-300 bg-gray-100 text-gray-800 hover:bg-gray-200'}`}
            title="Ara"
          >
            <Search size={18} /> Ara
          </button>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full text-left">
            <thead
              className={`${isDark ? 'bg-gray-700/50 text-gray-300' : 'bg-gray-50 text-gray-600'} font-semibold uppercase text-xs tracking-wider`}
            >
              <tr>
                <th className="py-4 px-6 rounded-l-lg">Ürün Kodu</th>
                <th className="py-4 px-6">Ürün Adı</th>
                <th className="py-4 px-6">Brüt Ağırlık</th>
                <th className="py-4 px-6">Net Ağırlık</th>
                <th className="py-4 px-6">Hurda Oranı</th>
                <th className="py-4 px-6">Kapasite</th>
                <th className="py-4 px-6">Kritik Seviye</th>
                <th className="py-4 px-6">Miktar</th>
                <th className="py-4 px-6">Maliyet</th>
                <th className="py-4 px-6">Fiyat</th>
                <th className="py-4 px-6 rounded-r-lg">Durum</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {filtrelenmisStok.length === 0 && !stokLoading && (
                <tr>
                  <td colSpan={11} className={`py-12 text-center ${textSub}`}>
                    Henüz stok kaydı yok. API bağlıysa listeyi yüklemek için &quot;Yenile&quot;ye basın veya backend&apos;den veri geldiğinden emin olun.
                  </td>
                </tr>
              )}
              {filtrelenmisStok.map((row) => {
                const kritik = row.durum === 'kritik';
                const durumBadgeCls = kritik
                  ? isDark ? 'bg-yellow-900/30 text-yellow-400 border-yellow-800' : 'bg-yellow-100 text-yellow-800 border-yellow-200'
                  : isDark ? 'bg-green-900/30 text-green-400 border-green-800' : 'bg-green-100 text-green-800 border-green-200';
                const maxBar = row.kapasite > 0 ? row.kapasite : (row.kritik > 0 ? row.kritik : 1);
                const dolulukYuzde = Math.min(100, Math.round((row.miktarSayi / maxBar) * 100));
                const barRenk = dolulukYuzde >= 90 ? 'bg-red-500' : dolulukYuzde >= 70 ? 'bg-amber-500' : 'bg-blue-500';
                const toplamMaliyet = (row.miktarSayi * row.birimMaliyet).toFixed(2);
                const toplamFiyat = (row.miktarSayi * row.birimFiyat).toFixed(2);

                return (
                  <tr
                    key={row.id ?? row.kod ?? Math.random()}
                    className={`transition duration-150 ${isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}`}
                  >
                    <td className={`py-4 px-6 font-mono ${textSub}`}>{row.kod}</td>
                    <td className={`py-4 px-6 font-medium ${textTitle}`}>{row.ad}</td>
                    <td className={`py-4 px-6 ${textSub}`}>{row.brutAgirlik != null ? `${row.brutAgirlik} kg` : '—'}</td>
                    <td className={`py-4 px-6 ${textSub}`}>{row.netAgirlik != null ? `${row.netAgirlik} kg` : '—'}</td>
                    <td className={`py-4 px-6 ${textSub}`}>{row.hurdaOrani != null ? `%${row.hurdaOrani}` : '—'}</td>
                    <td className="py-4 px-6">
                      <div className="flex items-center gap-1.5 min-w-0 w-[88px] max-w-[88px]">
                        <div
                          className={`flex-1 min-w-0 h-2 rounded-full overflow-hidden transition-all duration-500 ${
                            isDark ? 'bg-gray-700' : 'bg-gray-200'
                          }`}
                        >
                          <div
                            className={`h-full rounded-full ${barRenk} transition-all duration-500`}
                            style={{ width: `${dolulukYuzde}%` }}
                          />
                        </div>
                        <span className={`text-xs font-semibold tabular-nums w-8 ${textSub}`}>{dolulukYuzde}%</span>
                      </div>
                    </td>
                    <td className={`py-4 px-6 ${textSub}`}>{row.kritik}</td>
                    <td className={`py-4 px-6 font-bold ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{row.miktar}</td>
                    <td className={`py-4 px-6 ${textSub}`}>
                      <span className="font-medium whitespace-nowrap">{toplamMaliyet} ₺</span>
                      <span className={`block text-xs ${textSub} whitespace-nowrap`}>{row.birimMaliyet} ₺/br</span>
                    </td>
                    <td className={`py-4 px-6 ${textSub}`}>
                      <span className="font-semibold text-green-600 dark:text-green-400 whitespace-nowrap">{toplamFiyat} ₺</span>
                      <span className={`block text-xs ${textSub} whitespace-nowrap`}>{row.birimFiyat} ₺/br</span>
                    </td>
                    <td className="py-4 px-6">
                      <span className={`text-xs font-bold px-3 py-1 rounded-full border flex w-max items-center gap-1 ${durumBadgeCls}`}>
                        {kritik ? '⚠ Kritik' : '✔ Yeterli'}
                      </span>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default Stok;
