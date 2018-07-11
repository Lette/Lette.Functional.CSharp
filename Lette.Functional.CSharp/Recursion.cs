using System;

namespace Lette.Functional.CSharp
{
    public abstract class RecursionResult<T>
    {
        public static RecursionResult<T> Final(T result) => new FinalImpl(result);
        public static RecursionResult<T> Next(Func<RecursionResult<T>> next) => new NextImpl(next);

        public abstract TOut Match<TOut>(Func<T, TOut> final, Func<Func<RecursionResult<T>>, TOut> next);

        private class FinalImpl : RecursionResult<T>
        {
            private readonly T _result;

            public FinalImpl(T result)
            {
                _result = result;
            }

            public override TOut Match<TOut>(Func<T, TOut> final, Func<Func<RecursionResult<T>>, TOut> next)
            {
                return final(_result);
            }
        }

        public class NextImpl : RecursionResult<T>
        {
            private readonly Func<RecursionResult<T>> _next;

            public NextImpl(Func<RecursionResult<T>> next)
            {
                _next = next;
            }

            public override TOut Match<TOut>(Func<T, TOut> final, Func<Func<RecursionResult<T>>, TOut> next)
            {
                return next(_next);
            }
        }
    }

    public static class TailRecursion
    {
        public static T Run<T>(Func<RecursionResult<T>> f)
        {
            while (true)
            {
                var result = f();

                (bool isFinal, T finalValue, Func<RecursionResult<T>> next) =
                    result.Match<(bool, T, Func<RecursionResult<T>>)>(
                        final: x => (true, x, null),
                        next:  g => (false, default, g));

                if (isFinal)
                {
                    return finalValue;
                }

                f = next;
            }
        }
    }
}