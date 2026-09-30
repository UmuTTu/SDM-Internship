# SDM-Internship
## Genel Bilgiler:
-Backend ASP.NET Core ve Entity Framework ile yazıldı, veriler SQLite'ta tutuluyor.
Arayüz React ile yazıldı. Talep istatistikleri Python ile hesaplanıyor. Api'lar Swagger ile test edilebilir.
## Gereksinimler: .NET 10 | Node.js | Python 3
## Projeyi Çalıştırma:
-Backend **DemandTrack.Api** yolunda **dotnet run** komutu ile başlatılmalı.   
-Frontend **demandtrack-ui** yolunda **npm run dev** komutu ile başlatılmalı. (İlk defa çalıştırıldığında önce **npm install** ile node modules yüklenmeli)
-Swagger'a **[http://localhost:5046/swagger]** adresinden erişerek test edebilirsiniz.
-Tarayıcıdan **[http://localhost:5173]** adresinden siteye erişebilirsiniz.

## Proje Detayları:
- **"Frontend tarafında"** "Yeni talep oluşturma" sitenin en üstünde, Tarih (2020/1/1 - Günümüz) aralığında olmalı ve format YYYY-DD-MM şeklindedir.
- **"Talebin durumunu değiştirme"** ve "Talebi silme" direkt olarak talebin sağındadır. Sağ üstteki rapor butonu ile Talep istatistikleri "Alert" olarak pop up ile ekrana gösterilir.
- **DemandTrack.Api/Python/Report.py** talepleri **demands.csv** dosyasına aktarır. Ardından toplam, onaylanan, reddedilen ve revizyon bekleyen talepleri hesaplar.
- **Hata mesajları:** Arayüz, hatalı girişlerin çoğunu form üzerinde engeller. Olmayan bir talebi silmek ya da geçersiz veri göndermek gibi durumlar Swagger üzerinden test edilebilir.
