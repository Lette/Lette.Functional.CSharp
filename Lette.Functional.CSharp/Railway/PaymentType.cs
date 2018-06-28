using System;

namespace Lette.Functional.CSharp.Railway
{
    public abstract class PaymentType
    {
        public static PaymentType Individual(string name, string action)
            => new IndividualImpl(new PaymentService(name, action));

        public static PaymentType Parent(string name, string action)
            => new ParentImpl(new PaymentService(name, action));

        public static PaymentType Child(string originalTransactionKey, PaymentService paymentService)
            => new ChildImpl(new ChildPaymentService(originalTransactionKey, paymentService));

        public abstract T Match<T>(
            Func<PaymentService, T> individual,
            Func<PaymentService, T> parent,
            Func<ChildPaymentService, T> child);

        private class IndividualImpl : PaymentType
        {
            private readonly PaymentService _paymentService;

            public IndividualImpl(PaymentService paymentService)
            {
                _paymentService = paymentService;
            }

            public override T Match<T>(
                Func<PaymentService, T> individual,
                Func<PaymentService, T> parent,
                Func<ChildPaymentService, T> child)
            {
                return individual(_paymentService);
            }
        }

        private class ParentImpl : PaymentType
        {
            private readonly PaymentService _paymentService;

            public ParentImpl(PaymentService paymentService)
            {
                _paymentService = paymentService;
            }

            public override T Match<T>(
                Func<PaymentService, T> individual,
                Func<PaymentService, T> parent,
                Func<ChildPaymentService, T> child)
            {
                return parent(_paymentService);
            }
        }

        private class ChildImpl : PaymentType
        {
            private readonly ChildPaymentService _childPaymentService;

            public ChildImpl(ChildPaymentService childPaymentService)
            {
                _childPaymentService = childPaymentService;
            }

            public override T Match<T>(
                Func<PaymentService, T> individual,
                Func<PaymentService, T> parent,
                Func<ChildPaymentService, T> child)
            {
                return child(_childPaymentService);
            }
        }
    }

    public static class PaymentTypeExtensions
    {
        public static PaymentJsonModel ToJson(this PaymentType paymentType)
        {
            return paymentType.Match(
                individual: ps =>
                    new PaymentJsonModel
                    {
                        Action = ps.Action,
                        Name = ps.Name,
                        StartRecurrent = ChurchBoolean.False,
                        TransactionKey = Maybe<string>.Nothing
                    },
                parent: ps =>
                    new PaymentJsonModel
                    {
                        Action = ps.Action,
                        Name = ps.Name,
                        StartRecurrent = ChurchBoolean.True,
                        TransactionKey = Maybe<string>.Nothing
                    },
                child: cps =>
                    new PaymentJsonModel
                    {
                        Name = cps.PaymentService.Name,
                        Action = cps.PaymentService.Action,
                        StartRecurrent = ChurchBoolean.False,
                        TransactionKey = Maybe<string>.Just(cps.OriginalTransactionKey)
                    }
            );
        }
    }
}