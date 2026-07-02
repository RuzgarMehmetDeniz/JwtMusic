# JwtMusic — JWT Authenticated Music Streaming & Smart Recommendation Platform

JwtMusic, modern yazılım mimarisi kurallarına uygun olarak geliştirilmiş, JWT tabanlı rol ve paket hiyerarşisi barındıran, kullanıcıların dinleme alışkanlıklarına göre akıllı öneriler sunan tam donanımlı bir müzik akış (streaming) backend API ve istemci (UI) platformudur.

Proje, veritabanı performans optimizasyonlarından kullanıcı deneyimine (UX), dinamik veri yönetim panelinden kapsamlı hata sayfaları yönetimine kadar uçtan uca bir üretim (production) senaryosu modelidir.

---

## 🔐 Kimlik Doğrulama ve Giriş Sistemleri

Platforma erişim, JWT tabanlı güvenli bir kimlik doğrulama altyapısı ile korunmaktadır. Kullanıcılar sisteme kayıt olup güvenli bir şekilde giriş yapabilirler.

**Giriş Ekranı (Login)**
<img width="1915" height="1068" alt="Login" src="https://github.com/user-attachments/assets/905aee48-1021-411c-8401-10eb9ea32cd0" />

**Kayıt Ekranı (Register)**
<img width="1919" height="1068" alt="Register" src="https://github.com/user-attachments/assets/9ea9e8c7-c403-4298-b02a-66e456f2b922" />

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

<img width="1915" height="1064" alt="Recomand" src="https://github.com/user-attachments/assets/7cb306f6-bd18-440c-a557-b398ccb4f1f0" />

### 📊 Veritabanı Performans Optimizasyonu (Indexing)
Sürekli büyüyen Dinleme Geçmişi tablosunun öneri algoritması tarafından taranırken performans kaybı yaşatmaması için SQL Server tarafında **UserId** and **SongId** alanları üzerinde **Index** tanımlamaları yapılmıştır. Bu sayede yüz binlerce satır veri arasında bile sorgular milisaniyeler içinde sonuçlanır.

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

## 🖥️ Kullanıcı Arayüzü Özellikleri

### 👤 Müzik Keşif ve Sosyal Alanlar
Kullanıcıların popüler sanatçıları keşfettiği, tüm müzik kütüphanesine ulaştığı, beğendiği veya takip ettiği içerikleri listelediği modern ve dinamik arayüz sayfaları:

* **Popüler Sanatçılar Listesi**
<img width="1915" height="1068" alt="Artistlist" src="https://github.com/user-attachments/assets/005f2ed3-2490-41df-89cf-bb74ff343f07" />

* **Müzik Kütüphanesi (Şarkı Listesi)**
<img width="1911" height="1065" alt="SongList" src="https://github.com/user-attachments/assets/e1eb657a-ca7a-416e-b48b-cd2dcaa9bc23" />

* **Beğendiğim Şarkılarım**
<img width="1914" height="1064" alt="LikeList" src="https://github.com/user-attachments/assets/14f39401-e7ef-4fc7-949d-84497ab0a2d7" />

* **Takip Ettiğim Sanatçılar**
<img width="1912" height="1066" alt="FollowList" src="https://github.com/user-attachments/assets/11c2bfe4-bc14-4add-942f-63b9c9776d2e" />

* **Özel Çalma Listelerim (Playlists)**
<img width="1916" height="1067" alt="PlayList" src="https://github.com/user-attachments/assets/1cb6ef6c-2c22-4a07-bd97-b8c2dabf054e" />

* **Detaylı Dinleme Geçmişi**
<img width="1917" height="1063" alt="History" src="https://github.com/user-attachments/assets/b62a5925-17ef-42f6-b769-587fdf7b6f9d" />

### ⚙️ Ayarlar Sayfası ve Hesap Yönetimi
Kullanıcıların kendi profil bilgilerini (Ad, Soyad, Kullanıcı Adı, E-posta) and JWT token'dan çözümlenen anlık paket/rol durumlarını (Örn: `Rolünüz: Gold`) görüntüleyebildiği, bildirim tercihlerini yönetebildiği alan.

**Profil & Hesap Bilgileri**
<img width="1912" height="1066" alt="Settings1" src="https://github.com/user-attachments/assets/db9d901e-d166-4d03-9e2f-a366f8797a60" />

**Üyelik & Paket Durumu**
<img width="1911" height="1069" alt="Settings2" src="https://github.com/user-attachments/assets/6c43f97e-77c3-445d-b059-098b1b6cfee0" />

---

## 👑 Dinamik Veri Yönetim Paneli (Canlı Veri Merkezi)

Sistemin genel sağlık, istatistik ve içerik durumunu anlık olarak özetleyen, admin yetkisine sahip kullanıcıların erişebildiği dinamik yönetim paneli:

### 📊 Canlı Veri Merkezi ve İstatistik Paneli
* **Metrik Özetleri:** Toplam Sanatçı, Toplam Şarkı, Toplam Rol ve Toplam Kullanıcı sayıları üst barda kartlar halinde listelenir.
* **Analitik Grafikler (Rol Dağılımı):** Sistemdeki kullanıcıların `Gold`, `Premium`, `Basic` ve `Admin` rolleri arasındaki yüzdesel dağılımını gösteren dinamik çubuk grafikler (Progress Bars).
* **Son Hareketler & İstatistikler:** Son eklenen sanatçılar, son eklenen şarkılar, son kayıt olan kullanıcılar ve en çok şarkıya sahip olan sanatçılar grafiksel analizlerle sunulur.

<img width="1895" height="1076" alt="Dashboard" src="https://github.com/user-attachments/assets/dd58fd58-111b-4a02-802c-1143496533ee" />

### ⚙️ Admin İçerik ve Sistem Yönetimi (CRUD)
Admin paneli üzerinden sistemdeki tüm veriler üzerinde sayfalama altyapısıyla tam yetkili Ekle-Sil-Güncelle-Listele (CRUD) operasyonları gerçekleştirilir.

**Sanatçı Yönetim Ekranı**
<img width="1912" height="1059" alt="AdminArtist" src="https://github.com/user-attachments/assets/0e35b43d-715c-47a0-86a4-49424dacfb3e" />

**Şarkı Yönetim Ekranı**
<img width="1912" height="1065" alt="AdminSong" src="https://github.com/user-attachments/assets/94130e4f-bbef-45ee-b56f-3ead28e2d76c" />

**Rol Yönetim Ekranı**
<img width="1909" height="431" alt="AdminRole" src="https://github.com/user-attachments/assets/9c64d1d8-f57d-45ee-a673-a93c11173a7d" />

**Kullanıcı Yönetim Ekranı**
<img width="1907" height="813" alt="AdminUser" src="https://github.com/user-attachments/assets/16194bf8-f444-4bea-915f-45b0c064f567" />

---

## 📑 Veri Doğrulama ve Sayfalama (Validation & Pagination)

* **Gelişmiş Validasyon Kuralları:** Yeni sanatçı ekleme ve yeni şarkı ekleme işlemlerinde, verinin doğruluğunu korumak adına FluentValidation kuralları aktiftir (Boş geçilemez alanlar, karakter sınırları, format kontrolleri vb.). Geçersiz istekler doğrudan API katmanında yakalanarak UI tarafına bilgilendirme mesajı olarak dönülür.
* **Performans Odaklı Sayfalama (Pagination):** Sanatçılar, Şarkılar ve Kullanıcı yönetimi tablolarında veritabanından tüm kayıtların tek seferde çekilip arayüzü şişirmemesi adına hem API hem UI tarafında dinamik sayfalama mimarisi uygulanmıştır.

---

## 🛡️ Hata ve Durum Yönetimi (Error Pages)

Uygulama, hatalı veya yetkisiz istek senaryolarında kullanıcı deneyimini bozmamak adına hem API durum kodlarını yönetir hem de UI tarafında şu özel tasarım hata sayfalarını sunar:
* **401 Unauthorized Page:** Kimlik doğrulaması yapılmamış veya JWT token'ı geçersiz/süresi dolmuş kullanıcıları karşılayan ve giriş ekranına yönlendiren güvenli sayfa.
* **404 Not Found Page:** Sistemde karşılığı olmayan, URL satırından yanlış yazılmış ya da silinmiş isteklerde devreye giren enstrüman ikonlu özel **"Sayfa Bulunamadı"** sayfası.
* **500 Internal Server Error Page:** Backend tarafında oluşabilecek beklenmedik sistem hatalarında, kullanıcıya kod karmaşası yansıtmak yerine kurumsal bir arayüz sunan hata sayfası.

```
