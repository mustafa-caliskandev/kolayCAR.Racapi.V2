namespace KolayCAR.Broker.Domain.Models
{
    public class ReservationUpdate
    {
        public ReservationUpdatePropertyTypes ReservationUpdatePropertyType { get; set; }
        public ReservationUpdateProperties ReservationUpdateProperty { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public bool IsNumber { get; set; }
        public ReservationExtra AdditionalProduct { get; set; }
    }

    public enum ReservationUpdatePropertyTypes
    {
        Reservation,
        AdditionalProduct
    }

    public enum ReservationUpdateProperties
    {
        None,
        CustomerName,
        CustomerSurname,
        CustomerIdentityNumber,
        CustomerAddress,
        CustomerPhone,
        CustomerMail,
        CustomerBirthday,
        CustomerTitle,
        CustomerTaxOffice,
        CustomerTaxNumber,
        CustomerArrivalFlightNumber,
        CustomerNote,
        PickupLocationId,
        PickupLocationName,
        ReturnLocationId,
        ReturnLocationName,
        PickupDate,
        ReturnDate,
        RentalDuration,
        DailyPrice,
        ExtraPrice,
        OneWayFee,
        TotalPrice,
        PaidAmount,
        APIDailyPrice,
        APIExtraAmount,
        APIOneWayFee,
        APITotalAmount,
        APIPaidAmount,
        ServiceCharge,
        AddAdditionalProduct,
        RemoveAdditionalProduct,
        AdditionalProductPieceChange,
        AdditionalProductPriceChange,
        AdditionalProductAPIPriceChange,
        CouponDiscountAmount,
        APIReservationNumber,
        PartialRefund,
        ReservationDelete
    }

    public enum ReservationUpdateSQLProperties
    {
        None,
        MUSTERIAD,
        MUSTERISOYAD,
        MUSTERITCPASAPORT,
        MUSTERIADRES,
        MUSTERITELEFON,
        MUSTERIEPOSTA,
        CUSTOMERBIRTHDAY,
        MUSTERIUNVAN,
        MUSTERIVERGIDAIRE,
        MUSTERIVERGINO,
        MUSTERIGELISUCUSNO,
        MUSTERINOT,
        ALISYERID,
        ALISYERI,
        BIRAKISYERID,
        BIRAKISYERI,
        ALISTARIHI,
        BIRAKISTARIHI,
        KIRALAMASURESI,
        GUNLUKFIYAT,
        EXTRATUTAR,
        TEKYONTUTAR,
        TOPLAMTUTAR,
        ODENENTUTAR,
        APIDAILYPRICE,
        APIEXTRAAMOUNT,
        APIONEWAYFEE,
        APITOTALAMOUNT,
        APIPAIDAMOUNT,
        SERVICECHARGE,
        EXTRAID,
        EXTRAID2,
        PIECE,
        AMOUNT,
        APIPRICE,
        COUPONDISCOUNTAMOUNT,
        APIRESERVATIONNUMBER
    }
}
