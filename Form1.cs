using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YMH219_ChatApp
{
    public partial class Form1 : Form
    {
        // İstemci Değişkenleri
        private TcpClient istemci;
        private StreamReader istemciOkuyucu;
        private StreamWriter istemciYazici;
        private bool istemciBagli = false;

        // Sunucu Değişkenleri
        private TcpListener sunucu;
        private bool sunucuCalisiyor = false;
        private List<TcpClient> bagliIstemciler = new List<TcpClient>();
        private CancellationTokenSource sunucuIptal;

        // Kontroller
        private TabControl tabControl;
        
        // İstemci Sekmesi Kontrolleri
        private TextBox txtIstemciIp;
        private TextBox txtIstemciPort;
        private TextBox txtKullaniciAdi;
        private Button btnIstemciBaglan;
        private Button btnIstemciKes;
        private Label lblIstemciDurum;
        private ListBox lstIstemciMesajlar;
        private TextBox txtIstemciMesaj;
        private Button btnIstemciGonder;

        // Sunucu Sekmesi Kontrolleri
        private TextBox txtSunucuPort;
        private Button btnSunucuBaslat;
        private Button btnSunucuDurdur;
        private Label lblSunucuDurum;
        private ListBox lstSunucuIstemciler;
        private ListBox lstSunucuLog;

        public Form1()
        {
            InitializeComponent();
            ArayuzHazirla();
        }

        private void ArayuzHazirla()
        {
            this.Text = "YMH219 - TCP Chat Uygulaması (Sudem Cücemen - 235541021)";
            this.Size = new Size(680, 580);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl { Dock = DockStyle.Fill };
            TabPage tabIstemci = new TabPage("İstemci (Client)");
            TabPage tabSunucu = new TabPage("Sunucu (Server)");

            // --- İSTEMCİ SEKMESİ ---
            Label lblIp = new Label { Text = "Sunucu IP:", Location = new Point(15, 18), Size = new Size(70, 20) };
            txtIstemciIp = new TextBox { Text = "127.0.0.1", Location = new Point(85, 15), Size = new Size(90, 25) };

            Label lblPort = new Label { Text = "Port:", Location = new Point(185, 18), Size = new Size(35, 20) };
            txtIstemciPort = new TextBox { Text = "5000", Location = new Point(220, 15), Size = new Size(50, 25) };

            Label lblKullanici = new Label { Text = "Kullanıcı Adı:", Location = new Point(280, 18), Size = new Size(80, 20) };
            txtKullaniciAdi = new TextBox { Text = "Sudem", Location = new Point(365, 15), Size = new Size(80, 25) };

            btnIstemciBaglan = new Button { Text = "Bağlan", Location = new Point(455, 13), Size = new Size(80, 28), BackColor = Color.LightGreen };
            btnIstemciBaglan.Click += BtnIstemciBaglan_Click;

            btnIstemciKes = new Button { Text = "Bağlantıyı Kes", Location = new Point(540, 13), Size = new Size(100, 28), Enabled = false, BackColor = Color.LightCoral };
            btnIstemciKes.Click += BtnIstemciKes_Click;

            lblIstemciDurum = new Label { Text = "Durum: Bağlı Değil", Location = new Point(15, 50), Size = new Size(620, 20), ForeColor = Color.DarkRed, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            lstIstemciMesajlar = new ListBox { Location = new Point(15, 75), Size = new Size(625, 380), Font = new Font("Segoe UI", 9) };

            txtIstemciMesaj = new TextBox { Location = new Point(15, 470), Size = new Size(520, 30), Font = new Font("Segoe UI", 10), PlaceholderText = "Mesajınızı buraya yazın..." };
            txtIstemciMesaj.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { BtnIstemciGonder_Click(s, e); e.SuppressKeyPress = true; } };

            btnIstemciGonder = new Button { Text = "Gönder", Location = new Point(545, 468), Size = new Size(95, 30), BackColor = Color.LightSkyBlue };
            btnIstemciGonder.Click += BtnIstemciGonder_Click;

            tabIstemci.Controls.AddRange(new Control[] { lblIp, txtIstemciIp, lblPort, txtIstemciPort, lblKullanici, txtKullaniciAdi, btnIstemciBaglan, btnIstemciKes, lblIstemciDurum, lstIstemciMesajlar, txtIstemciMesaj, btnIstemciGonder });

            // --- SUNUCU SEKMESİ ---
            Label lblSPort = new Label { Text = "Dinlenecek Port:", Location = new Point(15, 18), Size = new Size(100, 20) };
            txtSunucuPort = new TextBox { Text = "5000", Location = new Point(120, 15), Size = new Size(60, 25) };

            btnSunucuBaslat = new Button { Text = "Sunucuyu Başlat", Location = new Point(200, 13), Size = new Size(130, 28), BackColor = Color.LightGreen };
            btnSunucuBaslat.Click += BtnSunucuBaslat_Click;

            btnSunucuDurdur = new Button { Text = "Sunucuyu Durdur", Location = new Point(340, 13), Size = new Size(130, 28), Enabled = false, BackColor = Color.LightCoral };
            btnSunucuDurdur.Click += BtnSunucuDurdur_Click;

            lblSunucuDurum = new Label { Text = "Sunucu Durumu: Kapalı", Location = new Point(15, 50), Size = new Size(620, 20), ForeColor = Color.DarkRed, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            Label lblIstemcilerBaslik = new Label { Text = "Bağlı İstemciler:", Location = new Point(15, 75), Size = new Size(150, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lstSunucuIstemciler = new ListBox { Location = new Point(15, 100), Size = new Size(200, 390) };

            Label lblLogBaslik = new Label { Text = "Sunucu Mesaj ve Olay Geçmişi:", Location = new Point(230, 75), Size = new Size(250, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lstSunucuLog = new ListBox { Location = new Point(230, 100), Size = new Size(410, 390) };

            tabSunucu.Controls.AddRange(new Control[] { lblSPort, txtSunucuPort, btnSunucuBaslat, btnSunucuDurdur, lblSunucuDurum, lblIstemcilerBaslik, lstSunucuIstemciler, lblLogBaslik, lstSunucuLog });

            tabControl.TabPages.Add(tabIstemci);
            tabControl.TabPages.Add(tabSunucu);
            this.Controls.Add(tabControl);
        }

        // ==========================
        // İSTEMCİ İŞLEMLERİ (CLIENT)
        // ==========================
        private async void BtnIstemciBaglan_Click(object sender, EventArgs e)
        {
            string ip = txtIstemciIp.Text.Trim();
            if (!int.TryParse(txtIstemciPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Lütfen geçerli bir port numarası giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string kullanici = txtKullaniciAdi.Text.Trim();
            if (string.IsNullOrWhiteSpace(kullanici))
            {
                MessageBox.Show("Lütfen bir kullanıcı adı belirleyiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                istemci = new TcpClient();
                await istemci.ConnectAsync(ip, port);
                NetworkStream ns = istemci.GetStream();
                istemciOkuyucu = new StreamReader(ns, Encoding.UTF8);
                istemciYazici = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                istemciBagli = true;
                btnIstemciBaglan.Enabled = false;
                btnIstemciKes.Enabled = true;
                lblIstemciDurum.Text = $"Durum: 🟢 {ip}:{port} adresine bağlandı ({kullanici})";
                lblIstemciDurum.ForeColor = Color.DarkGreen;

                // Sunucuya kullanıcı adımızı ilk mesaj olarak gönderelim
                await istemciYazici.WriteLineAsync($"[KATILDI]:{kullanici}");

                // Gelen mesajları dinlemeye başla
                _ = IstemciMesajDinle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sunucuya bağlanırken hata oluştu:\n" + ex.Message, "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                IstemciBaglantisiniSifirla();
            }
        }

        private async Task IstemciMesajDinle()
        {
            try
            {
                while (istemciBagli && istemci != null && istemci.Connected)
                {
                    string gelen = await istemciOkuyucu.ReadLineAsync();
                    if (gelen == null) break;

                    this.Invoke((MethodInvoker)delegate
                    {
                        lstIstemciMesajlar.Items.Add(gelen);
                        lstIstemciMesajlar.TopIndex = lstIstemciMesajlar.Items.Count - 1;
                    });
                }
            }
            catch
            {
                // Bağlantı koptuğunda
            }
            finally
            {
                if (istemciBagli)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show("Sunucu ile bağlantı kesildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        IstemciBaglantisiniSifirla();
                    });
                }
            }
        }

        private async void BtnIstemciGonder_Click(object sender, EventArgs e)
        {
            if (!istemciBagli || istemciYazici == null)
            {
                MessageBox.Show("Lütfen önce bir sunucuya bağlanın.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string metin = txtIstemciMesaj.Text.Trim();
            if (string.IsNullOrEmpty(metin)) return;

            string gonderen = txtKullaniciAdi.Text.Trim();
            Mesaj msg = new Mesaj(gonderen, metin);

            try
            {
                await istemciYazici.WriteLineAsync(msg.ToString());
                txtIstemciMesaj.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mesaj gönderilemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnIstemciKes_Click(object sender, EventArgs e)
        {
            IstemciBaglantisiniSifirla();
        }

        private void IstemciBaglantisiniSifirla()
        {
            istemciBagli = false;
            try { istemciOkuyucu?.Close(); } catch { }
            try { istemciYazici?.Close(); } catch { }
            try { istemci?.Close(); } catch { }

            btnIstemciBaglan.Enabled = true;
            btnIstemciKes.Enabled = false;
            lblIstemciDurum.Text = "Durum: 🔴 Bağlı Değil";
            lblIstemciDurum.ForeColor = Color.DarkRed;
        }

        // ==========================
        // SUNUCU İŞLEMLERİ (SERVER)
        // ==========================
        private void BtnSunucuBaslat_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtSunucuPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Lütfen geçerli bir port numarası giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                sunucu = new TcpListener(IPAddress.Any, port);
                sunucu.Start();
                sunucuCalisiyor = true;
                sunucuIptal = new CancellationTokenSource();

                btnSunucuBaslat.Enabled = false;
                btnSunucuDurdur.Enabled = true;
                lblSunucuDurum.Text = $"Sunucu Durumu: 🟢 Port {port} üzerinde dinleniyor...";
                lblSunucuDurum.ForeColor = Color.DarkGreen;

                SunucuyaLogEkle($"[SİSTEM] Sunucu port {port} üzerinde başlatıldı.");

                _ = SunucuIstemciKabulEt();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sunucu başlatılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SunucuIstemciKabulEt()
        {
            while (sunucuCalisiyor)
            {
                try
                {
                    TcpClient yeniIstemci = await sunucu.AcceptTcpClientAsync();
                    lock (bagliIstemciler)
                    {
                        bagliIstemciler.Add(yeniIstemci);
                    }

                    string ep = yeniIstemci.Client.RemoteEndPoint.ToString();
                    SunucuyaLogEkle($"[BAĞLANTI] Yeni istemci bağlandı: {ep}");
                    lstSunucuIstemciler.Items.Add(ep);

                    _ = SunucuIstemciyiYonet(yeniIstemci, ep);
                }
                catch
                {
                    break;
                }
            }
        }

        private async Task SunucuIstemciyiYonet(TcpClient client, string endpoint)
        {
            NetworkStream ns = client.GetStream();
            StreamReader sr = new StreamReader(ns, Encoding.UTF8);

            try
            {
                while (sunucuCalisiyor && client.Connected)
                {
                    string mesaj = await sr.ReadLineAsync();
                    if (mesaj == null) break;

                    SunucuyaLogEkle(mesaj);

                    // Gelen mesajı tüm bağlı istemcilere yayınla (Broadcast)
                    await SunucuTumuneYayinla(mesaj);
                }
            }
            catch { }
            finally
            {
                lock (bagliIstemciler)
                {
                    bagliIstemciler.Remove(client);
                }
                this.Invoke((MethodInvoker)delegate
                {
                    lstSunucuIstemciler.Items.Remove(endpoint);
                    SunucuyaLogEkle($"[AYRILDI] İstemci ayrıldı: {endpoint}");
                });
                try { client.Close(); } catch { }
            }
        }

        private async Task SunucuTumuneYayinla(string mesaj)
        {
            List<TcpClient> liste;
            lock (bagliIstemciler)
            {
                liste = new List<TcpClient>(bagliIstemciler);
            }

            foreach (var c in liste)
            {
                try
                {
                    if (c.Connected)
                    {
                        StreamWriter sw = new StreamWriter(c.GetStream(), Encoding.UTF8) { AutoFlush = true };
                        await sw.WriteLineAsync(mesaj);
                    }
                }
                catch { }
            }
        }

        private void SunucuyaLogEkle(string log)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lstSunucuLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {log}");
                lstSunucuLog.TopIndex = lstSunucuLog.Items.Count - 1;
            });
        }

        private void BtnSunucuDurdur_Click(object sender, EventArgs e)
        {
            sunucuCalisiyor = false;
            sunucuIptal?.Cancel();
            try { sunucu?.Stop(); } catch { }

            lock (bagliIstemciler)
            {
                foreach (var c in bagliIstemciler)
                {
                    try { c.Close(); } catch { }
                }
                bagliIstemciler.Clear();
            }

            lstSunucuIstemciler.Items.Clear();
            btnSunucuBaslat.Enabled = true;
            btnSunucuDurdur.Enabled = false;
            lblSunucuDurum.Text = "Sunucu Durumu: 🔴 Kapalı";
            lblSunucuDurum.ForeColor = Color.DarkRed;
            SunucuyaLogEkle("[SİSTEM] Sunucu durduruldu.");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            IstemciBaglantisiniSifirla();
            if (sunucuCalisiyor)
            {
                BtnSunucuDurdur_Click(this, EventArgs.Empty);
            }
            base.OnFormClosing(e);
        }
    }
}
