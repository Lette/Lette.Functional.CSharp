namespace Lette.Functional.CSharp.Railway
{
    public class PaymentService
    {
        public string Name { get; }
        public string Action { get; }

        public PaymentService(string name, string action)
        {
            Name = name;
            Action = action;
        }
    }
}