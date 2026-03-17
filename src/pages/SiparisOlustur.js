import React, { useState, useEffect, useMemo } from 'react';
import { Plus, ShoppingCart } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import ThemeDropdown from 'components/ThemeDropdown';
import { useToast } from 'contexts/ToastContext';
import NumberStepperInput from 'components/NumberStepperInput';
import { getUrunler, getSiparisler, createSiparis, updateSiparisStatus } from 'services';

/** Birim üretim süresine (saat) göre tahmini süre metni: "X saat" veya "X gün Y saat" */
const tahminiSureMetinBirimSureden = (toplamSaat) => {
  if (toplamSaat == null || toplamSaat <= 0) return '—';
  if (toplamSaat < 1) return `${(toplamSaat * 60).toFixed(0)} dk`;
  const gun = Math.floor(toplamSaat / 8);
  const saat = Math.round((toplamSaat % 8) * 10) / 10;
  if (gun >= 1 && saat > 0) return `${gun} gün ${saat} saat`;
  if (gun >= 1) return `${gun} gün`;
  return `${toplamSaat.toFixed(1)} saat`;
};

/** Miktar + (birim süre saat veya günlük üretim) ile tahmini üretim süresi metni. Öncelik: birim süre. */
const tahminiUretimSuresiMetin = (miktar, gunlukUretim, birimSure) => {
  const toplamSaat = birimSure > 0 ? miktar * birimSure : null;
  if (toplamSaat != null && toplamSaat > 0) return tahminiSureMetinBirimSureden(toplamSaat);
  if (!gunlukUretim || gunlukUretim <= 0) return '—';
  const gun = miktar / gunlukUretim;
  if (gun >= 1) return `${Math.ceil(gun)} gün`;
  const saat = Math.round(gun * 8);
  if (saat > 0) return `${saat} saat`;
  return '< 1 saat';
};

const ACIK_SIPARIS_DURUMLARI = ['Beklemede', 'Onaylandı', 'Üretimde', 'pending'];
const SIPARIS_DURUM_OPTIONS = ['Beklemede', 'Onaylandı', 'Üretimde', 'Sevk Edildi', 'İptal'];

const durumSelectCls = (isDark) =>
  isDark
    ? 'bg-gray-700 border-gray-600 text-gray-100 focus:border-blue-500 focus:ring-1 focus:ring-blue-500/50'
    : 'bg-white border-gray-300 text-gray-800 focus:border-blue-500 focus:ring-1 focus:ring-blue-400/40';

/** Aylık üretim kapasitesi (saat). Doluluk = (açık siparişler + bu sipariş) tahmini toplam saat / bu değer. */
const KAPASITE_SAAT = 200;

const SiparisOlustur = ({ isDark, musteriList = [], onRefreshMusteri }) => {
  const { toast } = useToast();
  const { bgCard, textTitle, textSub } = getThemeClasses(isDark);
  const [urunler, setUrunler] = useState([]);
  const [siparisler, setSiparisler] = useState([]);
  const [seciliMusteri, setSeciliMusteri] = useState('');
  const [seciliUrun, setSeciliUrun] = useState('');
  const [miktar, setMiktar] = useState(10);
  const [submitting, setSubmitting] = useState(false);
  const [durumUpdatingId, setDurumUpdatingId] = useState(null);

  const musterilerDropdown = useMemo(
    () =>
      musteriList.map((m) => ({
        id: m.idKod ?? m.id,
        unvan: m.unvan || `${m.ad} ${m.soyad}`.trim(),
        yetkili: `${m.ad} ${m.soyad}`.trim(),
        tel: m.telefon || '',
      })),
    [musteriList]
  );

  useEffect(() => {
    getUrunler().then(setUrunler).catch(() => setUrunler([]));
    getSiparisler().then(setSiparisler).catch(() => setSiparisler([]));
  }, []);

  useEffect(() => {
    if (musterilerDropdown.length && !seciliMusteri) setSeciliMusteri(musterilerDropdown[0].id);
  }, [musterilerDropdown, seciliMusteri]);
  useEffect(() => {
    if (urunler.length && seciliUrun === '') setSeciliUrun(urunler[0]?.id ?? '');
  }, [urunler, seciliUrun]);

  const urun = urunler.find((u) => u.id === seciliUrun || u.id === Number(seciliUrun));

  // Adet/miktar girince otomatik hesaplamalar
  const birimSure = urun?.birimSure ?? 0;
  const birimMaliyet = urun?.birimMaliyet ?? 0;
  const gunlukUretim = urun?.gunlukUretim ?? 0;
  const tahminiSure = miktar * birimSure; // saat (birim süreye göre)
  const tahminiUretimSuresi = tahminiUretimSuresiMetin(miktar, gunlukUretim, birimSure); // öncelik: birim süre
  const toplamMaliyet = (miktar * birimMaliyet).toFixed(2);
  const toplamSatis = urun ? (urun.birimFiyat * miktar).toFixed(2) : '0.00';
  const kar = urun ? (miktar * (urun.birimFiyat - birimMaliyet)).toFixed(2) : '0.00';

  const findUrunBySiparis = (s) =>
    urunler.find((x) =>
      x.productId === s.urunId ||
      x.id === s.urunKodu ||
      x.urun_kodu === s.urunKodu ||
      x.ad === s.urunAdi
    );
  const acikSiparisler = siparisler.filter((s) => ACIK_SIPARIS_DURUMLARI.includes(s.durum));
  const acikToplamSure = acikSiparisler.reduce((acc, s) => {
    const u = findUrunBySiparis(s);
    return acc + s.miktar * (u?.birimSure ?? 0);
  }, 0);
  const yeniSiparisSure = tahminiSure;
  const toplamDolulukSure = acikToplamSure + yeniSiparisSure;
  const kapasiteDolulukYuzde = Math.min(100, Math.round((toplamDolulukSure / KAPASITE_SAAT) * 100));
  const acikYuzde = Math.min(100, (acikToplamSure / KAPASITE_SAAT) * 100);
  const yeniSiparisYuzde = Math.min(100 - acikYuzde, (yeniSiparisSure / KAPASITE_SAAT) * 100);

  const handleDurumChange = async (siparisId, yeniDurum) => {
    if (!siparisId || !yeniDurum) return;
    setDurumUpdatingId(siparisId);
    const prev = siparisler;
    setSiparisler((list) =>
      list.map((s) => (s.id === siparisId ? { ...s, durum: yeniDurum } : s))
    );
    try {
      await updateSiparisStatus(siparisId, yeniDurum);
    } catch (err) {
      setSiparisler(prev);
      toast(err?.message || 'Sipariş durumu güncellenemedi');
    } finally {
      setDurumUpdatingId(null);
    }
  };

  const handleSiparisEkle = async (e) => {
    e.preventDefault();
    const musteri = musterilerDropdown.find((m) => m.id === seciliMusteri || String(m.id) === String(seciliMusteri));
    const musteriAdi = musteri?.unvan ?? '';
    const urunAdi = urun?.ad ?? '';
    if (!musteriAdi || !urunAdi) {
      toast('Lütfen müşteri ve ürün seçin.');
      return;
    }
    if (!urun?.productId) {
      toast('Ürün ID bulunamadı. Ürün listesini yenileyin.');
      return;
    }
    setSubmitting(true);
    try {
      await createSiparis({
        customerName: musteriAdi,
        productId: urun.productId,
        quantity: miktar,
        salePrice: urun?.birimFiyat ?? null,
      });
      toast('Sipariş oluşturuldu');
      getSiparisler().then(setSiparisler).catch(() => setSiparisler([]));
    } catch (err) {
      toast(err?.message || 'Sipariş oluşturulamadı');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Yeni Sipariş Formu */}
      <div className={`p-6 rounded-xl shadow-sm border transition-colors duration-300 ${bgCard}`}>
        <h2 className={`text-lg font-bold mb-4 flex items-center gap-2 ${textTitle}`}>
          <Plus size={22} className="text-blue-500" /> Yeni Sipariş Oluştur
        </h2>
        <form onSubmit={handleSiparisEkle} className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="min-w-0">
            <ThemeDropdown
              label="Müşteri"
              options={musterilerDropdown}
              value={seciliMusteri}
              onChange={setSeciliMusteri}
              renderLabel={(m) => m.unvan}
              placeholder="Müşteri seçin"
              isDark={isDark}
            />
          </div>
          <div className="min-w-0">
            <ThemeDropdown
              label="Ürün"
              options={urunler}
              value={seciliUrun}
              onChange={(v) => setSeciliUrun(v)}
              renderLabel={(u) => `${u.ad} (${u.birim})`}
              placeholder="Ürün seçin"
              isDark={isDark}
            />
          </div>
          <div className="min-w-0 flex flex-col">
            <label className={`block text-sm font-medium mb-1 ${textSub}`}>Miktar (Adet / birim)</label>
            <NumberStepperInput
              value={miktar}
              onChange={(e) => setMiktar(Number(e.target.value) || 1)}
              min={1}
              isDark={isDark}
              className="w-full min-w-0"
            />
          </div>
          <div className="min-w-0 flex flex-col justify-end gap-2">
            <div className={`text-sm space-y-1 ${textSub}`}>
              <div>Tahmini süre: <span className={`font-semibold ${textTitle}`}>{tahminiSure.toFixed(1)} saat</span></div>
              <div>Tahmini üretim süresi: <span className={`font-semibold ${textTitle}`}>{tahminiUretimSuresi}</span> {birimSure > 0 ? <span className="text-xs">(birim: {birimSure} saat)</span> : <span className="text-xs">({gunlukUretim} {urun?.birim ?? ''}/gün)</span>}</div>
              <div>Maliyet: <span className={`font-semibold ${isDark ? 'text-amber-200/90' : 'text-amber-800/90'}`}>{toplamMaliyet} ₺</span></div>
              <div>Satış fiyatı: <span className={`font-semibold ${isDark ? 'text-emerald-200/90' : 'text-emerald-800/90'}`}>{toplamSatis} ₺</span></div>
              <div>Kar: <span className={`font-semibold ${isDark ? 'text-sky-200/90' : 'text-sky-800/90'}`}>{kar} ₺</span></div>
            </div>
            <button
              type="submit"
              disabled={submitting || !urunler.length}
              className="flex items-center justify-center gap-2 px-4 py-2 rounded-lg bg-blue-600 hover:bg-blue-700 text-white font-semibold transition-colors mt-2 disabled:opacity-50"
            >
              <ShoppingCart size={18} /> {submitting ? 'Ekleniyor...' : 'Sipariş Ekle'}
            </button>
          </div>
        </form>

        {/* Kapasite doluluk oranı (açık siparişler + yeni sipariş) */}
        <div className={`mt-6 pt-6 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
          <h3 className={`text-sm font-semibold mb-2 ${textSub}`}>Kapasite doluluk oranı (açık siparişler + bu sipariş)</h3>
          <div className="flex items-center gap-4">
            <div className={`flex-1 h-3 rounded-full overflow-hidden flex ${isDark ? 'bg-gray-700' : 'bg-gray-200'}`}>
              {/* Açık siparişler */}
              {acikYuzde > 0 && (
                <div
                  className={`h-full min-w-0 transition-all duration-500 ${
                    kapasiteDolulukYuzde >= 90 ? 'bg-red-500' : kapasiteDolulukYuzde >= 70 ? 'bg-amber-500' : 'bg-blue-500'
                  } ${yeniSiparisYuzde > 0 ? 'rounded-l-full' : 'rounded-full'}`}
                  style={{ width: `${acikYuzde}%` }}
                />
              )}
              {/* Bu sipariş (eklenen miktar) */}
              {yeniSiparisYuzde > 0 && (
                <div
                  className={`h-full min-w-0 bg-emerald-500 transition-all duration-500 ${acikYuzde > 0 ? 'rounded-r-full' : 'rounded-full'}`}
                  style={{ width: `${yeniSiparisYuzde}%` }}
                />
              )}
            </div>
            <span className={`text-sm font-bold tabular-nums w-12 ${textTitle}`}>{kapasiteDolulukYuzde}%</span>
          </div>
          <p className={`text-xs mt-1 ${textSub}`}>
            Açık siparişler: {acikToplamSure.toFixed(1)} saat + bu sipariş: {yeniSiparisSure.toFixed(1)} saat = {toplamDolulukSure.toFixed(1)} / {KAPASITE_SAAT} saat (aylık kapasite). Süre = miktar × ürün birim üretim süresi (saat).
          </p>
          <p className={`text-xs mt-0.5 ${textSub}`}>
            <span className={isDark ? 'text-blue-400' : 'text-blue-600'}>■</span> Açık siparişler · <span className="text-emerald-500">■</span> Bu sipariş
          </p>
        </div>
      </div>

      {/* Sipariş Listesi (Mock) */}
      <div className={`p-6 rounded-xl shadow-sm border overflow-hidden transition-colors duration-300 ${bgCard}`}>
        <h2 className={`text-lg font-bold mb-6 ${textTitle}`}>Son Siparişler</h2>
        <div className="overflow-x-auto">
          <table className="min-w-full text-left">
            <thead
              className={`${isDark ? 'bg-gray-700/50 text-gray-300' : 'bg-gray-50 text-gray-600'} font-semibold uppercase text-xs tracking-wider`}
            >
              <tr>
                <th className="py-4 px-6 rounded-l-lg">Sipariş No</th>
                <th className="py-4 px-6">Müşteri</th>
                <th className="py-4 px-6">Ürün</th>
                <th className="py-4 px-6">Miktar</th>
                <th className="py-4 px-6">Tahmini üretim süresi</th>
                <th className="py-4 px-6">Birim Fiyat</th>
                <th className="py-4 px-6">Toplam</th>
                <th className="py-4 px-6">Tarih</th>
                <th className="py-4 px-6 rounded-r-lg">Durum</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {siparisler.map((s) => {
                const m = musterilerDropdown.find(
                  (x) =>
                    x.id === s.musteriId ||
                    String(x.id) === String(s.musteriId) ||
                    (s.musteriAdi && (x.unvan === s.musteriAdi || (x.yetkili && x.yetkili.trim() === s.musteriAdi)))
                );
                const u = findUrunBySiparis(s);
                const toplam = (s.miktar * s.birimFiyat).toFixed(2);
                const tahminiSureSatir = tahminiUretimSuresiMetin(s.miktar, u?.gunlukUretim, u?.birimSure);
                return (
                  <tr
                    key={s.id ?? s.no ?? s.urunId + '-' + s.miktar}
                    className={`transition duration-150 ${isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}`}
                  >
                    <td className={`py-4 px-6 font-mono ${textSub}`}>{s.no ?? `SIP-${s.id}`}</td>
                    <td className={`py-4 px-6 font-medium ${textTitle}`}>{m?.unvan ?? s.musteriAdi ?? s.musteriId ?? '—'}</td>
                    <td className={`py-4 px-6 ${textSub}`}>{u?.ad ?? s.urunAdi ?? s.urunId ?? '—'}</td>
                    <td className={`py-4 px-6 font-bold ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                      {s.miktar} {u?.birim ?? ''}
                    </td>
                    <td className={`py-4 px-6 ${textSub}`}>{tahminiSureSatir}</td>
                    <td className={`py-4 px-6 ${textSub}`}>{s.birimFiyat} ₺</td>
                    <td className={`py-4 px-6 font-semibold ${textTitle}`}>{toplam} ₺</td>
                    <td className={`py-4 px-6 ${textSub}`}>{s.tarih ?? '—'}</td>
                    <td className="py-4 px-6">
                      <select
                        value={s.durum}
                        onChange={(e) => handleDurumChange(s.id, e.target.value)}
                        disabled={durumUpdatingId === s.id}
                        className={`w-full min-w-[130px] px-3 py-2 rounded-lg border text-sm font-medium outline-none transition-colors cursor-pointer disabled:opacity-60 disabled:cursor-not-allowed ${durumSelectCls(isDark)}`}
                      >
                        {SIPARIS_DURUM_OPTIONS.map((opt) => (
                          <option key={opt} value={opt}>
                            {opt}
                          </option>
                        ))}
                      </select>
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

export default SiparisOlustur;
