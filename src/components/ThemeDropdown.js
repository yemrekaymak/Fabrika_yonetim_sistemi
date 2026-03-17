import React, { useState, useRef, useEffect } from 'react';
import { ChevronDown } from 'lucide-react';

/** Tema uyumlu, yumuşak geçişli dropdown (Sipariş müşteri/ürün ve Personel departman seçimi) */
const ThemeDropdown = ({ options, value, onChange, renderLabel, placeholder, isDark, label, disabled, className }) => {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const selected = options.find((o) => o.id === value);
  const triggerCls = isDark
    ? 'bg-gray-700 border-gray-600 text-white hover:border-gray-500 focus:ring-blue-500/40'
    : 'bg-white border-gray-300 text-gray-800 hover:border-gray-400 focus:ring-blue-400/30';
  const disabledCls = disabled ? 'opacity-60 cursor-not-allowed pointer-events-none' : '';
  const panelCls = isDark
    ? 'bg-gray-800 border-gray-600 shadow-xl shadow-black/20'
    : 'bg-white border-gray-200 shadow-xl shadow-gray-200/80';
  const optionCls = isDark
    ? 'hover:bg-gray-700/80 text-gray-200'
    : 'hover:bg-gray-100 text-gray-700';
  const optionActiveCls = isDark ? 'bg-blue-900/40 text-blue-300' : 'bg-blue-50 text-blue-800';

  return (
    <div ref={ref} className={`relative w-full ${className ?? ''}`}>
      {label && (
        <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
          {label}
        </label>
      )}
      <button
        type="button"
        disabled={disabled}
        onClick={() => !disabled && setOpen((o) => !o)}
        className={`w-full min-w-0 px-3 py-2.5 rounded-lg border text-left flex items-center justify-between transition-all duration-200 outline-none focus:ring-2 ${triggerCls} ${disabledCls}`}
      >
        <span className="truncate">{selected ? renderLabel(selected) : placeholder}</span>
        <ChevronDown
          size={18}
          className={`flex-shrink-0 ml-2 transition-transform duration-300 ${open ? 'rotate-180' : ''}`}
        />
      </button>
      <div
        className={`absolute z-50 left-0 right-0 mt-1 rounded-lg border overflow-hidden transition-all duration-300 ease-out ${
          open ? 'opacity-100 translate-y-0 visible' : 'opacity-0 -translate-y-2 pointer-events-none invisible'
        } ${panelCls}`}
      >
        <div
          className="max-h-52 overflow-y-auto overscroll-contain scroll-smooth"
          style={{
            scrollbarWidth: 'thin',
            scrollbarColor: isDark ? '#4b5563 transparent' : '#d1d5db transparent',
          }}
        >
          {options.map((opt) => {
            const isActive = opt.id === value;
            return (
              <button
                key={opt.id}
                type="button"
                onClick={() => {
                  onChange(opt.id);
                  setOpen(false);
                }}
                className={`w-full px-3 py-2.5 text-left text-sm transition-colors duration-200 ${optionCls} ${isActive ? optionActiveCls : ''}`}
              >
                {renderLabel(opt)}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default ThemeDropdown;
