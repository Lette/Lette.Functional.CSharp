using System;

namespace Lette.Functional.CSharp.Ploeh.DepInj
{
    public class Reservation
    {
        public Reservation(DateTimeOffset date, int quantity)
        {
            Date = date;
            Quantity = quantity;
        }

        public DateTimeOffset Date { get; }
        public int Quantity { get; }
        public bool IsAccepted { get; set; }
    }
}