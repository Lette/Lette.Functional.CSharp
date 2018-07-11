using System;
using System.Runtime.InteropServices.ComTypes;

namespace Lette.Functional.CSharp
{
    public static class Functional
    {
        // id :: a -> a
        // F#: let id x = x
        public static T Id<T>(T t) => t; // normal (no need for currying)
        //public static Func<T, T> Id<T>() => x => x;

        // const :: b -> a -> b
        // F#:      let const x _ = x
        // F#:      let const x = (fun _ -> x)
        // Haskell: const x _ = x
        // Haskell: const x = \_ -> x
        public static TOut Const<TIn, TOut>(TOut constantOutput, TIn ignoredInput)
            => constantOutput;
        public static Func<TIn, TOut> Const<TIn, TOut>(this TOut constantOutput)
            => ignoredInput => constantOutput;

        public static ConstBuilder<TOut> ToConst<TOut>(this TOut constantOutput)
        {
            return new ConstBuilder<TOut>(constantOutput);
        }

        public class ConstBuilder<TOut>
        {
            private readonly TOut _constantOutput;

            public ConstBuilder(TOut constantOutput)
            {
                _constantOutput = constantOutput;
            }

            public Func<TIn, TOut> WithInput<TIn>()
            {
                return _ => _constantOutput;
            }
        }

        // compose :: (a -> b) -> (b -> c) -> (a -> c)
        // Haskell: compose f g x = g (f x)
        // F#:      let compose f g x = g (f x)
        //          let compose f g = f >> g
        //          let compose = (>>)
        // composeLeft = compose

        public static Func<T1, T3> Compose<T1, T2, T3>(this Func<T1, T2> first, Func<T2, T3> second)
            => input => second(first(input));

        public static Func<Func<T1, T2>, Func<Func<T2, T3>, Func<T1, T3>>> Compose<T1, T2, T3>()
            => first => second => input => second(first(input));

        public static Func<T1, T3> ComposeLeft<T1, T2, T3>(this Func<T1, T2> first, Func<T2, T3> second)
            => input => second(first(input));

        public static Func<Func<T1, T2>, Func<Func<T2, T3>, Func<T1, T3>>> ComposeLeft<T1, T2, T3>()
            => first => second => input => second(first(input));

        // composeRight :: (b -> c) -> (a -> b) -> (a -> c)
        // Haskell: composeRight f g x = f (g x)
        //          composeRight f g = f . g
        //          composeRight = (.)
        // F#:      let composeRight f g x = f (g x)
        //          let composeRight f g = f << g

        public static Func<T1, T3> ComposeRight<T1, T2, T3>(this Func<T2, T3> first, Func<T1, T2> second)
            => input => first(second(input));

        public static Func<Func<T2, T3>, Func<Func<T1, T2>, Func<T1, T3>>> ComposeRight<T1, T2, T3>()
            => first => second => input => first(second(input));

        // forwardPipe :: a -> (a -> b) -> b
        // F#: let forwardPipe x f = x |> f
        // F#: let (|>) x f = f x
        public static TOut ForwardPipe<TIn, TOut>(this TIn input, Func<TIn, TOut> func)
            => func(input);
        public static Func<Func<TIn, TOut>, TOut> ForwardPipe<TIn, TOut>(TIn input)
            => func => func(input);

        // Currying for 2, 3 and 4 parameter functions
        public static Func<T1, Func<T2, T3>> Curry<T1, T2, T3>(this Func<T1, T2, T3> f)
            => t1 => t2 => f(t1, t2);
        public static Func<T1, Func<T2, Func<T3, T4>>> Curry<T1, T2, T3, T4>(this Func<T1, T2, T3, T4> f)
            => t1 => t2 => t3 => f(t1, t2, t3);
        public static Func<T1, Func<T2, Func<T3, Func<T4, T5>>>> Curry<T1, T2, T3, T4, T5>(this Func<T1, T2, T3, T4, T5> f)
            => t1 => t2 => t3 => t4 => f(t1, t2, t3, t4);

        public static Func<T2, T1, TOut> Flip<T1, T2, TOut>(Func<T1, T2, TOut> f)
        {
            return (t2, t1) => f(t1, t2);
        }
    }
}