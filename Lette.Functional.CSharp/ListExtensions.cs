using System;
using System.Collections.Generic;
using System.Linq;

namespace Lette.Functional.CSharp
{
    public static class ListExtensions
    {
        // FUNCTOR

        // fmap :: (a -> b) -> f a -> f b
        public static Func<List<TIn>, List<TOut>> FMap<TIn, TOut>(this Func<TIn, TOut> f)
        {
            return inputList => inputList.Select(f).ToList();
        }

        // APPLICATIVE

        // pure :: a -> f a
        public static List<T> Pure<T>(this T t) => new List<T> { t };

        // apply :: f(a -> b) -> f a -> f b
        public static Func<List<TIn>, List<TOut>> Apply<TIn, TOut>(this List<Func<TIn, TOut>> mf)
        {
            return input => mf.SelectMany(f => input.Select(i => f(i))).ToList();
        }
    }
}