namespace KolayCAR.Broker.API.Models
{
    public class BankBinCode
    {
        public int Id { get; set; }
        public long BinStart { get; set; }
        public long BinEnd { get; set; }

        public string Schema { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public string BankCountry { get; set; }
        public string BankName { get; set; }
        public string BrandName { get; set; }
        public bool? InstallmentSupported { get; set; }

        public bool? Prepaid { get; set; }
    }
}
