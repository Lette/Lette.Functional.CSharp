using System;

namespace Lette.Functional.CSharp.Railway
{
    public abstract class ChurchBoolean
    {
        public static ChurchBoolean True => new TrueImpl();
        public static ChurchBoolean False => new FalseImpl();
        public abstract TResult Match<TResult>(Func<TResult> @true, Func<TResult> @false);

        private class TrueImpl : ChurchBoolean
        {
            public override TResult Match<TResult>(Func<TResult> @true, Func<TResult> @false)
            {
                return @true();
            }
        }

        private class FalseImpl : ChurchBoolean
        {
            public override TResult Match<TResult>(Func<TResult> @true, Func<TResult> @false)
            {
                return @false();
            }
        }
    }
}