namespace Lette.Functional.CSharp.Railway
{
    public class PaymentJsonModel
    {
        public string Name { get; set; }
        public string Action { get; set; }
        public ChurchBoolean StartRecurrent { get; set; }
        public Maybe<string> TransactionKey { get; set; }
    }
}