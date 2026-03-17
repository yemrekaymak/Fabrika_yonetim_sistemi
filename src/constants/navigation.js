/**
 * Uygulama navigasyon sabitleri
 * Sidebar menü öğeleri ve sayfa tanımları
 */

import { Home, Activity, Box, Zap, Users, ShoppingCart, Calculator, UserCircle } from 'lucide-react';

export const MENU_ITEMS = [
  { id: 'dashboard', label: 'Ana Panel', icon: Home },
  { id: 'uretim', label: 'Üretim Hattı', icon: Activity },
  { id: 'siparis-olustur', label: 'Sipariş Oluşturma', icon: ShoppingCart },
  { id: 'musteriler', label: 'Müşteriler', icon: UserCircle },
  { id: 'muhasebe', label: 'Muhasebe', icon: Calculator },
  { id: 'ai', label: 'AI Tahminleri', icon: Zap },
  { id: 'stok', label: 'Stok Depo', icon: Box },
  { id: 'personel', label: 'Personel', icon: Users },
];

export const PAGE_TITLES = {
  dashboard: 'Genel Bakış',
  uretim: 'Üretim Takibi',
  muhasebe: 'Muhasebe',
  'muhasebe-duzenle': 'Gider Düzenle',
  'makine-ekle': 'Makine Düzenle',
  'makine-bilgi': 'Makine Bilgisi',
  'siparis-olustur': 'Sipariş Oluşturma',
  musteriler: 'Müşteriler',
  'musteriler-ekle': 'Müşteri Ekle',
  'musteri-bilgi': 'Müşteri Bilgisi',
  stok: 'Depo Yönetimi',
  'urun-ekle': 'Ürün Ekle',
  ai: 'Yapay Zeka',
  personel: 'Personel',
  'personel-bilgi': 'Personel Bilgisi',
  'personel-ekle': 'Personel Düzenle',
  'personel-duzenle': 'Bilgi Düzenle',
};

/** Breadcrumb: her tab için yol (üst sayfaya tek tıkla dönüş) */
export const BREADCRUMB_PATH = {
  dashboard: [{ tabId: 'dashboard', label: 'Genel Bakış' }],
  uretim: [{ tabId: 'uretim', label: 'Üretim Takibi' }],
  'makine-ekle': [{ tabId: 'uretim', label: 'Üretim Takibi' }, { tabId: 'makine-ekle', label: 'Makine Düzenle' }],
  'makine-bilgi': [{ tabId: 'uretim', label: 'Üretim Takibi' }, { tabId: 'makine-bilgi', label: 'Makine Bilgisi' }],
  'siparis-olustur': [{ tabId: 'siparis-olustur', label: 'Sipariş Oluşturma' }],
  musteriler: [{ tabId: 'musteriler', label: 'Müşteriler' }],
  'musteriler-ekle': [{ tabId: 'musteriler', label: 'Müşteriler' }, { tabId: 'musteriler-ekle', label: 'Müşteri Düzenle' }],
  'musteri-bilgi': [{ tabId: 'musteriler', label: 'Müşteriler' }, { tabId: 'musteri-bilgi', label: 'Müşteri Bilgisi' }],
  muhasebe: [{ tabId: 'muhasebe', label: 'Muhasebe' }],
  'muhasebe-duzenle': [{ tabId: 'muhasebe', label: 'Muhasebe' }, { tabId: 'muhasebe-duzenle', label: 'Gider Düzenle' }],
  ai: [{ tabId: 'ai', label: 'Yapay Zeka' }],
  stok: [{ tabId: 'stok', label: 'Depo Yönetimi' }],
  'urun-ekle': [{ tabId: 'stok', label: 'Depo Yönetimi' }, { tabId: 'urun-ekle', label: 'Ürün Ekle' }],
  personel: [{ tabId: 'personel', label: 'Personel' }],
  'personel-bilgi': [{ tabId: 'personel', label: 'Personel' }, { tabId: 'personel-bilgi', label: 'Personel Bilgisi' }],
  'personel-ekle': [{ tabId: 'personel', label: 'Personel' }, { tabId: 'personel-ekle', label: 'Personel Düzenle' }],
  'personel-duzenle': [{ tabId: 'personel', label: 'Personel' }, { tabId: 'personel-bilgi', label: 'Personel Bilgisi' }, { tabId: 'personel-duzenle', label: 'Bilgi Düzenle' }],
};
