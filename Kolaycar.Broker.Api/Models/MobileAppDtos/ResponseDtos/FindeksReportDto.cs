using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.ResponseDtos
{
    public class FindeksReportDto
    {
        public bool CompanyDecision { get; set; }
        public DateTime ReportDate { get; set; }
        public int CompanyScore { get; set; }
        public string CompanyNote { get; set; }
        public int CustomerAge { get; set; }
        public int CustomerLicenseAge { get; set; }
        public string CompanyDecisionDesc { get; set; }
        public List<CompanySegmentList> CompanySegmentList { get; set; }
        public List<CarGroupList> CarGroupList { get; set; }
    }

    public class CarGroupList
    {
        public string CarGroupCode { get; set; }
        public bool YoungDriverPacked { get; set; }
    }

    public class CompanySegmentList
    {
        public int CompanySegmentId { get; set; }
        public string CompanySegment { get; set; }
    }
}
