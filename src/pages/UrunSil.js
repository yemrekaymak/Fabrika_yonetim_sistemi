import React, { useState } from 'react';
import { ArrowLeft, Trash2 } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import { deleteUrun } from 'services';
import { useToast } from 'contexts/ToastContext';

const UrunSil = ({ isDark, stokList, onBack, onRefresh }) => {
  const { toast } = useToast();
  const { textTitle, textSub, borderCol } = getThemeClasses(isDark);
  const [deletingId, setDeletingId] = useState(null);
  const list = Array.isArray(stokList) ? stokList : [];

  const handleSil = async (id) => {
    if (!id) return;
    if (!window.confirm('Bu ürünü silmek istediğinize emin misiniz?')) return;
    setDeletingId(id);
    try {
      await deleteUrun(id);
      toast('Ürün silindi');
      onRefresh?.();
    } catch (err) {
      toast(err?.message || 'Silinemedi');
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
      <button
        type="button"
        onClick={onBack}
        className={`flex items-center gap-2 mb-6 text-sm font-medium ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`}
      >
        <ArrowLeft size={18} /> Geri
      </button>

      <h2 className={`text-xl font-bold mb-6 border-b pb-2 ${textTitle} ${borderCol}`}>Ürün Sil</h2>

      {list.length === 0 ? (
        <p className={`${textSub}`}>Silinecek stok kaydı yok.</p>
      ) : (
        <ul className="space-y-2">
          {list.map((row) => (
            <li
              key={row.id ?? row.kod ?? Math.random()}
              className={`flex items-center justify-between gap-4 py-3 px-4 rounded-lg border ${isDark ? 'border-gray-600 bg-gray-700/30' : 'border-gray-200 bg-gray-50'}`}
            >
              <div className="min-w-0">
                <span className={`font-mono text-sm ${textSub}`}>{row.kod}</span>
                <span className={`mx-2 ${textSub}`}>·</span>
                <span className={`font-medium ${textTitle}`}>{row.ad || '—'}</span>
                <span className={`text-sm ${textSub}`}> (Miktar: {row.miktar ?? 0})</span>
              </div>
              <button
                type="button"
                onClick={() => handleSil(row.id)}
                disabled={deletingId === row.id}
                className="flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm font-medium bg-red-600 hover:bg-red-700 text-white disabled:opacity-50"
              >
                <Trash2 size={16} />
                {deletingId === row.id ? 'Siliniyor...' : 'Sil'}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default UrunSil;
