import React, { useState, useEffect, useMemo } from 'react';
import { Sparkles, AlertTriangle, ListChecks, History } from 'lucide-react';
import { ProductionLineChart } from 'charts';
import { getThemeClasses } from 'utils/theme';
import { getMakineler, getSiparisler } from 'services';
import { riskLabelTr, priorityTone, extractWarnings } from 'utils/aiFormat';

const ACIK_DURUMLAR = ['Beklemede', 'Onaylandı', 'Üretimde'];

const riskBadgeClass = (risk, isDark) => {
  const base = 'text-xs font-bold px-3 py-1 rounded-full border';
  if (risk === 'düşük') return `${base} ${isDark ? 'bg-green-900/30 text-green-300 border-green-800' : 'bg-green-100 text-green-800 border-green-200'}`;
  if (risk === 'yüksek' || risk === 'kritik') return `${base} ${isDark ? 'bg-red-900/30 text-red-300 border-red-800' : 'bg-red-100 text-red-800 border-red-200'}`;
  return `${base} ${isDark ? 'bg-amber-900/30 text-amber-300 border-amber-800' : 'bg-amber-100 text-amber-800 border-amber-200'}`;
};

const Dashboard = ({ isDark, onNavigateToAi }) => {
  const { bgCard, textTitle, textSub } = getThemeClasses(isDark);
  const [makineler, setMakineler] = useState([]);
  const [siparisler, setSiparisler] = useState([]);
  const [lastAi, setLastAi] = useState(null);
  const [aiHistory, setAiHistory] = useState([]);

  useEffect(() => {
    getMakineler().then(setMakineler).catch(() => setMakineler([]));
    getSiparisler().then(setSiparisler).catch(() => setSiparisler([]));
  }, []);

  useEffect(() => {
    try {
      const raw = localStorage.getItem('fabrika_last_ai_result');
      setLastAi(raw ? JSON.parse(raw) : null);
      const hist = localStorage.getItem('fabrika_ai_history');
      setAiHistory(hist ? JSON.parse(hist) : []);
    } catch {
      setLastAi(null);
      setAiHistory([]);
    }
  }, []);

  const acikToplam = useMemo(() => {
    return siparisler
      .filter((s) => ACIK_DURUMLAR.includes(s.durum))
      .reduce((sum, s) => sum + (Number(s.miktar) || 0), 0);
  }, [siparisler]);

  const grafikVerisi = useMemo(() => {
    const byDate = {};
    const today = new Date();
    for (let i = 6; i >= 0; i--) {
      const d = new Date(today);
      d.setDate(d.getDate() - i);
      const key = d.toISOString().slice(0, 10);
      byDate[key] = { key, toplam: 0, label: d.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit' }) };
    }
    siparisler.forEach((s) => {
      const raw = s.tarih || '';
      const dateKey = raw.includes('T') ? raw.slice(0, 10) : raw.slice(0, 10) || (raw.includes('.') ? raw.split('.').reverse().join('-') : raw);
      if (byDate[dateKey]) byDate[dateKey].toplam += Number(s.miktar) || 0;
    });
    const sorted = Object.values(byDate).sort((a, b) => a.key.localeCompare(b.key));
    const max = Math.max(1, ...sorted.map((x) => x.toplam));
    return sorted.map((x) => ({
      saat: x.label,
      uretim: x.toplam,
      hedef: Math.round(max * 0.8),
    }));
  }, [siparisler]);

  const riskSeviyesi = lastAi ? riskLabelTr(lastAi.risk_level) : null;
  const uyarilar = useMemo(() => extractWarnings(lastAi?.analysis, 6), [lastAi]);
  const aksiyonlar = lastAi?.recommendations && Array.isArray(lastAi.recommendations) ? lastAi.recommendations : [];

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className={`p-6 rounded-xl shadow-sm border transition-colors duration-300 ${bgCard}`}>
          <h3 className={`text-sm font-medium ${textSub}`}>Açık Sipariş Toplamı</h3>
          <p className={`text-3xl font-bold mt-2 ${textTitle}`}>{acikToplam.toLocaleString('tr-TR')}</p>
          <span className="text-blue-500 text-xs font-bold">Beklemede + Onaylandı + Üretimde</span>
        </div>
        <div className={`p-6 rounded-xl shadow-sm border transition-colors duration-300 ${bgCard}`}>
          <h3 className={`text-sm font-medium ${textSub}`}>Aktif Makine</h3>
          <p className={`text-3xl font-bold mt-2 ${textTitle}`}>{makineler.length}</p>
          <span className="text-blue-500 text-xs font-bold">Kayıtlı makine</span>
        </div>
        <div
          className={`p-6 rounded-xl shadow-sm border transition-colors duration-300 ${
            isDark ? 'bg-gray-800 border-purple-900/50' : 'bg-white border-purple-200'
          }`}
        >
          <h3 className="text-purple-500 text-sm font-medium">AI Tahmini</h3>
          <p className={`text-xl font-bold mt-2 ${textTitle}`}>
            {riskSeviyesi ? (riskSeviyesi === 'düşük' ? 'Düşük risk' : riskSeviyesi === 'yüksek' || riskSeviyesi === 'kritik' ? 'Yüksek risk' : 'Orta risk') : '—'}
          </p>
          <span className="text-purple-500 text-xs font-bold">
            {lastAi ? 'Son analiz sonucu' : 'AI Tahminleri sayfasından analiz çalıştırın'}
          </span>
        </div>
      </div>

      <div className={`p-8 rounded-xl shadow-sm border transition-colors duration-300 min-h-[400px] ${bgCard}`}>
        <div className="flex flex-wrap justify-between items-center gap-4 mb-4">
          <h3 className={`font-bold text-lg ${textTitle}`}>Sipariş Bazlı Üretim Özeti (Son 7 Gün)</h3>
          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={() => onNavigateToAi?.()}
              className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold border transition-colors ${
                isDark
                  ? 'border-purple-500/60 text-purple-400 hover:bg-purple-500/20'
                  : 'border-purple-400 text-purple-700 hover:bg-purple-50'
              }`}
              title="AI analiz sayfasına git"
            >
              <Sparkles size={18} /> AI Analiz
            </button>
            <div className={`flex gap-4 text-xs font-medium ${textSub}`}>
              <span className="flex items-center gap-1">
                <span className="w-3 h-3 bg-blue-500 rounded-full" /> Toplam miktar
              </span>
              <span className="flex items-center gap-1">
                <span className="w-3 h-3 bg-green-500 rounded-full opacity-50" /> Hedef
              </span>
            </div>
          </div>
        </div>
        <ProductionLineChart isDark={isDark} data={grafikVerisi} />
      </div>

      <div className={`p-6 rounded-xl shadow-sm border transition-colors duration-300 ${bgCard}`}>
        <div className="flex flex-wrap items-center justify-between gap-4 mb-4">
          <div className="flex items-center gap-3">
            <h2 className={`text-lg font-bold flex items-center gap-2 ${textTitle}`}>
              <AlertTriangle size={22} className="text-amber-500" /> AI Uyarıları
            </h2>
            {riskSeviyesi != null && (
              <span className={riskBadgeClass(riskSeviyesi, isDark)}>
                {riskSeviyesi === 'düşük' ? 'Düşük risk' : riskSeviyesi === 'yüksek' || riskSeviyesi === 'kritik' ? 'Yüksek risk' : 'Orta risk'}
              </span>
            )}
          </div>
        </div>
        <div className={`space-y-2 mb-4 ${textSub}`}>
          {uyarilar.length ? (
            uyarilar.map((uyari, i) => (
              <p key={i} className="text-sm flex items-start gap-2">
                <span className="text-amber-500 mt-0.5">•</span> {uyari}
              </p>
            ))
          ) : (
            <p className="text-sm">
              {lastAi ? 'Özet metin yok.' : 'AI Tahminleri sayfasından bir analiz çalıştırın; sonuç burada görünecektir.'}
            </p>
          )}
        </div>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div>
            <h3 className={`text-sm font-semibold mb-2 flex items-center gap-2 ${textTitle}`}>
              <ListChecks size={16} /> Aksiyon önerileri
            </h3>
            <ul className={`text-sm space-y-1.5 ${textSub}`}>
              {aksiyonlar.length ? (
                aksiyonlar.map((aksiyon, i) => (
                  <li key={i} className={`rounded-lg border p-3 ${isDark ? 'border-gray-700 bg-gray-900/40' : 'border-gray-200 bg-gray-50'}`}>
                    <div className="flex flex-wrap items-center gap-2">
                      <span className={`text-sm font-semibold ${textTitle}`}>
                        {typeof aksiyon === 'string' ? aksiyon : aksiyon?.action || 'Öneri'}
                      </span>
                      {typeof aksiyon !== 'string' && (
                        <span className={priorityTone(aksiyon?.priority, isDark)}>öncelik: {aksiyon?.priority}</span>
                      )}
                    </div>
                    {typeof aksiyon !== 'string' && (aksiyon?.description || aksiyon?.detail) && (
                      <div className={`mt-1 text-sm ${textSub}`}>{aksiyon?.description || aksiyon?.detail}</div>
                    )}
                  </li>
                ))
              ) : (
                <li className="opacity-80">Son analizde öneri yok.</li>
              )}
            </ul>
          </div>
          <div>
            <h3 className={`text-sm font-semibold mb-2 flex items-center gap-2 ${textTitle}`}>
              <History size={16} /> Geçmiş analizler
            </h3>
            <ul className={`text-sm space-y-2 max-h-32 overflow-y-auto ${textSub}`}>
              {[...aiHistory].reverse().map((log, i) => (
                <li key={i} className="flex items-baseline gap-2">
                  <span className="flex-shrink-0 text-xs font-mono opacity-80">{log.tarih}</span>
                  <span>{log.mesaj}</span>
                </li>
              ))}
              {!aiHistory.length && <li className="opacity-80">Henüz kayıt yok.</li>}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
