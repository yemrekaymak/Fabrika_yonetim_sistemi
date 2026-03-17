# fabrika_dashboard11.html — Kullanılan Veriler Analizi

Projede hiçbir kod değiştirilmeden, sadece bu HTML dosyasında kullanılan veri yapıları özetlenmiştir.

---

## 1. Sabit veri (dosya içinde tanımlı)

### 1.1 PERSONEL_DATA (satır 657–663)

**Amaç:** Üretim personeli listesi; maaş, yol, yemek ve SGK hesaplamasında kullanılıyor.

| Alan         | Tip   | Açıklama                          |
|-------------|-------|-----------------------------------|
| id          | string| Personel kodu (örn. P004, P005)   |
| pozisyon    | string| ustabasi / pres_operatoru         |
| kidem       | number| Kıdem (yıl), örn. 6.8             |
| brutmaas    | number| Brüt maaş (TL); CSV’de 0, manuel dolduruluyor |
| yol         | number| Yol ücreti (TL/ay)                |
| yemek       | number| Yemek (TL/ay)                     |
| devamsizlik | number| Devamsızlık (gün)                 |
| perf        | number| Performans puanı (0–100)          |

**Kayıt sayısı:** 5 (P004–P008).  
**Kullanıldığı yerler:** Personel tablosu, toplam brüt/SGK/yol+yemek, Sabit Giderler sayfasına otomatik aktarım.

---

### 1.2 URUN_DATA (satır 665–716)

**Amaç:** Ürün master verisi; maliyet ve satış fiyatı hesaplamalarında kullanılıyor.

| Alan     | Tip   | Açıklama                                      |
|----------|-------|-----------------------------------------------|
| kod      | string| Ürün kodu (örn. D3043, D3200)                 |
| pres     | string| Pres kategorisi: agir_1000t, agir_800t, orta_400t, hafif_250t |
| calisan  | number| Çalışan sayısı (bazen 3.5 gibi kesirli)       |
| gunluk   | number| Günlük üretim (adet)                          |
| brut     | number| Brüt ağırlık (kg)                             |
| net      | number| Net ağırlık (kg)                              |
| hurda    | number| Hurda oranı (0–1 arası, örn. 0.1586)         |
| isilislem| number| İsıl işlem var mı (0 veya 1)                  |
| malzeme  | string| cubuk / sac / rulo                             |

**Kayıt sayısı:** 52 ürün.  
**Kullanıldığı yerler:** Ürün seçimi dropdown’u, tek ürün maliyet/fiyat hesabı, toplu ürün tablosu, CSV export, ürün stok hareketi dropdown’u.

---

### 1.3 state (satır 721–730) — Global hesaplama state’i

| Alan           | Varsayılan | Açıklama |
|----------------|------------|----------|
| sabitTamponlu  | 0          | Sabit giderler toplamı × (1 + tampon %) |
| sabitTane      | 0          | Tane başına sabit gider payı (TL/adet)   |
| karOran        | 30         | Kâr marjı (%)                            |
| kalipAmort     | 0.5        | Kalıp amortismanı (TL/adet)              |
| hammaddeKg     | 0          | Hammadde kg fiyatı (TL/kg)               |
| isilIslemMaliyet | 0       | İsıl işlem maliyeti (TL/adet)            |
| istasyon       | 1          | İstasyon (hat) sayısı                    |
| aylikUretim    | 0          | Aylık toplam üretim adedi                 |

Bu değerler formlardan (`updateParams`, `updateSabitToplam`) güncelleniyor; hesaplamalarda kullanılıyor.

---

## 2. Kullanıcı girişi / form verileri

### 2.1 Sabit giderler (sayfa: sabitgider)

- **İşçilik (CSV’den otomatik dolduruluyor):** sg-brutmaaş, sg-sgk, sg-yolyemek  
- **Diğer:** sg-elektrik, sg-bakim, sg-nakliye, sg-kira, sg-diger (TL/ay)  
- **Tampon:** sg-tampon (varsayılan %8)

### 2.2 Üretim parametreleri (sayfa: parametreler)

- p-istasyon (varsayılan 1), p-aylikuretim  
- p-kar (varsayılan 30), p-kalip (varsayılan 0.5)  
- p-hammadde-kg, p-isilislem  

### 2.3 Personel tablosu (manuel giriş)

- Her personel için: brut maaş (pm-{i}), yol (py-{i}), yemek (pye-{i}).  
- SGK satırda brüt × 0.225 ile hesaplanıyor.

---

## 3. localStorage ile saklanan veriler

Tümü `LS.get('key')` / `LS.set('key', value)` ile okunup yazılıyor.

| Key                  | İçerik örneği | Kullanıldığı sayfa / işlem |
|----------------------|---------------|----------------------------|
| hammadde_hareketler  | { id, tarih, malzeme, tip: giriş/çıkış, kg, adet, tedarikci, not } | Hammadde stoku, giriş/çıkış listesi |
| min_levels           | { cubuk, sac, rulo } (kg) | Kritik stok seviyeleri (çubuk, sac, rulo) |
| urun_hareketler      | { id, tarih, kod, tip: uretim/sevkiyat/iade/fire, miktar, aciklama } | Ürün stok hareketi |
| urun_min_levels      | { [urunKodu]: minSeviye } | Ürün bazlı min. stok |
| tedarikciler         | { id, ad, malzeme, kgFiyat, teslimSure, tel, not } | Tedarikçi & Sipariş |
| siparisler           | { id, sipNo, tedarikci, malzeme, miktar, birimFiyat, tutar, siparisT, teslimT, durum: acik/yolda/tamamlandi/iptal, not } | Sipariş listesi |

---

## 4. Hesaplama formülleri (veri kullanımı)

- **SGK:** Brüt maaş × 0.225 (işveren payı).  
- **Sabit toplam:** Brüt + SGK + Yol+Yemek + Elektrik + Bakım + Nakliye + Kira + Diğer; üzerine tampon % uygulanıyor.  
- **Tane başına sabit pay:** (Sabit tamponlu) ÷ İstasyon sayısı ÷ Aylık üretim adedi.  
- **Ürün değişken maliyet:**  
  - Hammadde = net kg × hammadde kg fiyatı  
  - Hurda kaybı = Hammadde × hurda oranı  
  - Kalıp = kalipAmort (sabit)  
  - İsıl işlem = isilislem ? isilIslemMaliyet : 0  
- **Toplam maliyet:** Sabit pay + değişken toplam.  
- **Satış fiyatı:** Toplam maliyet × (1 + karOran/100).

---

## 5. Özet tablo

| Veri kaynağı      | Nerede tanımlı / saklı | Kullanım yeri |
|-------------------|------------------------|----------------|
| Personel listesi  | PERSONEL_DATA (5 kayıt)| Personel tablosu, sabit gider (işçilik) |
| Ürün listesi      | URUN_DATA (52 kayıt)   | Ürün seçimi, maliyet/fiyat, toplu tablo, stok hareketi |
| Hesaplama state   | state objesi           | Tüm maliyet/fiyat ve formül adımları |
| Sabit giderler    | Form input’ları        | Sabit toplam, tampon, tane payı |
| Parametreler      | Form input’ları        | Tane payı, kâr, kalıp, hammadde, isıl işlem |
| Hammadde hareketi | localStorage            | Hammadde stoku sayfası |
| Ürün hareketi     | localStorage            | Ürün stok hareketi sayfası |
| Tedarikçi / sipariş | localStorage          | Tedarikçi & Sipariş sayfası |

Bu analiz sadece **fabrika_dashboard11.html** içindeki veri kullanımını açıklar; React projesinde (fabrika-yonetim) herhangi bir değişiklik yapılmamıştır.
