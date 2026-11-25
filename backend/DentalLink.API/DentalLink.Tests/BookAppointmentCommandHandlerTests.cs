//using DentalLink.Application.Common.Interfaces;
//using DentalLink.Application.Features.Appointments;
//using DentalLink.Domain.Entities;
//using DentalLink.Domain.Exceptions;
//using DentalLink.Infrastructure.Data;
//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;

//namespace DentalLink.Tests.Features;

//public class BookAppointmentCommandHandlerTests
//{
//    private DentalLinkDbContext CreateDbContext(string dbName)
//    {
//        var options = new DbContextOptionsBuilder<DentalLinkDbContext>()
//            .UseInMemoryDatabase(databaseName: dbName)
//            .Options;
//        return new DentalLinkDbContext(options);
//    }

//    [Fact]
//    public async Task Handle_WhenValidBooking_ShouldCreateAppointmentAndBookSlot()
//    {
//        // Arrange
//        var db = CreateDbContext(Guid.NewGuid().ToString());

//        var user = new User { Id = 1, FullName = "Доктор Тест", Email = "doc@test.tj", Phone = "+992900000010" };
//        var spec = new DoctorSpecialization { Id = 1, Code = "therapist", Name = "Терапевт" };
//        var doctor = new Doctor { Id = 1, UserId = 1, User = user, SpecializationId = 1, Specialization = spec };
//        var cabinet = new Cabinet { Id = 1, Name = "Кабинет №1" };
//        var service = new DentalService { Id = 1, Name = "Лечение кариеса", Price = 200m, DurationMinutes = 30, IsActive = true };
//        var slot = new TimeSlot
//        {
//            Id = 201,
//            ScheduleId = 1,
//            DoctorId = 1,
//            Doctor = doctor,
//            CabinetId = 1,
//            Cabinet = cabinet,
//            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
//            StartTime = new TimeOnly(10, 0),
//            EndTime = new TimeOnly(10, 30),
//            StatusId = LookupIds.TimeSlotStatus.Free
//        };
//        var status = new AppointmentStatus { Id = LookupIds.AppointmentStatus.Confirmed, Code = "confirmed", Name = "Подтверждена" };

//        db.Users.Add(user);
//        db.DoctorSpecializations.Add(spec);
//        db.Doctors.Add(doctor);
//        db.Cabinets.Add(cabinet);
//        db.DentalServices.Add(service);
//        db.TimeSlots.Add(slot);
//        db.AppointmentStatuses.Add(status);
//        await db.SaveChangesAsync();

//        var lockMock = new Mock<IDistributedLockService>();
//        lockMock.Setup(l => l.TryAcquireLockAsync(
//            It.IsAny<string>(),
//            It.IsAny<TimeSpan>(),
//            It.IsAny<TimeSpan>(),
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new Mock<IDisposable>().Object);

//        var handler = new BookAppointmentCommandHandler(db, lockMock.Object);

//        var command = new BookAppointmentCommand(
//            SlotId: 201,
//            ServiceId: 1,
//            PatientName: "Исмоил Сомони",
//            PatientPhone: "+992900112233",
//            PatientEmail: "ismoil@test.tj",
//            Complaint: "Острая зубная боль"
//        );

//        // Act
//        var result = await handler.Handle(command, CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.Appointment.Should().NotBeNull();
//        result.Appointment.PatientName.Should().Be("Исмоил Сомони");
//        result.Appointment.DoctorName.Should().Be("Доктор Тест");
//        result.Appointment.StatusCode.Should().Be("confirmed");
//        result.TelegramBotDeepLink.Should().Contain("t.me/");

//        var bookedSlot = await db.TimeSlots.FindAsync(201L);
//        bookedSlot!.StatusId.Should().Be(LookupIds.TimeSlotStatus.Booked);

//        var patient = await db.Patients.FirstOrDefaultAsync(p => p.Phone == "+992900112233");
//        patient.Should().NotBeNull();
//        patient!.FullName.Should().Be("Исмоил Сомони");

//        var apptInDb = await db.Appointments.Include(a => a.AppointmentServices).FirstOrDefaultAsync();
//        apptInDb.Should().NotBeNull();
//        apptInDb!.AppointmentServices.Should().HaveCount(1);
//        apptInDb.AppointmentServices.First().PriceAtBooking.Should().Be(200m);
//    }
//}
