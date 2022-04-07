using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lette.Functional.CSharp
{
    public class State { }

    public class Thing
    {
        private State _state;

        public Thing(State state)
        {
            _state = state;
        }

        void F() {}
        void G(int a) {}
        string H() => "some result";
        int I(bool b) => 42;
    }

    public delegate (T, TState) StateFn<T, TState>(TState state);

    //public delegate (T, string) StrStateFn<T>(string state);

    public static class StateExtensions
    {
        public static StateFn<T, TState> ToState<T, TState>(this T item) => state => (item, state);

        //public static StrStateFn<TOut> Bind<TIn, TOut>(this StrStateFn<TIn> m, Func<TIn, StrStateFn<TOut>> f)
        //{
        //    return state =>
        //    {
        //        var (x, newState) = m(state);
        //        return f(x)(newState);
        //    };
        //}

        public static StateFn<TOut, TState> Bind<TIn, TOut, TState>(this StateFn<TIn, TState> m, Func<TIn, StateFn<TOut, TState>> f)
        {
            return state =>
            {
                var (x, newState) = m(state);
                return f(x)(newState);
            };
        }

        public static StateFn<TOut, TState> Map<TIn, TOut, TState>(this StateFn<TIn, TState> m, Func<TIn, TOut> f)
        {
            return state =>
            {
                var (v, newState) = m(state);
                return (f(v), newState);
            };
        }

        public static StateFn<T, TState> MapState<T, TState>(this StateFn<T, TState> m, Func<TState, TState> f)
        {
            return state =>
            {
                var (v, newState) = m(state);
                return (v, f(newState));
            };
        }
    }
}
