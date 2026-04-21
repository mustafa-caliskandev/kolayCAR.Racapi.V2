using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Elitcar
{
    public static class LocationMapper
    {

        public static List<CommonModels.Location> Map()
        {
            var _locations = new List<CommonModels.Location>();

            _locations = new List<CommonModels.Location>
            {
                    new CommonModels.Location
                    {
                        LocationCode = "116",
                        LocationName = "Dalaman Büro"
                    }
                    ,
                     new CommonModels.Location
                     {
                         LocationCode = "117",
                         LocationName = "Dalaman Otel Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "164",
                         LocationName = "Zonguldak Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "165",
                         LocationName = "Elazığ Büro"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "174",
                         LocationName = "Zafer Havalimani"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "175",
                         LocationName = "Adana Şakirpaşa Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "176",
                         LocationName = "Alanya Gazipaşa Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "177",
                         LocationName = "Antalya Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "178",
                         LocationName = "Bodrum Milas Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "179",
                         LocationName = "Bursa Yenişehir Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "180",
                         LocationName = "Dalaman Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "181",
                         LocationName = "Elazığ Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "182",
                         LocationName = "Erzurum Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "183",
                         LocationName = "Esenboğa Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "184",
                         LocationName = "Eskişehir Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "185",
                         LocationName = "Gaziantep Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "186",
                         LocationName = "Ordu - Giresun Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "188",
                         LocationName = "İstanbul Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "189",
                         LocationName = "Adnan Menderes Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "190",
                         LocationName = "Kahramanmaraş Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "191",
                         LocationName = "Kars Harakani Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "192",
                         LocationName = "Kayseri Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "193",
                         LocationName = "Konya Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "194",
                         LocationName = "Malatya Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "196",
                         LocationName = "Ordu - Giresun Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "197",
                         LocationName = "Sabiha Gökçen Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "198",
                         LocationName = "Samsun Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "199",
                         LocationName = "Sivas Nuri Demirağ Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "200",
                         LocationName = "Trabzon Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "201",
                         LocationName = "Balıkesir Büro"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "202",
                         LocationName = "Bursa Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "203",
                         LocationName = "Samsun Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "205",
                         LocationName = "Diyarbakır Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "206",
                         LocationName = "Sinop Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "207",
                         LocationName = "Kastamonu Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "208",
                         LocationName = "Bartın Büro"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "209",
                         LocationName = "Karabük Büro"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "210",
                         LocationName = "Artvin Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "211",
                         LocationName = "Düzce Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "212",
                         LocationName = "Sakarya Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "213",
                         LocationName = "Merzifon Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "214",
                         LocationName = "Rize Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "215",
                         LocationName = "Amasya Merzifon Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "216",
                         LocationName = "Çorum Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "217",
                         LocationName = "Amasya Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "218",
                         LocationName = "Kastamonu Adres Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "219",
                         LocationName = "Batman Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "220",
                         LocationName = "Mardin Havalimanı"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "16",
                         LocationName = "Adres Teslimi | Avrupa"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "17",
                         LocationName = "Adres Teslimi | Anadolu"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "5",
                         LocationName = "Avrupa Yakası Otel"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "6",
                         LocationName = "Anadolu Yakası Otel"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "74",
                         LocationName = "Kuşadası Otel Teslimi"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "7",
                         LocationName = "Anadolu Yakası Büro"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "8",
                         LocationName = "Avrupa Yakası Büro"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "10",
                         LocationName = "Otogar | Esenler"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "11",
                         LocationName = "Otogar | Harem"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "14",
                         LocationName = "Konya Büro"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "19",
                         LocationName = "Ankara Büro"
                     },
                     new CommonModels.Location
                     {
                         LocationCode = "25",
                         LocationName = "Antalya Büro"
                     },
                    new CommonModels.Location
                    {
                        LocationCode = "31",
                        LocationName = "İzmir Büro"
                    },
                    new CommonModels.Location
                    {
                        LocationCode = "37",
                        LocationName = "Adana Büro"
                    },
                    new CommonModels.Location
                    {
                        LocationCode = "43",
                        LocationName = "Trabzon Büro"
                    },
                    new CommonModels.Location
                    {
                        LocationCode = "49",
                        LocationName = "Alanya Büro"
                    },
                    new CommonModels.Location
                    {
                        LocationCode = "54",
                        LocationName = "Bodrum Büro"
                    },
                    new CommonModels.Location
                    {
                        LocationCode = "59",
                        LocationName = "Bursa Büro"
                    },
                   new CommonModels.Location
                   {
                       LocationCode = "63",
                       LocationName = "Gaziantep Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "67",
                       LocationName = "Kayseri Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "81",
                       LocationName = "Samsun Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "86",
                       LocationName = "Malatya Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "99",
                       LocationName = "Eskişehir Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "102",
                       LocationName = "Giresun Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "105",
                       LocationName = "Ordu Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "15",
                       LocationName = "Konya Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "20",
                       LocationName = "Ankara Adres Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "26",
                       LocationName = "Antalya Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "32",
                       LocationName = "İzmir Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "38",
                       LocationName = "Adana Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "39",
                       LocationName = "Adana Otogar"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "44",
                       LocationName = "Trabzon Otogar"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "50",
                       LocationName = "Alanya Adres Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "51",
                       LocationName = "Alanya Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "55",
                       LocationName = "Bodrum Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "60",
                       LocationName = "Bursa Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "64",
                       LocationName = "Gaziantep Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "68",
                       LocationName = "Kayseri Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "70",
                       LocationName = "Kayseri Otogar"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "82",
                       LocationName = "Samsun Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "83",
                       LocationName = "Samsun Otogar"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "161",
                       LocationName = "Antalya Otel Teslimi (Lara)"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "21",
                       LocationName = "Ankara Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "27",
                       LocationName = "Antalya Otogar"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "28",
                       LocationName = "Antalya Adres Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "33",
                       LocationName = "Izmir Otogar"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "40",
                       LocationName = "Adana Adres Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "45",
                       LocationName = "Trabzon Adres Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "56",
                       LocationName = "Bodrum Adres Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "69",
                       LocationName = "Kayseri Adres Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "22",
                       LocationName = "Aşti Otogar Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "34",
                       LocationName = "İzmir Adres Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "46",
                       LocationName = "Trabzon Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "159",
                       LocationName = "Alaçatı Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "160",
                       LocationName = "Çeşme Otel Teslimi"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "172",
                       LocationName = "Çeşme Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "173",
                       LocationName = "Alaçatı Büro"
                   },
                   new CommonModels.Location
                   {
                       LocationCode = "221",
                       LocationName = "Balıkesir Koca Seyit Havalimanı"
                   }
             };

            return _locations;
        }
    }
}
