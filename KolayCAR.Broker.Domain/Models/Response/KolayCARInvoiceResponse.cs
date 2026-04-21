namespace KolayCAR.Broker.Domain.Models.Response
{
    public class KolayCARInvoiceResponse
    {
        public class PostInvoiceResponse
        {
            public string MuhasebeFaturaNo { get; set; }
            public string FaturaTipi { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class PostCancelInvoiceResponse
        {
            public string MuhasebeFaturaNo { get; set; }
            public string FaturaTipi { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}
