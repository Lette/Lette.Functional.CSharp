using System;
using System.Collections.Generic;

namespace Lette.Functional.CSharp
{
    public abstract class Maybe<T>
    {
        public static Maybe<T> Just(T value) => new JustImpl(value);
        public static Maybe<T> Nothing => new NothingImpl();

        public abstract TOut Match<TOut>(Func<T, TOut> just, Func<TOut> nothing);
        public abstract void Match(Action<T> just, Action nothing);

        private class JustImpl : Maybe<T>
        {
            private static readonly bool WrappedTypeCanBeNull =
                typeof(T).IsClass
                ||
                typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

            private readonly T _value;

            public JustImpl(T value)
            {
                if (WrappedTypeCanBeNull && Equals(value, default))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _value = value;
            }

            public override TOut Match<TOut>(Func<T, TOut> just, Func<TOut> nothing)
            {
                return just(_value);
            }

            public override void Match(Action<T> just, Action nothing)
            {
                just(_value);
            }

            public override string ToString()
            {
                return "Just " + _value;
            }
        }

        private class NothingImpl : Maybe<T>
        {
            public override TOut Match<TOut>(Func<T, TOut> just, Func<TOut> nothing)
            {
                return nothing();
            }

            public override void Match(Action<T> just, Action nothing)
            {
                nothing();
            }

            public override string ToString()
            {
                return "Nothing";
            }
        }

        private static readonly MaybeComparer<T> Comparer = new MaybeComparer<T>();

        public override bool Equals(object obj)
        {
            return Comparer.Equals(this, (Maybe<T>)obj);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
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

    public static class Maybe
    {
        // FUNCTOR

        // fmap :: (a -> b) -> (f a -> f b)
        public static Func<Maybe<TIn>, Maybe<TOut>> FMap<TIn, TOut>(this Func<TIn, TOut> f)
        {
            return input => FMap(f, input);
        }

        // fmap :: (a -> b) -> f a -> f b
        public static Maybe<TOut> FMap<TIn, TOut>(this Func<TIn, TOut> f, Maybe<TIn> input)
        {
            return input.Match(
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
        public static Maybe<T> Pure<T>(this T value) => Maybe<T>.Just(value);

        // apply :: f (a -> b) -> (f a -> f b)
        public static Func<Maybe<TIn>, Maybe<TOut>> Apply<TIn, TOut>(this Maybe<Func<TIn, TOut>> mf)
        {
            return input => mf.Apply(input);
        }

        // apply :: f (a -> b) -> f a -> f b
        public static Maybe<TOut> Apply<TIn, TOut>(this Maybe<Func<TIn, TOut>> mf, Maybe<TIn> input)
        {
            return mf.Match(
                just:    f  => input.Match(
                    just:    x  => Maybe<TOut>.Just(f(x)),
                    nothing: () => Maybe<TOut>.Nothing),
                nothing: () => Maybe<TOut>.Nothing);
        }

        // MONAD

        // join :: m (m a) -> m a
        public static Maybe<T> Join<T>(Maybe<Maybe<T>> maybes)
        {
            return maybes.Match(
                just:    m  => m,
                nothing: () => Maybe<T>.Nothing);
        }

        // bind :: m a -> (a -> m b) -> m b
        public static Maybe<TOut> Bind<TIn, TOut>(
            this Maybe<TIn> input,
            Func<TIn, Maybe<TOut>> f)
        {
            // Standard definition of Bind, should work for all Monads!
            return Join(FMap(f, input));
        }

        // altBind :: (a -> m b) -> (m a -> m b)
        public static Func<Maybe<TIn>, Maybe<TOut>> AltBind<TIn, TOut>(this Func<TIn, Maybe<TOut>> f)
        {
            return input => input.Bind(f);
        }

        // KLEISLI COMPOSITION

        // (>=>) :: Monad m => (a -> m b) -> (b -> m c) -> (a -> m c)
        public static Func<TIn, Maybe<TOut>> KBind<TIn, TIntermediate, TOut>(
            this Func<TIn, Maybe<TIntermediate>> first,
            Func<TIntermediate, Maybe<TOut>> second)
        {
            return input => Pure(input).Bind(first).Bind(second);
        }

        // UTILITY

        // toResult :: (a -> Maybe b) -> String -> (a -> Result b)
        public static Func<TIn, Result<TOut>> ToResult<TIn, TOut>(this Func<TIn, Maybe<TOut>> func, string errorMessage)
        {
            return input => func(input).Match(
                just:    x  => Result<TOut>.Ok(x),
                nothing: () => Result<TOut>.Error(errorMessage));
        }
    }
}