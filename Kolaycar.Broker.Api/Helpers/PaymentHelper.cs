using KolayCAR.Broker.Domain.Models.Requests;

namespace KolayCAR.Broker.API.Helpers
{
    public class PaymentHelper
    {
        public static bool CheckPayment(PostReservationRequest postReservationRequest)
        {
            if (postReservationRequest != null)
            {
                if (postReservationRequest.BankId != 0 ||
                    postReservationRequest.BankVendorId != 0 ||
                    !string.IsNullOrEmpty(postReservationRequest.CreditCardHolder) ||
                    !string.IsNullOrEmpty(postReservationRequest.CreditCardNumber) ||
                    postReservationRequest.ExpiredYear != 0 ||
                    postReservationRequest.ExpiredMonth != 0 ||
                    !string.IsNullOrEmpty(postReservationRequest.SecurityCode) ||
                    !string.IsNullOrEmpty(postReservationRequest.CustomerIPAddress))
                {
                    return postReservationRequest.BankId != 0 &&
                    postReservationRequest.BankVendorId != 0 &&
                    !string.IsNullOrEmpty(postReservationRequest.CreditCardHolder) &&
                    !string.IsNullOrEmpty(postReservationRequest.CreditCardNumber) &&
                    postReservationRequest.ExpiredYear != 0 &&
                    postReservationRequest.ExpiredMonth != 0 &&
                    !string.IsNullOrEmpty(postReservationRequest.SecurityCode) &&
                    !string.IsNullOrEmpty(postReservationRequest.CustomerIPAddress);
                }

                return true;
            }

            return false;
        }
    }
}
