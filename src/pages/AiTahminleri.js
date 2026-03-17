import React, { useMemo, useState } from 'react';
import { Zap, AlertTriangle, CheckCircle2 } from 'lucide-react';
import { getThemeClasses } from 'utils/theme';
import { aiAnalyze } from 'services/aiService';
import { riskTone, priorityTone, extractWarnings } from 'utils/aiFormat';

export default function AiTahminleri({ isDark }) {
  const { textTitle, textSub } = getThemeClasses(isDark);
  const panelCls = isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200';
  const inputCls = isDark
    ? 'bg-gray-900 border-gray-700 text-gray-100 placeholder:text-gray-500'
    : 'bg-white border-gray-300 text-gray-900 placeholder:text-gray-400';

  const [orderIdRaw, setOrderIdRaw] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [result, setResult] = useState(null);

  const orderId = useMemo(() => {
    const v = String(orderIdRaw || '').trim();
    if (!v) return null;
    // Kullanıcı "12" veya "SIP-12" gibi değerler girebilir → sayıyı ayıkla
    const digits = v.match(/\d+/g)?.join('') || '';
    const n = Number(digits || v);
    if (!Number.isFinite(n) || n <= 0) return NaN;
    return Math.floor(n);
  }, [orderIdRaw]);

  const run = async () => {
    setError('');
    setResult(null);
    if (Number.isNaN(orderId)) {
      setError('order_id int olmalı (pozitif). Örn: 12 veya SIP-12 (boş bırakabilirsiniz).');
      return;
    }
    setLoading(true);
    try {
      const res = await aiAnalyze({ orderId });
      setResult(res);
      try {
        localStorage.setItem('fabrika_last_ai_result', JSON.stringify(res));
        const history = JSON.parse(localStorage.getItem('fabrika_ai_history') || '[]');
        history.push({
          tarih: new Date().toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }),
          mesaj: res.analysis || 'AI analizi tamamlandı.',
        });
        localStorage.setItem('fabrika_ai_history', JSON.stringify(history.slice(-10)));
      } catch (_) {}
    } catch (e) {
      setError(e?.message || 'AI analizi başarısız oldu.');
    } finally {
      setLoading(false);
    }
  };

  const tone = riskTone(result?.risk_level);

  return (
    <div className="w-full space-y-6">
      <div className={`rounded-xl border p-6 ${panelCls}`}>
        <div className="flex items-start gap-4">
          <div className={`p-3 rounded-lg ${isDark ? 'bg-gray-700' : 'bg-gray-100'}`}>
            <Zap className="text-purple-400" size={28} />
          </div>
          <div className="flex-1 min-w-0">
            <h2 className={`text-2xl font-bold ${textTitle}`}>AI Tahminleri</h2>
            <p className={`mt-1 ${textSub}`}>
              Analiz, fabrika veritabanındaki siparişler, kapasite, personel ve stok verisine göre yapılır. İstek formatı: <span className={textTitle}>mode</span> (string, bizde sabit <span className={textTitle}>risk_analysis</span>) ve opsiyonel <span className={textTitle}>order_id</span> (int).
            </p>
          </div>
        </div>

        <div className="mt-6 grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
          <div className="md:col-span-2">
            <label className={`block text-sm font-medium mb-2 ${textSub}`}>Sipariş ID (opsiyonel, int)</label>
            <input
              value={orderIdRaw}
              onChange={(e) => setOrderIdRaw(e.target.value)}
              placeholder="Örn: 12 veya SIP-12 (boş bırakılırsa genel analiz)"
              className={`w-full px-3 py-2 rounded-lg border outline-none focus:ring-2 focus:ring-purple-500/40 ${inputCls}`}
            />
          </div>
          <button
            type="button"
            onClick={run}
            disabled={loading}
            className={`w-full px-4 py-2 rounded-lg font-semibold transition
              ${loading ? 'opacity-70 cursor-not-allowed' : 'hover:opacity-95'}
              ${isDark ? 'bg-purple-600 text-white' : 'bg-purple-600 text-white'}`}
          >
            {loading ? 'Analiz yapılıyor…' : 'Analiz Yap'}
          </button>
        </div>
      </div>

      {error && (
        <div className={`rounded-xl border p-5 ${isDark ? 'bg-red-950/30 border-red-900' : 'bg-red-50 border-red-200'}`}>
          <div className="flex items-start gap-3">
            <AlertTriangle className="text-red-500" size={22} />
            <div className="flex-1 min-w-0">
              <div className={`font-semibold ${isDark ? 'text-red-200' : 'text-red-800'}`}>Hata</div>
              <div className={`${isDark ? 'text-red-200/80' : 'text-red-700'}`}>{error}</div>
              <div className={`mt-2 text-sm ${isDark ? 'text-red-200/70' : 'text-red-700/80'}`}>
                AI servisi kapalı. ai klasöründe start-ai.bat çalıştırın. Yerel backend kullanıyorsanız tekrar deneyin. Backend’in `AiService:BaseUrl` ayarı buna işaret etmeli.
              </div>
            </div>
          </div>
        </div>
      )}

      {result && (
        <div className={`rounded-xl border p-6 ${panelCls}`}>
          <div className="flex flex-wrap items-center gap-3">
            <div className={`px-3 py-1 rounded-full text-sm font-bold ${tone.cls}`}>
              Risk: {tone.label}
            </div>
            <div className={`${textSub}`}>
              Kapasite: <span className={`${textTitle} font-semibold`}>{Number(result.capacity_utilization).toFixed(1)}%</span>
            </div>
            <div className="ml-auto flex items-center gap-2">
              <CheckCircle2 className="text-emerald-500" size={18} />
              <span className={`${textSub} text-sm`}>Sonuç alındı</span>
            </div>
          </div>

          <div className="mt-4">
            <div className={`text-sm font-semibold mb-1 ${textSub}`}>Analiz</div>
            <div className={`${textTitle} leading-relaxed`}>{result.analysis}</div>
          </div>

          <div className="mt-5">
            <div className={`text-sm font-semibold mb-2 ${textSub}`}>Uyarılar (özet)</div>
            <div className={`space-y-2 ${textSub}`}>
              {extractWarnings(result.analysis).map((uyari, i) => (
                <p key={i} className="text-sm flex items-start gap-2">
                  <span className="text-amber-500 mt-0.5">•</span> {uyari}
                </p>
              ))}
              {extractWarnings(result.analysis).length === 0 && (
                <div className={`${textSub}`}>Uyarı bulunamadı.</div>
              )}
            </div>
          </div>

          <div className="mt-5">
            <div className={`text-sm font-semibold mb-2 ${textSub}`}>Öneriler</div>
            <div className="space-y-2">
              {(result.recommendations || []).map((r, idx) => (
                <div
                  key={`${r.action || 'action'}-${idx}`}
                  className={`rounded-lg border p-4 ${isDark ? 'border-gray-700 bg-gray-900/40' : 'border-gray-200 bg-gray-50'}`}
                >
                  <div className="flex flex-wrap items-center gap-2">
                    <span className={`text-sm font-bold ${textTitle}`}>{r.action}</span>
                    <span className={priorityTone(r.priority, isDark)}>öncelik: {r.priority}</span>
                  </div>
                  <div className={`mt-1 ${textSub}`}>{r.description}</div>
                </div>
              ))}
              {(!result.recommendations || result.recommendations.length === 0) && (
                <div className={`${textSub}`}>Öneri bulunamadı.</div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

