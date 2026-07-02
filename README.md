# JwtMusic — JWT Authenticated Music Streaming & Smart Recommendation Platform

JwtMusic, modern yazılım mimarisi kurallarına uygun olarak geliştirilmiş, JWT tabanlı rol ve paket hiyerarşisi barındıran, kullanıcıların dinleme alışkanlıklarına göre akıllı öneriler sunan tam donanımlı bir müzik akış (streaming) backend API ve istemci (UI) platformudur.

Proje, veritabanı performans optimizasyonlarından kullanıcı deneyimine (UX), dinamik veri yönetim panelinden (Admin Dashboard) kapsamlı hata sayfaları yönetimine kadar uçtan uca bir üretim (production) senaryosu modelidir.

---

## 🛠️ Teknolojiler ve Mimari Yapı

Proje, sorumlulukların ayrılması ilkesine (Separation of Concerns) sadık kalınarak **2 Temel Katman (UI & API)** halinde kurgulanmıştır:

* **Backend (Web API):** .NET 8.0, ASP.NET Core Web API
* **Database & ORM:** SQL Server & Entity Framework Core (EF Core)
* **Güvenlik & Kimlik Doğrulama:** JWT (JSON Web Token) & ASP.NET Core Identity
* **Veri Transferi & Doğrulama:** DTO (Data Transfer Object) kullanımı, AutoMapper / Mapping süreçleri ve FluentValidation
* **Frontend (UI):** Modern arayüz bileşenleri, JWT entegrasyonu ve dinamik sayfalamalı mimari

---

## 🔥 Öne Çıkan Gelişmiş Özellikler & İş Mantığı

### 🧠 Akıllı Şarkı Öneri Algoritması
Sistem, statik veya rastgele şarkı listelemek yerine kullanıcıların dinleme geçmişini analiz eder. Bir kullanıcının en çok dinlediği şarkılar ile diğer kullanıcıların ortak dinleme alışkanlıkları (Collaborative Filtering mantığıyla) veritabanı seviyesinde taranarak, her kullanıcıya özel ve dinamik bir **"Bunları da Sevebilirsiniz" (ML.NET Yapay Zeka Analizi)** listesi üretilir.

### 📊 Veritabanı Performans Optimizasyonu (Indexing)
Sürekli büyüyen Dinleme Geçmişi tablosunun öneri algoritması tarafından taranırken performans kaybı yaşatmaması için SQL Server tarafında **UserId** ve **SongId** alanları üzerinde **Index** tanımlamaları yapılmıştır. Bu sayede yüz binlerce satır veri arasında bile sorgular milisaniyeler içinde sonuçlanır.

### 🔐 JWT ve Rol Tabanlı Paket Hiyerarşisi
Kullanıcılar sistemde `BASIC`, `GOLD` ve `PREMIUM` olmak üzere hiyerarşik rollere sahiptir. 
* Her şarkının erişim sağlayabileceği bir minimum paket seviyesi (`RequiredRole`) bulunur. Arayüzde şarkılar üzerinde `BASIC`, `GOLD`, `PREMIUM` etiketleri dinamik olarak basılır.
* Geliştirilen iş mantığı sayesinde daha yüksek pakete sahip olan bir kullanıcı (Örn: `Gold`), hiyerarşik olarak altındaki tüm içeriklere de kesintisiz erişim sağlayabilir. 
* **Gelişmiş UX (İçerik Kilidi):** Paket yetersizliği durumunda API `403 Forbidden` yanıtı fırlatır ve UI tarafında kullanıcıya şık bir *"Bu İçerik Kilitli - Bu şarkıyı dinlemek için daha yüksek bir üyelik paketi gerekiyor"* modalı (popup) gösterilerek "Paketi Yükselt" aksiyonuna yönlendirme yapılır.

### 👥 Sosyal Etkileşim & Dinamik Geçmiş
* **Popüler Sanatçılar & Takip Sistemi:** Ana sayfada popüler sanatçılar listelenir. Kullanıcılar tek tıkla sanatçıları takibe alabilir, "Takip Et" / "Takip Ediliyor" durumları asenkron olarak güncellenir ve "Takip Ettiklerim" sayfasından yönetilebilir.
* **Şarkı Beğenme (Like System):** Kullanıcılar şarkıları beğenebilir (Many-to-Many ilişki) ve beğendikleri şarkılara "Beğendiğim Şarkılar" sekmesinden anında ulaşabilirler.
* **Özel Çalma Listeleri (Playlist):** Kullanıcılar şarkıların yanındaki menü aracılığıyla dinamik olarak tarayıcı üzerinden yeni çalma listesi oluşturabilir (`Örn: Sevdiğim Şarkılar`) ve şarkıları bu listelere ekleyebilirler.
* **Dinleme Geçmişi ve Yönetimi:** Kullanıcının dinlediği tüm şarkılar anlık olarak toplam dinlenme sayısı, dinlenen şarkı sayısı ve toplam sanatçı sayısı gibi metriklerle tarihsel olarak tutulur. Kullanıcı dilerse "Geçmişi Temizle" butonuyla tüm dinleme geçmişini tek seferde silebilir.

---

## 🖥️ Kullanıcı ve Yönetim (Admin) Arayüzü

### 👤 Kullanıcı Arayüzü Özellikleri
* **Ayarlar Sayfası:** Kullanıcıların kendi profil bilgilerini (Ad, Soyad, Kullanıcı Adı, E-posta) ve JWT token'dan çözümlenen anlık paket/rol durumlarını (Örn: `Rolünüz: Gold`) görüntüleyebildiği, güvenli çıkış yapabildiği alan.
* **Müzik Keşif Alanı:** Sanatçılar, şarkılar, entegre müzik çalar (Player) barı, dinleme geçmişi ve kişiselleştirilmiş öneri listeleri.

### 👑 Dinamik Veri Yönetim Paneli (Admin Dashboard)
Sistemin genel sağlık ve istatistik durumunu anlık olarak özetleyen, admin yetkisine sahip kullanıcıların erişebildiği dinamik yönetim merkezi:
* **Metrik Özetleri:** Toplam Sanatçı, Toplam Şarkı, Toplam Rol ve Toplam Kullanıcı sayıları üst barda kartlar halinde listelenir.
* **Analitik Grafikler (Rol Dağılımı):** Sistemdeki kullanıcıların `Gold`, `Premium`, `Basic` ve `Admin` rolleri arasındaki yüzdesel dağılımını gösteren dinamik çubuk grafikler (Progress Bars).
* **Son Hareketler & İstatistikler:** * Son Eklenen 5 Sanatçı
    * Son Eklenen 5 Şarkı
    * Son Kayıt Olan 5 Kullanıcı (Sahip oldukları tüm rollerle birlikte)
    * En Çok Şarkıya Sahip Olan Sanatçılar (Grafiksel veri analiziyle)
* **Hızlı Erişim Barı:** Doğrudan panel üzerinden `Yeni Sanatçı`, `Yeni Şarkı`, `Yeni Rol` ve `Yeni Kullanıcı` ekleme ekranlarına hızlı geçiş köprüleri.

---

## 📑 Veri Doğrulama ve Sayfalama (Validation & Pagination)

* **Gelişmiş Validasyon Kuralları:** Yeni sanatçı ekleme ve yeni şarkı ekleme işlemlerinde, verinin doğruluğunu korumak adına FluentValidation kuralları aktiftir (Boş geçilemez alanlar, karakter sınırları, format kontrolleri vb.). Geçersiz istekler doğrudan API katmanında yakalanarak UI tarafına bilgilendirme mesajı olarak dönülür.
* **Performans Odaklı Sayfalama (Pagination):** Sanatçılar, Şarkılar ve Kullanıcı yönetimi tablolarında veritabanından tüm kayıtların tek seferde çekilip arayüzü şişirmemesi adına hem API hem UI tarafında dinamik sayfalama (alt gezinme barları ile sayfa değiştirme) mimarisi uygulanmıştır.
* **Tam Kapsamlı CRUD:** Sanatçılar Yönetimi, Şarkı Yönetimi, Rol Yönetimi ve Kullanıcı Yönetimi sayfalarının tamamı tam yetkili Ekle-Sil-Güncelle-Listele (CRUD) operasyonlarına sahiptir.

---

## 🛡️ Hata ve Durum Yönetimi (Error Pages)

Uygulama, hatalı veya yetkisiz istek senaryolarında kullanıcı deneyimini bozmamak adına hem API durum kodlarını yönetir hem de UI tarafında şu özel tasarım hata sayfalarını sunar:
* **401 Unauthorized Page:** Kimlik doğrulaması yapılmamış veya JWT token'ı geçersiz/süresi dolmuş kullanıcıları karşılayan ve giriş ekranına yönlendiren güvenli sayfa.
* **404 Not Found Page:** Sistemde karşılığı olmayan, URL satırından yanlış yazılmış (`Örn: /Login/SingInasdasd`) ya da silinmiş isteklerde devreye giren enstrüman ikonlu özel **"Sayfa Bulunamadı"** sayfası.
* **500 Internal Server Error Page:** Backend tarafında oluşabilecek beklenmedik sistem hatalarında, kullanıcıya kod karmaşası yansıtmak yerine kurumsal bir arayüz sunan hata sayfası.
