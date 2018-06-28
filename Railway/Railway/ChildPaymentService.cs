namespace Lette.Functional.CSharp.Railway
{
    public class ChildPaymentService
    {
        public string OriginalTransactionKey { get; }
        public PaymentService PaymentService { get; }

        public ChildPaymentService(string originalTransactionKey, PaymentService paymentService)
        {
            OriginalTransactionKey = originalTransactionKey;
            PaymentService = paymentService;
        }
    }
}