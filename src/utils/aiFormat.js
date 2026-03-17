export function normalizeRiskLevel(level) {
  const k = String(level || '').trim().toUpperCase();
  if (k === 'CRITICAL' || k === 'KRITIK' || k === 'KRİTİK') return 'CRITICAL';
  if (k === 'HIGH' || k === 'YUKSEK' || k === 'YÜKSEK') return 'HIGH';
  if (k === 'MEDIUM' || k === 'ORTA') return 'MEDIUM';
  if (k === 'LOW' || k === 'DUSUK' || k === 'DÜŞÜK') return 'LOW';
  return k || 'UNKNOWN';
}

export function riskTone(level) {
  const k = normalizeRiskLevel(level);
  if (k === 'CRITICAL') return { key: 'critical', label: 'KRİTİK', cls: 'bg-red-600 text-white' };
  if (k === 'HIGH') return { key: 'high', label: 'YÜKSEK', cls: 'bg-orange-600 text-white' };
  if (k === 'MEDIUM') return { key: 'medium', label: 'ORTA', cls: 'bg-amber-500 text-black' };
  if (k === 'LOW') return { key: 'low', label: 'DÜŞÜK', cls: 'bg-emerald-600 text-white' };
  return { key: 'unknown', label: 'BİLİNMİYOR', cls: 'bg-gray-500 text-white' };
}

export function riskLabelTr(level) {
  const k = normalizeRiskLevel(level);
  if (k === 'LOW') return 'düşük';
  if (k === 'MEDIUM') return 'orta';
  if (k === 'HIGH') return 'yüksek';
  if (k === 'CRITICAL') return 'kritik';
  return '—';
}

export function priorityTone(priority, isDark) {
  const p = String(priority || '').trim().toUpperCase();
  const base = 'text-xs px-2 py-0.5 rounded-full border';
  if (p === 'HIGH' || p === 'YUKSEK' || p === 'YÜKSEK') {
    return `${base} ${isDark ? 'bg-red-900/30 text-red-200 border-red-900' : 'bg-red-100 text-red-800 border-red-200'}`;
  }
  if (p === 'MEDIUM' || p === 'ORTA') {
    return `${base} ${isDark ? 'bg-amber-900/30 text-amber-200 border-amber-900' : 'bg-amber-100 text-amber-800 border-amber-200'}`;
  }
  if (p === 'LOW' || p === 'DUSUK' || p === 'DÜŞÜK') {
    return `${base} ${isDark ? 'bg-emerald-900/30 text-emerald-200 border-emerald-900' : 'bg-emerald-100 text-emerald-800 border-emerald-200'}`;
  }
  return `${base} ${isDark ? 'bg-gray-800 text-gray-200 border-gray-700' : 'bg-gray-100 text-gray-800 border-gray-200'}`;
}

export function extractWarnings(analysisText, limit = 6) {
  const text = String(analysisText || '').trim();
  if (!text) return [];
  const parts = text
    .split(/\n|•|\.(?=\s|$)/)
    .map((s) => s.trim())
    .filter(Boolean);
  return parts.slice(0, Math.max(0, limit));
}

