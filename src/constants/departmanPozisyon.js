/**
 * Departman ve pozisyon seçenekleri.
 * API bağlandığında bu dosyada getDepartmanlar() / getPozisyonlar() ile değiştirilebilir.
 */
export const DEPARTMAN_OPTIONS = [
  { id: 'Üretim', ad: 'Üretim' },
  { id: 'Lojistik', ad: 'Lojistik' },
  { id: 'Bakım', ad: 'Bakım' },
  { id: 'Kalite', ad: 'Kalite' },
  { id: 'İdari', ad: 'İdari' },
  { id: 'Diğer', ad: 'Diğer' },
];

export const DEPARTMAN_FILTRE_OPTIONS = [
  { id: '', ad: 'Tüm departmanlar' },
  ...DEPARTMAN_OPTIONS,
];

/** Personel ekleme/düzenlemede kullanılan pozisyon listesi */
export const POZISYON_OPTIONS = [
  { id: 'Ustabaşı', ad: 'Ustabaşı' },
  { id: 'Pres Operatörü', ad: 'Pres Operatörü' },
  { id: 'Yemekhane Görevlisi', ad: 'Yemekhane Görevlisi' },
  { id: 'Satış Müdürü', ad: 'Satış Müdürü' },
  { id: 'Üretim Müdürü', ad: 'Üretim Müdürü' },
];

export const POZISYON_BY_DEPARTMAN = {
  Üretim: [
    { id: 'Üretim Operatörü', ad: 'Üretim Operatörü' },
    { id: 'Üretim Şefi', ad: 'Üretim Şefi' },
    { id: 'Hat Sorumlusu', ad: 'Hat Sorumlusu' },
    { id: 'Kaynak Operatörü', ad: 'Kaynak Operatörü' },
  ],
  Lojistik: [
    { id: 'Depo Sorumlusu', ad: 'Depo Sorumlusu' },
    { id: 'Forklift Operatörü', ad: 'Forklift Operatörü' },
    { id: 'Sevkiyat Sorumlusu', ad: 'Sevkiyat Sorumlusu' },
    { id: 'Stok Kontrol', ad: 'Stok Kontrol' },
  ],
  Bakım: [
    { id: 'Bakım Teknisyeni', ad: 'Bakım Teknisyeni' },
    { id: 'Bakım Şefi', ad: 'Bakım Şefi' },
    { id: 'Elektrik Teknisyeni', ad: 'Elektrik Teknisyeni' },
    { id: 'Makine Bakım', ad: 'Makine Bakım' },
  ],
  Kalite: [
    { id: 'Kalite Kontrol Uzmanı', ad: 'Kalite Kontrol Uzmanı' },
    { id: 'Kalite Şefi', ad: 'Kalite Şefi' },
    { id: 'Laborant', ad: 'Laborant' },
  ],
  İdari: [
    { id: 'İnsan Kaynakları Uzmanı', ad: 'İnsan Kaynakları Uzmanı' },
    { id: 'Muhasebe', ad: 'Muhasebe' },
    { id: 'İdari İşler Sorumlusu', ad: 'İdari İşler Sorumlusu' },
  ],
  Diğer: [
    { id: 'Genel İşçi', ad: 'Genel İşçi' },
    { id: 'Stajyer', ad: 'Stajyer' },
    { id: 'Diğer', ad: 'Diğer' },
  ],
};
