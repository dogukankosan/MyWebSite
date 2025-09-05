# 🌐 MyWebSite

![License](https://img.shields.io/github/license/dogukankosan/MyWebSite)
![Stars](https://img.shields.io/github/stars/dogukankosan/MyWebSite)
![Last Commit](https://img.shields.io/github/last-commit/dogukankosan/MyWebSite)

> **MyWebSite**: Kişisel veya kurumsal portföy, blog ve içerik yönetimi için geliştirilmiş, mobil uyumlu ve modern bir web sitesi projesi.

---

## 🚀 Özellikler

- ⚡ Hızlı ve responsive (mobil uyumlu) tasarım
- 🛠️ Modern web teknolojileri: HTML5, CSS3, JavaScript, Bootstrap
- 👤 Yönetici paneliyle içerik ve proje yönetimi
- 🖼️ Proje ekleme/güncelleme, görsel yükleme ve dinamik portföy listesi
- 💬 Blog, CV, sosyal medya ve iletişim bölümleri
- 📧 SMTP üzerinden mail gönderme ve test maili
- 🔒 Admin paneli için yetkilendirme (role-based)
- 🌍 SEO ve erişilebilirlik desteği
- 🧩 Temiz, modüler ve kolay genişletilebilir kod yapısı

---

## 🏗️ Teknik Altyapı

- **Backend:** ASP.NET Core MVC (C#)
- **Frontend:** Bootstrap tabanlı responsive tasarım, Razor view’lar, modern HTML/CSS
- **Veritabanı:** SQL Server, işlemler için stored procedure kullanımı
- **Mail Servisi:** SMTP ile entegre, yönetici panelinden ayarlanabilir (AdminMailController ve MailSender.cs ile)
- **Güvenlik:** Yetkilendirme ([Authorize]) ile sadece admin erişimli bölümler
- **Kod Standartları:** Temiz ve okunabilir C#, validator’larla model doğrulama (FluentValidation)
- **Statik Dosyalar:** wwwroot altında organize JS, CSS ve görseller

---

## 📸 Ekran Görüntüsü

<img width="1896" height="990" alt="image" src="https://github.com/user-attachments/assets/f48ce9a7-5719-4e10-8f8d-e8fdc02c1298" />

---

## 🛠️ Kurulum

```bash
git clone https://github.com/dogukankosan/MyWebSite.git
cd MyWebSite
# Gerekli NuGet paketlerini yükleyin:
dotnet restore
# Geliştirme ortamında çalıştırmak için:
dotnet run
```
> Veritabanı bağlantı stringi ve mail ayarlarını `appsettings.json` dosyasından yapılandırmayı unutmayın.

---

## ⚡ Kullanım

- Siteyi başlatınca yönetici paneli (/Admin...) ile içerik, proje ve kullanıcı yönetimini kolayca yapabilirsiniz.
- Proje ekleme/güncelleme formlarında görsel yükleyebilir, proje detaylarını ve linklerini girebilirsiniz.
- İletişim ve mail işlemleri için SMTP ayarlarını admin panelinden güncelleyebilirsiniz.

---

## 📁 Proje Yapısı

```
MyWebSite/
├── Controllers/           # MVC controller'lar (Admin paneli, mail, proje vs.)
├── Models/                # Veri modelleri (Projeler, kullanıcılar, mail ayarları)
├── Views/                 # Razor view dosyaları (Kullanıcı ve admin arayüzleri)
├── Classes/               # Yardımcı sınıflar (SQLCrud, MailSender vb.)
├── Business/              # Validator ve iş kuralları
├── wwwroot/               # JS, CSS ve görseller
│   ├── AdminThema/
│   ├── js/
│   └── img/
├── appsettings.json       # Ayar dosyası
└── README.md
```

## 🤝 Katkı

Katkı sağlamak için projeyi forklayabilir ve pull request gönderebilirsiniz.

---

## 📄 Lisans

MIT License

---

## 📬 İletişim

- 👨‍💻 Geliştirici: [@dogukankosan](https://github.com/dogukankosan)  
- 🐞 Suggestions or issues: [Issues sekmesi](https://github.com/dogukankosan/LogoWhatsappEntegrasyon/issues)

---

