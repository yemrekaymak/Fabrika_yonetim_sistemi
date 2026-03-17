import React from 'react';
import { ArrowLeft, Users, Pencil } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';

const PersonelBilgi = ({ isDark, person, onBack, onEdit }) => {
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);

  if (!person) {
    return (
      <div className={`p-6 rounded-xl shadow-sm border w-full ${bgCard}`}>
        <button
          type="button"
          onClick={onBack}
          className={`flex items-center gap-2 text-sm font-medium mb-4 ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`}
        >
          <ArrowLeft size={18} /> Listeye dön
        </button>
        <p className={textSub}>Personel bulunamadı.</p>
      </div>
    );
  }

  const kalanIzin = (person.yillikIzinHakki ?? 0) - (person.kullanilanIzin ?? 0);
  const kidemYil = person.iseGirisTarihi
    ? (() => {
        try {
          const [d, m, y] = String(person.iseGirisTarihi).split(/[.\-/]/);
          const giris = new Date(parseInt(y, 10), parseInt(m, 10) - 1, parseInt(d, 10));
          if (Number.isNaN(giris.getTime())) return null;
          const yil = (Date.now() - giris.getTime()) / (365.25 * 24 * 60 * 60 * 1000);
          return yil >= 0 ? yil : null;
        } catch {
          return null;
        }
      })()
    : null;

  const linkCls = `flex items-center gap-2 text-sm font-medium ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`;

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${bgCard}`}>
      <div className="flex flex-wrap items-center gap-4 mb-6">
        <button type="button" onClick={onBack} className={linkCls}>
          <ArrowLeft size={18} /> Listeye dön
        </button>
        {onEdit && (
          <button
            type="button"
            onClick={onEdit}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold bg-blue-600 hover:bg-blue-700 text-white transition-colors`}
          >
            <Pencil size={18} /> Düzenle
          </button>
        )}
      </div>

      <div className={`mb-6 pb-4 border-b ${borderCol}`}>
        <div className="flex items-center gap-4">
          <div className={`w-14 h-14 rounded-full flex items-center justify-center flex-shrink-0 ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
            <Users size={28} />
          </div>
          <div>
            <h2 className={`text-2xl font-bold ${textTitle}`}>{person.firstName} {person.lastName}</h2>
            <div className={`text-sm ${textSub}`}>{person.pozisyon || '—'}</div>
          </div>
        </div>
      </div>

      <div className="space-y-6 max-w-2xl">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Personel ID</div>
            <div className={`font-medium ${textTitle}`}>{person.id != null ? person.id : '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>TC Kimlik No</div>
            <div className={`font-medium ${textTitle}`}>{person.tcKimlikNo ?? '—'}</div>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Telefon</div>
            <div className={`font-medium ${textTitle}`}>{person.telefon ?? '—'}</div>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Brüt maaş (₺/ay)</div>
            <div className={`font-medium ${textTitle}`}>{person.maas != null ? person.maas.toLocaleString('tr-TR') : '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Pozisyon</div>
            <div className={`font-medium ${textTitle}`}>{person.pozisyon ?? '—'}</div>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>İşe giriş tarihi</div>
            <div className={`font-medium ${textTitle}`}>{person.iseGirisTarihi ?? '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Kıdem (yıl)</div>
            <div className={`font-medium ${textTitle}`}>
              {kidemYil != null ? kidemYil.toLocaleString('tr-TR', { minimumFractionDigits: 1, maximumFractionDigits: 1 }) : (person.kidem != null ? Number(person.kidem).toLocaleString('tr-TR', { minimumFractionDigits: 1, maximumFractionDigits: 1 }) : '—')}
            </div>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Yemek (₺/ay)</div>
            <div className={`font-medium ${textTitle}`}>{person.yemek != null ? person.yemek.toLocaleString('tr-TR') : '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Yol ücreti (₺/ay)</div>
            <div className={`font-medium ${textTitle}`}>{person.yol != null ? person.yol.toLocaleString('tr-TR') : '—'}</div>
          </div>
        </div>
        <div>
          <div className={`text-xs font-medium uppercase tracking-wider mb-2 ${textSub}`}>İzin bilgileri</div>
          <div className={`p-4 rounded-lg ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
            <div className="grid grid-cols-3 gap-4 text-center">
              <div>
                <div className={`text-xs ${textSub}`}>Yıllık hak</div>
                <div className={`font-semibold ${textTitle}`}>{person.yillikIzinHakki ?? 0} gün</div>
              </div>
              <div>
                <div className={`text-xs ${textSub}`}>Kullanılan</div>
                <div className={`font-semibold ${textTitle}`}>{person.kullanilanIzin ?? 0} gün</div>
              </div>
              <div>
                <div className={`text-xs ${textSub}`}>Kalan</div>
                <div className={`font-semibold ${textTitle}`}>{kalanIzin} gün</div>
              </div>
            </div>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Fazla mesai (saat)</div>
            <div className={`font-medium ${textTitle}`}>{person.fazlaMesaiSaat != null ? `${person.fazlaMesaiSaat} saat` : '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Performans puanı</div>
            <div className={`font-medium ${textTitle}`}>{person.performansPuani != null ? person.performansPuani.toLocaleString('tr-TR') : '—'}</div>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Ortalama günlük üretim</div>
            <div className={`font-medium ${textTitle}`}>{person.ortalamaGunlukUretim != null ? person.ortalamaGunlukUretim.toLocaleString('tr-TR') : '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Devamsızlık (gün)</div>
            <div className={`font-medium ${textTitle}`}>{person.devamsizlikGun != null ? person.devamsizlikGun : '—'}</div>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Acil durum kişisi</div>
            <div className={`font-medium ${textTitle}`}>{person.acilDurumKisi ?? '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Acil durum telefonu</div>
            <div className={`font-medium ${textTitle}`}>{person.acilDurumTel ?? '—'}</div>
          </div>
        </div>
        <div>
          <div className={`text-xs font-medium uppercase tracking-wider mb-2 ${textSub}`}>Eğitim / Sertifikalar</div>
          <ul className={`space-y-2 ${textTitle}`}>
            {person.egitimSertifikalari?.length
              ? person.egitimSertifikalari.map((s, i) => (
                  <li key={i} className="flex items-center gap-2">
                    <span className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${isDark ? 'bg-blue-400' : 'bg-blue-600'}`} />
                    {s}
                  </li>
                ))
              : <li className={textSub}>—</li>}
          </ul>
        </div>
      </div>
    </div>
  );
};

export default PersonelBilgi;
