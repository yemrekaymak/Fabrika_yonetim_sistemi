import React, { useState } from 'react';
import { ArrowLeft, TrendingDown, Plus, Pencil, Trash2, Check, Square } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import { useToast } from 'contexts/ToastContext';
import { createGider, deleteGider } from 'services';

const inputCls = (isDark) =>
  `w-full p-3 rounded-lg border bg-transparent ${isDark ? 'border-gray-600 text-white placeholder-gray-500' : 'border-gray-300 text-gray-900 placeholder-gray-400'}`;

const GiderDüzenle = ({ isDark, giderList = [], onRefresh, onBack }) => {
  const { toast } = useToast();
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [view, setView] = useState('menu'); // 'menu' | 'ekle' | 'liste'
  const [editingOriginalKalem, setEditingOriginalKalem] = useState(null);
  const [kalem, setKalem] = useState('');
  const [tutar, setTutar] = useState('');
  const [tip, setTip] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const sabitListe = giderList.filter((g) => g.tip === 'sabit');
  const degiskenListe = giderList.filter((g) => g.tip === 'degisken');

  const handleSave = async (e) => {
    e.preventDefault();
    const kalemTrim = kalem.trim();
    const tutarNum = parseFloat(tutar);
    if (!kalemTrim || Number.isNaN(tutarNum) || tutarNum < 0) return;
    if (!tip) {
      alert('Lütfen gider tipi seçin.');
      return;
    }
    setSubmitting(true);
    try {
      if (editingOriginalKalem) {
        await deleteGider(editingOriginalKalem);
      }
      await createGider({ kalem: kalemTrim, tutar: tutarNum, tip });
      toast(editingOriginalKalem ? 'Gider güncellendi' : 'Gider kaydedildi');
      setEditingOriginalKalem(null);
      setKalem('');
      setTutar('');
      setTip('');
      setView('menu');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'İşlem başarısız');
    } finally {
      setSubmitting(false);
    }
  };

  const handleEdit = (g) => {
    setEditingOriginalKalem(g.kalem);
    setKalem(g.kalem);
    setTutar(String(g.tutar));
    setTip(g.tip);
    setView('ekle');
  };

  const handleRemove = async (id, kalemAd) => {
    if (!window.confirm(`"${kalemAd}" gider kalemini listeden çıkarmak istiyor musunuz?`)) return;
    try {
      await deleteGider(kalemAd);
      toast('Gider silindi');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'Silinemedi');
    }
  };

  const formatTL = (n) => (n != null ? n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '—');
  const linkCls = `flex items-center gap-2 text-sm font-medium ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`;

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${bgCard}`}>
      <div className="flex flex-wrap items-center gap-4 mb-6">
        <button type="button" onClick={onBack} className={linkCls}>
          <ArrowLeft size={18} /> Listeye dön
        </button>
        {view !== 'menu' && (
          <button type="button" onClick={() => { setView('menu'); setEditingOriginalKalem(null); setKalem(''); setTutar(''); setTip(''); }} className={linkCls}>
            <ArrowLeft size={18} /> Geri
          </button>
        )}
      </div>

      {view === 'menu' && (
        <>
          <h2 className={`text-xl font-bold mb-8 border-b pb-2 text-center ${textTitle} ${borderCol}`}>
            Gider Düzenle
          </h2>
          <div className="flex flex-col sm:flex-row justify-center items-center gap-4 min-h-[280px]">
            <button
              type="button"
              onClick={() => { setEditingOriginalKalem(null); setKalem(''); setTutar(''); setTip(''); setView('ekle'); }}
              className="flex items-center gap-3 px-8 py-4 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-semibold text-lg shadow-lg transition-colors"
            >
              <Plus size={28} /> Gider Ekle
            </button>
            <button
              type="button"
              onClick={() => setView('liste')}
              className="flex items-center gap-3 px-8 py-4 rounded-xl border-2 border-red-500/80 text-red-500 hover:bg-red-500/10 font-semibold text-lg transition-colors"
            >
              <Pencil size={28} /> Gider Düzenle
            </button>
          </div>
        </>
      )}

      {view === 'ekle' && (
        <div className="max-w-md">
          <h2 className={`text-xl font-bold mb-6 border-b pb-2 ${textTitle} ${borderCol}`}>
            {editingOriginalKalem ? 'Gideri Güncelle' : 'Yeni Gider Ekle'}
          </h2>
          <form onSubmit={handleSave} className="space-y-5">
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Kalem adı *</label>
              <input
                type="text"
                value={kalem}
                onChange={(e) => setKalem(e.target.value)}
                placeholder="Örn: Elektrik, Kira"
                className={inputCls(isDark)}
                required
              />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Tutar (₺/ay) *</label>
              <input
                type="number"
                min={0}
                step={0.01}
                value={tutar}
                onChange={(e) => setTutar(e.target.value)}
                placeholder="Örn: 18500"
                className={inputCls(isDark)}
                required
              />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${textSub}`}>Gider tipi</label>
              <div className={`p-4 rounded-lg border ${isDark ? 'border-gray-600 bg-gray-700/30' : 'border-gray-300 bg-gray-50'}`}>
                <div className="grid gap-3 grid-cols-2">
                  <button
                    type="button"
                    onClick={() => setTip(tip === 'sabit' ? '' : 'sabit')}
                    className={`flex items-center gap-2 rounded-lg px-3 py-2 border text-left transition-all ${
                      tip === 'sabit'
                        ? 'bg-[#2A334B] border-blue-400 text-white shadow-[0_0_0_1px_rgba(77,144,254,0.4)]'
                        : isDark
                          ? 'bg-transparent border-gray-600 text-gray-200 hover:border-gray-500'
                          : 'bg-transparent border-gray-200 text-gray-700 hover:border-gray-300'
                    }`}
                  >
                    {tip === 'sabit' ? (
                      <span className="flex items-center justify-center w-5 h-5 rounded bg-blue-500 shrink-0">
                        <Check size={14} className="text-white" strokeWidth={2.5} />
                      </span>
                    ) : (
                      <Square size={18} className={`shrink-0 ${isDark ? 'text-gray-400' : 'text-gray-500'}`} strokeWidth={1.5} />
                    )}
                    <span className="text-sm font-medium">Sabit (aylık)</span>
                  </button>
                  <button
                    type="button"
                    onClick={() => setTip(tip === 'degisken' ? '' : 'degisken')}
                    className={`flex items-center gap-2 rounded-lg px-3 py-2 border text-left transition-all ${
                      tip === 'degisken'
                        ? 'bg-[#2A334B] border-blue-400 text-white shadow-[0_0_0_1px_rgba(77,144,254,0.4)]'
                        : isDark
                          ? 'bg-transparent border-gray-600 text-gray-200 hover:border-gray-500'
                          : 'bg-transparent border-gray-200 text-gray-700 hover:border-gray-300'
                    }`}
                  >
                    {tip === 'degisken' ? (
                      <span className="flex items-center justify-center w-5 h-5 rounded bg-blue-500 shrink-0">
                        <Check size={14} className="text-white" strokeWidth={2.5} />
                      </span>
                    ) : (
                      <Square size={18} className={`shrink-0 ${isDark ? 'text-gray-400' : 'text-gray-500'}`} strokeWidth={1.5} />
                    )}
                    <span className="text-sm font-medium">Değişken (üretim)</span>
                  </button>
                </div>
              </div>
            </div>
            <div className="flex gap-3 pt-2">
              <button
                type="submit"
                disabled={submitting}
                className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white font-semibold rounded-lg transition-colors"
              >
                <TrendingDown size={18} /> {submitting ? 'Kaydediliyor...' : (editingOriginalKalem ? 'Güncelle' : 'Kaydet')}
              </button>
              <button
                type="button"
                onClick={() => { setView('menu'); setEditingOriginalKalem(null); setKalem(''); setTutar(''); setTip(''); }}
                className={`px-5 py-2.5 font-semibold rounded-lg border transition-colors ${isDark ? 'border-gray-600 text-gray-300 hover:bg-gray-700' : 'border-gray-300 text-gray-700 hover:bg-gray-100'}`}
              >
                İptal
              </button>
            </div>
          </form>
        </div>
      )}

      {view === 'liste' && (
        <div className="w-full max-w-3xl">
          <h2 className={`text-xl font-bold mb-6 border-b pb-2 ${textTitle} ${borderCol}`}>
            Gider Kalemleri
          </h2>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <section>
              <h3 className={`text-sm font-semibold uppercase tracking-wider mb-3 ${textSub}`}>Sabit giderler</h3>
              <div className="space-y-2">
                {sabitListe.length === 0 ? (
                  <p className={`text-sm ${textSub}`}>Sabit gider yok.</p>
                ) : (
                  sabitListe.map((g) => (
                    <div
                      key={g.id}
                      className={`flex items-center justify-between gap-3 p-4 border rounded-lg ${isDark ? 'bg-gray-700/50 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
                    >
                      <div>
                        <span className={`font-medium ${textTitle}`}>{g.kalem}</span>
                        <span className={`ml-2 tabular-nums ${textSub}`}>{formatTL(g.tutar)} ₺</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <button
                          type="button"
                          onClick={() => handleEdit(g)}
                          className={`p-2 rounded-lg transition-colors ${isDark ? 'text-blue-400 hover:bg-blue-900/30' : 'text-blue-600 hover:bg-blue-50'}`}
                          title="Düzenle"
                        >
                          <Pencil size={18} />
                        </button>
                        <button
                          type="button"
                          onClick={() => handleRemove(g.id, g.kalem)}
                          className={`p-2 rounded-lg transition-colors ${isDark ? 'text-red-400 hover:bg-red-900/30' : 'text-red-600 hover:bg-red-50'}`}
                          title="Listeden çıkar"
                        >
                          <Trash2 size={18} />
                        </button>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </section>
            <section>
              <h3 className={`text-sm font-semibold uppercase tracking-wider mb-3 ${textSub}`}>Değişken giderler</h3>
              <div className="space-y-2">
                {degiskenListe.length === 0 ? (
                  <p className={`text-sm ${textSub}`}>Değişken gider yok.</p>
                ) : (
                  degiskenListe.map((g) => (
                    <div
                      key={g.id}
                      className={`flex items-center justify-between gap-3 p-4 border rounded-lg ${isDark ? 'bg-gray-700/50 border-gray-600' : 'bg-gray-50 border-gray-200'}`}
                    >
                      <div>
                        <span className={`font-medium ${textTitle}`}>{g.kalem}</span>
                        <span className={`ml-2 tabular-nums ${textSub}`}>{formatTL(g.tutar)} ₺</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <button
                          type="button"
                          onClick={() => handleEdit(g)}
                          className={`p-2 rounded-lg transition-colors ${isDark ? 'text-blue-400 hover:bg-blue-900/30' : 'text-blue-600 hover:bg-blue-50'}`}
                          title="Düzenle"
                        >
                          <Pencil size={18} />
                        </button>
                        <button
                          type="button"
                          onClick={() => handleRemove(g.id, g.kalem)}
                          className={`p-2 rounded-lg transition-colors ${isDark ? 'text-red-400 hover:bg-red-900/30' : 'text-red-600 hover:bg-red-50'}`}
                          title="Listeden çıkar"
                        >
                          <Trash2 size={18} />
                        </button>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </section>
          </div>
        </div>
      )}
    </div>
  );
};

export default GiderDüzenle;
