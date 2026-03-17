import React, { useState, useEffect } from 'react';
import { ArrowLeft, Save } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import ThemeDropdown from 'components/ThemeDropdown';
import { useToast } from 'contexts/ToastContext';
import { updatePersonel } from 'services';
import { POZISYON_OPTIONS } from 'constants/departmanPozisyon';

const inputCls = (isDark) =>
  `w-full p-3 rounded-lg border bg-transparent ${isDark ? 'border-gray-600 text-white placeholder-gray-500' : 'border-gray-300 text-gray-900 placeholder-gray-400'}`;

const personToFormState = (p) => {
  if (!p) return {};
  const certs = Array.isArray(p.egitimSertifikalari) ? p.egitimSertifikalari : [];
  return {
    firstName: p.firstName ?? '',
    lastName: p.lastName ?? '',
    tcKimlikNo: p.tcKimlikNo ?? '',
    telefon: p.telefon ?? '',
    pozisyon: p.pozisyon ?? '',
    brutMaas: p.maas != null ? String(p.maas) : '',
    yol: p.yol != null ? String(p.yol) : '',
    yemek: p.yemek != null ? String(p.yemek) : '',
    iseGirisTarihi: p.iseGirisTarihi ?? '',
    acilDurumKisi: p.acilDurumKisi ?? '',
    acilDurumTel: p.acilDurumTel ?? '',
    yillikIzinHakki: p.yillikIzinHakki != null ? String(p.yillikIzinHakki) : '14',
    kullanilanIzin: p.kullanilanIzin != null ? String(p.kullanilanIzin) : '0',
    fazlaMesaiSaat: p.fazlaMesaiSaat != null ? String(p.fazlaMesaiSaat) : '',
    performansPuani: p.performansPuani != null ? String(p.performansPuani) : '',
    ortalamaGunlukUretim: p.ortalamaGunlukUretim != null ? String(p.ortalamaGunlukUretim) : '',
    devamsizlikGun: p.devamsizlikGun != null ? String(p.devamsizlikGun) : '',
    egitimSertifikalariText: certs.join('\n'),
  };
};

const PersonelDuzenle = ({ isDark, person, onSave, onBack }) => {
  const { toast } = useToast();
  const [submitting, setSubmitting] = useState(false);
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
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
  const [egitimSertifikalariText, setEgitimSertifikalariText] = useState('');

  useEffect(() => {
    const s = personToFormState(person);
    if (person) {
      setFirstName(s.firstName);
      setLastName(s.lastName);
      setTcKimlikNo(s.tcKimlikNo);
      setTelefon(s.telefon ?? '');
      setPozisyon(s.pozisyon ?? '');
      setBrutMaas(s.brutMaas);
      setYol(s.yol);
      setYemek(s.yemek);
      setIseGirisTarihi(s.iseGirisTarihi);
      setAcilDurumKisi(s.acilDurumKisi);
      setAcilDurumTel(s.acilDurumTel);
      setYillikIzinHakki(s.yillikIzinHakki);
      setKullanilanIzin(s.kullanilanIzin);
      setFazlaMesaiSaat(s.fazlaMesaiSaat);
      setPerformansPuani(s.performansPuani);
      setOrtalamaGunlukUretim(s.ortalamaGunlukUretim);
      setDevamsizlikGun(s.devamsizlikGun);
      setEgitimSertifikalariText(s.egitimSertifikalariText);
    }
  }, [person]);

  const parseSertifikalar = (text) =>
    text
      .split(/[\n,]+/)
      .map((s) => s.trim())
      .filter(Boolean);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!person?.id) return;
    if (!firstName.trim() || !lastName.trim()) {
      toast('Ad ve soyad zorunludur.');
      return;
    }
    setSubmitting(true);
    try {
      await updatePersonel(person.id, {
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
      });
      toast('Bilgiler güncellendi');
      onSave?.();
    } catch (err) {
      toast(err?.message || 'Güncellenemedi');
    } finally {
      setSubmitting(false);
    }
  };

  if (!person) {
    return (
      <div className={`p-6 rounded-xl shadow-sm border w-full ${bgCard}`}>
        <button type="button" onClick={onBack} className={`flex items-center gap-2 text-sm font-medium mb-4 ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`}>
          <ArrowLeft size={18} /> Geri
        </button>
        <p className={textSub}>Personel bulunamadı.</p>
      </div>
    );
  }

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${bgCard}`}>
      <div className="flex flex-wrap items-center gap-4 mb-6">
        <button type="button" onClick={onBack} className={`flex items-center gap-2 text-sm font-medium ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`}>
          <ArrowLeft size={18} /> Geri
        </button>
      </div>

      <h2 className={`text-xl font-bold mb-6 border-b pb-2 ${textTitle} ${borderCol}`}>
        Personel Bilgilerini Düzenle — {person.firstName} {person.lastName}
      </h2>

      <div className="flex justify-center">
        <div className="w-full max-w-md">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Ad</label>
              <input type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)} placeholder="Örn: Ahmet" className={inputCls(isDark)} required />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Soyad</label>
              <input type="text" value={lastName} onChange={(e) => setLastName(e.target.value)} placeholder="Örn: Yılmaz" className={inputCls(isDark)} required />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>TC Kimlik No</label>
              <input type="text" value={tcKimlikNo} onChange={(e) => setTcKimlikNo(e.target.value)} placeholder="11 haneli" className={inputCls(isDark)} maxLength={11} />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Telefon</label>
              <input type="tel" value={telefon} onChange={(e) => setTelefon(e.target.value)} placeholder="Örn: 0532 111 2233" className={inputCls(isDark)} />
            </div>
            <div>
              <ThemeDropdown label="Pozisyon" options={POZISYON_OPTIONS} value={pozisyon} onChange={setPozisyon} renderLabel={(p) => p.ad} placeholder="Pozisyon seçin" isDark={isDark} />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Brüt maaş (₺/ay)</label>
              <input type="number" value={brutMaas} onChange={(e) => setBrutMaas(e.target.value)} placeholder="Örn: 18500" min="0" className={inputCls(isDark)} />
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Yol ücreti (₺/ay)</label>
                <input type="number" value={yol} onChange={(e) => setYol(e.target.value)} placeholder="450" min="0" className={inputCls(isDark)} />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Yemek (₺/ay)</label>
                <input type="number" value={yemek} onChange={(e) => setYemek(e.target.value)} placeholder="600" min="0" className={inputCls(isDark)} />
              </div>
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>İşe giriş tarihi</label>
              <input type="text" value={iseGirisTarihi} onChange={(e) => setIseGirisTarihi(e.target.value)} placeholder="Örn: 15.03.2022" className={inputCls(isDark)} />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Acil durum iletişim kişisi</label>
              <input type="text" value={acilDurumKisi} onChange={(e) => setAcilDurumKisi(e.target.value)} placeholder="Örn: Ayşe Yılmaz" className={inputCls(isDark)} />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Acil durum telefonu (11 hane)</label>
              <input type="tel" value={acilDurumTel} onChange={(e) => setAcilDurumTel(e.target.value)} placeholder="0532 111 2233" className={inputCls(isDark)} maxLength={11} />
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Yıllık izin hakkı (gün)</label>
                <input type="number" value={yillikIzinHakki} onChange={(e) => setYillikIzinHakki(e.target.value)} min="0" className={inputCls(isDark)} />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Kullanılan izin (gün)</label>
                <input type="number" value={kullanilanIzin} onChange={(e) => setKullanilanIzin(e.target.value)} min="0" className={inputCls(isDark)} />
              </div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Fazla mesai (saat)</label>
                <input type="number" value={fazlaMesaiSaat} onChange={(e) => setFazlaMesaiSaat(e.target.value)} min="0" step="0.5" className={inputCls(isDark)} />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Performans puanı</label>
                <input type="number" value={performansPuani} onChange={(e) => setPerformansPuani(e.target.value)} min="0" max="100" step="0.1" className={inputCls(isDark)} />
              </div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Ortalama günlük üretim</label>
                <input type="number" value={ortalamaGunlukUretim} onChange={(e) => setOrtalamaGunlukUretim(e.target.value)} min="0" step="0.01" className={inputCls(isDark)} />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${textSub}`}>Devamsızlık (gün)</label>
                <input type="number" value={devamsizlikGun} onChange={(e) => setDevamsizlikGun(e.target.value)} min="0" className={inputCls(isDark)} />
              </div>
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Eğitim / Sertifikalar</label>
              <textarea value={egitimSertifikalariText} onChange={(e) => setEgitimSertifikalariText(e.target.value)} placeholder="Her satıra bir sertifika veya virgülle ayırın" className={`${inputCls(isDark)} min-h-[100px] resize-y`} rows={4} />
            </div>
            <div className="flex gap-3 pt-2">
              <button type="submit" disabled={submitting} className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors disabled:opacity-50">
                <Save size={18} /> {submitting ? 'Kaydediliyor...' : 'Kaydet'}
              </button>
              <button type="button" onClick={onBack} className={`px-5 py-2.5 font-semibold rounded-lg border transition-colors ${isDark ? 'border-gray-600 text-gray-300 hover:bg-gray-700' : 'border-gray-300 text-gray-700 hover:bg-gray-100'}`}>
                İptal
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default PersonelDuzenle;
