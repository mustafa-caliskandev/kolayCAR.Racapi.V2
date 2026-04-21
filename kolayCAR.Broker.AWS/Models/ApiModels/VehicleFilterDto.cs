using System;
using System.Collections.Generic;
using System.Text;

namespace kolayCAR.Broker.AWS.Models.ApiModels
{
    public class VehicleFilterDto
    {
        /// <summary>
        /// 0 => Varsayılan sıralama
        /// 1 => En düşük fiyat
        /// 2 => En yüksek fiyat
        /// 3 => Tedarikçi adı
        /// </summary>
        public int SortType { get; set; }

        public int MinimumKm { get; set; }
        public int MaximumKm { get; set; }

        public int MinimumPrice { get; set; }
        public int MaximumPrice { get; set; }

        public int MinimumDeposit { get; set; }
        public int MaximumDeposit { get; set; }

        public bool CampaignVehicles { get; set; }

        public int[] DriverAgeList { get; set; }
        public int[] LicenseYearList { get; set; }

        public string[] CategoryList { get; set; }
        public string[] DeliveryTypeList { get; set; }
        public string[] FuelList { get; set; }
        public string[] ModelList { get; set; }
        public string[] TransmissionList { get; set; }
        public string[] VendorList { get; set; }

        public List<Tuple<int, int>> SelectedPriceList { get; set; }
        public List<Tuple<int, int>> SelectedDepositList { get; set; }
        public List<Tuple<int, int>> SelectedKmList { get; set; }

        public string[] PersonList { get; set; }
    }
}
