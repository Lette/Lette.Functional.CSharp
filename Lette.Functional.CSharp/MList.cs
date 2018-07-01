using System;
using System.Collections.Generic;

namespace Lette.Functional.CSharp
{
    public abstract class MList<T>
    {
        public static MList<T> Empty => new EmptyImpl();
        public static MList<T> List(T head, MList<T> tail) => new ListImpl(head, tail);

        public abstract TOut Match<TOut>(Func<TOut> empty, Func<T, MList<T>, TOut> list);

        private class EmptyImpl : MList<T>
        {
            public override TOut Match<TOut>(Func<TOut> empty, Func<T, MList<T>, TOut> list)
            {
                return empty();
            }

            public override string ToString()
            {
                return "[]";
            }
        }

        private class ListImpl : MList<T>
        {
            private readonly T _head;
            private readonly MList<T> _tail;

            public ListImpl(T head, MList<T> tail)
            {
                _head = head;
                _tail = tail;
            }

            public override TOut Match<TOut>(Func<TOut> empty, Func<T, MList<T>, TOut> list)
            {
                return list(_head, _tail);
            }

            public override string ToString()
            {
                return _head + " :: " + _tail;
            }
        }

        private readonly MListComparer<T> _comparer = new MListComparer<T>();

        public override bool Equals(object obj)
        {
            return _comparer.Equals(this, (MList<T>)obj);
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode(this);
        }
    }

    public static class MList
    {
        // FUNCTOR

        // fmap :: (a -> b) -> f a -> f b
        public static Func<MList<TIn>, MList<TOut>> FMap<TIn, TOut>(this Func<TIn, TOut> f)
        {
            return input => f.FMap(input);
        }

        public static MList<TOut> FMap<TIn, TOut>(this Func<TIn, TOut> f, MList<TIn> input)
        {
            return input.Match(
                empty: ()      => MList<TOut>.Empty,
                list:  (x, xs) => MList<TOut>.List(f(x), f.FMap(xs)));
        }

        // APPLICATIVE

        // pure :: a -> f a
        public static MList<T> Pure<T>(this T t) => MList<T>.List(t, MList<T>.Empty);

        // apply :: f(a -> b) -> (f a -> f b)
        public static Func<MList<TIn>, MList<TOut>> Apply<TIn, TOut>(this MList<Func<TIn, TOut>> mf)
        {
            // fs <*> xs = [f x | f <- fs, x <- xs]

            return input => mf.Apply(input);
        }

        // apply :: f(a -> b) -> f a -> f b
        public static MList<TOut> Apply<TIn, TOut>(this MList<Func<TIn, TOut>> mf, MList<TIn> input)
        {
            // fs <*> xs = [f x | f <- fs, x <- xs]

            MList<TOut> Inner(Func<TIn, TOut> g, MList<TIn> xs, MList<TOut> acc)
                => xs.Match(
                    empty: ()      => acc,
                    list:  (y, ys) => Inner(g, ys, MList<TOut>.List(g(y), acc)));

            MList<TOut> Outer(MList<Func<TIn, TOut>> fs, MList<TIn> xs, MList<TOut> acc)
                => fs.Match(
                    empty: ()      => acc,
                    list:  (g, gs) => Outer(gs, xs, Inner(g, xs, acc)));

            return Outer(mf, input, MList<TOut>.Empty).Reverse();
        }

        // UTILITY

        public static int Length<T>(this MList<T> list)
        {
            return list.Match(
                empty: ()      => 0,
                list:  (x, xs) => 1 + xs.Length());
        }

        public static MList<T> Reverse<T>(this MList<T> list)
        {
            var result = MList<T>.Empty;

            MList<T> Inner(MList<T> xs, MList<T> acc) 
                => xs.Match(
                    empty: ()      => acc,
                    list:  (y, ys) => Inner(ys, MList<T>.List(y, acc)));

            return Inner(list, result);
        }
    }

    public class MListComparer<T> : IEqualityComparer<MList<T>>
    {
        public bool Equals(MList<T> first, MList<T> second)
        {
            return first.Match(
                empty: ()      => second.Match(
                    empty: ()      => true,
                    list:  (x, xs) => false),
                list:  (x, xs) => second.Match(
                    empty: ()      => false,
                    list:  (y, ys) => x.Equals(y) && Equals(xs, ys)));
        }

        public int GetHashCode(MList<T> mlist)
        {
            return mlist.Match(
                empty: ()      => typeof(T).GetHashCode(),
                list:  (x, xs) => x.GetHashCode() + 31 * GetHashCode(xs));
        }
    }
}