# JwtMusic — JWT Authenticated Music Streaming & Smart Recommendation Platform

JwtMusic, modern yazılım mimarisi kurallarına uygun olarak geliştirilmiş, JWT tabanlı rol ve paket hiyerarşisi barındıran, kullanıcıların dinleme alışkanlıklarına göre akıllı öneriler sunan tam donanımlı bir müzik akış (streaming) backend API ve istemci (UI) platformudur.

Proje, veritabanı performans optimizasyonlarından kullanıcı deneyimine (UX), dinamik veri yönetim panelinden (Admin Dashboard) kapsamlı hata sayfaları yönetimine kadar uçtan uca bir üretim (production) senaryosu modelidir.

---

## 🛠️ Teknolojiler ve Mimari Yapı

Proje, sorumlulukların ayrılması ilkesine (Separation of Concerns) sadık kalınarak **2 Temel Katman (UI & API)** halinde kurgulanmıştır:

*   **Backend (Web API):** .NET 8.0, ASP.NET Core Web API
*   **Database & ORM:** SQL Server & Entity Framework Core (EF Core)
*   **Güvenlik & Kimlik Doğrulama:** JWT (JSON Web Token) & ASP.NET Core Identity
*   **Veri Transferi & Doğrulama:** DTO (Data Transfer Object) kullanımı, AutoMapper / Mapping süreçleri ve FluentValidation
*   **Frontend (UI):** Modern arayüz bileşenleri, JWT entegrasyonu ve dinamik sayfalamalı mimari

---

## 🔥 Öne Çıkan Gelişmiş Özellikler & İş Mantığı

### 🧠 Akıllı Şarkı Öneri Algoritması
Sistem, statik veya rastgele şarkı listelemek yerine kullanıcıların dinleme geçmişini analiz eder. Bir kullanıcının en çok dinlediği şarkılar ile diğer kullanıcıların ortak dinleme alışkanlıkları (Collaborative Filtering mantığıyla) veritabanı seviyesinde taranarak, her kullanıcıya özel ve dinamik bir **"Önerilen Şarkılar"** listesi üretilir.

### 📊 Veritabanı Performans Optimizasyonu (Indexing)
Sürekli büyüyen Dinleme Geçmişi tablosunun öneri algoritması tarafından taranırken performans kaybı yaşatmaması için SQL Server tarafında **UserId** ve **SongId** alanları üzerinde **Index** tanımlamaları yapılmıştır. Bu sayede yüz binlerce satır veri arasında bile sorgular milisaniyeler içinde sonuçlanır.

### 🔐 JWT ve Rol Tabanlı Paket Hiyerarşisi
Kullanıcılar sistemde `BASIC`, `GOLD` ve  `PREMIUM`  olmak üzere hiyerarşik rollere sahiptir. 
*   Her şarkının erişim sağlayabileceği bir minimum paket seviyesi (`RequiredRole`) bulunur.
*   Geliştirilen iş mantığı sayesinde daha yüksek pakete sahip olan bir kullanıcı (Örn: `Gold`), hiyerarşik olarak altındaki tüm içeriklere de kesintisiz erişim sağlayabilir. Paket yetersizliği durumunda API `403 Forbidden` yanıtı fırlatır.

### 👥 Sosyal Etkileşim & Dinamik Geçmiş
*   **Şarkı Beğenme (Like System):** Kullanıcılar şarkıları beğenebilir (Many-to-Many ilişki).
*   **Sanatçı Takip Etme:** Kullanıcılar favori sanatçılarını takibe alabilir.
*   **Özel Çalma Listeleri (Playlist):** Kullanıcılar kendi çalma listelerini oluşturup içine dinamik olarak şarkı ekleyebilirler.
*   **Dinleme Geçmişi:** Kullanıcının dinlediği tüm şarkılar anlık olarak tarihsel veriyle veritabanına kaydedilir ve kullanıcı profilinde listelenir.

---

## 🖥️ Kullanıcı ve Yönetim (Admin) Arayüzü

### 👤 Kullanıcı Arayüzü Özellikleri
*   **Ayarlar Sayfası:** Kullanıcıların kendi profil bilgilerini, e-posta adreslerini, kullanıcı adlarını ve JWT token'dan çözümlenen anlık paket/rol durumlarını görüntüleyebildiği alan.
*   **Müzik Keşif Alanı:** Sanatçılar, şarkılar, dinleme geçmişi ve kişiselleştirilmiş öneri listeleri.

### 👑 Dinamik Veri Yönetim Paneli (Admin Dashboard)
Sistemin genel sağlık ve istatistik durumunu anlık olarak özetleyen dinamik yönetim merkezi:
*   **Metrik Özetleri:** Toplam sanatçı sayısı, toplam şarkı sayısı, toplam rol sayısı ve toplam kullanıcı sayısı.
*   **Analitik Tablo:** Sistemdeki kullanıcıların en çok hangi pakete/role sahip olduğunu gösteren dağılım tablosu.
*   **Son Hareketler (Top 5):** Veritabanına son eklenen 5 sanatçı, son eklenen 5 şarkı ve sisteme son kayıt olan 5 kullanıcı.
*   **Sanatçı Analizi:** En çok şarkıya sahip olan sanatçıların listesi.
*   **Hızlı Erişim Barı:** Doğrudan panel üzerinden yeni sanatçı, yeni şarkı, yeni rol ve yeni kullanıcı ekleme ekranlarına geçiş köprüleri.

---

## 📑 Veri Doğrulama ve Sayfalama (Validation & Pagination)

*   **Gelişmiş Validasyon Kuralları:** Yeni sanatçı ekleme ve yeni şarkı ekleme işlemlerinde, verinin doğruluğunu korumak adına FluentValidation kuralları aktiftir (Boş geçilemez alanlar, karakter sınırları, format kontrolleri vb.). Geçersiz istekler doğrudan API katmanında yakalanarak UI tarafına bilgilendirme mesajı olarak dönülür.
*   **Performans Odaklı Sayfalama (Pagination):** Sanatçılar ve Şarkılar listelerinde veritabanından tüm kayıtların tek seferde çekilip arayüzü şişirmemesi adına hem API hem UI tarafında sayfalama mimarisi uygulanmıştır.
*   **Tam Kapsamlı CRUD:** Sanatçılar, Şarkılar, Roller (Paketler) ve Kullanıcılar sayfalarının tamamı tam yetkili Ekle-Sil-Güncelle-Listele (CRUD) operasyonlarına sahiptir.

---

## 🛡️ Hata ve Durum Yönetimi (Error Pages)

Uygulama, hatalı veya yetkisiz istek senaryolarında kullanıcı deneyimini bozmamak adına hem API durum kodlarını yönetir hem de UI tarafında şu özel tasarım hata sayfalarını sunar:
*   **401 Unauthorized Page:** Kimlik doğrulaması yapılmamış veya JWT token'ı geçersiz/süresi dolmuş kullanıcıları karşılayan ve giriş ekranına yönlendiren güvenli sayfa.
*   **404 Not Found Page:** Sistemde karşılığı olmayan, silinmiş veya yanlış yönlendirilmiş isteklerde devreye giren özel sayfa.
*   **500 Internal Server Error Page:** Backend tarafında oluşabilecek beklenmedik sistem hatalarında, kullanıcıya kod karmaşası yansıtmak yerine kurumsal bir arayüz sunan hata sayfası.
