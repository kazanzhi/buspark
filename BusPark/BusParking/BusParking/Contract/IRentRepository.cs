using System.Collections.Generic;

namespace BusParking.Contract
{
    public interface IRentRepository
    {
        List<Driver> GetDrivers();
        List<BusStatus> GetBusRentInfo();
        void AddBusStatus(BusStatus busStatus);
        void SaveDriver(Driver driver);
    }
}
