namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    public class ErrorDto
    {
        public string ReservationToken { get; set; }
        public string PaymentCode { get; set; }
        public string Message { get; set; }
        public string SystemMessage { get; set; }
        public string ErrorStage { get; set; }
        public string ErrorType { get; set; }
    }
}
