import React from 'react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts';

const RENK_PALETI = ['#3b82f6', '#a855f7', '#22c55e', '#f59e0b', '#ef4444'];

const CustomTooltipContent = ({ active, payload, label, isDark }) => {
  if (!active || !payload?.length) return null;
  const textColor = isDark ? '#f1f5f9' : '#1e293b';
  return (
    <div style={{ color: textColor }}>
      <div style={{ fontWeight: 600, marginBottom: 4 }}>{label}</div>
      {payload.map((entry) => (
        <div key={entry.dataKey} style={{ fontSize: 13 }}>
          {entry.name} : {entry.value}
        </div>
      ))}
    </div>
  );
};

const DepoBarChart = ({ isDark, data }) => {
  const chartData = Array.isArray(data) && data.length > 0
    ? data.map((d, i) => ({
        malzeme: d.malzeme || d.name || d.ad || d.label || '—',
        miktar: Number(d.miktar ?? d.stockQuantity ?? d.miktarSayi ?? 0),
        renk: d.renk || RENK_PALETI[i % RENK_PALETI.length],
      }))
    : [];

  const gridStroke = isDark ? '#374151' : '#e5e7eb';
  const axisStroke = isDark ? '#9ca3af' : '#6b7280';
  const tooltipBg = isDark ? '#1e293b' : '#fff';
  const tooltipBorder = isDark ? '#334155' : '#e2e8f0';
  const tooltipCursorFill = isDark ? 'rgba(15, 23, 42, 0.82)' : 'rgba(0, 0, 0, 0.04)';

  return (
    <div className="w-full h-64 mt-4">
      {chartData.length === 0 ? (
        <div
          className={`w-full h-full rounded-lg border flex items-center justify-center text-sm ${
            isDark ? 'border-gray-700 bg-gray-800/40 text-gray-400' : 'border-gray-200 bg-gray-50 text-gray-500'
          }`}
        >
          Depo grafiği için veri yok.
        </div>
      ) : (
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartData} margin={{ top: 16, right: 16, left: 0, bottom: 8 }}>
          <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} />
          <XAxis dataKey="malzeme" stroke={axisStroke} tick={{ fontSize: 12 }} />
          <YAxis stroke={axisStroke} tick={{ fontSize: 12 }} />
          <Tooltip
            content={({ active, payload, label }) => (
              <CustomTooltipContent active={active} payload={payload} label={label} isDark={isDark} />
            )}
            contentStyle={{
              backgroundColor: tooltipBg,
              border: `1px solid ${tooltipBorder}`,
              borderRadius: 14,
              padding: '10px 14px',
              boxShadow: isDark ? '0 4px 16px rgba(0,0,0,0.35)' : '0 2px 8px rgba(0,0,0,0.06)',
              transition: 'none',
            }}
            cursor={{ fill: tooltipCursorFill, stroke: isDark ? '#475569' : '#e2e8f0', strokeWidth: 1 }}
            animationDuration={0}
            isAnimationActive={false}
          />
          <Bar dataKey="miktar" name="Miktar" radius={[4, 4, 0, 0]} maxBarSize={80} isAnimationActive={false}>
            {chartData.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.renk} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
      )}
    </div>
  );
};

export default DepoBarChart;
