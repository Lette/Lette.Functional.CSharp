using System;
using System.Collections.Generic;

namespace Lette.Functional.CSharp.Ploeh.DepInj
{
    public abstract class ReservationsProgram<T>
    {
        public static ReservationsProgram<T> Free(ReservationsInstruction<ReservationsProgram<T>> instruction)
            => new FreeImpl(instruction);

        public static ReservationsProgram<T> Pure(T t)
            => new PureImpl(t);

        public abstract TResult Match<TResult>(
            Func<ReservationsInstruction<ReservationsProgram<T>>, TResult> free,
            Func<T, TResult> pure);

        private class FreeImpl : ReservationsProgram<T>
        {
            private readonly ReservationsInstruction<ReservationsProgram<T>> _instruction;

            public FreeImpl(ReservationsInstruction<ReservationsProgram<T>> instruction)
            {
                _instruction = instruction;
            }

            public override TResult Match<TResult>(Func<ReservationsInstruction<ReservationsProgram<T>>, TResult> free, Func<T, TResult> pure)
            {
                return free(_instruction);
            }
        }

        private class PureImpl : ReservationsProgram<T>
        {
            private readonly T _t;

            public PureImpl(T t)
            {
                _t = t;
            }

            public override TResult Match<TResult>(Func<ReservationsInstruction<ReservationsProgram<T>>, TResult> free, Func<T, TResult> pure)
            {
                return pure(_t);
            }
        }
    }

    public static class ReservationsProgram
    {
        public static ReservationsProgram<TResult> Select<T, TResult>(
            this ReservationsProgram<T> source,
            Func<T, TResult> selector)
        {
            return source.SelectMany(x => ReservationsProgram<TResult>.Pure(selector(x)));
        }

        public static ReservationsProgram<TResult> SelectMany<T, TResult>(
            this ReservationsProgram<T> source,
            Func<T, ReservationsProgram<TResult>> selector)
        {
            return source.Match(
                free: i => ReservationsProgram<TResult>.Free(i.Select(p => p.SelectMany(selector))),
                pure: x => selector(x));
        }

        public static ReservationsProgram<TResult> SelectMany<T, U, TResult>(
            this ReservationsProgram<T> source,
            Func<T, ReservationsProgram<U>> k,
            Func<T, U, TResult> s)
        {
            return source
                .SelectMany(x => k(x)
                    .SelectMany(y => ReservationsProgram<TResult>.Pure(s(x, y))));
        }

        public static ReservationsProgram<bool> IsReservationInFuture(Reservation reservation)
        {
            return ReservationsProgram<bool>.Free(
                ReservationsInstruction<ReservationsProgram<bool>>.IsReservationInFuture(
                    reservation,
                    x => ReservationsProgram<bool>.Pure(x)));
        }

        public static ReservationsProgram<IReadOnlyCollection<Reservation>> ReadReservations(DateTimeOffset date)
        {
            return ReservationsProgram<IReadOnlyCollection<Reservation>>.Free(
                ReservationsInstruction<ReservationsProgram<IReadOnlyCollection<Reservation>>>.ReadReservations(
                    date,
                    x => ReservationsProgram<IReadOnlyCollection<Reservation>>.Pure(x)));
        }

        public static ReservationsProgram<int> Create(Reservation reservation)
        {
            return ReservationsProgram<int>.Free(
                ReservationsInstruction<ReservationsProgram<int>>.Create(
                    reservation,
                    x => ReservationsProgram<int>.Pure(x)));
        }
    }
}