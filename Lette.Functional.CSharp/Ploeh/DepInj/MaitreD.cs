using System.Linq;

namespace Lette.Functional.CSharp.Ploeh.DepInj
{
    public interface IMaitreD
    {
        ReservationsProgram<int?> TryAccept(Reservation reservation);
    }

    public class MaitreD : IMaitreD
    {
        public MaitreD(int capacity)
        {
            Capacity = capacity;
        }

        public ReservationsProgram<int?> TryAccept(Reservation reservation)
        {
            return ReservationsProgram
                .IsReservationInFuture(reservation)
                .SelectMany(isInFuture =>
                    {
                        if (!isInFuture)
                        {
                            return ReservationsProgram<int?>.Pure(null);
                        }

                        return ReservationsProgram
                            .ReadReservations(reservation.Date)
                            .SelectMany(reservations =>
                                {
                                    var reservedSeats = reservations.Sum(r => r.Quantity);

                                    if (Capacity < reservedSeats + reservation.Quantity)
                                    {
                                        return ReservationsProgram<int?>.Pure(null);
                                    }

                                    reservation.IsAccepted = true;

                                    return ReservationsProgram
                                        .Create(reservation)
                                        .Select(x => new int?(x));
                                });
                    });
        }

        public int Capacity { get; }
    }
}