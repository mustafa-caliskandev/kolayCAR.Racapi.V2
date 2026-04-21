using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Additionalproductvendor
    {
        public int Id { get; set; }
        public int Vendorid { get; set; }
        public int Productid { get; set; }
        public int? Apiproductid { get; set; }
        public string Apiproductcode { get; set; }
        public string Apiproductname { get; set; }
        public decimal? Apiproductprice { get; set; }
        public int? Apivendorid { get; set; }
        public int? Rentaltype { get; set; }
        public bool? Quantityincreasable { get; set; }
        public int? Currencyid { get; set; }
        public DateTime? Recorddate { get; set; }
        public bool? Active { get; set; }
        public decimal? Defaultprice { get; set; }
    }
}
