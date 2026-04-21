using System;

namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class GetVehicleClassesResponse
    {
        public int Vehicleclassid { get; set; }
        public bool? Active { get; set; }
        public int Categoryid { get; set; }
        public int Typeid { get; set; }
        public int Personid { get; set; }
        public int Baggageid { get; set; }
        public int Transmissionid { get; set; }
        public int Fuelid { get; set; }
        public string ModelName { get; set; }
        public string BrandName { get; set; }

    }
}
