import React from 'react';
import { ArrowLeft, Plus, Trash2 } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';

const StokDuzenle = ({ isDark, onBack, onUrunEkle, onUrunSil }) => {
  const { textTitle, borderCol } = getThemeClasses(isDark);

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
      <div className="flex flex-wrap items-center gap-4 mb-6">
        <button
          type="button"
          onClick={onBack}
          className={`flex items-center gap-2 text-sm font-medium ${isDark ? 'text-gray-400 hover:text-white' : 'text-gray-600 hover:text-gray-900'}`}
        >
          <ArrowLeft size={18} /> Listeye dön
        </button>
      </div>

      <h2 className={`text-xl font-bold mb-8 border-b pb-2 text-center ${textTitle} ${borderCol}`}>Stok Düzenle</h2>

      <div className="flex flex-col sm:flex-row justify-center items-center gap-4 min-h-[280px]">
        <button
          type="button"
          onClick={onUrunEkle}
          className="flex items-center gap-3 px-8 py-4 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-semibold text-lg shadow-lg transition-colors"
        >
          <Plus size={28} />
          Ürün Ekle
        </button>
        <button
          type="button"
          onClick={onUrunSil}
          className="flex items-center gap-3 px-8 py-4 rounded-xl border-2 border-red-500/80 text-red-500 hover:bg-red-500/10 font-semibold text-lg transition-colors"
        >
          <Trash2 size={28} />
          Ürün Sil
        </button>
      </div>
    </div>
  );
};

export default StokDuzenle;
