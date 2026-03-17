import React, { useState, useMemo, useCallback, useEffect } from 'react';
import { TrendingDown, TrendingUp, Pencil, RefreshCw } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import { getSiparisler } from 'services';

const AY_ADLARI = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];

/** Kelime başlarını büyük yapar */
const toTitleCase = (str) =>
  (str || '').split(/\s+/).filter(Boolean).map((w) => w.charAt(0).toUpperCase() + w.slice(1)).join(' ') || str || '';

/** Sevk Edildi siparişlerinden aylık satış toplamları (tarih: YYYY-MM-DD veya DD.MM.YYYY) */
const aylikSatislarFromSiparisler = (siparisler) => {
  const byMonth = {};
  siparisler.forEach((s) => {
    const tutar = s.miktar * (s.birimFiyat ?? 0);
    const tarih = (s.tarih || '').split('T')[0].trim();
    let ay, yil;
    if (tarih.includes('-')) {
      const [y, m] = tarih.split('-');
      yil = y;
      ay = parseInt(m, 10) - 1;
    } else {
      const parts = tarih.split('.');
      if (parts.length < 3) return;
      ay = parseInt(parts[1], 10) - 1;
      yil = parts[2];
    }
    if (!yil || Number.isNaN(ay) || ay < 0 || ay > 11) return;
    const key = `${ay + 1}.${yil}`;
    if (!byMonth[key]) byMonth[key] = { ay, yil, toplam: 0, byMusteri: {} };
    byMonth[key].toplam += tutar;
    const mKey = s.musteriAdi || 'Belirsiz';
    byMonth[key].byMusteri[mKey] = (byMonth[key].byMusteri[mKey] || 0) + tutar;
  });
  return Object.entries(byMonth)
    .map(([key, v]) => ({
      aciklama: `${AY_ADLARI[v.ay]} ${v.yil} satışları`,
      tutar: v.toplam,
      yil: parseInt(v.yil, 10),
      ay: v.ay,
      musteriDetay: Object.entries(v.byMusteri).map(([unvan, t]) => ({ unvan, tutar: t })),
    }))
    .sort((a, b) => (a.yil !== b.yil ? a.yil - b.yil : a.ay - b.ay));
};

const DURUM_SEVK_EDILDI = 'Sevk Edildi';
const DURUMLAR_TAHMINI = ['Beklemede', 'Onaylandı', 'Üretimde'];

const Muhasebe = ({ isDark, giderList = [], onRefresh, onMuhasebeDuzenle }) => {
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [siparisler, setSiparisler] = useState([]);

  useEffect(() => {
    getSiparisler().then(setSiparisler).catch(() => setSiparisler([]));
  }, []);

  const sabitGiderler = useMemo(() => giderList.filter((g) => g.tip === 'sabit'), [giderList]);
  const degiskenGiderler = useMemo(() => giderList.filter((g) => g.tip === 'degisken'), [giderList]);

  const toplamSabit = sabitGiderler.reduce((s, g) => s + g.tutar, 0);
  const toplamDegisken = degiskenGiderler.reduce((s, g) => s + g.tutar, 0);
  const toplamGider = toplamSabit + toplamDegisken;

  const sevkEdilenSiparisler = useMemo(
    () => siparisler.filter((s) => s.durum === DURUM_SEVK_EDILDI),
    [siparisler]
  );
  const satislar = useMemo(() => aylikSatislarFromSiparisler(sevkEdilenSiparisler), [sevkEdilenSiparisler]);
  const ortalamaGelir = useMemo(
    () => (satislar.length ? Math.round(satislar.reduce((s, x) => s + x.tutar, 0) / satislar.length) : 0),
    [satislar]
  );

  const tahminiSiparisler = useMemo(
    () => siparisler.filter((s) => DURUMLAR_TAHMINI.includes(s.durum)),
    [siparisler]
  );
  const tahminiGelir = useMemo(
    () => tahminiSiparisler.reduce((acc, s) => acc + s.miktar * (s.birimFiyat ?? 0), 0),
    [tahminiSiparisler]
  );

  const netKar = ortalamaGelir - toplamGider;

  const handleRefresh = useCallback(() => {
    onRefresh?.();
    getSiparisler().then(setSiparisler).catch(() => setSiparisler([]));
  }, [onRefresh]);

  const formatTL = (n) => (n != null ? n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '—');

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap justify-end items-center gap-2">
        <button
          type="button"
          onClick={handleRefresh}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold border transition-colors ${isDark ? 'border-gray-600 text-gray-300 hover:bg-gray-700' : 'border-gray-300 text-gray-700 hover:bg-gray-100'}`}
            title="Gider ve sipariş listesini yenile"
          >
            <RefreshCw size={18} /> Yenile
        </button>
        {onMuhasebeDuzenle && (
          <button
            type="button"
            onClick={onMuhasebeDuzenle}
            className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold bg-blue-600 hover:bg-blue-700 text-white transition-colors"
          >
            <Pencil size={18} /> Düzenle
          </button>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Sol: Giderler */}
        <div className={`p-6 rounded-xl shadow-sm border transition-colors duration-300 ${bgCard}`}>
          <h2 className={`text-lg font-bold mb-4 flex items-center gap-2 ${textTitle}`}>
            <TrendingDown className="text-red-500" size={22} /> Giderler
          </h2>

          <div className="space-y-4">
            <h3 className={`text-sm font-semibold uppercase tracking-wider ${textSub}`}>{toTitleCase('Sabit aylık giderler')}</h3>
            <div className="space-y-2">
              {sabitGiderler.map((g) => (
                <div
                  key={g.id}
                  className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border ${isDark ? 'bg-gray-700/40 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
                >
                  <span className={`font-medium ${textTitle}`}>{g.kalem}</span>
                  <span className={`font-semibold tabular-nums ${textTitle}`}>{formatTL(g.tutar)} ₺</span>
                </div>
              ))}
              <div className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border-2 ${borderCol} ${isDark ? 'bg-gray-700/60' : 'bg-gray-100'}`}>
                <span className={`font-semibold ${textTitle}`}>{toTitleCase('Toplam sabit')}</span>
                <span className={`font-bold tabular-nums ${textTitle}`}>{formatTL(toplamSabit)} ₺</span>
              </div>
            </div>

            <h3 className={`text-sm font-semibold uppercase tracking-wider mt-4 ${textSub}`}>{toTitleCase('Değişken giderler (üretim)')}</h3>
            <div className="space-y-2">
              {degiskenGiderler.map((g) => (
                <div
                  key={g.id}
                  className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border ${isDark ? 'bg-gray-700/40 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
                >
                  <span className={`font-medium ${textTitle}`}>{g.kalem}</span>
                  <span className={`font-semibold tabular-nums ${textTitle}`}>{formatTL(g.tutar)} ₺</span>
                </div>
              ))}
              <div className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border-2 ${borderCol} ${isDark ? 'bg-gray-700/60' : 'bg-gray-100'}`}>
                <span className={`font-semibold ${textTitle}`}>{toTitleCase('Toplam değişken')}</span>
                <span className={`font-bold tabular-nums ${textTitle}`}>{formatTL(toplamDegisken)} ₺</span>
              </div>
            </div>

            <div className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border-2 ${isDark ? 'bg-red-900/20 border-red-800' : 'bg-red-50 border-red-200'}`}>
              <span className={`font-bold ${textTitle}`}>{toTitleCase('Toplam gider (aylık)')}</span>
              <span className="font-bold text-red-600 dark:text-red-400 tabular-nums">{formatTL(toplamGider)} ₺</span>
            </div>
          </div>
        </div>

        {/* Orta: Gelirler */}
        <div className={`p-6 rounded-xl shadow-sm border transition-colors duration-300 ${bgCard}`}>
          <h2 className={`text-lg font-bold mb-4 flex items-center gap-2 ${textTitle}`}>
            <TrendingUp className="text-green-500" size={22} /> Gelirler
          </h2>

          <div className="space-y-4">
            <h3 className={`text-sm font-semibold uppercase tracking-wider ${textSub}`}>{toTitleCase('Müşterilere yapılan satışlar (Sevk Edildi)')}</h3>
            <p className={`text-xs ${textSub} -mt-1`}>{toTitleCase('Ortalama aylık gelir, sevk edilen siparişlere göre hesaplanır.')}</p>
            <div className="space-y-2">
              {satislar.length === 0 ? (
                <div className={`px-4 py-3 rounded-lg border ${isDark ? 'bg-gray-700/40 border-gray-600' : 'bg-gray-50 border-gray-200'} ${textSub}`}>
                  {toTitleCase('Sevk edilmiş sipariş yok')}
                </div>
              ) : (
                satislar.map((s) => (
                  <div key={s.aciklama} className="space-y-2">
                    <div
                      className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border ${isDark ? 'bg-gray-700/40 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
                    >
                      <span className={`font-medium ${textTitle}`}>{toTitleCase(s.aciklama)}</span>
                      <span className={`font-semibold tabular-nums ${textTitle}`}>{formatTL(s.tutar)} ₺</span>
                    </div>
                    {s.musteriDetay && s.musteriDetay.length > 0 && (
                      <div className="space-y-2 pl-2 border-l-2 border-gray-400/50">
                        {s.musteriDetay.map((m) => (
                          <div
                            key={m.unvan}
                            className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border ${isDark ? 'bg-gray-700/40 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
                          >
                            <span className={`font-medium ${textTitle}`}>{m.unvan}</span>
                            <span className={`font-semibold tabular-nums ${textTitle}`}>{formatTL(m.tutar)} ₺</span>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                ))
              )}
              <div className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border-2 ${borderCol} ${isDark ? 'bg-gray-700/60' : 'bg-gray-100'}`}>
                <span className={`font-semibold ${textTitle}`}>{toTitleCase('Ortalama aylık')}</span>
                <span className={`font-bold tabular-nums ${textTitle}`}>{formatTL(ortalamaGelir)} ₺</span>
              </div>
            </div>

            <h3 className={`text-sm font-semibold uppercase tracking-wider mt-4 ${textSub}`}>{toTitleCase('Tahmini (bekleyen siparişler)')}</h3>
            <div className={`flex flex-col gap-2 w-full px-4 py-4 rounded-lg border ${isDark ? 'bg-gray-700/40 border-gray-600' : 'bg-gray-50 border-gray-200'}`}>
              <p className={`text-sm ${textSub}`}>
                {toTitleCase('Beklemede, Onaylandı ve Üretimde durumundaki siparişlerin toplam tahmini geliri.')}
              </p>
              <div className="flex justify-between items-center">
                <span className={`text-sm ${textSub}`}>{toTitleCase('Sipariş adedi')}</span>
                <span className={`font-semibold ${textTitle}`}>{tahminiSiparisler.length} sipariş</span>
              </div>
              <div className="flex justify-between items-center pt-1 border-t border-gray-600/50">
                <span className={`font-semibold ${textTitle}`}>{toTitleCase('Tahmini gelir')}</span>
                <span className="font-bold text-green-600 dark:text-green-400 tabular-nums">{formatTL(tahminiGelir)} ₺</span>
              </div>
            </div>

            <div className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border-2 ${isDark ? 'bg-green-900/20 border-green-800' : 'bg-green-50 border-green-200'}`}>
              <span className={`font-bold ${textTitle}`}>{toTitleCase('Ortalama gelir (aylık)')}</span>
              <span className="font-bold text-green-600 dark:text-green-400 tabular-nums">{formatTL(ortalamaGelir)} ₺</span>
            </div>

            <div className={`flex justify-between items-center w-full px-4 py-3 rounded-lg border-2 ${netKar >= 0 ? (isDark ? 'bg-emerald-900/20 border-emerald-800' : 'bg-emerald-50 border-emerald-200') : (isDark ? 'bg-red-900/20 border-red-800' : 'bg-red-50 border-red-200')}`}>
              <span className={`font-bold ${textTitle}`}>{toTitleCase('Net kar (aylık)')}</span>
              <span className={`font-bold tabular-nums ${netKar >= 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'}`}>
                {formatTL(netKar)} ₺
              </span>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
};

export default Muhasebe;
