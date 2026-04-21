namespace kolayCAR.Broker.AWS.Models.AwsModels
{
    public class SearchDto
    {
        public int PickupPoint { get; set; }
        public string PickupPointName { get; set; }
        public string PickupDate { get; set; }
        public string PickupTime { get; set; }
        public bool IsPickupPointAirport { get; set; }

        public int DropPoint { get; set; }
        public string DropPointName { get; set; }
        public string DropDate { get; set; }
        public string DropTime { get; set; }
        public bool IsDropPointAirport { get; set; }
    }
}
