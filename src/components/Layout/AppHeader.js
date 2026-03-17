import React from 'react';
import { Home, Activity, Box, ShoppingCart, Users, Moon, Sun, Calculator, UserCircle, ChevronRight } from 'lucide-react';
import { PAGE_TITLES, BREADCRUMB_PATH } from 'constants/navigation';

const TAB_ICONS = {
  dashboard: Home,
  uretim: Activity,
  'makine-ekle': Activity,
  'makine-bilgi': Activity,
  'siparis-olustur': ShoppingCart,
  musteriler: UserCircle,
  'musteriler-ekle': UserCircle,
  'musteri-bilgi': UserCircle,
  muhasebe: Calculator,
  'muhasebe-duzenle': Calculator,
  stok: Box,
  'urun-ekle': Box,
  personel: Users,
  'personel-bilgi': Users,
  'personel-ekle': Users,
  'personel-duzenle': Users,
};

const AppHeader = ({ activeTab, setActiveTab, isDark, toggleTheme, userName }) => {
  const bgCard = isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200';
  const textTitle = isDark ? 'text-white' : 'text-gray-800';
  const textMuted = isDark ? 'text-gray-400' : 'text-gray-500';
  const linkHover = isDark ? 'hover:text-blue-400' : 'hover:text-blue-600';

  const Icon = TAB_ICONS[activeTab];
  const path = BREADCRUMB_PATH[activeTab] ?? [{ tabId: activeTab, label: PAGE_TITLES[activeTab] ?? activeTab }];

  return (
    <>
      {path.length > 0 && (
        <nav
          className={`flex items-center gap-1 text-sm mb-2 ${textMuted}`}
          aria-label="Sayfa yolu"
        >
          {path.map((seg, i) => {
            const isLast = i === path.length - 1;
            return (
              <span key={seg.tabId} className="flex items-center gap-1">
                {i > 0 && <ChevronRight size={14} className="flex-shrink-0" />}
                {isLast ? (
                  <span className="font-medium text-inherit">{seg.label}</span>
                ) : (
                  <button
                    type="button"
                    onClick={() => setActiveTab(seg.tabId)}
                    className={`${linkHover} transition-colors truncate max-w-[140px] sm:max-w-none`}
                  >
                    {seg.label}
                  </button>
                )}
              </span>
            );
          })}
        </nav>
      )}
      <header
        className={`flex justify-between items-center mb-8 p-4 rounded-xl shadow-sm border transition-colors duration-300 ${bgCard}`}
      >
        <h1 className={`text-2xl font-bold capitalize flex items-center gap-2 ${textTitle}`}>
          {Icon && <Icon className="text-blue-600" />}
          {PAGE_TITLES[activeTab] ?? activeTab}
        </h1>

        <div className="flex items-center gap-4">
        <button
          onClick={toggleTheme}
          className={`p-2 rounded-lg transition-colors ${
            isDark ? 'bg-gray-700 text-yellow-400 hover:bg-gray-600' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
          }`}
          title={isDark ? 'Aydınlık Mod' : 'Karanlık Mod'}
          type="button"
        >
          {isDark ? <Sun size={20} /> : <Moon size={20} />}
        </button>

        <div
          className={`px-4 py-2 rounded-lg text-sm font-semibold border ${
            isDark ? 'bg-blue-900/30 text-blue-300 border-blue-800' : 'bg-blue-50 text-blue-800 border-blue-100'
          }`}
        >
          {userName ?? ''}
        </div>
      </div>
    </header>
    </>
  );
};

export default AppHeader;
