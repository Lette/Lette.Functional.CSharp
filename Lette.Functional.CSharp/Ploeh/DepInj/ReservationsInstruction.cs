using System;
using System.Collections.Generic;

namespace Lette.Functional.CSharp.Ploeh.DepInj
{
    public abstract class ReservationsInstruction<T>
    {
        public static ReservationsInstruction<T> IsReservationInFuture(Reservation reservation, Func<bool, T> continuation)
            => new IsReservationInFutureImpl((reservation, continuation));

        public static ReservationsInstruction<T> ReadReservations(DateTimeOffset dt, Func<IReadOnlyCollection<Reservation>, T> continuation)
            => new ReadReservationImpl((dt, continuation));

        public static ReservationsInstruction<T> Create(Reservation reservation, Func<int, T> continuation)
            => new CreateImpl((reservation, continuation));

        public abstract TResult Match<TResult>(
            Func<(Reservation, Func<bool, T>), TResult> isReservationInFuture,
            Func<(DateTimeOffset, Func<IReadOnlyCollection<Reservation>, T>), TResult> readReservations,
            Func<(Reservation, Func<int, T>), TResult> create);

        private class IsReservationInFutureImpl : ReservationsInstruction<T>
        {
            private readonly (Reservation reservation, Func<bool, T> continuation) _tuple;

            public IsReservationInFutureImpl((Reservation reservation, Func<bool, T> continuation) tuple)
            {
                _tuple = tuple;
            }

            public override TResult Match<TResult>(
                Func<(Reservation, Func<bool, T>), TResult> isReservationInFuture,
                Func<(DateTimeOffset, Func<IReadOnlyCollection<Reservation>, T>), TResult> readReservations,
                Func<(Reservation, Func<int, T>), TResult> create)
            {
                return isReservationInFuture(_tuple);
            }
        }

        private class ReadReservationImpl : ReservationsInstruction<T>
        {
            private readonly (DateTimeOffset dt, Func<IReadOnlyCollection<Reservation>, T> continuation) _tuple;

            public ReadReservationImpl((DateTimeOffset dt, Func<IReadOnlyCollection<Reservation>, T> continuation) tuple)
            {
                _tuple = tuple;
            }

            public override TResult Match<TResult>(
                Func<(Reservation, Func<bool, T>), TResult> isReservationInFuture,
                Func<(DateTimeOffset, Func<IReadOnlyCollection<Reservation>, T>), TResult> readReservations,
                Func<(Reservation, Func<int, T>), TResult> create)
            {
                return readReservations(_tuple);
            }
        }

        private class CreateImpl : ReservationsInstruction<T>
        {
            private readonly (Reservation reservation, Func<int, T> continuation) _tuple;

            public CreateImpl((Reservation reservation, Func<int, T> continuation) tuple)
            {
                _tuple = tuple;
            }

            public override TResult Match<TResult>(
                Func<(Reservation, Func<bool, T>), TResult> isReservationInFuture,
                Func<(DateTimeOffset, Func<IReadOnlyCollection<Reservation>, T>), TResult> readReservations,
                Func<(Reservation, Func<int, T>), TResult> create)
            {
                return create(_tuple);
            }
        }
    }

    public static class ReservationsInstruction
    {
        public static ReservationsInstruction<TResult> Select<T, TResult>(
            this ReservationsInstruction<T> source,
            Func<T, TResult> selector)
        {
            return source.Match<ReservationsInstruction<TResult>>(
                isReservationInFuture: t =>
                    ReservationsInstruction<TResult>.IsReservationInFuture(t.Item1, b => selector(t.Item2(b))),
                readReservations: t =>
                    ReservationsInstruction<TResult>.ReadReservations(t.Item1, d => selector(t.Item2(d))),
                create: t =>
                    ReservationsInstruction<TResult>.Create(t.Item1, r => selector(t.Item2(r))));
        }
    }
}