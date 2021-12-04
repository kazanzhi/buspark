using BusParking.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusParking.Controllers
{
    [Route("api/rent")]
    [ApiController]
    public class RentController : ControllerBase
    {
        private readonly IRentRepository _rentRepository;

        public RentController(IRentRepository rentRepository)
        {
            _rentRepository = rentRepository;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            List<Driver> users = _rentRepository.GetDrivers();

            var driver = users.FirstOrDefault(u => u.Username == loginRequest.Username);

            if (driver is null)
            {
                return BadRequest(new
                {
                    Message = "Username doesn't exist!"
                });
            }

            if (VerityPassword(loginRequest.Password, driver.Password))
                return Ok(new { Message = "Successfull", Id = driver.Id });
            else
                return BadRequest(new { Message = "Wrong password!" });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest registerRequest)
        {
            if (registerRequest.Username.Length < 3 || registerRequest.Username.Length > 15)
            {
                return BadRequest(new
                {
                    Message = "Username must be longer than 3 and shorter than 15."
                });
            }

            if (registerRequest.Password.Length < 4 || registerRequest.Password.Length > 20)
            {
                return BadRequest(new
                {
                    Message = "Password must be longer than 4 and shorter than 20."
                });
            }

            var driver = new Driver
            {
                Username = registerRequest.Username,
                Password = HashPassword(registerRequest.Password)
            };

            try
            {
                _rentRepository.SaveDriver(driver);
            }
            catch (PostgresException ex)
            {
                var error = ex.Message;
                return error.Contains("duplicate key value violates unique constraint") ?
                    BadRequest(new
                    {
                        Message = "User with such username exists."
                    }) :
                    BadRequest(new
                    {
                        Message = "Unknown error occured. Please, try again later."
                    });
            }

            return Ok(new { Id = driver.Id });
        }

        [HttpGet("rent-info")]
        public IActionResult RentInfo()
        {
            return Ok(_rentRepository.GetBusRentInfo());
        }

        [HttpPost("rent-bus")]
        public IActionResult RentBus([FromBody] RentBusRequest rentBusRequest)
        {
            // 1) check that bus is available(free)
            var rentStatus = _rentRepository.GetBusRentInfo();
            var busInfo = rentStatus.FirstOrDefault(r =>
                r.Bus.Id == rentBusRequest.BusId);

            if (rentBusRequest.DriverId == 0)
                return BadRequest(new { Message = "Wrong DriverId." });

            if (busInfo is null || busInfo.Status == "rent")
            {
                return BadRequest(new
                {
                    Message = "Either bus doesn't exist or bus is in rent."
                });
            }

            // 2) save to db
            // | rentBusRequest.DriverId, rentBusRequest.BusId, status=rent
            BusStatus busStatus = new BusStatus
            {
                Driver = new Driver { Id = rentBusRequest.DriverId },
                Bus = new Bus { Id = rentBusRequest.BusId },
                Date = DateTime.Now,
                Status = "rent"
            };

            _rentRepository.AddBusStatus(busStatus);

            return Ok();
        }

        [HttpPost("unrent-bus")]
        public IActionResult UnRentBus([FromBody] UnRentBusRequest rentBusRequest)
        {
            // 1) check that bus is rented(not free)
            var rentStatus = _rentRepository.GetBusRentInfo();
            var busInfo = rentStatus.FirstOrDefault(r =>
                r.Bus.Id == rentBusRequest.BusId);

            if (rentBusRequest.DriverId == 0)
                return BadRequest(new { Message = "Wrong DriverId." });

            if (busInfo is null || busInfo.Status == "unrent")
            {
                return BadRequest(new
                {
                    Message = "Either bus doesn't exist or bus is not in rent."
                });
            }

            // 2) if rented -> save data to DB
            // | rentBusRequest.DriverId, rentBusRequest.BusId, status=unrent
            BusStatus busStatus = new BusStatus
            {
                Driver = new Driver { Id = rentBusRequest.DriverId },
                Bus = new Bus { Id = rentBusRequest.BusId },
                Date = DateTime.Now,
                Status = "unrent"
            };

            _rentRepository.AddBusStatus(busStatus);

            return Ok();
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerityPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
