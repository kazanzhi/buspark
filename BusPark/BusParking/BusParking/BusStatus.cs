using System;

namespace BusParking
{
    public class BusStatus
    {
        public int Id { get; set; }
        public Driver Driver { get; set; }
        public Bus Bus { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}
