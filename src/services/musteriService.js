import { api } from './api';
import { customerFromApi, customerToApi } from './mappers';

export async function getMusteriler() {
  const list = await api.get('/api/Customer/tum-musteri-listesi');
  return (Array.isArray(list) ? list : []).map(customerFromApi);
}

export async function getMusteri(id) {
  const r = await api.get(`/api/Customer/musteri-detay-getir/${id}`);
  return customerFromApi(r);
}

export async function createMusteri(data) {
  const body = customerToApi(data);
  delete body.id;
  const created = await api.post('/api/Customer/yeni-musteri-kaydi-ekle', body);
  return customerFromApi(created);
}

export async function updateMusteri(id, data) {
  const body = customerToApi({ ...data, id });
  await api.put(`/api/Customer/musteri-bilgisi-guncelle/${id}`, body);
  return customerFromApi({ ...body, id });
}

export async function deleteMusteri(id) {
  await api.delete(`/api/Customer/musteri-kaydi-sil/${id}`);
}
