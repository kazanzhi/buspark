using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BusParking
{
    public class RentRepository
    {
        private const string _defaultConnectionStringName = "DevDefault";
        private readonly IConfiguration _configuration;

        public RentRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SaveDriver(Driver driver)
        {
            var sqlExpressionToInsert = @"INSERT INTO driver(
	username, password)
	VALUES (@username, @password);";

            using IDbConnection connection = new NpgsqlConnection(GetDefaultConnectionString());

            connection.Query<int>(sqlExpressionToInsert, new
            {
                username = driver.Username,
                password = driver.Password,
            });
        }

        public List<Driver> GetDrivers()
        {
            const string sql = @"SELECT id as Id, username as Username, password as Password
    FROM driver;";

            using IDbConnection connection = new NpgsqlConnection(GetDefaultConnectionString());

            var drives = connection.Query<Driver>(sql);

            return drives.ToList();
        }

        public List<BusStatus> GetBusRentInfo()
        {
            using IDbConnection connection = new NpgsqlConnection(GetDefaultConnectionString());

            const string sqlExpressionToGetAllStories = @"SELECT lastbusstatus.id as Id,
lastbusstatus.date as Date, 
lastbusstatus.status as Status,

b.id as Id, 
b.vinnumber as VinNumber, 
b.routenumber as RouteNumber, 
b.mark as Mark, 
b.maxpassangers as MaxPassangers,

d.id as Id,
d.username as Username

FROM bus as b
LEFT JOIN (
select row_number() over (partition by bs_i.busid
                                order by bs_i.date desc) as rOrder
						 , bs_i.id
                         , bs_i.busid
                         , bs_i.driverid
                         , bs_i.status
                         , bs_i.date
                      from busstatus as bs_i
) as lastbusstatus on lastbusstatus.busid = b.id and lastbusstatus.rOrder = 1
LEFT JOIN driver d on lastbusstatus.driverid = d.id;";

            var rows = connection.Query<BusStatus, Bus, Driver, BusStatus>(sqlExpressionToGetAllStories,
                (busstatus, bus, driver) =>
            {
                busstatus.Driver = driver;
                busstatus.Bus = bus;
                return busstatus;
            }, splitOn: "Id");

            var result = rows.ToList();

            return result;
        }

        public void AddBusStatus(BusStatus busStatus)
        {
            var sqlExpressionToInsert = @"INSERT INTO busstatus
(driverid, busid, date, status)
VALUES
(@driverid, @busid, @date, @status);";

            using IDbConnection connection = new NpgsqlConnection(GetDefaultConnectionString());

            connection.Query<int>(sqlExpressionToInsert, new
            {
                driverid = busStatus.Driver.Id,
                busid = busStatus.Bus.Id,
                date = busStatus.Date,
                status = busStatus.Status,
            });
        }

        private string GetDefaultConnectionString()
        {
            return _configuration.GetConnectionString(_defaultConnectionStringName);
        }
    }
}
