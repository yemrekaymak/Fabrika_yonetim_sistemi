import React from 'react';
import { ChevronUp, ChevronDown } from 'lucide-react';

/**
 * Sayı girişi + büyük, net hover’lı yükselt/düşür butonları.
 * Projedeki tüm number input’larda tutarlı görünüm için kullanılır.
 */
const NumberStepperInput = ({
  value,
  onChange,
  min,
  max,
  step = 1,
  placeholder,
  className = '',
  isDark = false,
  id,
  disabled,
  ...rest
}) => {
  const numValue = typeof value === 'string' ? parseFloat(value) || 0 : Number(value) ?? 0;
  const numStep = typeof step === 'string' ? parseFloat(step) || 1 : Number(step) ?? 1;
  const numMin = min != null ? (typeof min === 'string' ? parseFloat(min) : min) : -Infinity;
  const numMax = max != null ? (typeof max === 'string' ? parseFloat(max) : max) : Infinity;

  const roundToStep = (n) => {
    if (numStep >= 1) return Math.round(n);
    const decimals = String(numStep).split('.')[1]?.length ?? 2;
    const factor = Math.pow(10, decimals);
    return Math.round(n * factor) / factor;
  };

  const handleInputChange = (e) => {
    const v = e.target.value;
    if (v === '' || v === '-') {
      onChange?.(e);
      return;
    }
    const n = parseFloat(v);
    if (!Number.isNaN(n)) onChange?.(e);
  };

  const stepUp = () => {
    const raw = Math.min(numMax, numValue + numStep);
    const next = roundToStep(raw);
    if (next !== numValue) {
      const synthetic = { target: { value: String(next) } };
      onChange?.(synthetic);
    }
  };

  const stepDown = () => {
    const raw = Math.max(numMin, numValue - numStep);
    const next = roundToStep(raw);
    if (next !== numValue) {
      const synthetic = { target: { value: String(next) } };
      onChange?.(synthetic);
    }
  };


  const inputCls = isDark
    ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-500'
    : 'bg-white border-gray-300 text-gray-800 placeholder-gray-400';
  const btnCls = isDark
    ? 'text-gray-300 hover:bg-gray-600 hover:text-white border-gray-600'
    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900 border-gray-300';
  const btnActiveCls = isDark ? 'active:bg-gray-500' : 'active:bg-gray-200';

  return (
    <div className={`number-stepper-wrapper flex rounded-lg border overflow-hidden ${isDark ? 'border-gray-600' : 'border-gray-300'} ${className}`}>
      <input
        type="number"
        id={id}
        value={value}
        onChange={handleInputChange}
        min={min}
        max={max}
        step={step}
        placeholder={placeholder}
        disabled={disabled}
        className={`flex-1 min-w-0 px-3 py-2.5 border-0 rounded-none outline-none focus:ring-2 focus:ring-blue-500/30 ${inputCls}`}
        {...rest}
      />
      <div className={`flex flex-col flex-shrink-0 w-9 h-full min-h-[42px] border-l ${isDark ? 'border-gray-600' : 'border-gray-300'}`}>
        <button
          type="button"
          tabIndex={-1}
          onClick={stepUp}
          disabled={disabled || numValue >= numMax}
          className={`flex items-center justify-center h-1/2 min-h-[20px] border-b ${isDark ? 'border-gray-600' : 'border-gray-200'} ${btnCls} ${btnActiveCls} transition-colors disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-transparent ${isDark ? 'disabled:hover:text-gray-300' : 'disabled:hover:text-gray-600'}`}
          title="Artır"
          aria-label="Artır"
        >
          <ChevronUp size={16} strokeWidth={2.25} className="pointer-events-none" />
        </button>
        <button
          type="button"
          tabIndex={-1}
          onClick={stepDown}
          disabled={disabled || numValue <= numMin}
          className={`flex items-center justify-center h-1/2 min-h-[20px] ${btnCls} ${btnActiveCls} transition-colors disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-transparent ${isDark ? 'disabled:hover:text-gray-300' : 'disabled:hover:text-gray-600'}`}
          title="Azalt"
          aria-label="Azalt"
        >
          <ChevronDown size={16} strokeWidth={2.25} className="pointer-events-none" />
        </button>
      </div>
    </div>
  );
};

export default NumberStepperInput;
