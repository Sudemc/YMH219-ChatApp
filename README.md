# YMH219 - İstemci-Sunucu (TCP/IP) Chat Uygulaması

Bu proje, **Nesne Tabanlı Programlama (YMH 219)** dersi kapsamında TCP/IP protokolü üzerinden asenkron mesajlaşma yapabilen bir Masaüstü Sohbet Uygulamasıdır.

## Öğrenci Bilgileri
* **Adı Soyadı:** Sudem CÜCEMEN
* **Öğrenci Numarası:** 235541021
* **Öğretim / Şube:** İkinci Öğretim (İ.Ö.) - A Şubesi (Alttan Alan Öğrenci)

---

## Proje Açıklaması
Uygulama, hem **Sunucu (Server)** hem de **İstemci (Client)** modlarını tek bir arayüz üzerinde sekme yapısıyla barındırır. `TcpListener` ve `TcpClient` sınıfları kullanılarak yerel ağda veya aynı makinede (`127.0.0.1`) çoklu istemcilerin birbirleriyle gerçek zamanlı mesajlaşmasını sağlar.

---

## Kullanılan Teknolojiler
* **Programlama Dili:** C# (.NET 9 / Windows Forms)
* **Ağ Protokolü:** TCP/IP (`System.Net.Sockets.TcpListener`, `TcpClient`, `NetworkStream`)
* **Asenkron Programlama:** `async` / `await` ve `Task` mimarisi
* **Metin Akışı:** `StreamReader` ve `StreamWriter` (UTF-8)

---

## Uygulamanın Temel Özellikleri
1. **İki Mod Bir Arada:** Tek bir çalıştırılabilir dosya (.exe) üzerinden hem sunucu açılabilir hem de ikinci bir kopya açılarak istemci olunabilir.
2. **Kullanıcı Adı Desteği:** İstemciler kendi belirledikleri takma ad ile sohbete katılır.
3. **Zaman Damgalı Mesajlar:** Gönderilen her mesaj `[HH:mm:ss] Kullanıcı: Mesaj` formatında görüntülenir.
4. **Bağlantı ve Hata Yönetimi:** Sunucu kapalıyken veya bağlantı koptuğunda program çökmez; kullanıcıya açıklayıcı hata mesajı gösterilir.
5. **Sunucu Bilgilendirmesi:** Sunucu sekmesinde o an bağlı olan istemcilerin IP ve port bilgileri ile tüm mesaj trafiği anlık loglanır.

---

## Kurulum ve Çalıştırma Adımları
1. Projeyi klonlayın:
   ```bash
   git clone https://github.com/kullaniciadi/YMH219-ChatApp.git
   ```
2. `YMH219-ChatApp.csproj` dosyasını Visual Studio ile açın ve derleyin.
3. Uygulamadan iki adet pencere başlatın:
   - **1. Pencere:** "Sunucu (Server)" sekmesine geçip "Sunucuyu Başlat" butonuna basın (Port: 5000).
   - **2. Pencere:** "İstemci (Client)" sekmesinde kullanıcı adınızı yazıp "Bağlan" butonuna basın.
4. Mesajınızı yazıp "Gönder" butonuna veya klavyeden `Enter` tuşuna basarak sohbet edin.

---

## Ekran Görüntüleri
*(Uygulama çalışırken alınan en az 2 ekran görüntüsü)*

### 1. İstemci Sohbet Ekranı
![İstemci Ekranı](screenshots/client_chat.png)

### 2. Sunucu Yönetim ve Log Ekranı
![Sunucu Ekranı](screenshots/server_log.png)

---

## Bilinen Sınırlamalar
* Mesajlar şu an ağdaki tüm bağlı istemcilere genel yayın (Broadcast) olarak iletilmektedir; özel (fısıltı) mesajlaşma bir sonraki aşamada eklenebilir.
