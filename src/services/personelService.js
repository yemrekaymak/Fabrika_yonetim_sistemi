import { api } from './api';
import { personnelFromApi, personnelToApi } from './mappers';

export async function getPersonel() {
  const list = await api.get('/api/Personnel/tum-personel-listesi');
  return (Array.isArray(list) ? list : []).map(personnelFromApi);
}

export async function getPersonelDetay(id) {
  const r = await api.get(`/api/Personnel/personel-detay-getir/${id}`);
  return personnelFromApi(r);
}

export async function createPersonel(data) {
  const body = personnelToApi(data);
  const created = await api.post('/api/Personnel/yeni-personel-ekle', body);
  return personnelFromApi(created);
}

export async function updatePersonel(id, data) {
  const body = personnelToApi({ ...data, id });
  await api.put(`/api/Personnel/personel-bilgisi-guncelle/${id}`, body);
  return personnelFromApi({ ...body, id });
}

export async function deletePersonel(id) {
  await api.delete(`/api/Personnel/personel-kaydi-sil/${id}`);
}
