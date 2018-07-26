using System;
using System.Collections.Generic;

namespace Lette.Functional.CSharp.Ploeh.DepInj
{
    public static class ReservationsProgramInterpreter
    {
        public static T Interpret<T>(
            this ReservationsProgram<T> program,
            string connectionString)
        {
            return program.Match(
                pure: x => x,
                free: i => i.Match(
                    isReservationInFuture: t =>
                        t.Item2(IsReservationInFuture(t.Item1))
                            .Interpret(connectionString),
                    readReservations: t =>
                        t.Item2(ReadReservations(t.Item1, connectionString))
                            .Interpret(connectionString),
                    create: t =>
                        t.Item2(Create(t.Item1, connectionString))
                            .Interpret(connectionString)));
        }

        public static bool IsReservationInFuture(Reservation reservation)
        {
            return reservation.Date > DateTimeOffset.Now;
        }

        public static IReadOnlyCollection<Reservation> ReadReservations(DateTimeOffset dt, string connectionString)
        {
            return new List<Reservation>();
        }

        public static int Create(Reservation reservation, string connectionString)
        {
            return 1;
        }
    }
}