  # SDM-Internship
## Genel Bilgiler:
- **Backend** ASP.NET Core ve Entity Framework ile yazıldı, veriler SQLite'ta tutuluyor.
Arayüz React ile yazıldı. Talep istatistikleri Python ile hesaplanıyor. Api'lar Swagger ile test edilebilir.
## Gereksinimler: .NET 10 | Node.js | Python 3
## Projeyi Çalıştırma:
- **Backend** **DemandTrack.Api** yolunda **dotnet run** komutu ile başlatılmalı.   
- **Frontend** **demandtrack-ui** yolunda **npm run dev** komutu ile başlatılmalı. (İlk defa çalıştırıldığında önce **npm install** ile node modules yüklenmeli)
- **Swagger"'a** **[http://localhost:5046/swagger]** adresinden erişerek test edebilirsiniz.
- **Tarayıcıdan** **[http://localhost:5173]** adresinden siteye erişebilirsiniz.

## Proje Detayları:
- **Frontend tarafında** "Yeni talep oluşturma" sitenin en üstünde, Tarih (2020/1/1 - Günümüz) aralığında olmalı ve format YYYY-MM-DD şeklindedir.
- **Talebin durumunu değiştirme** ve "Talebi silme" direkt olarak talebin sağındadır. Sağ üstteki rapor butonu ile Talep istatistikleri "Alert" olarak pop up ile ekrana gösterilir.
- **DemandTrack.Api/Python/Report.py** talepleri **demands.csv** dosyasına aktarır. Ardından toplam, onaylanan, reddedilen ve revizyon bekleyen talepleri hesaplar.
- **Hata mesajları:** Arayüz, hatalı girişlerin çoğunu form üzerinde engeller. Olmayan bir talebi silmek ya da geçersiz veri göndermek gibi durumlar Swagger üzerinden test edilebilir.

## Feedback Sonrası:
- **Rapor** oluşturulurken Python yoksa: "Python bulunamadı. Python'un kurulu olduğundan ve 'py' komutunun çalıştığından emin olun." hatası anlaşılır bir biçimde veriliyor.

- "Talep detayını görüntüleme" kısmı eklendi detayları talep değiştirilme detayları olarak yorumladım, Talebin durumu örneğin "Yeni"den, "Reddedildi"ye alınırsa değiştirme tarih ve saati ile birlikte detayları gösteriyor. Görmek istediğiniz talebin yan tarafındaki Mavi Saat butonuna basarak test edebilirsiniz.
- Veri giriş alanı, kolonlardan ince bir çizgi ve Seçilenleri sil logosu ile birbirinden ayrıldı.
- Değiştir ve Sil ikon haline getirildi.
- Sil artık alert olarak onay mesajı soruyor.
- Sütunun solundaki "checkbox"ları seçip, yukarıdaki "Seçilenleri Sil" butonuna basarak toplu silme gerçekleştirebilirsiniz.
- Sayfanın altında (20-50-100) diye seçerek bir sayfada kaç sıra gösterileceğini seçebilir, aynı sırada sağda önceki ve sonraki butonları ile farklı sayfalar arasında hareket edebilirsiniz.
- Talep tarihi ekleme kaldırıldı artık otomatik olarak bugünün tarihine ekliyor.
- TalepNo ve Database ID birbirinden ayrıldı, Talep no TN(AYIN GÜNÜ)(SAAT)(DAKİKA) olarak oluşturuluyor, eğer aynı TalepNo'dan başka var ise +1 ekleniyor, sonu 9 ile bitiyorsa 0'a geri dönüyor. Daha önce oluşturulan Taleplerde bu sistem olmadığı için "Gün" kısmından sonra sonları direkt "0000" ile bitiyor.
- Sütunları Tarih, Başlık, Oluşturan kişi vb. kategorilerine tıklayarak sıralayabilirsiniz, 3. basışta sıralamayı bırakıyor.
