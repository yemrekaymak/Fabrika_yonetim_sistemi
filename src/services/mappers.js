/**
 * Render API yanıtlarını frontend formatına (ve tersi) çevirir.
 * Backend path'leri: /api/Customer/*, /api/Machine/*, /api/Order/*, /api/Personnel/*, /api/Product/*, /api/Stock/*
 */

/** Backend Customer (id int, isimSoyisim, mail, tel, firmaIsmi) → Frontend (id, idKod, ad, soyad, email, telefon, tip, unvan) */
export function customerFromApi(r) {
  if (!r) return null;
  const parts = (r.isimSoyisim || '').trim().split(/\s+/);
  const ad = parts[0] || '';
  const soyad = parts.slice(1).join(' ') || '';
  const id = r.id != null ? r.id : null;
  return {
    id,
    idKod: id != null ? String(id) : '',
    ad,
    soyad,
    email: r.mail ?? null,
    telefon: r.tel ?? null,
    tip: r.firmaIsmi ? 'kurumsal' : 'bireysel',
    unvan: r.firmaIsmi ?? null,
  };
}

/** Frontend → Backend Customer (POST/PUT) */
export function customerToApi(u) {
  if (!u) return null;
  return {
    id: u.id,
    isimSoyisim: [u.ad, u.soyad].filter(Boolean).join(' ').trim() || ' ',
    mail: u.email || null,
    tel: u.telefon || null,
    firmaIsmi: u.unvan || null,
  };
}

/** Backend Machine → Frontend (id, idKod, ad, detay) */
export function machineFromApi(r) {
  if (!r) return null;
  const id = r.id ?? r.Id;
  return {
    id,
    idKod: id != null ? `MK-${id}` : '',
    ad: r.machine_name ?? '',
    detay: r.details ?? r.Details ?? null,
  };
}

/** Frontend → Backend Machine (machine_name, details) */
export function machineToApi(u) {
  if (!u) return null;
  return {
    Id: u.id ?? undefined,
    ProductId: u.productId ?? 0,
    machine_name: (u.ad || '').trim() || null,
    details: (u.detay || '').trim() || null,
  };
}

/** Backend Personnel (snake_case veya PascalCase) → Frontend (firstName, lastName, tcKimlikNo, ...) */
export function personnelFromApi(r) {
  if (!r) return null;
  const certStr = r.egitim_sertifikalari ?? r.Certifications ?? r.egitimSertifikalari;
  const egitimSertifikalari = typeof certStr === 'string'
    ? certStr.split(/[\n,;]+/).map((s) => s.trim()).filter(Boolean)
    : Array.isArray(certStr) ? certStr : [];
  const iseGiris = r.ise_giris_tarihi ?? r.HireDate ?? r.iseGirisTarihi;
  const iseGirisTarihi = iseGiris
    ? (typeof iseGiris === 'string' && iseGiris.includes('T')
        ? iseGiris.split('T')[0].split('-').reverse().join('.')
        : iseGiris)
    : null;
  const tcNo = r.tcNo ?? r.TcNo ?? '';
  return {
    id: r.id ?? r.Id ?? null,
    firstName: r.ad ?? r.FirstName ?? '',
    lastName: r.soyad ?? r.LastName ?? '',
    tcKimlikNo: tcNo || null,
    telefon: r.telefon ?? r.PhoneNumber ?? null,
    department: r.departman ?? r.Department ?? '',
    pozisyon: r.pozisyon ?? r.Position ?? null,
    maas: r.maas ?? r.Salary ?? null,
    yol: r.yol ?? r.TransportAllowance ?? null,
    yemek: r.yemek ?? r.MealAllowance ?? null,
    iseGirisTarihi,
    yillikIzinHakki: r.yillik_izin_hakki ?? r.TotalAnnualLeave ?? 14,
    kullanilanIzin: r.kullanilan_izin ?? r.UsedLeave ?? 0,
    fazlaMesaiSaat: r.fazla_mesai_saat ?? r.OvertimeHours ?? 0,
    performansPuani: r.performans_puani ?? r.PerformanceScore ?? null,
    ortalamaGunlukUretim: r.ortalama_gunluk_uretim ?? r.AverageDailyProduction ?? null,
    devamsizlikGun: r.devamsizlik_gun ?? r.AbsenteeismDays ?? null,
    acilDurumKisi: r.acil_durum_kisi ?? r.EmergencyContactName ?? null,
    acilDurumTel: r.acil_durum_tel ?? r.EmergencyContactPhone ?? null,
    egitimSertifikalari,
  };
}

function normalizeTcNo(v) {
  const digits = String(v ?? '').replace(/\D/g, '').slice(0, 11);
  return digits.padStart(11, '0');
}
function normalizeTelefon(v) {
  const digits = String(v ?? '').replace(/\D/g, '');
  if (digits.length >= 11) return '0' + digits.slice(-10);
  if (digits.length === 10 && digits.startsWith('5')) return '0' + digits;
  return digits ? '0' + digits.slice(-10).padStart(10, '0') : '05000000000';
}

/** Frontend → Backend Personnel */
export function personnelToApi(u) {
  if (!u) return null;
  const certStr = Array.isArray(u.egitimSertifikalari)
    ? u.egitimSertifikalari.join(', ')
    : '';
  let iseGiris = u.iseGirisTarihi;
  if (iseGiris && typeof iseGiris === 'string') {
    if (iseGiris.includes('.')) {
      const [d, m, y] = iseGiris.split('.');
      iseGiris = `${y}-${(m || '').padStart(2, '0')}-${(d || '').padStart(2, '0')}T00:00:00`;
    } else if (/^\d{4}-\d{2}-\d{2}/.test(iseGiris)) {
      iseGiris = iseGiris.includes('T') ? iseGiris : iseGiris + 'T00:00:00';
    }
  }
  const tel = normalizeTelefon(u.telefon);
  const acilTel = (u.acilDurumTel && String(u.acilDurumTel).replace(/\D/g, '').length >= 10)
    ? normalizeTelefon(u.acilDurumTel)
    : tel;
  const ad = String(u.firstName ?? '').trim().slice(0, 200);
  const soyad = String(u.lastName ?? '').trim().slice(0, 200);
  return {
    ad: ad || '-',
    soyad: soyad || '-',
    tcNo: normalizeTcNo(u.tcKimlikNo),
    telefon: tel,
    departman: u.department || null,
    pozisyon: u.pozisyon || null,
    maas: u.maas ?? null,
    ise_giris_tarihi: iseGiris || null,
    yillik_izin_hakki: u.yillikIzinHakki ?? 14,
    kullanilan_izin: u.kullanilanIzin ?? 0,
    fazla_mesai_saat: u.fazlaMesaiSaat ?? 0,
    performans_puani: u.performansPuani ?? null,
    ortalama_gunluk_uretim: u.ortalamaGunlukUretim ?? null,
    devamsizlik_gun: u.devamsizlikGun ?? null,
    egitim_sertifikalari: certStr || null,
    acil_durum_kisi: (u.acilDurumKisi && String(u.acilDurumKisi).trim()) ? String(u.acilDurumKisi).trim() : null,
    acil_durum_tel: acilTel,
    aktif: true,
  };
}

/** Backend sipariş durumu İngilizce ise Türkçeye çevirir (tek kaynak) */
function normalizeSiparisDurum(status) {
  const s = (status ?? '').toString().trim().toLowerCase();
  if (s === 'pending') return 'Beklemede';
  if (s === 'approved' || s === 'onaylandı') return 'Onaylandı';
  if (s === 'in_progress' || s === 'in progress' || s === 'üretimde') return 'Üretimde';
  if (s === 'shipped' || s === 'sevk edildi') return 'Sevk Edildi';
  if (s === 'cancelled' || s === 'iptal') return 'İptal';
  return status && String(status).trim() ? String(status).trim() : 'Beklemede';
}

/** Backend Order (musteri_adi, urun_adi, quantity, status, created_at; PascalCase olabilir) → Frontend */
export function orderFromApi(r) {
  if (!r) return null;
  const created = (r.created_at ?? r.CreatedAt)
    ? (String(r.created_at ?? r.CreatedAt).split('T')[0] || r.created_at || r.CreatedAt)
    : '';
  const rawStatus = r.status ?? r.Status ?? 'Beklemede';
  return {
    id: r.id ?? r.Id,
    no: (r.id ?? r.Id) != null ? `SIP-${r.id ?? r.Id}` : '',
    musteriId: null,
    musteriAdi: r.customer_name ?? r.CustomerName ?? r.musteri_adi ?? r.MusteriAdi ?? null,
    urunAdi: r.urun_adi ?? r.UrunAdi ?? null, // yeni şemada yok; UI ürün listesinden bulabilir
    urunKodu: r.urun_kodu ?? r.UrunKodu ?? null,
    urunId: r.product_id ?? r.ProductId ?? r.urun_kodu ?? r.UrunKodu ?? null,
    miktar: Number(r.quantity ?? r.Quantity ?? r.miktar ?? 0) || 0,
    birimFiyat: Number(r.sale_price ?? r.SalePrice ?? 0) || 0,
    tarih: created,
    durum: normalizeSiparisDurum(rawStatus),
  };
}

/** Frontend → Backend Order (PUT) */
export function orderToApi(u) {
  if (!u) return null;
  return {
    id: u.id ?? u.Id,
    musteri_adi: u.musteriAdi ?? null,
    urun_adi: u.urunAdi ?? null,
    urun_kodu: u.urunKodu ?? null,
    quantity: Number(u.miktar ?? u.quantity ?? 0) || 0,
    sale_price: Number(u.birimFiyat ?? 0) || 0,
    status: u.durum ?? null,
  };
}

/** Backend Product (urun_kodu key; PascalCase veya snake_case) → Frontend */
export function productFromApi(r) {
  if (!r) return null;
  const urunKodu = r.urun_kodu ?? r.UrunKodu ?? '';
  const urunAdi = r.urun_adi ?? r.UrunAdi ?? '';
  return {
    id: urunKodu,
    productId: r.id ?? r.Id ?? null, // backend Products.Id (int) - orders.product_id ile eşleşir
    ad: urunAdi || urunKodu || (r.ham_madde ?? r.HamMadde ?? `Ürün ${urunKodu}`),
    birim: 'Adet',
    birimFiyat: Number(r.sale_price ?? r.SalePrice ?? r.base_cost ?? r.BaseCost ?? 0) || 0,
    birimMaliyet: Number(r.base_cost ?? r.BaseCost ?? 0) || 0,
    birimSure: Number(r.birim_uretim_suresi_saat ?? r.BirimUretimSuresiSaat ?? 0) || 0,
    gunlukUretim: r.gunluk_uretim ?? 0,
    urun_kodu: urunKodu,
    ham_madde: r.ham_madde ?? r.HamMadde ?? '',
    malzeme_tipi: r.malzeme_tipi ?? r.MalzemeTipi ?? '',
    pres_kategorisi: r.pres_kategorisi ?? r.PresKategorisi ?? '',
    brut_agirlik_kg: r.brut_agirlik_kg ?? r.BrutAgirlikKg ?? 0,
    net_agirlik_kg: r.net_agirlik_kg ?? r.NetAgirlikKg ?? 0,
    hurda_orani: r.hurda_orani ?? r.HurdaOrani ?? 0,
    malzeme_verimi: r.malzeme_verimi ?? 0,
    calisan_sayisi: r.calisan_sayisi ?? 0,
    has_heat_treatment: r.has_heat_treatment ?? false,
    current_stock: r.current_stock ?? r.CurrentStock ?? 0,
    machines: r.machines ?? r.Machines ?? [],
  };
}

/** Frontend → Backend Product (POST/PUT). Zorunlu: malzeme_tipi, pres_kategorisi (boş string), machines (dizi). */
export function productToApi(u) {
  if (!u) return null;
  const malzemeTipi = (u.malzeme_tipi ?? u.malzemeTipi ?? '').toString().trim() || '-';
  const presKategorisi = (u.pres_kategorisi ?? u.presKategorisi ?? '').toString().trim() || '-';
  const machines = Array.isArray(u.machines) ? u.machines : [];
  return {
    urun_kodu: (u.urun_kodu ?? u.ad ?? '').toString().trim() || null,
    urun_adi: (u.urun_adi ?? u.ad ?? '').toString().trim() || null,
    ham_madde: (u.ham_madde ?? u.ad ?? '').toString().trim() || null,
    malzeme_tipi: malzemeTipi,
    pres_kategorisi: presKategorisi,
    brut_agirlik_kg: Number(u.brut_agirlik_kg ?? 0) || 0,
    net_agirlik_kg: Number(u.net_agirlik_kg ?? 0) || 0,
    hurda_orani: Number(u.hurda_orani ?? 0) || 0,
    base_cost: Number(u.base_cost ?? u.birimMaliyet ?? 0) || 0,
    sale_price: u.birimFiyat != null && Number(u.birimFiyat) >= 0 ? Number(u.birimFiyat) : null,
    birim_uretim_suresi_saat: u.birimSure != null && Number(u.birimSure) > 0 ? Number(u.birimSure) : null,
    current_stock: Math.max(0, parseInt(u.current_stock ?? 0, 10)) || 0,
    machines,
  };
}

/** Ürün (productFromApi çıktısı) → Depo tablosu satır formatı (brüt/net ağırlık, hurda oranı, miktar, maliyet vb.) */
export function productToStokRow(p) {
  if (!p) return null;
  const miktarSayi = p.current_stock ?? 0;
  const kritik = 100;
  const durum = miktarSayi < kritik ? 'kritik' : 'yeterli';
  return {
    id: p.id,
    kod: p.urun_kodu ?? p.ad ?? '',
    ad: p.ad ?? p.ham_madde ?? '',
    brutAgirlik: p.brut_agirlik_kg,
    netAgirlik: p.net_agirlik_kg,
    hurdaOrani: p.hurda_orani,
    kapasite: p.gunlukUretim ?? 0,
    kritik,
    miktar: String(miktarSayi),
    miktarSayi,
    birimMaliyet: p.birimMaliyet ?? p.base_cost ?? 0,
    birimFiyat: p.birimFiyat ?? p.base_cost ?? 0,
    durum,
  };
}

/** Backend Stock (kod, ad, miktarSayi, kapasite, ...) → Frontend */
export function stockFromApi(r) {
  if (!r) return null;
  const miktarSayi = r.miktarSayi ?? 0;
  const kod = r.kod ?? r.stokKodu ?? '';
  return {
    id: r.id ?? kod,
    kod,
    ad: r.ad ?? '',
    miktar: `${miktarSayi}`,
    miktarSayi,
    kapasite: r.kapasite ?? 0,
    kritik: r.kritik ?? 0,
    birimMaliyet: Number(r.birimMaliyet) ?? 0,
    birimFiyat: Number(r.birimFiyat) ?? 0,
    durum: r.durum ?? 'yeterli',
  };
}

/** Frontend → Backend Stock (Swagger: kod, ad, miktar, miktarSayi, kapasite, kritik, birimMaliyet, birimFiyat, durum) */
export function stockToApi(u) {
  if (!u) return null;
  const miktarSayi = Number(u.miktarSayi) || 0;
  return {
    kod: (u.kod ?? u.stokKodu ?? '').trim() || null,
    ad: (u.ad ?? '').trim() || null,
    miktar: u.miktar ?? String(miktarSayi),
    miktarSayi,
    kapasite: Number(u.kapasite) || 0,
    kritik: Number(u.kritik) || 0,
    birimMaliyet: Number(u.birimMaliyet) || 0,
    birimFiyat: Number(u.birimFiyat) || 0,
    durum: u.durum ?? null,
  };
}

/** Müşteri → Sipariş dropdown */
export function customerToDropdown(r) {
  if (!r) return null;
  const full = customerFromApi(r);
  return {
    id: full.idKod ?? full.id,
    unvan: full.unvan || `${full.ad} ${full.soyad}`.trim(),
    yetkili: `${full.ad} ${full.soyad}`.trim(),
    tel: full.telefon || '',
  };
}
