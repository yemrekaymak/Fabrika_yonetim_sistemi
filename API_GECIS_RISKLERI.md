# API Bağlandığında Sıkıntı Çıkarabilecek Noktalar

Bu belge, backend API’ler bağlandığında **kesin veya olası** sorun çıkarabilecek tüm yerleri listeler.

---

## 1. Kritik: Veri kaynağı ve state başlangıcı

### 1.1 Personel listesi (MainContent.js)

- **Şu an:** `useState(PERSONEL_LISTESI)` — liste sabit mock ile başlıyor.
- **Risk:** API’den ilk yükleme yapılmazsa sayfa her açılışta mock ile açılır; API’den gelen liste state’e hiç yazılmaz.
- **Yapılacak:** Uygulama açılışında (veya Personel sekmesine girildiğinde) `GET /personel` (veya eşdeğeri) çağrılıp `setPersonelList(response)` yapılmalı. `PERSONEL_LISTESI` sadece loading/fallback için kullanılabilir veya kaldırılabilir.

### 1.2 Sipariş Oluşturma (SiparisOlustur.js)

- **Şu an:** Müşteri, ürün ve sipariş listesi tamamen `MOCK_MUSTERILER`, `MOCK_URUNLER`, `MOCK_SIPARISLER` sabitleri.
- **Risk:** API’den müşteri/ürün/sipariş çekilmez; dropdown’lar ve tablo hep mock’ta kalır.
- **Yapılacak:**
  - Müşteri listesi: API’den alınıp state’e yazılmalı (örn. `useEffect` + `GET /musteriler`).
  - Ürün listesi: API’den alınıp state’e yazılmalı (örn. `GET /urunler`).
  - Sipariş listesi: API’den alınıp state’e yazılmalı (örn. `GET /siparisler`).
  - İlk değer: `useState(MOCK_MUSTERILER[0]?.id)` gibi mock’a bağlı ilk seçim kaldırılmalı; liste boşsa veya yükleniyorsa boş string veya null kullanılmalı.

---

## 2. Kritik: ID tipi (sayı vs string)

### 2.1 Personel `id`

- **Şu an:** Mock’ta `id: 1`, `id: 2` (number). `personelList.find((p) => p.id === selectedPersonId)` ve `PersonelEkle` içinde `nextId = Math.max(...personelList.map((p) => p.id)) + 1`.
- **Risk:** Backend çoğu zaman `id`’yi string (UUID veya string ID) döner. O zaman:
  - `selectedPersonId` ile `p.id` karşılaştırması `"123" === 123` gibi false kalabilir.
  - `Math.max(...personelList.map((p) => p.id))` string id’lerde `NaN` verir, yeni eklemede hata veya yanlış id.
- **Yapılacak:** API’den gelen id tipine göre:
  - Tüm id karşılaştırmalarında tutarlı tip kullanın (string ise her yerde string).
  - Yeni personel eklerken id’yi **sunucudan dönülen değer** ile kullanın; client’ta `Math.max(...)+1` ile id üretmeyin.

### 2.2 Sipariş / Müşteri / Ürün id’leri

- **Şu an:** Mock’ta `id: 'M1'`, `id: 'UR-001'` (string).
- **Risk:** Backend sayısal id (örn. `musteriId: 5`) kullanıyorsa, mevcut kod çalışır ama alan adları farklı olabilir (örn. `customerId` vs `musteriId`). API spec’e göre alan eşlemesi yapılmalı.

---

## 3. Kritik: Mutasyonlar (ekleme / silme / güncelleme)

### 3.1 Personel Ekle (PersonelEkle.js)

- **handleAdd:** `setPersonelList([...personelList, yeni])` — sadece local state güncelleniyor.
- **Risk:** API’ye POST atılmıyor; sayfa yenilenince eklenen kayıt kaybolur. Ayrıca `yeni` objesi mock şemaya göre (firstName, lastName, department, …); API farklı alan adı veya zorunlu alan bekliyorsa 400/422 hatası çıkar.
- **Yapılacak:** Form submit’te `POST /personel` (veya eşdeğeri) çağrılmalı; başarılı yanıttan dönen kayıt (id dahil) listeye eklenmeli. Hata durumunda kullanıcıya mesaj gösterilmeli.

### 3.2 Personel Çıkar (PersonelEkle.js)

- **handleRemove:** `setPersonelList(personelList.filter((p) => p.id !== id))` — sadece local state.
- **Risk:** API’de silme (DELETE) yapılmaz; sayfa yenilenince kişi yine görünür.
- **Yapılacak:** Onay sonrası `DELETE /personel/:id` (veya eşdeğeri) çağrılmalı; başarılıysa state’ten çıkarılmalı.

### 3.3 Sipariş Oluştur (SiparisOlustur.js)

- **handleSiparisEkle:** Sadece `alert(...)` — hiç state güncellemesi veya API çağrısı yok.
- **Risk:** Sipariş gerçekten oluşturulmaz; liste ve kapasite hesapları değişmez.
- **Yapılacak:** `POST /siparisler` (veya eşdeğeri) çağrılmalı; başarılı yanıt sonrası sipariş listesi state’i güncellenmeli (veya listeyi API’den yeniden çekmeli). Alert yerine toast/notification tercih edilebilir.

---

## 4. Grafik ve tablo verileri (sadece mock)

### 4.1 Dashboard grafikleri

- **ProductionLineChart:** Veriyi doğrudan `URETIM_GRAFIK_VERISI` sabitinden alıyor; `data` prop’u yok.
- **DepoBarChart:** `data` prop’u varsa onu kullanıyor, yoksa `DEPO_DOLULUK_VERISI` sabiti. Dashboard’dan `data` geçilmiyor, yani şu an hep sabit.
- **Risk:** API’den üretim/depo verisi gelse bile grafikler hâlâ mock’u gösterecek.
- **Yapılacak:** Dashboard’da API’den veri çekilip ProductionLineChart’a `data` prop’u ile verilmeli. DepoBarChart zaten `data` destekliyor; Dashboard’dan API verisi prop olarak geçilmeli.

### 4.2 Stok sayfası (Stok.js)

- **Şu an:** Tablo `STOK_TABLOSU` sabiti ile dolduruluyor.
- **Risk:** API’den stok listesi gelse bile tablo hâlâ sabit veriyi gösterir.
- **Yapılacak:** Stok listesi API’den alınıp state’e konmalı; tablo bu state’ten render edilmeli.

### 4.3 Üretim sayfası (Uretim.js)

- **Şu an:** `MAKINE_LISTESI` sabiti.
- **Risk:** Makine listesi API’den gelmezse ekranda hep mock kalır.
- **Yapılacak:** Makine listesi API’den çekilip state ile kullanılmalı.

### 4.4 Ürün Ekle (UrunEkle.js)

- **handleSubmit:** Sadece `onBack()` — form verisi hiçbir yere gönderilmiyor.
- **Risk:** Ürün eklenmez; API’ye POST yok.
- **Yapılacak:** Form verisi API’ye POST edilmeli (backend’in beklediği alan adlarına uygun şekilde).

---

## 5. Loading ve hata yönetimi

- **Şu an:** Hiçbir sayfada `loading` veya `error` state’i yok; API çağrıları da yok.
- **Risk:** API yavaş veya hata verdiğinde kullanıcı boş ekran veya donma görür; hata mesajı gösterilmez.
- **Yapılacak:** Her API kullanan yerde en azından:
  - Yükleme sırasında spinner/skeleton,
  - Hata durumunda kullanıcıya mesaj ve gerekirse yeniden dene butonu.

---

## 6. window.confirm ve alert

- **PersonelEkle.js:** `window.confirm` ile “çıkarmak istiyor musunuz?” — bu kalabilir, sadece onay sonrası DELETE API çağrılmalı.
- **SiparisOlustur.js:** `alert('Sipariş oluşturuldu (mock)...')` — API bağlandığında kaldırılmalı veya toast/snackbar ile değiştirilmeli.

---

## 7. Özet tablo

| Yer | Mevcut durum | API bağlanınca risk |
|-----|----------------|---------------------|
| MainContent – personelList | useState(PERSONEL_LISTESI) | Liste API’den gelmezse hep mock kalır |
| PersonelEkle – handleAdd | Sadece setPersonelList | POST yok, id client’ta üretiliyor |
| PersonelEkle – handleRemove | Sadece filter | DELETE yok |
| PersonelBilgi – person | personelList.find(p.id === selectedPersonId) | Backend id string ise eşleşme bozulabilir |
| SiparisOlustur – müşteri/ürün/sipariş | Hep mock sabit | API verisi kullanılmaz |
| SiparisOlustur – handleSiparisEkle | Sadece alert | POST yok |
| Dashboard – grafikler | Sabit chartData | API verisi grafiğe bağlanmaz |
| Stok – tablo | STOK_TABLOSU sabit | API stok verisi kullanılmaz |
| Uretim – makine listesi | MAKINE_LISTESI sabit | API makine listesi kullanılmaz |
| UrunEkle – handleSubmit | Sadece onBack() | POST yok |
| Genel | Loading/error yok | Donma veya sessiz hata riski |

---

## 8. Önerilen sıra

1. **API servis katmanı:** `src/api/services/` altında personel, sipariş, stok, üretim için fonksiyonlar (get, create, delete vb.).
2. **ID ve alan uyumu:** Backend spec’e göre id tipi ve alan adları (camelCase/snake_case) netleştirilip tüm kullanımlar buna göre yapılsın.
3. **Personel akışı:** Liste API’den yükleme → Ekle POST → Çıkar DELETE → loading/error.
4. **Sipariş akışı:** Müşteri/ürün/sipariş listeleri API’den → Sipariş oluştur POST → loading/error.
5. **Stok, Üretim, Dashboard:** İlgili veriler API’den çekilip mevcut tablo/grafik bileşenlerine prop olarak verilsin.
6. **Ürün Ekle:** Form POST ile API’ye gönderilsin.

Bu adımlar uygulandığında mevcut kodlar API’ye geçişte sıkıntı çıkaran noktalar giderilmiş olur.
