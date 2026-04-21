using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class FiloNovaRequestBase
    {
        public class MasterRequest
        {
            public string brokerCode { get; set; }
            public int langId { get; set; }

        }

        public class AvailabilityRequest : MasterRequest
        {
            public AvailabilityQueryParameters queryParameters { get; set; }
            public int channelCode { get; set; }

        }

        public class ReservationQueryParameters : MasterRequest
        {
            public AvailabilityQueryParameters reservationQueryParameters { get; set; }
            public int channelCode { get; set; }

        }

        public class AdditionalProductQueryParameters : MasterRequest
        {
            public AvailabilityQueryParameters queryParameters { get; set; }
            public string groupCodeId { get; set; }

        }

        public class AvailabilityQueryParameters
        {
            public string pickupBranchId { get; set; }
            public string dropoffBranchId { get; set; }
            public string pickupDateTime { get; set; }
            public string dropoffDateTime { get; set; }

        }

        public class PostReservationRequest : ReservationQueryParameters
        {
            public ReservationCustomerParameters reservationCustomerParameters { get; set; }
            public ReservationEquimentParameters reservationEquimentParameters { get; set; }
            public List<ReservationAdditionalProduct> reservationAdditionalProducts { get; set; }
            public ReservationPriceParameters reservationPriceParameters { get; set; }

        }

        public class DummyContactData
        {
            public string name { get; set; }
            public string surname { get; set; }
            public string fullName { get; set; }
            public string email { get; set; }
            public string phoneNumber { get; set; }
            public string governmentId { get; set; }
            public string referenceNumber { get; set; }
        }

        public class ReservationCustomerParameters
        {
            public string brokerCode { get; set; }
            public DummyContactData dummyContactData { get; set; }
        }

        public class ReservationBillingParameters
        {
            public int billingType { get; set; }
        }

        public class ReservationEquimentParameters : ReservationBillingParameters
        {
            public string groupCodeId { get; set; }
        }

        public class ReservationAdditionalProduct : ReservationBillingParameters
        {
            public string productId { get; set; }
            public int value { get; set; }
        }

        public class ReservationPriceParameters
        {
            public int paymentType { get; set; }
            public string trackingNumber { get; set; }
            public int paymentMethodCode { get; set; }
        }

        public class PostCancelReservation
        {
            public string reservationId { get; set; }
            public int? cancellationReason { get; set; }
            public string pnrNumber { get; set; }
            public int langId { get; set; }
            public int? channelCode { get; set; }
        }
    }


}
