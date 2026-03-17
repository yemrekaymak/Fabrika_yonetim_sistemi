import React from 'react';
import { Activity, Pencil, Info } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';

const Uretim = ({ isDark, makineList = [], onMakineEkle, onMakineBilgi }) => {
  const { bgCard, textTitle, textSub, borderCol } = getThemeClasses(isDark);

  return (
    <div className={`p-6 rounded-xl shadow-sm border w-full transition-colors duration-300 ${bgCard}`}>
      <div className={`flex flex-wrap justify-between items-center gap-4 mb-6 border-b pb-4 ${borderCol}`}>
        <h2 className={`text-xl font-bold ${textTitle}`}>Makine Takibi</h2>
        <button
          type="button"
          onClick={onMakineEkle}
          className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold bg-blue-600 hover:bg-blue-700 text-white transition-colors"
        >
          <Pencil size={18} /> Düzenle
        </button>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {makineList.map((makine) => (
          <div
            key={makine.id}
            className={`flex items-center justify-between gap-4 p-5 border rounded-lg transition shadow-sm ${isDark ? 'bg-gray-700 border-gray-600 hover:border-blue-500' : 'bg-white border-gray-200 hover:border-blue-400'}`}
          >
            <div className="flex items-center gap-4 min-w-0">
              <div className={`w-12 h-12 rounded-full flex items-center justify-center flex-shrink-0 ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                <Activity size={24} />
              </div>
              <div className="min-w-0">
                <div className={`font-bold ${textTitle}`}>{makine.ad}</div>
                <div className={`text-sm ${textSub}`}>{makine.detay}</div>
              </div>
            </div>
            <button
              type="button"
              onClick={() => onMakineBilgi?.(makine)}
              className={`p-2 rounded-lg flex-shrink-0 transition-colors ${isDark ? 'text-blue-400 hover:bg-blue-900/40' : 'text-blue-600 hover:bg-blue-50'}`}
              title="Makine bilgilerini görüntüle"
            >
              <Info size={22} />
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Uretim;
