import React from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { URETIM_GRAFIK_VERISI } from 'constants/chartData';

const ProductionLineChart = ({ isDark, data }) => {
  const chartData = data && data.length > 0 ? data : URETIM_GRAFIK_VERISI;
  const gridStroke = isDark ? '#374151' : '#e5e7eb';
  const axisStroke = isDark ? '#9ca3af' : '#6b7280';
  const tooltipBg = isDark ? '#1f2937' : '#fff';
  const tooltipBorder = isDark ? '#374151' : '#e5e7eb';

  return (
    <div className="w-full h-72 mt-6">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={chartData} margin={{ top: 8, right: 16, left: 0, bottom: 8 }}>
          <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} />
          <XAxis dataKey="saat" stroke={axisStroke} tick={{ fontSize: 12 }} />
          <YAxis stroke={axisStroke} tick={{ fontSize: 12 }} />
          <Tooltip
            contentStyle={{
              backgroundColor: tooltipBg,
              border: `1px solid ${tooltipBorder}`,
              borderRadius: 8,
            }}
            labelStyle={{ color: axisStroke }}
            formatter={(value) => [value, '']}
            labelFormatter={(label) => `Dönem: ${label}`}
          />
          <Legend wrapperStyle={{ fontSize: 12 }} />
          <Line
            type="monotone"
            dataKey="uretim"
            name="Üretim"
            stroke="#3b82f6"
            strokeWidth={2}
            dot={{ fill: isDark ? '#1f2937' : '#fff', stroke: '#3b82f6', strokeWidth: 2 }}
          />
          <Line
            type="monotone"
            dataKey="hedef"
            name="Hedef"
            stroke="#10b981"
            strokeWidth={2}
            strokeDasharray="5 5"
            dot={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};

export default ProductionLineChart;
