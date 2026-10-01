using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KolayCAR.Broker.API.Mappers
{
    public static class ExtraMapper
    {
        public static ReservationExtra Map(this Extra extra, int piece) =>
            extra != null ? new ReservationExtra
            {
                ExtraId = extra.ExtraId,
                ExtraCode = extra.ExtraCode.ToString(),
                ApiExtraCode = extra.ExtraCode.ToString(),
                ExtraName = extra.ExtraName,
                ExtraDescription = extra.ExtraDescription,
                ExtraRentalType = extra.ExtraRentalType,
                ExtraQuantityIncreasable = extra.ExtraQuantityIncreasable,
                Price = extra.Price,
                Piece = piece
            }
            : null;

        public static Extra Map(this ReservationExtra extra) =>
            extra != null ? new ReservationExtra
            {
                ExtraId = extra.ExtraId,
                ExtraCode = extra.ExtraCode,
                ApiExtraCode = extra.ExtraCode,
                ExtraName = extra.ExtraName,
                ExtraDescription = extra.ExtraDescription,
                ExtraRentalType = extra.ExtraRentalType,
                ExtraQuantityIncreasable = extra.ExtraQuantityIncreasable,
                Price = extra.Price,
            }
            : null;

        // public static CyrptExtra Map(this Extra extra) =>
        //extra != null ? new CyrptExtra
        //{
        //    I = extra.ExtraId,
        //    C = extra.ExtraCode,
        //    AC = extra.ApiExtraCode,
        //    R = (int)extra.ExtraRentalType,
        //    T = (int)extra.ExtraType,              
        //    A = extra.ApiPrice,
        //    P = extra.Price,
        //}
        //: null;
        public static CyrptExtra Map(this Extra extra) =>
         extra != null ? new CyrptExtra
         {
             I = extra.ExtraId,
             C = extra.ExtraCode,
             R = extra.ExtraRentalType == null ? ExtraRentalTypes.Daily : (ExtraRentalTypes)extra.ExtraRentalType,
             T = extra.ExtraType == null ? AdditionalProductTypes.Extra : (AdditionalProductTypes)extra.ExtraType,
             P = extra.Price,
             A = extra.ApiPrice,
             AC = extra.ApiExtraCode,
             N = extra.ExtraName,
             V = extra.VendorExtraExists,
             CD = extra.Code
         }
         : null;
        public static Extra MapCyrpt(this Extra extra, List<CyrptExtra> extras)
        {
            // A nested broker can return its original Code while exposing a different
            // local ExtraCode. The Code stored in the token is the exact reservation key.
            var cyrtpExtra = extras?.FirstOrDefault(e => e.CD == extra.Code);

            // Keep compatibility with older tokens that do not contain CD.
            if (cyrtpExtra == null)
            {
                string[] entityArr = Encoding.UTF8.GetString(Convert.FromBase64String(extra.Code)).Split("~");
                if (entityArr.Length == 2)
                    cyrtpExtra = extras?.FirstOrDefault(e => e.I == entityArr[0].ToIntNullSafe() && e.C == entityArr[1]);
            }

            if (cyrtpExtra == null)
                throw new ArgumentException("Extra code is not valid for this reservation.", nameof(extra));

            extra.ExtraId = cyrtpExtra.I;
            extra.ExtraCode = cyrtpExtra.C;
            extra.ExtraRentalType = cyrtpExtra.R;
            extra.ExtraType = cyrtpExtra.T;
            extra.Price = cyrtpExtra.P;
            extra.ApiPrice = cyrtpExtra.A;
            extra.ApiExtraCode = cyrtpExtra.AC;
            extra.ExtraName = cyrtpExtra.N;
            extra.VendorExtraExists = cyrtpExtra.V;
            extra.Code = cyrtpExtra.CD;

            return extra;
        }

        public static List<Extra> MapExtras(this List<ReservationExtra> reservationExtras)
        {
            var _extras = new List<Extra>();

            if (reservationExtras != null && reservationExtras.Count != 0)
                foreach (var extra in reservationExtras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        public static List<Extra> Map(this List<Additionalproduct> extras)
        {
            var _extras = new List<Extra>();

            foreach (var extra in extras)
            {
                _extras.Add(new Extra
                {
                    ExtraId = extra.Productid,
                    ExtraCode = extra.Productcode,
                    ApiExtraCode = extra.Productcode,
                    ExtraName = extra.Productname,
                    ExtraDescription = extra.Productdescription,
                    ExtraQuantityIncreasable = extra.Quantityincreasable.ToBoolNullSafe(),
                    DefaultPrice = extra.Defaultprice.ToFloatNullSafe(),
                    ShowDayCountStart = extra.Showdaycountstart,
                    ShowDayCountEnd = extra.Showdaycountend,
                    Sequence = extra.Sequence.ToIntNullSafe(),
                    VendorExtraExists = extra.VendorExtraExists.ToBoolNullSafe()
                });
            }

            return _extras;
        }
    }
}
