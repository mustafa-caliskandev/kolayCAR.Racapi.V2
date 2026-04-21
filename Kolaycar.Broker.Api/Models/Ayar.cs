namespace KolayCAR.Broker.API.Models
{
    public partial class Ayar
    {
        public int Id { get; set; }
        public string Smtpsunucu { get; set; }
        public string Smtpmail { get; set; }
        public string Smtppwd { get; set; }
        public string Smtpport { get; set; }
        public string Smtpmailalici { get; set; }
        public bool? Smtpssl { get; set; }
        public bool? Sslaktif { get; set; }
        public string Googledogrulama { get; set; }
        public string Googleanalytics { get; set; }
        public string Googleremarketing { get; set; }
        public string Googleconversion { get; set; }
        public string Googlediger { get; set; }
        public bool Autorateactive { get; set; }
    }
}
