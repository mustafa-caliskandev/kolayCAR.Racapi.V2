using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Currentaccounthistory
    {
        public long Recid { get; set; }
        public int? Vendorid { get; set; }
        public int? Agencyid { get; set; }
        public string Documentnumber { get; set; }
        public byte? Type { get; set; }
        public DateTime? Registereddate { get; set; }
        public DateTime? Transactiondate { get; set; }
        public string Note { get; set; }
        public decimal? Amount { get; set; }
        public int? Currencyid { get; set; }
        public DateTime? Respickupdate { get; set; }
        public DateTime? Resreturndate { get; set; }
        public byte? Transactiontype { get; set; }
        public long? Resid { get; set; }
    }
}
