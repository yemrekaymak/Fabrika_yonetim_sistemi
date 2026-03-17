import { api } from './api';
import { productFromApi, productToApi } from './mappers';

/** Backend: /api/Product/liste, yeni-urun-ekle, urun-getir, urun-guncelle, sil */
const PRODUCT_BASE = '/api/Product';

export async function getUrunler() {
  const list = await api.get(`${PRODUCT_BASE}/liste`);
  return (Array.isArray(list) ? list : []).map(productFromApi);
}

export async function getUrun(id) {
  const r = await api.get(`${PRODUCT_BASE}/urun-getir/${encodeURIComponent(id)}`);
  return productFromApi(r);
}

export async function createUrun(data) {
  const body = productToApi(data);
  delete body.id;
  const created = await api.post(`${PRODUCT_BASE}/yeni-urun-ekle`, body);
  return productFromApi(created);
}

export async function updateUrun(id, data) {
  const body = productToApi({ ...data, urun_kodu: id });
  const updated = await api.put(`${PRODUCT_BASE}/urun-guncelle/${encodeURIComponent(id)}`, body);
  return productFromApi(updated);
}

export async function deleteUrun(id) {
  await api.delete(`${PRODUCT_BASE}/sil/${encodeURIComponent(id)}`);
}
