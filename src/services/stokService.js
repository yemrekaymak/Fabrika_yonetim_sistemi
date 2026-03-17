import { api } from './api';
import { stockFromApi, stockToApi } from './mappers';

/** Swagger: GET/POST /api/stok, GET/PUT/DELETE /api/stok/{code} */
const STOCK_BASE = '/api/stok';

export async function getStok() {
  const list = await api.get(STOCK_BASE);
  return (Array.isArray(list) ? list : []).map(stockFromApi);
}

export async function getStokDetay(codeOrId) {
  const code = String(codeOrId ?? '');
  if (!code) return null;
  try {
    const r = await api.get(`${STOCK_BASE}/${encodeURIComponent(code)}`);
    return stockFromApi(r);
  } catch {
    const list = await api.get(STOCK_BASE);
    const found = Array.isArray(list) ? list.find((r) => (r.kod || r.stokKodu || r.id) === code) : null;
    return found ? stockFromApi(found) : null;
  }
}

export async function createStok(data) {
  const body = stockToApi(data);
  delete body.id;
  const created = await api.post(STOCK_BASE, body);
  return stockFromApi(created);
}

export async function updateStok(codeOrId, data) {
  const code = String(codeOrId ?? '');
  const body = stockToApi({ ...data, kod: code, id: codeOrId });
  const updated = await api.put(`${STOCK_BASE}/${encodeURIComponent(code)}`, body);
  return stockFromApi(updated);
}

export async function deleteStok(codeOrId) {
  const code = String(codeOrId ?? '');
  await api.delete(`${STOCK_BASE}/${encodeURIComponent(code)}`);
}
