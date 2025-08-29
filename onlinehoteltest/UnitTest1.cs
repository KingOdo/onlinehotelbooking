using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OnlineHotelBooking;
using System.Linq;
using System.Runtime.InteropServices;

namespace onlinehoteltest
{
    [TestClass]

    public class OnlineHotelTest
    {
        private Room room;
        private Customer customer;
        private IOnlineBookingService bookings;
        private Reservation reservation;
        private DateTime checkInDate;
        private DateTime checkOutDate;

        [TestInitialize]
        public void roomConstructorinit()
        {
            room= new Room(102, "Standard", 130.00m, true);
            customer = new Customer("King", "King@gmail.com", "+23100033453");
            bookings= OnlineBookingServiceFactory.CreateOnlineBookingService("Concord Hotel");
            bookings.AddRoom(room); 

        }

        [TestMethod]
        //initialize the room constructors??
        public void roomInitialize()
        {
            var room = new Room(112, "deluxe", 130.00m, true);
            Assert.AreEqual(112, room.RoomNumber);
            Assert.AreEqual("deluxe", room.Type);
            Assert.AreEqual(130.00m, room.PricePerNight);
            Assert.IsTrue(room.IsAvailable);
        }

        [TestMethod]
        public void mutableRoomTest()
        {
            var room = new Room(103, "penthouse", 250.00m, true);
            room.IsAvailable = false;
            Assert.IsFalse(room.IsAvailable);
        }

        [TestMethod]
        public void CustomerinittializerTest()
        {
            var customer = new Customer("leo", "leo@gmail.com", "222-3345");
            Assert.AreEqual("leo", customer.Name);
            Assert.AreEqual("leo@gmail.com", customer.Email);
            Assert.AreEqual("222-3345", customer.Phone);
        }

        [TestMethod]
        public void ReservationTestCalculations()
        {
            var customer = new Customer("leo", "leo@gmail.com", "222-3345");
            var room = new Room(102, "deluxe", 200.00m, true);
            var reservation= new Reservation(1001, customer, room, DateTime.Now, DateTime.Now.AddDays(1));

            Assert.AreEqual(1001, reservation.ReservationId);
            Assert.AreEqual(200.00m, room.PricePerNight); //try TOtalcost.
        }

        [TestMethod]
        public void reservationinvaliddateTest()
        {
            var customer = new Customer("leo", "leo@gmail.com", "222-3345");
            var room = new Room(102, "deluxe", 200.00m, true);
            var checkInDate = DateTime.Today.AddDays(2);
            var checkOutDate = DateTime.Today.AddDays(1);


           // var reservation = new Reservation(2, customer, room, DateTime.Today, DateTime.Today);
            var res= Assert.ThrowsException<ArgumentException>(() => new Reservation(2, customer, room, checkInDate, checkOutDate));

        }

        [TestMethod]
        public void createReservationTest()
        {
            var res=bookings.MakeReservation(customer, 102, DateTime.Now, DateTime.Now.AddDays(2));

            Assert.IsTrue(res.Success);
            Assert.IsInstanceOfType(res.Data, typeof(Reservation));
        }

        [TestMethod]
        public void addNewRoomTest()
        {
            var room = new Room(114, "suite", 300.00m, true);
            var res = bookings.AddRoom(room);

            Assert.IsTrue(res.Success);
            Assert.IsTrue(bookings.GetAllRooms().Any(r => r.RoomNumber == room.RoomNumber));
        }

        [TestMethod]
        public void removeReservationTest()
        {
            var reservation = bookings.MakeReservation(customer, 102, DateTime.Now, DateTime.Now.AddDays(2)).Data as Reservation;

            var terminateRes= bookings.CancelReservation(reservation.ReservationId);
            Assert.IsTrue(terminateRes.Success);
            Assert.IsNull(bookings.GetReservationById(reservation.ReservationId));
        }
    }
}
