import { api } from './api';
import { machineFromApi, machineToApi } from './mappers';

export async function getMakineler() {
  const list = await api.get('/api/Machine/tum-makine-listesi');
  return (Array.isArray(list) ? list : []).map(machineFromApi);
}

export async function getMakine(id) {
  const r = await api.get(`/api/Machine/makine-detay-getir/${id}`);
  return machineFromApi(r);
}

export async function createMakine(data) {
  const body = machineToApi(data);
  delete body.Id;
  delete body.id;
  const created = await api.post('/api/Machine/yeni-makine-ekle', body);
  return machineFromApi(created);
}

export async function updateMakine(id, data) {
  const body = machineToApi({ ...data, id });
  const updated = await api.put(`/api/Machine/makine-bilgisi-guncelle/${id}`, body);
  return machineFromApi(updated || { ...body, id });
}

export async function deleteMakine(id) {
  await api.delete(`/api/Machine/makine-kaydi-sil/${id}`);
}
