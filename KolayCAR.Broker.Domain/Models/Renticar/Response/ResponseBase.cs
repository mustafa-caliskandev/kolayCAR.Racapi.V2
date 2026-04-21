using System;

namespace KolayCAR.Broker.Domain.Models.Renticar.Response
{
    public class ResponseBase
    {
    }
    public class Search
    {
        public string _id { get; set; }
        public string pickup_location { get; set; }
        public string pickup_location_name { get; set; }
        public DateTime pickup_datetime { get; set; }
        public string return_location { get; set; }
        public string return_location_name { get; set; }
        public DateTime return_datetime { get; set; }
        public int day { get; set; }
    }

    public class TotalPrice
    {
        public double? USD { get; set; }
        public double? EUR { get; set; }
        public double? TRY { get; set; }
    }
    public class Provision
    {
        public double? USD { get; set; }
        public double? EUR { get; set; }
        public double? TRY { get; set; }
    }
    public class DropPrice
    {
        public double? USD { get; set; }
        public double? EUR { get; set; }
        public double? TRY { get; set; }
    }
    public class DailyPrice
    {
        public double? USD { get; set; }
        public double? EUR { get; set; }
        public double? TRY { get; set; }
    }
}
