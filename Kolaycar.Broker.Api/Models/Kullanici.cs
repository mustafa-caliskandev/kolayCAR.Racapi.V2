namespace KolayCAR.Broker.API.Models
{
    public partial class Kullanici
    {
        public int Kullaniciid { get; set; }
        public string Eposta { get; set; }
        public string Pwd { get; set; }
        public bool? Aktif { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Telefon { get; set; }
        public int Roleid { get; set; }
    }
}
