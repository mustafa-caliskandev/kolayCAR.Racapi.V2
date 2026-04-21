using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Models
{
    public class FormattedExtra
    {
        public int Id { get; set; }                  // 2
        public int Piece { get; set; }           // 1
        public float Price { get; set; }           // 2547.82
        public string Name { get; set; }             // Babysitz
        public string ApiCode { get; set; }            // 3
        public ExtraRentalTypes RentalType { get; set; }         // 1
        public float ApiPrice { get; set; }         // 55
        public string Description { get; set; }      // Es bietet eine sichere Reise...
    }
}
