/**
 * Sipariş oluşturma ve Muhasebe sayfalarında ortak kullanılan mock sipariş ve müşteri verisi
 */

export const MOCK_MUSTERILER = [
  { id: 'M1', unvan: 'ABC Otomotiv A.Ş.', yetkili: 'Ahmet Yılmaz', tel: '0212 555 0101' },
  { id: 'M2', unvan: 'XYZ Metal San. Tic. Ltd.', yetkili: 'Fatma Demir', tel: '0312 555 0202' },
  { id: 'M3', unvan: 'Delta Endüstri A.Ş.', yetkili: 'Mehmet Kaya', tel: '0216 555 0303' },
  { id: 'M4', unvan: 'Omega Makine Ltd.', yetkili: 'Zeynep Öz', tel: '0232 555 0404' },
];

export const MOCK_URUNLER = [
  { id: 'UR-001', ad: 'Çelik Levha (5mm)', birim: 'Adet', birimFiyat: 85, birimMaliyet: 72, birimSure: 0.5, gunlukUretim: 50 },
  { id: 'UR-002', ad: 'Plastik Granül PP', birim: 'kg', birimFiyat: 12, birimMaliyet: 9.5, birimSure: 0.02, gunlukUretim: 2000 },
  { id: 'UR-003', ad: 'Endüstriyel Boya', birim: 'Lt', birimFiyat: 45, birimMaliyet: 38, birimSure: 0.05, gunlukUretim: 500 },
  { id: 'UR-004', ad: 'Kauçuk Conta', birim: 'Adet', birimFiyat: 8, birimMaliyet: 6, birimSure: 0.01, gunlukUretim: 5000 },
  { id: 'UR-005', ad: 'Paslanmaz Vida M8', birim: 'Adet', birimFiyat: 2.5, birimMaliyet: 1.8, birimSure: 0.005, gunlukUretim: 15000 },
];

export const MOCK_SIPARISLER = [
  { no: 'SIP-2025-001', musteriId: 'M1', urunId: 'UR-001', miktar: 120, birimFiyat: 85, tarih: '25.02.2025', durum: 'Onaylandı' },
  { no: 'SIP-2025-002', musteriId: 'M2', urunId: 'UR-002', miktar: 500, birimFiyat: 12, tarih: '24.02.2025', durum: 'Üretimde' },
  { no: 'SIP-2025-003', musteriId: 'M3', urunId: 'UR-003', miktar: 80, birimFiyat: 45, tarih: '23.02.2025', durum: 'Sevk Edildi' },
  { no: 'SIP-2025-004', musteriId: 'M1', urunId: 'UR-004', miktar: 2000, birimFiyat: 8, tarih: '26.02.2025', durum: 'Beklemede' },
  { no: 'SIP-2025-005', musteriId: 'M4', urunId: 'UR-005', miktar: 5000, birimFiyat: 2.5, tarih: '26.02.2025', durum: 'Onaylandı' },
];
