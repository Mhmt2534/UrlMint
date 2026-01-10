# UrlMint - URL Kısaltma Servisi

UrlMint, ASP.NET Core 9.0 ve PostgreSQL kullanarak geliştirilmiş modern bir URL kısaltma servisidir. Base62 encoding algoritması kullanarak veritabanı ID'lerini kısa ve güvenli URL kodlarına dönüştürür.

## 🚀 Özellikler

- ✅ URL kısaltma ve yönlendirme
- ✅ Base62 encoding ile kısa kod üretimi
- ✅ Tıklama sayısı takibi
- ✅ URL bilgisi sorgulama
- ✅ Tüm URL'leri listeleme
- ✅ PostgreSQL veritabanı desteği
- ✅ OpenAPI/Swagger desteği (Development ortamında)

## 📋 Gereksinimler

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (12 veya üzeri)
- Visual Studio 2022, VS Code veya herhangi bir .NET uyumlu IDE

## 🔧 Kurulum

### 1. Projeyi Klonlayın

```bash
git clone <repository-url>
cd UrlMint
```

### 2. Veritabanı Yapılandırması

PostgreSQL veritabanınızı oluşturun:

```sql
CREATE DATABASE urlmint;
```

### 3. Connection String Yapılandırması

`UrlMint/appsettings.Development.json` dosyasını oluşturun (veya `appsettings.Development.json.example` dosyasını kopyalayıp düzenleyin):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=urlmint;Username=postgres;Password=your_password"
  }
}
```

**Not:** `your_password` kısmını kendi PostgreSQL şifrenizle değiştirin.

### 4. Veritabanı Migration'larını Çalıştırın

Proje dizininde terminal açın ve şu komutu çalıştırın:

```bash
cd UrlMint
dotnet ef database update
```

Eğer `dotnet ef` komutu bulunamazsa, önce EF Core tools'u yükleyin:

```bash
dotnet tool install --global dotnet-ef
```

### 5. Projeyi Çalıştırın

```bash
dotnet run --project UrlMint/UrlMint.csproj
```

Uygulama varsayılan olarak `http://localhost:5064` adresinde çalışacaktır. Port numarası `Properties/launchSettings.json` dosyasından kontrol edilebilir.

## 📡 API Endpoints

### 1. URL Kısaltma

Uzun bir URL'yi kısaltır. Eğer aynı URL daha önce kısaltılmışsa, mevcut kısa URL'i döner.

**Endpoint:** `POST /api/url/shorten`

**Request Body:**
```json
{
  "longUrl": "https://www.example.com/very/long/url/path"
}
```

**Response (201 Created):**
```json
{
  "shortUrl": "http://localhost:5064/abc123",
  "shortCode": "abc123",
  "longUrl": "https://www.example.com/very/long/url/path",
  "createdAt": "2025-01-08T10:30:00Z"
}
```

**Response (200 OK - Mevcut URL):**
```json
{
  "shortUrl": "http://localhost:5064/abc123",
  "shortCode": "abc123",
  "longUrl": "https://www.example.com/very/long/url/path",
  "createdAt": "2025-01-08T10:30:00Z"
}
```

**Hata Durumları:**
- `400 Bad Request`: URL boş veya geçersiz format

---

### 2. Kısa URL'e Yönlendirme

Kısa URL kodunu kullanarak orijinal URL'e yönlendirir ve tıklama sayısını artırır.

**Endpoint:** `GET /api/url/{code}`

**Örnek:** `GET /api/url/abc123`

**Response:**
- `302 Redirect`: Orijinal URL'e yönlendirme yapar

**Hata Durumları:**
- `404 Not Found`: URL bulunamadı
- `400 Bad Request`: Geçersiz kısa URL kodu

---

### 3. URL Bilgisi Sorgulama

Kısa URL koduna ait detaylı bilgileri döner (tıklama sayısı dahil).

**Endpoint:** `GET /api/url/info/{code}`

**Örnek:** `GET /api/url/info/abc123`

**Response (200 OK):**
```json
{
  "shortCode": "abc123",
  "longUrl": "https://www.example.com/very/long/url/path",
  "createdAt": "2025-01-08T10:30:00Z",
  "clickCount": 42
}
```

**Hata Durumları:**
- `404 Not Found`: URL bulunamadı
- `400 Bad Request`: Geçersiz kısa URL kodu

---

### 4. Tüm URL'leri Listeleme

Veritabanındaki tüm kısaltılmış URL'leri listeler.

**Endpoint:** `GET /api/url/all`

**Response (200 OK):**
```json
[
  {
    "shortCode": "abc123",
    "longUrl": "https://www.example.com/very/long/url/path",
    "createdAt": "2025-01-08T10:30:00Z",
    "clickCount": 42
  },
  {
    "shortCode": "xyz789",
    "longUrl": "https://www.another-example.com/page",
    "createdAt": "2025-01-08T09:15:00Z",
    "clickCount": 15
  }
]
```

---

## 🧪 Test Etme

### cURL Örnekleri

**URL Kısaltma:**
```bash
curl -X POST http://localhost:5064/api/url/shorten \
  -H "Content-Type: application/json" \
  -d "{\"longUrl\": \"https://www.example.com\"}"
```

**URL Bilgisi:**
```bash
curl http://localhost:5064/api/url/info/abc123
```

**Tüm URL'leri Listeleme:**
```bash
curl http://localhost:5064/api/url/all
```

### Postman veya HTTP Client

Proje içinde `UrlMint.http` dosyası bulunmaktadır. Bu dosyayı Visual Studio Code'da REST Client extension'ı ile veya JetBrains Rider'da kullanabilirsiniz.

---

## 🏗️ Proje Yapısı

```
UrlMint/
├── Controllers/
│   └── ShortUrlController.cs      # API endpoint'leri
├── Domain/
│   ├── DTO/
│   │   └── ShortenUrlRequest.cs   # Request DTO
│   ├── Entities/
│   │   └── ShortUrl.cs            # Veritabanı entity
│   └── Interfaces/
│       ├── IShortUrlRepository.cs # Repository interface
│       └── IUrlEncoder.cs         # Encoder interface
├── Infrastructure/
│   ├── Encoding/
│   │   └── Base62Encoder.cs       # Base62 encoding implementasyonu
│   ├── Persistence/
│   │   ├── UrlMintDbContext.cs    # EF Core DbContext
│   │   └── Migrations/             # Veritabanı migration'ları
│   └── Repositories/
│       └── ShortUrlRepository.cs  # Repository implementasyonu
└── Program.cs                      # Uygulama giriş noktası
```

---


## 📝 Teknolojiler

- **.NET 9.0** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core 9.0** - ORM
- **PostgreSQL** - Veritabanı
- **Npgsql** - PostgreSQL provider

---

## 🐛 Sorun Giderme

### Migration Hatası

Eğer migration çalıştırırken hata alırsanız:

```bash
# Migration'ları sıfırlayın (DİKKAT: Veriler silinir!)
dotnet ef database drop
dotnet ef database update
```

### Connection String Hatası

- PostgreSQL servisinin çalıştığından emin olun
- Connection string'deki bilgilerin doğru olduğunu kontrol edin
- Veritabanının oluşturulduğunu doğrulayın

### Port Çakışması

`Properties/launchSettings.json` dosyasından port numarasını değiştirebilirsiniz.

---

## 📄 Lisans

Bu proje [LICENSE](LICENSE) dosyasında belirtilen lisans altındadır.
