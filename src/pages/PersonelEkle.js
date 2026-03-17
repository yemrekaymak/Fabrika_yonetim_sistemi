import React, { useState } from 'react';
import { ArrowLeft, UserPlus, Users, Trash2 } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import ThemeDropdown from 'components/ThemeDropdown';
import { useToast } from 'contexts/ToastContext';
import { createPersonel, deletePersonel } from 'services';
import { POZISYON_OPTIONS } from 'constants/departmanPozisyon';

const inputCls = (isDark) =>
  `w-full p-3 rounded-lg border bg-transparent ${isDark ? 'border-gray-600 text-white placeholder-gray-500' : 'border-gray-300 text-gray-900 placeholder-gray-400'}`;

const PersonelEkle = ({ isDark, personelList = [], onRefresh, onBack }) => {
  const { toast } = useToast();
  const [submitting, setSubmitting] = useState(false);
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [view, setView] = useState('menu'); // 'menu' | 'ekle' | 'cikar'
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [tcKimlikNo, setTcKimlikNo] = useState('');
  const [telefon, setTelefon] = useState('');
  const [pozisyon, setPozisyon] = useState('');
  const [brutMaas, setBrutMaas] = useState('');
  const [yol, setYol] = useState('');
  const [yemek, setYemek] = useState('');
  const [iseGirisTarihi, setIseGirisTarihi] = useState('');
  const [acilDurumKisi, setAcilDurumKisi] = useState('');
  const [acilDurumTel, setAcilDurumTel] = useState('');
  const [yillikIzinHakki, setYillikIzinHakki] = useState('14');
  const [kullanilanIzin, setKullanilanIzin] = useState('0');
  const [fazlaMesaiSaat, setFazlaMesaiSaat] = useState('');
  const [performansPuani, setPerformansPuani] = useState('');
  const [ortalamaGunlukUretim, setOrtalamaGunlukUretim] = useState('');
  const [devamsizlikGun, setDevamsizlikGun] = useState('');
  const [egitimSertifikalariText, setEgitimSertifikalariText] = useState(''); // satır veya virgülle ayrılmış

  const parseSertifikalar = (text) =>
    text
      .split(/[\n,]+/)
      .map((s) => s.trim())
      .filter(Boolean);

  const handleAdd = async (e) => {
    e.preventDefault();
    if (!firstName.trim() || !lastName.trim()) return;
    setSubmitting(true);
    try {
      const yeni = {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        tcKimlikNo: tcKimlikNo.trim() || null,
        telefon: telefon.trim() || null,
        department: null,
        pozisyon: pozisyon.trim() || null,
        maas: brutMaas !== '' ? parseFloat(brutMaas) : null,
        yol: yol !== '' ? parseFloat(yol) : null,
        yemek: yemek !== '' ? parseFloat(yemek) : null,
        iseGirisTarihi: iseGirisTarihi.trim() || null,
        acilDurumKisi: acilDurumKisi.trim() || null,
        acilDurumTel: acilDurumTel.trim() || null,
        yillikIzinHakki: yillikIzinHakki !== '' ? parseInt(yillikIzinHakki, 10) : 14,
        kullanilanIzin: kullanilanIzin !== '' ? parseInt(kullanilanIzin, 10) : 0,
        fazlaMesaiSaat: fazlaMesaiSaat !== '' ? parseFloat(fazlaMesaiSaat) : 0,
        performansPuani: performansPuani !== '' ? parseFloat(performansPuani) : null,
        ortalamaGunlukUretim: ortalamaGunlukUretim !== '' ? parseFloat(ortalamaGunlukUretim) : null,
        devamsizlikGun: devamsizlikGun !== '' ? parseInt(devamsizlikGun, 10) : null,
        egitimSertifikalari: parseSertifikalar(egitimSertifikalariText),
      };
      await createPersonel(yeni);
      setFirstName('');
      setLastName('');
      setTcKimlikNo('');
      setTelefon('');
      setPozisyon('');
      setBrutMaas('');
      setYol('');
      setYemek('');
      setIseGirisTarihi('');
      setAcilDurumKisi('');
      setAcilDurumTel('');
      setYillikIzinHakki('14');
      setKullanilanIzin('0');
      setFazlaMesaiSaat('');
      setPerformansPuani('');
      setOrtalamaGunlukUretim('');
      setDevamsizlikGun('');
      setEgitimSertifikalariText('');
      toast('Personel eklendi');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'Personel eklenemedi');
    } finally {
      setSubmitting(false);
    }
  };

  const handleRemove = async (id, adSoyad) => {
    const mesaj = adSoyad
      ? `${adSoyad} listeden çıkarmak istiyor musunuz?`
      : 'Bu personeli listeden çıkarmak istiyor musunuz?';
    if (!window.confirm(mesaj)) return;
    try {
      await deletePersonel(id);
      toast('Personel listeden çıkarıldı');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'Personel çıkarılamadı');
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
            Personel Düzenle
          </h2>
          <div className="flex flex-col sm:flex-row justify-center items-center gap-4 min-h-[280px]">
            <button
              type="button"
              onClick={() => setView('ekle')}
              className="flex items-center gap-3 px-8 py-4 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-semibold text-lg shadow-lg transition-colors"
            >
              <UserPlus size={28} /> Personel Ekle
            </button>
            <button
              type="button"
              onClick={() => setView('cikar')}
              className="flex items-center gap-3 px-8 py-4 rounded-xl border-2 border-red-500/80 text-red-500 hover:bg-red-500/10 font-semibold text-lg transition-colors"
            >
              <Trash2 size={28} /> Personel Çıkar
            </button>
          </div>
        </>
      )}

      {view === 'ekle' && (
        <div className="flex justify-center">
          <div className="w-full max-w-md">
            <h2 className={`text-xl font-bold mb-6 border-b pb-2 text-center ${textTitle} ${borderCol}`}>
              Yeni Personel Ekle
            </h2>
            <form onSubmit={handleAdd} className="space-y-5">
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Ad</label>
                <input
                  type="text"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  placeholder="Örn: Ahmet"
                  className={inputCls(isDark)}
                  required
                />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Soyad</label>
                <input
                  type="text"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  placeholder="Örn: Yılmaz"
                  className={inputCls(isDark)}
                  required
                />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>TC Kimlik No</label>
                <input
                  type="text"
                  value={tcKimlikNo}
                  onChange={(e) => setTcKimlikNo(e.target.value)}
                  placeholder="11 haneli"
                  className={inputCls(isDark)}
                  maxLength={11}
                  pattern="[0-9]{11}"
                  title="11 haneli rakam"
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
                <ThemeDropdown
                  label="Pozisyon"
                  options={POZISYON_OPTIONS}
                  value={pozisyon}
                  onChange={setPozisyon}
                  renderLabel={(p) => p.ad}
                  placeholder="Pozisyon seçin"
                  isDark={isDark}
                />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Brüt maaş (₺/ay)</label>
                <input
                  type="number"
                  value={brutMaas}
                  onChange={(e) => setBrutMaas(e.target.value)}
                  placeholder="Örn: 18500"
                  min="0"
                  className={inputCls(isDark)}
                />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Yol ücreti (₺/ay)</label>
                <input
                  type="number"
                  value={yol}
                  onChange={(e) => setYol(e.target.value)}
                  placeholder="Örn: 450"
                  min="0"
                  className={inputCls(isDark)}
                />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Yemek (₺/ay)</label>
                <input
                  type="number"
                  value={yemek}
                  onChange={(e) => setYemek(e.target.value)}
                  placeholder="Örn: 600"
                  min="0"
                  className={inputCls(isDark)}
                />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>İşe giriş tarihi</label>
                <input
                  type="text"
                  value={iseGirisTarihi}
                  onChange={(e) => setIseGirisTarihi(e.target.value)}
                  placeholder="Örn: 15.03.2022"
                  className={inputCls(isDark)}
                />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Acil durum iletişim kişisi</label>
                <input
                  type="text"
                  value={acilDurumKisi}
                  onChange={(e) => setAcilDurumKisi(e.target.value)}
                  placeholder="Örn: Ayşe Yılmaz"
                  className={inputCls(isDark)}
                />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Acil durum telefonu (11 hane)</label>
                <input
                  type="tel"
                  value={acilDurumTel}
                  onChange={(e) => setAcilDurumTel(e.target.value)}
                  placeholder="Örn: 0532 111 2233"
                  className={inputCls(isDark)}
                  maxLength={11}
                />
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className={`block text-sm font-medium mb-2 ${textSub}`}>Yıllık izin hakkı (gün)</label>
                  <input
                    type="number"
                    value={yillikIzinHakki}
                    onChange={(e) => setYillikIzinHakki(e.target.value)}
                    placeholder="14"
                    min="0"
                    className={inputCls(isDark)}
                  />
                </div>
                <div>
                  <label className={`block text-sm font-medium mb-2 ${textSub}`}>Kullanılan izin (gün)</label>
                  <input
                    type="number"
                    value={kullanilanIzin}
                    onChange={(e) => setKullanilanIzin(e.target.value)}
                    placeholder="0"
                    min="0"
                    className={inputCls(isDark)}
                  />
                </div>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className={`block text-sm font-medium mb-2 ${textSub}`}>Fazla mesai (saat)</label>
                  <input
                    type="number"
                    value={fazlaMesaiSaat}
                    onChange={(e) => setFazlaMesaiSaat(e.target.value)}
                    placeholder="0"
                    min="0"
                    step="0.5"
                    className={inputCls(isDark)}
                  />
                </div>
                <div>
                  <label className={`block text-sm font-medium mb-2 ${textSub}`}>Performans puanı</label>
                  <input
                    type="number"
                    value={performansPuani}
                    onChange={(e) => setPerformansPuani(e.target.value)}
                    placeholder="Opsiyonel"
                    min="0"
                    max="100"
                    step="0.1"
                    className={inputCls(isDark)}
                  />
                </div>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className={`block text-sm font-medium mb-2 ${textSub}`}>Ortalama günlük üretim</label>
                  <input
                    type="number"
                    value={ortalamaGunlukUretim}
                    onChange={(e) => setOrtalamaGunlukUretim(e.target.value)}
                    placeholder="Opsiyonel"
                    min="0"
                    step="0.01"
                    className={inputCls(isDark)}
                  />
                </div>
                <div>
                  <label className={`block text-sm font-medium mb-2 ${textSub}`}>Devamsızlık (gün)</label>
                  <input
                    type="number"
                    value={devamsizlikGun}
                    onChange={(e) => setDevamsizlikGun(e.target.value)}
                    placeholder="0"
                    min="0"
                    className={inputCls(isDark)}
                  />
                </div>
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Eğitim / Sertifikalar</label>
                <textarea
                  value={egitimSertifikalariText}
                  onChange={(e) => setEgitimSertifikalariText(e.target.value)}
                  placeholder="Her satıra bir sertifika veya virgülle ayırın"
                  className={`${inputCls(isDark)} min-h-[100px] resize-y`}
                  rows={4}
                />
              </div>
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

      {view === 'cikar' && (
        <div className="flex justify-center">
          <div className="w-full max-w-lg">
            <h2 className={`text-xl font-bold mb-6 border-b pb-2 text-center ${textTitle} ${borderCol}`}>
              Mevcut Personel
            </h2>
            <div className="space-y-3">
              {personelList.length === 0 ? (
                <p className={`text-sm text-center ${textSub}`}>Listede personel yok.</p>
              ) : (
                personelList.map((p) => (
                  <div
                    key={p.id}
                    className={`flex items-center justify-between gap-4 p-4 border rounded-lg ${isDark ? 'bg-gray-700/50 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
                  >
                    <div className="flex items-center gap-3">
                      <div className={`w-10 h-10 rounded-full flex items-center justify-center ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                        <Users size={20} />
                      </div>
                      <div>
                        <div className={`font-semibold ${textTitle}`}>{p.firstName} {p.lastName}</div>
                        <div className={`text-sm ${textSub}`}>{p.pozisyon || '—'}</div>
                      </div>
                    </div>
                    <button
                      type="button"
                      onClick={() => handleRemove(p.id, `${p.firstName} ${p.lastName}`)}
                      className={`p-2 rounded-lg transition-colors ${isDark ? 'text-red-400 hover:bg-red-900/30' : 'text-red-600 hover:bg-red-50'}`}
                      title="Listeden çıkar"
                    >
                      <Trash2 size={18} />
                    </button>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PersonelEkle;
