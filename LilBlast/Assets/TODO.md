# Urunlesme Oncesi To-Do Listesi

## [P0] Platform ve SDK Entegrasyonlari
- [ ] Oyun icindeyken baglanti koparsa session'i guvenli sekilde offline moda al, servis cagri hatalarini yakala ve runtime crash'lerini engelle.
- [ ] Offline moddayken periyodik internet kontrolleri yapip baglanti geri geldiginde oturumu login moduna gecirecek akisi kur.
- [ ] LoginManager icinde OAuth akislariyla "devam et" butonlarini etkinlestir ve test et.
- [ ] Facebook ve Google resmi SDK'larini projeye dahil et.
- [ ] Sign in with Apple, Game Center / Google Play Games gibi zorunlu platform ozelliklerini ekle.

## [P0] Gameplay ve Level Sistemleri
- [ ] Level bitisinde tahtada kesisen/ust uste gelen ozel bloklarin uygulamayi cokkertmesini engelleyen collision ve lifecycle fix'lerini uygula.
- [ ] Level bitisindeki patlama sirasi icin: ozel bloklar patladiktan sonra yeni bloklar spawn olsun, kalan bloklar sonradan temizlensin ve bu islemler arasina ~0.5 sn'lik bekleme ekle.
- [ ] Level sonu animasyonunda sahnede kalan her bloktan 3 coin kazandiran akisi kur ve coinlerin oyuncuya yazildigini dogrula.
- [ ] Tek sahne uzerinden level yukleme altyapisini tasarla; kullanicinin mevcut seviyesine gore board parametrelerini olustur ve her level icin uygun arkaplan/tema sec.
- [ ] Tamamlanan seviyelerin zorluk konfiglerini kaydet ve ayni seviye yeniden acildiginda ayni DifficultyConfig ile yuklenmesini, ayrica yalnizca tamamlanan seviyelerin LevelButton uzerinden yeniden oynanabilmesini sagla.
- [x] Level panelindeki butonlari yoneten bir `LevelButtonController` yaz; `LevelStatus`, `isCompleted` ve `isLastLevelReached` durumlariyla ikon/lock/star gorsellerini yonet.
- [ ] Modify power-up'in hedef bloklari secme/degistirme fonksiyonunu yeniden kurgula; UI, cooldown ve inventory entegrasyonuyla beraber test et.
- [ ] Lil karakterinin manipule edilebilir davranislarini (hareket, emote, yardim ipuclari vb.) tasarla ve gameplay eventlerine bagla.

## [P1] Ekonomi ve Backend
- [ ] Power-up consume flow su anda her kullanimda `/inventory/consume` endpoint'ine vurus yapiyor; client-side kuyruk ve flush stratejisi tasarla.
- [ ] Kuyruk icin yeniden deneme, zaman asimi ve oturum sonu flush kurallarini implement et.
- [ ] Backend tarafinda toplu consume payload'larini kabul edecek dogrulama ve monitoringleri ekle.

## [P1] Live Ops, Analitik ve Ekonomi Kontrolu
- [ ] Remote config / feature flag katmanini kurup seviye zorluk ve etkinlikleri uzaktan yonetilebilir hale getir.
- [ ] Ekonomi olaylari ve oyuncu ilerlemesi icin event taksonomisi hazirla ve analitik aracina (Firebase, DataDog vb.) gonder.
- [ ] Crash, performance ve anomali takibi icin uyarilarla birlikte raporlama entegrasyonu yap.

## [P1] Kalite, Build ve Yayinlama
- [ ] Otomatiklestirilmis oyun ici test senaryolari ve smoke testleri icin bir suite olustur.
- [ ] CI/CD pipeline'inda iOS ve Android nightly build'leri, imzalama ve Store teslimlerini otomatiklestir.
- [ ] TestFlight / Closed Testing dagitimlariyla cihaz matrisi uzerinde performans ve uyumluluk dogrulamasi yap.

## [P2] Kullanici Deneyimi ve Icerik Hazirligi
- [ ] FTUE/tutorial akisini son haline getirip oyuncu davranisi telemetrisiyle dogrula.
- [ ] UI mikro-animasyonlari, buton geri bildirimi ve kisayollar icin polish pass uygulayarak oturumu saglamlastir.
- [ ] Store asset'leri (ikon, screenshot, video) ve lokalizasyon paketlerini teslim oncesi finalize et.

## [P2] Sosyal ve Topluluk Ozellikleri
- [ ] Kullanici istatistik payload'larini backendden donup donmedigini logla, alinan verilerin UI'da gosterilip gosterilmedigini denetle ve eksikse bagla.
- [ ] Backend arkadas listesi sorgusunu tamamla, donen her arkadas icin FriendPrefab instantiate edip profil verilerini UI'da sergile.
- [ ] Arkadas ekleme/istek akislari icin backend entegrasyonunu sagla ve arkadas profillerinin temel bilgilerini goruntule.
- [ ] Liderlik tablosu icin backend sorgularini bagla, leaderboard verisini cache'leyip UI'da sirali sekilde goster.

## [P3] Market ve Monetizasyon
- [ ] Ilk surum icin hafif bir market tasarla: referans uygulamalarini analiz et, temel urun seti/currency akisini belirle ve calisan bir prototype sun.
