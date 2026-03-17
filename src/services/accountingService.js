import { api } from './api';

/** Backend Expense (gider_adi, tutar, gider_tipi, tarih) → Frontend (id, kalem, tutar, tip) */
function expenseFromApi(r) {
  if (!r) return null;
  const kalem = r.gider_adi ?? '';
  return {
    id: kalem || `gider-${r.tutar}-${Date.now()}`,
    kalem,
    tutar: r.tutar ?? 0,
    tip: r.gider_tipi ?? 'sabit',
  };
}

/** Frontend → Backend Expense */
function expenseToApi(u) {
  if (!u) return null;
  return {
    gider_adi: u.kalem || null,
    tutar: Number(u.tutar) || 0,
    gider_tipi: u.tip || 'sabit',
    tarih: new Date().toISOString(),
  };
}

/** Backend Income → Frontend */
function incomeFromApi(r) {
  if (!r) return null;
  return {
    id: r.gelir_adi ?? `gelir-${r.tutar}-${Date.now()}`,
    kalem: r.gelir_adi ?? '',
    tutar: r.tutar ?? 0,
    tarih: r.tarih ?? null,
  };
}

export async function getGiderListesi() {
  const list = await api.get('/api/Accounting/gider-listesi');
  return (Array.isArray(list) ? list : []).map(expenseFromApi);
}

export async function createGider(data) {
  const body = expenseToApi(data);
  const created = await api.post('/api/Accounting/gider-kaydet', body);
  return expenseFromApi(created);
}

export async function deleteGider(giderAdi) {
  await api.delete(`/api/Accounting/gider-sil/${encodeURIComponent(giderAdi)}`);
}

export async function getGelirListesi() {
  const list = await api.get('/api/Accounting/gelir-listesi');
  return (Array.isArray(list) ? list : []).map(incomeFromApi);
}

export async function createGelir(data) {
  await api.post('/api/Accounting/gelir-kaydet', {
    gelir_adi: data.kalem || null,
    tutar: Number(data.tutar) || 0,
    tarih: new Date().toISOString(),
  });
}

export async function deleteGelir(gelirAdi) {
  await api.delete(`/api/Accounting/gelir-sil/${encodeURIComponent(gelirAdi)}`);
}

export async function getOzetRapor() {
  return api.get('/api/Accounting/ozet-rapor');
}

// AI analiz endpoint'i – Backend'te /api/Accounting/ai-analiz tanımlı olmalı
export async function getAiAnaliz(prompt) {
  return api.post('/api/Accounting/ai-analiz', {
    prompt: prompt || 'Muhasebe özet analizi',
  });
}
