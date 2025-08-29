
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineHotelBooking
{
    /// <summary>
    /// Represents a hotel room
    /// </summary>
    public class Room
    {
        public int RoomNumber { get; private set; }
        public string Type { get; private set; }
        public decimal PricePerNight { get; private set; }
        public bool IsAvailable { get; set; }

        public Room(int roomNumber, string type, decimal pricePerNight, bool isAvailable)
        {
            RoomNumber = roomNumber;
            Type = type;
            PricePerNight = pricePerNight;
            IsAvailable = isAvailable;
        }
    }

    /// <summary>
    /// Represents a customer
    /// </summary>
    public class Customer
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }

        public Customer(string name, string email, string phone)
        {
            Name = name;
            Email = email;
            Phone = phone;
        }
    }

    /// <summary>
    /// Represents a room reservation
    /// </summary>
    public class Reservation
    {
        public int ReservationId { get; private set; }
        public Customer Customer { get; private set; }
        public Room Room { get; private set; }
        public DateTime CheckInDate { get; private set; }
        public DateTime CheckOutDate { get; private set; }
        public decimal TotalCost
        {
            get
            {
                TimeSpan duration = CheckOutDate - CheckInDate;
                return Room.PricePerNight * duration.Days;
            }
        }

        public Reservation(int reservationId, Customer customer, Room room, DateTime checkInDate, DateTime checkOutDate)
        {
            if (checkOutDate <= checkInDate)
                throw new ArgumentException("Check-out date must be after check-in date");

            ReservationId = reservationId;
            Customer = customer;
            Room = room;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
        }
    }

    /// <summary>
    /// Represents the result of an operation
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public object Data { get; private set; }

        public OperationResult(bool success, string message = "", object data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }

    /// <summary>
    /// Exception specific to online hotel booking operations
    /// </summary>
    public class OnlineBookingException : Exception
    {
        public OnlineBookingException(string message) : base(message) { }
        public OnlineBookingException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Interface for online hotel booking operations
    /// </summary>
    public interface IOnlineBookingService
    {
        OperationResult AddRoom(Room room);
        List<Room> GetAllRooms();
        List<Room> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate);
        OperationResult MakeReservation(Customer customer, int roomNumber, DateTime checkInDate, DateTime checkOutDate);
        List<Reservation> GetAllReservations();
        OperationResult CancelReservation(int reservationId);
        Reservation GetReservationById(int reservationId);
    }

    /// <summary>
    /// Service that implements online hotel booking operations
    /// </summary>
    public class OnlineBookingService : IOnlineBookingService
    {
        private readonly string _hotelName;
        private readonly List<Room> _rooms;
        private readonly List<Reservation> _reservations;
        private int _nextReservationId;

        public OnlineBookingService(string hotelName)
        {
            _hotelName = hotelName;
            _rooms = new List<Room>();
            _reservations = new List<Reservation>();
            _nextReservationId = 1000;
        }

        public string HotelName => _hotelName;

        public OperationResult AddRoom(Room room)
        {
            if (_rooms.Any(r => r.RoomNumber == room.RoomNumber))
            {
                return new OperationResult(false, $"Room with number {room.RoomNumber} already exists.");
            }

            _rooms.Add(room);
            return new OperationResult(true, "Room Added Successfully.", room);
        }

        public List<Room> GetAllRooms()
        {
            return _rooms.ToList();
        }

        public List<Room> GetAvailableRooms()
        {
            return _rooms.Where(r => r.IsAvailable).ToList();
        }

        public List<Room> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            if (checkOutDate <= checkInDate)
            {
                throw new ArgumentException("Check-out date must be after check-in date");
            }

            // Get all available rooms
            var availableRooms = new List<Room>(_rooms.Where(r => r.IsAvailable));

            // Filter out rooms that have reservations during the requested period
            foreach (var reservation in _reservations)
            {
                // Check if there's any overlap between the requested period and the reservation period
                bool overlaps = (checkInDate < reservation.CheckOutDate && checkOutDate > reservation.CheckInDate);

                if (overlaps)
                {
                    // Remove the room from available rooms
                    availableRooms.RemoveAll(r => r.RoomNumber == reservation.Room.RoomNumber);
                }
            }

            return availableRooms;
        }

        public OperationResult MakeReservation(Customer customer, int roomNumber, DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                // Validate input
                if (customer == null)
                {
                    return new OperationResult(false, "Customer information is required.");
                }

                if (checkOutDate <= checkInDate)
                {
                    return new OperationResult(false, "Check-out date must be after check-in date.");
                }

                // Find the room
                var room = _rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
                if (room == null)
                {
                    return new OperationResult(false, $"Room with number {roomNumber} not found.");
                }

                // Check if the room is available for the requested dates
                var availableRooms = GetAvailableRooms(checkInDate, checkOutDate);
                if (!availableRooms.Any(r => r.RoomNumber == roomNumber))
                {
                    return new OperationResult(false, $"Room {roomNumber} is not available for the selected dates.");
                }

                // Create the reservation
                var reservationId = _nextReservationId++;
                var reservation = new Reservation(reservationId, customer, room, checkInDate, checkOutDate);

                // Add the reservation
                _reservations.Add(reservation);

                return new OperationResult(true, "Reservation created successfully.", reservation);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"An error occurred: {ex.Message}");
            }
        }

        public List<Reservation> GetAllReservations()
        {
            return _reservations.ToList();
        }

        public Reservation GetReservationById(int reservationId)
        {
            return _reservations.FirstOrDefault(r => r.ReservationId == reservationId);
        }

        public OperationResult CancelReservation(int reservationId)
        {
            var reservation = _reservations.FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                return new OperationResult(false, $"Reservation with ID {reservationId} not found.");
            }

            _reservations.Remove(reservation);
            return new OperationResult(true, "Reservation cancelled successfully.");
        }
    }

    /// <summary>
    /// Factory for creating an online hotel booking service
    /// </summary>
    public static class OnlineBookingServiceFactory
    {
        public static IOnlineBookingService CreateOnlineBookingService(string hotelName)
        {
            return new OnlineBookingService(hotelName);
        }
    }
}