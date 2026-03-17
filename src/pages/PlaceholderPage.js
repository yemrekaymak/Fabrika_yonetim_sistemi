import React from 'react';
import { Zap } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';

const PLACEHOLDER_CONFIG = {
  ai: {
    icon: Zap,
    title: 'Yapay Zeka Modülü',
    description: 'Bu modül geliştirme aşamasındadır. API bağlandığında tahmin grafikleri burada gösterilecek.',
    iconClass: 'text-purple-400',
  },
};

const PlaceholderPage = ({ activeTab, isDark }) => {
  const { textTitle, textSub } = getThemeClasses(isDark);
  const config = PLACEHOLDER_CONFIG[activeTab];

  if (!config) return null;

  const Icon = config.icon;
  const wrapperCls = isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-300';
  const iconWrapperCls = isDark ? 'bg-gray-700' : 'bg-gray-100';

  return (
    <div
      className={`flex flex-col items-center justify-center h-96 rounded-xl border-2 border-dashed p-8 w-full ${wrapperCls}`}
    >
      <div className={`p-4 rounded-full mb-4 ${iconWrapperCls}`}>
        <Icon size={40} className={config.iconClass} />
      </div>
      <h3 className={`text-xl font-bold mb-2 ${textTitle}`}>{config.title}</h3>
      <p className={`text-center max-w-2xl leading-relaxed ${textSub}`}>{config.description}</p>
    </div>
  );
};

export default PlaceholderPage;
