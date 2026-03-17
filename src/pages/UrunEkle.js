import React, { useState } from 'react';
import { ArrowLeft, Box } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import NumberStepperInput from 'components/NumberStepperInput';
import { useToast } from 'contexts/ToastContext';
import ThemeDropdown from 'components/ThemeDropdown';
import { createUrun } from 'services';

const BIRIM_OPTIONS = [
  { id: 'saat', ad: 'Saat' },
  { id: 'dakika', ad: 'Dakika' },
];

const UrunEkle = ({ isDark, makineList = [], onBack, onRefresh }) => {
  const { toast } = useToast();
  const { textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [submitting, setSubmitting] = useState(false);
  const [urunKodu, setUrunKodu] = useState('');
  const [urunAdi, setUrunAdi] = useState('');
  const [hammaddeTuru, setHammaddeTuru] = useState('');
  const [birimUretimSuresi, setBirimUretimSuresi] = useState('');
  const [birimSureBirim, setBirimSureBirim] = useState('');
  const [brutAgirlik, setBrutAgirlik] = useState('');
  const [netAgirlik, setNetAgirlik] = useState('');
  const [hurdaOrani, setHurdaOrani] = useState('');
  const [gunlukUretim, setGunlukUretim] = useState('');
  const [birimMaliyet, setBirimMaliyet] = useState('');
  const [birimFiyat, setBirimFiyat] = useState('');
  const [guncelStok, setGuncelStok] = useState('');
  const [makineFlags, setMakineFlags] = useState({});

  const toggleMakine = (id) => {
    setMakineFlags((prev) => ({ ...prev, [id]: !prev[id] }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!urunKodu.trim()) {
      toast('Ürün kodu zorunludur.');
      return;
    }
    setSubmitting(true);
    try {
      const birimSureSayi = parseFloat(String(birimUretimSuresi).replace(',', '.')) || 0;
      const birimSureSaat =
        birimSureSayi > 0 && birimSureBirim === 'dakika'
          ? birimSureSayi / 60
          : birimSureSayi > 0
            ? birimSureSayi
            : null;
      const productData = {
        urun_kodu: urunKodu.trim(),
        ad: (urunAdi.trim() || urunKodu.trim()) || '',
        urun_adi: (urunAdi.trim() || urunKodu.trim()) || '',
        ham_madde: hammaddeTuru.trim() || '',
        malzeme_tipi: hammaddeTuru.trim() || '-',
        pres_kategorisi: '-',
        base_cost: Number(birimMaliyet) || 0,
        birimMaliyet: Number(birimMaliyet) || 0,
        birimFiyat: Number(birimFiyat) || 0,
        sale_price: Number(birimFiyat) || null,
        birim_uretim_suresi_saat: birimSureSaat,
        brut_agirlik_kg: Number(brutAgirlik) || 0,
        net_agirlik_kg: Number(netAgirlik) || 0,
        hurda_orani: Number(hurdaOrani) || 0,
        current_stock: Math.max(0, parseInt(String(guncelStok), 10) || 0),
        machines: makineList.filter((m) => !!makineFlags[m.id]).map((m) => ({ Id: m.id, machine_name: m.ad || '', details: '' })),
      };
      await createUrun(productData);
      toast('Ürün eklendi');
      onRefresh?.();
      onBack();
    } catch (err) {
      toast(err?.message || 'Ürün eklenemedi');
    } finally {
      setSubmitting(false);
    }
  };

  const inputCls = `w-full p-3 rounded-lg border bg-transparent ${isDark ? 'border-gray-600 text-white placeholder-gray-500' : 'border-gray-300 text-gray-900 placeholder-gray-400'}`;
  const checkboxLabelCls = (checked) =>
    `flex items-center gap-2 cursor-pointer rounded-lg px-3 py-2 border transition-colors ${
      checked
        ? isDark
          ? 'bg-blue-900/40 border-blue-600'
          : 'bg-blue-50 border-blue-300'
        : isDark
          ? 'border-gray-600 hover:border-gray-500'
          : 'border-gray-200 hover:border-gray-300'
    }`;
  const checkboxTextCls = (checked) =>
    `text-sm font-medium ${checked ? (isDark ? 'text-blue-200' : 'text-blue-800') : isDark ? 'text-gray-200' : 'text-gray-900'}`;

  const boxSectionCls = `${isDark ? 'border-gray-600 bg-gray-700/30' : 'border-gray-300 bg-gray-50'} p-4 rounded-lg border`;
  const boxGridCls = 'grid gap-3';

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
      <button
        type="button"
        onClick={onBack}
        className={`flex items-center gap-2 mb-6 text-sm font-medium ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`}
      >
        <ArrowLeft size={18} /> Listeye dön
      </button>

      <h2 className={`text-xl font-bold mb-6 border-b pb-2 ${textTitle} ${borderCol}`}>Yeni Ürün Ekle</h2>

      <form onSubmit={handleSubmit} className="max-w-2xl space-y-5">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Ürün Kodu *</label>
            <input
              type="text"
              value={urunKodu}
              onChange={(e) => setUrunKodu(e.target.value)}
              placeholder="Örn: D3043, D3213"
              className={inputCls}
              required
            />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Ürün Adı (opsiyonel)</label>
            <input
              type="text"
              value={urunAdi}
              onChange={(e) => setUrunAdi(e.target.value)}
              placeholder="Listede görünecek isim"
              className={inputCls}
            />
          </div>
        </div>

        <div>
          <label className={`block text-sm font-medium mb-2 ${textSub}`}>Hammadde türü</label>
          <input
            type="text"
            value={hammaddeTuru}
            onChange={(e) => setHammaddeTuru(e.target.value)}
            placeholder="Örn: Çelik, Sac"
            className={inputCls}
          />
        </div>

        <div className="flex flex-col sm:flex-row sm:gap-4 gap-4">
          <div className="flex-1 min-w-0 flex flex-col">
            <label className={`text-sm font-medium mb-2 ${textSub}`}>Birim üretim süresi</label>
            <NumberStepperInput
              value={birimUretimSuresi}
              onChange={(e) => setBirimUretimSuresi(e.target.value)}
              min={0}
              step={0.01}
              placeholder="Örn: 0.5"
              isDark={isDark}
              className="w-full"
            />
          </div>
          <div className="sm:w-40 flex flex-col">
            <label className={`text-sm font-medium mb-2 ${textSub}`}>Birim *</label>
            <ThemeDropdown
              options={BIRIM_OPTIONS}
              value={birimSureBirim}
              onChange={setBirimSureBirim}
              renderLabel={(o) => o.ad}
              placeholder="Seçiniz"
              isDark={isDark}
            />
          </div>
        </div>
        <p className={`text-xs ${textSub} -mt-1`}>
          Analizlerin doğru yapılabilmesi için lütfen birim üretim süresini doğru ve eksiksiz girdiğinizden emin olunuz.
        </p>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Brüt ağırlık (kg)</label>
            <NumberStepperInput
              value={brutAgirlik}
              onChange={(e) => setBrutAgirlik(e.target.value)}
              min={0}
              step={0.1}
              placeholder="Örn: 6"
              isDark={isDark}
              className="w-full"
            />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Net ağırlık (kg)</label>
            <NumberStepperInput
              value={netAgirlik}
              onChange={(e) => setNetAgirlik(e.target.value)}
              min={0}
              step={0.1}
              placeholder="Örn: 5.7"
              isDark={isDark}
              className="w-full"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Hurda oranı (%)</label>
            <NumberStepperInput
              value={hurdaOrani}
              onChange={(e) => setHurdaOrani(e.target.value)}
              min={0}
              max={100}
              step={0.1}
              placeholder="Örn: 5"
              isDark={isDark}
              className="w-full"
            />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Günlük üretim (kapasite)</label>
            <NumberStepperInput
              value={gunlukUretim}
              onChange={(e) => setGunlukUretim(e.target.value)}
              min={0}
              step={1}
              placeholder="Örn: 120"
              isDark={isDark}
              className="w-full"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Birim maliyet (₺)</label>
            <NumberStepperInput
              value={birimMaliyet}
              onChange={(e) => setBirimMaliyet(e.target.value)}
              min={0}
              step={0.01}
              placeholder="Örn: 12.50"
              isDark={isDark}
              className="w-full"
            />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Satış fiyatı (₺)</label>
            <NumberStepperInput
              value={birimFiyat}
              onChange={(e) => setBirimFiyat(e.target.value)}
              min={0}
              step={0.01}
              placeholder="Örn: 18.00"
              isDark={isDark}
              className="w-full"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Güncel stok</label>
            <NumberStepperInput
              value={guncelStok}
              onChange={(e) => setGuncelStok(e.target.value)}
              min={0}
              step={1}
              placeholder="Örn: 0"
              isDark={isDark}
              className="w-full"
            />
          </div>
          <div aria-hidden="true" />
        </div>

        <div>
          <label className={`block text-sm font-medium mb-2 ${textSub}`}>Pres / Makine kullanımı</label>
          <div className={boxSectionCls}>
            {makineList.length === 0 ? (
              <p className={`text-sm ${textSub}`}>Henüz makine eklenmemiş. Üretim Hattından makine ekleyebilirsiniz.</p>
            ) : (
              <div className={`${boxGridCls} grid-cols-2 sm:grid-cols-4`}>
                {makineList.map((makine) => {
                  const checked = !!makineFlags[makine.id];
                  const label = makine.ad || makine.idKod || `Makine #${makine.id}`;
                  return (
                    <label key={makine.id} className={checkboxLabelCls(checked)}>
                      <input
                        type="checkbox"
                        checked={checked}
                        onChange={() => toggleMakine(makine.id)}
                        className="rounded border-gray-500 text-blue-600 focus:ring-blue-500"
                      />
                      <span className={checkboxTextCls(checked)}>{label}</span>
                    </label>
                  );
                })}
              </div>
            )}
          </div>
        </div>

        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={submitting}
            className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white font-semibold rounded-lg transition-colors"
          >
            <Box size={18} /> {submitting ? 'Kaydediliyor...' : 'Kaydet'}
          </button>
          <button
            type="button"
            onClick={onBack}
            className={`px-5 py-2.5 font-semibold rounded-lg border transition-colors ${isDark ? 'border-gray-600 text-gray-300 hover:bg-gray-700' : 'border-gray-300 text-gray-700 hover:bg-gray-100'}`}
          >
            İptal
          </button>
        </div>
      </form>
    </div>
  );
};

export default UrunEkle;
