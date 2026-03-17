import React from 'react';
import { LogOut } from 'lucide-react';
import { MENU_ITEMS } from 'constants/navigation';

const Sidebar = ({ activeTab, setActiveTab, onLogout }) => {
  return (
    <div className="h-screen w-64 bg-gray-900 text-gray-300 fixed left-0 top-0 flex flex-col shadow-xl z-20 border-r border-gray-800">
      <div className="px-4 py-6 text-xl font-bold text-white border-b border-gray-800 tracking-wider flex items-center gap-1 pl-7">
        <img
          src={`${process.env.PUBLIC_URL || ''}/factory-logo.png`}
          alt=""
          className="w-auto object-contain flex-shrink-0 self-center bg-gray-900"
          style={{ height: '4em' }}
          aria-hidden
        />
        {' '}
        FABRİKA <span className="text-blue-500">YS</span>
      </div>
      <nav className="flex-1 p-4 space-y-2">
        {MENU_ITEMS.map((item) => {
          const Icon = item.icon;
          return (
            <div
              key={item.id}
              onClick={() => setActiveTab(item.id)}
              className={`flex items-center gap-3 p-3 rounded-lg cursor-pointer transition-all duration-200 font-medium ${
                activeTab === item.id ||
                (item.id === 'personel' && ['personel-bilgi', 'personel-ekle', 'personel-duzenle'].includes(activeTab)) ||
                (item.id === 'musteriler' && ['musteriler-ekle', 'musteri-bilgi'].includes(activeTab))
                  ? 'bg-blue-600 text-white shadow-lg translate-x-1'
                  : 'hover:bg-gray-800 hover:text-white'
              }`}
            >
              <Icon size={20} /> {item.label}
            </div>
          );
        })}
      </nav>
      <div className="p-4 border-t border-gray-800">
        <button
          type="button"
          onClick={onLogout}
          className="flex items-center gap-3 text-red-400 hover:text-red-300 transition w-full p-2 hover:bg-gray-800 rounded"
        >
          <LogOut size={20} /> Çıkış Yap
        </button>
      </div>
    </div>
  );
};

export default Sidebar;
