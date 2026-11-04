using System;

namespace YMH219_ChatApp
{
    // Mesaj verilerini tutan sınıf
    public class Mesaj
    {
        public string Gonderen { get; set; }
        public string Icerik { get; set; }
        public DateTime Zaman { get; set; }

        public Mesaj(string gonderen, string icerik)
        {
            Gonderen = gonderen;
            Icerik = icerik;
            Zaman = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Zaman:HH:mm:ss}] {Gonderen}: {Icerik}";
        }
    }
}
