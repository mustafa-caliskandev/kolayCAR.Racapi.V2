using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.CommentDtos
{
    public class VendorLocationCommentDto
    {
        public int Index { get; set; }
        public int LanguageId { get; set; }

        public string MemberPictureUrl { get; set; }
        public string MemberFullName { get; set; }

        public string Comment { get; set; }
        public string CommentSource { get; set; }
        public DateTime CommentDate { get; set; }

        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorLogoUrl { get; set; }
        public string VendorContentUrl { get; set; }

        public int ReviewCount { get; set; }
        public decimal Score { get; set; }
        public decimal VendorScore { get; set; }
        public Dictionary<string, decimal> SpecialScores { get; set; }

        public static VendorLocationCommentDto Create(int index, int languageId, string memberPictureUrl, string memberFullName, string comment, string commentSource, DateTime commentDate, int vendorId, string vendorName, string vendorLogoUrl, int reviewCount, decimal score, Dictionary<string, decimal> specialScores)
        {
            return new VendorLocationCommentDto()
            {
                Index = index,
                LanguageId = languageId,
                MemberPictureUrl = memberPictureUrl,
                MemberFullName = memberFullName,
                Comment = comment,
                CommentSource = commentSource,
                CommentDate = commentDate,
                VendorId = vendorId,
                VendorName = vendorName,
                VendorLogoUrl = vendorLogoUrl,
                ReviewCount = reviewCount,
                Score = score,
                SpecialScores = specialScores
            };
        }
    }
}
