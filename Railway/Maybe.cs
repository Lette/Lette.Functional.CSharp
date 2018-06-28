using System;
using System.Collections.Generic;

namespace Lette.Functional.CSharp
{
    public abstract class Maybe<T>
    {
        public static Maybe<T> Just(T value) => new JustImpl(value);
        public static Maybe<T> Nothing => new NothingImpl();

        public abstract TResult Match<TResult>(Func<T, TResult> just, Func<TResult> nothing);

        private class JustImpl : Maybe<T>
        {
            private readonly T _value;

            public JustImpl(T value)
            {
                _value = value;
            }

            public override TResult Match<TResult>(Func<T, TResult> just, Func<TResult> nothing)
            {
                return just(_value);
            }

            public override string ToString()
            {
                return "Just " + _value;
            }
        }

        private class NothingImpl : Maybe<T>
        {
            public override TResult Match<TResult>(Func<T, TResult> just, Func<TResult> nothing)
            {
                return nothing();
            }

            public override string ToString()
            {
                return "Nothing";
            }
        }

        private readonly MaybeComparer<T> _comparer = new MaybeComparer<T>();

        public override bool Equals(object obj)
        {
            return _comparer.Equals(this, (Maybe<T>)obj);
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode(this);
        }
    }

    public class MaybeComparer<T> : IEqualityComparer<Maybe<T>>
    {
        public bool Equals(Maybe<T> first, Maybe<T> second)
        {
            return first.Match(
                just:    x  => second.Match(
                    just:    y  => x.Equals(y),
                    nothing: () => false),
                nothing: () => second.Match(
                    just:    _  => false,
                    nothing: () => true));
        }

        public int GetHashCode(Maybe<T> maybe)
        {
            return maybe.Match(
                just:    x  => x.GetHashCode(),
                nothing: () => typeof(T).GetHashCode());
        }
    }

    public static class MaybeExtensions
    {
        // FUNCTOR

        // fmap :: (a -> b) -> f a -> f b
        public static Func<Maybe<TIn>, Maybe<TOut>> FMap<TIn, TOut>(this Func<TIn, TOut> f)
        {
            return maybeIn => maybeIn.Match(
                just:    x  => Maybe<TOut>.Just(f(x)),
                nothing: () => Maybe<TOut>.Nothing);
        }

        // (<$) :: a -> f b -> f a
        // (<$) =  fmap . const
        public static Func<Maybe<TIn>, Maybe<TOut>> FConstMap<TIn, TOut>(this TOut constantOutput)
        {
            // This impl is faulty since it will map nothing to just, which is not a
            // structure preserving operation
            //return discardedInput => Maybe<TOut>.Just(constantOutput);

            // This impl is correct! (And it is the default impl in Haskell.)
            return FMap(constantOutput.ToConst().WithInput<TIn>());
        }

        // APPLICATIVE

        // pure :: a -> f a
        public static Maybe<T> Pure<T>(this T t) => Maybe<T>.Just(t);

        // apply :: f(a -> b) -> f a -> f b
        public static Func<Maybe<TIn>, Maybe<TOut>> Apply<TIn, TOut>(this Maybe<Func<TIn, TOut>> mf)
        {
            return mi => mf.Match(
                just:    f  => mi.Match(
                    just:    i  => Maybe<TOut>.Just(f(i)),
                    nothing: () => Maybe<TOut>.Nothing),
                nothing: () => Maybe<TOut>.Nothing);
        }

        // MONAD

        // bind :: m a -> (a -> m b) -> m b
        public static Maybe<TOut> Bind<TIn, TOut>(
            this Maybe<TIn> input,
            Func<TIn, Maybe<TOut>> f)
        {
            return input.Match(
                just:    x  => f(x),
                nothing: () => Maybe<TOut>.Nothing);
        }

        // altBind :: (a -> m b) -> m a -> m b
        public static Func<Maybe<TIn>, Maybe<TOut>> AltBind<TIn, TOut>(this Func<TIn, Maybe<TOut>> f)
        {
            return input => input.Match(
                just:    x  => f(x),
                nothing: () => Maybe<TOut>.Nothing);
        }

        // KLEISLI COMPOSITION

        // (>=>) :: Monad m => (a -> m b) -> (b -> m c) -> a -> m c
        public static Func<TIn, Maybe<TOut>> KBind<TIn, TIntermediate, TOut>(
            this Func<TIn, Maybe<TIntermediate>> first,
            Func<TIntermediate, Maybe<TOut>> second)
        {
            return input => Pure(input).Bind(first).Bind(second);
        }

        // UTILITY

        public static Func<TIn, Result<TOut>> ToResult<TIn, TOut>(this Func<TIn, Maybe<TOut>> func, string errorMessage)
        {
            return @in => func(@in).Match(
                just: @out => Result<TOut>.Ok(@out),
                nothing: () => Result<TOut>.Error(errorMessage));
        }
    }
}