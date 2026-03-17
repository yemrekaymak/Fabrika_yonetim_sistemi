import { api } from './api';
import { orderFromApi, orderToApi } from './mappers';

export async function getSiparisler(durum) {
  const path = durum
    ? `/api/Order/tum-siparis-listesi?durum=${encodeURIComponent(durum)}`
    : '/api/Order/tum-siparis-listesi';
  const list = await api.get(path);
  return (Array.isArray(list) ? list : []).map(orderFromApi);
}

export async function getSiparis(id) {
  const r = await api.get(`/api/Order/siparis-detay-getir/${id}`);
  return orderFromApi(r);
}

/** Backend: OrderCreateRequest { MusteriAdi, UrunKodu?, UrunAdi?, Miktar } - ürün UrunKodu veya UrunAdi ile bulunur */
/** Backend (factory.db uyumu): OrderCreateRequest { ProductId(int), Quantity(int), SalePrice?, CustomerName, DeliveryDate?, Notes? } */
export async function createSiparis({ customerName, productId, quantity, salePrice, deliveryDate, notes } = {}) {
  const body = {
    ProductId: Number(productId) || 0,
    Quantity: Number(quantity) || 0,
    SalePrice: salePrice != null && Number(salePrice) >= 0 ? Number(salePrice) : null,
    CustomerName: customerName || '',
    DeliveryDate: deliveryDate || null,
    Notes: notes || '',
  };
  const created = await api.post('/api/Order/yeni-siparis-olustur', body);
  return orderFromApi(created);
}

export async function updateSiparis(id, data) {
  const body = orderToApi(data);
  const updated = await api.put(`/api/Order/siparis-tum-verileri-duzelt/${id}`, body);
  return orderFromApi(updated);
}

/** Swagger: OrderStatusUpdateRequest { Status } */
export async function updateSiparisStatus(id, status) {
  await api.patch(`/api/Order/siparis-durumu-guncelle/${id}`, { Status: status ?? null });
}

export async function deleteSiparis(id) {
  await api.delete(`/api/Order/siparis-kaydi-sil/${id}`);
}
