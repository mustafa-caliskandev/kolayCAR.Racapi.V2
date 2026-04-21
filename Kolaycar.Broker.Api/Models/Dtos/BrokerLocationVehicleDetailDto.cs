namespace KolayCAR.Broker.API.Models.Dtos
{
    public class BrokerLocationVehicleDetailDto
    {
        public int LanguageId { get; set; }
        public int LocationId { get; set; }
        public int RentalCount { get; set; }
        public int VehicleId { get; set; }
        public string LocationName { get; set; }
        public string VehicleName { get; set; }
        public string VehicleImageUrl { get; set; }
        public string VehicleContentUrl { get; set; }
    }
}
