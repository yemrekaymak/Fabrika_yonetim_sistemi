import React, { useState, useEffect } from 'react';
import { ArrowLeft, UserCircle, ShoppingCart } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import { getSiparisler } from 'services';

const MusteriBilgi = ({ isDark, musteri, onBack }) => {
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [siparisler, setSiparisler] = useState([]);

  useEffect(() => {
    getSiparisler().then(setSiparisler).catch(() => setSiparisler([]));
  }, []);

  const musteriAdiMatch = (s) => {
    if (!musteri) return false;
    const unvan = musteri.unvan || '';
    const adSoyad = [musteri.ad, musteri.soyad].filter(Boolean).join(' ').trim();
    return s.musteriAdi === unvan || s.musteriAdi === adSoyad || (unvan && s.musteriAdi?.includes(unvan));
  };

  const gecmisSiparisler = musteri ? siparisler.filter(musteriAdiMatch) : [];

  const formatTL = (n) => (n != null ? n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '—');

  if (!musteri) {
    return (
      <div className={`p-6 rounded-xl shadow-sm border w-full ${bgCard}`}>
        <button
          type="button"
          onClick={onBack}
          className={`flex items-center gap-2 text-sm font-medium mb-4 ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`}
        >
          <ArrowLeft size={18} /> Listeye dön
        </button>
        <p className={textSub}>Müşteri bulunamadı.</p>
      </div>
    );
  }

  const linkCls = `flex items-center gap-2 text-sm font-medium ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`;

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${bgCard}`}>
      <div className="flex flex-wrap items-center gap-4 mb-6">
        <button type="button" onClick={onBack} className={linkCls}>
          <ArrowLeft size={18} /> Listeye dön
        </button>
      </div>

      <div className={`mb-6 pb-4 border-b ${borderCol}`}>
        <div className="flex items-center gap-4">
          <div className={`w-14 h-14 rounded-full flex items-center justify-center flex-shrink-0 ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
            <UserCircle size={28} />
          </div>
          <div>
            <h2 className={`text-2xl font-bold ${textTitle}`}>
              {musteri.tip === 'kurumsal' ? (musteri.unvan || `${musteri.ad} ${musteri.soyad}`) : `${musteri.ad} ${musteri.soyad}`}
            </h2>
            <div className={`text-sm ${textSub}`}>
              {musteri.tip === 'kurumsal' ? 'Kurumsal' : 'Bireysel'}
              {musteri.tip === 'kurumsal' && musteri.unvan && ` · ${musteri.ad} ${musteri.soyad}`}
            </div>
          </div>
        </div>
      </div>

      <div className="space-y-6 max-w-2xl mb-8">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Ad</div>
            <div className={`font-medium ${textTitle}`}>{musteri.ad ?? '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Soyad</div>
            <div className={`font-medium ${textTitle}`}>{musteri.soyad ?? '—'}</div>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Müşteri ID</div>
            <div className={`font-mono font-medium ${textTitle}`}>{musteri.idKod ?? '—'}</div>
          </div>
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>E-posta</div>
            <div className={`font-medium ${textTitle}`}>{musteri.email ?? '—'}</div>
          </div>
        </div>
        <div>
          <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Telefon</div>
          <div className={`font-medium ${textTitle}`}>{musteri.telefon ?? '—'}</div>
        </div>
        {musteri.tip === 'kurumsal' && musteri.unvan && (
          <div>
            <div className={`text-xs font-medium uppercase tracking-wider ${textSub}`}>Şirket unvanı</div>
            <div className={`font-medium ${textTitle}`}>{musteri.unvan}</div>
          </div>
        )}
      </div>

      <div>
        <h3 className={`text-lg font-bold mb-4 flex items-center gap-2 ${textTitle}`}>
          <ShoppingCart size={22} className="text-blue-500" /> Geçmiş Siparişler
        </h3>
        {gecmisSiparisler.length === 0 ? (
          <p className={`${textSub}`}>Bu müşteriye ait sipariş bulunmuyor.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left">
              <thead className={`${isDark ? 'bg-gray-700/50 text-gray-300' : 'bg-gray-50 text-gray-600'} font-semibold uppercase text-xs tracking-wider`}>
                <tr>
                  <th className="py-3 px-4 rounded-l-lg">Sipariş No</th>
                  <th className="py-3 px-4">Ürün</th>
                  <th className="py-3 px-4">Miktar</th>
                  <th className="py-3 px-4">Birim Fiyat</th>
                  <th className="py-3 px-4">Toplam</th>
                  <th className="py-3 px-4">Tarih</th>
                  <th className="py-3 px-4 rounded-r-lg">Durum</th>
                </tr>
              </thead>
              <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                {gecmisSiparisler.map((s) => {
                  const toplam = s.miktar * (s.birimFiyat ?? 0);
                  return (
                    <tr key={s.id ?? s.no ?? s.urunId + '-' + s.miktar} className={isDark ? 'hover:bg-gray-700/30' : 'hover:bg-gray-50'}>
                      <td className={`py-3 px-4 font-mono text-sm ${textSub}`}>{s.no ?? `SIP-${s.id}`}</td>
                      <td className={`py-3 px-4 font-medium ${textTitle}`}>{s.urunAdi ?? s.urunKodu ?? '—'}</td>
                      <td className={`py-3 px-4 ${textSub}`}>{s.miktar?.toLocaleString('tr-TR') ?? '—'}</td>
                      <td className={`py-3 px-4 ${textSub}`}>{formatTL(s.birimFiyat)} ₺</td>
                      <td className={`py-3 px-4 font-medium ${textTitle}`}>{formatTL(toplam)} ₺</td>
                      <td className={`py-3 px-4 ${textSub}`}>{s.tarih ?? '—'}</td>
                      <td className="py-3 px-4">
                        <span className={`text-xs font-medium px-2 py-1 rounded ${isDark ? 'bg-gray-600 text-gray-200' : 'bg-gray-200 text-gray-700'}`}>
                          {s.durum}
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default MusteriBilgi;
