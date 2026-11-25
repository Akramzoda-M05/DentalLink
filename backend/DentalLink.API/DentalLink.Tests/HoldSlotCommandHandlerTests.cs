//using DentalLink.Application.Common.Interfaces;
//using DentalLink.Application.Features.Slots;
//using DentalLink.Domain.Entities;
//using DentalLink.Domain.Exceptions;
//using DentalLink.Infrastructure.Data;
//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;

//namespace DentalLink.Tests.Features;

//public class HoldSlotCommandHandlerTests
//{
//    private DentalLinkDbContext CreateDbContext(string dbName)
//    {
//        var options = new DbContextOptionsBuilder<DentalLinkDbContext>()
//            .UseInMemoryDatabase(databaseName: dbName)
//            .Options;
//        return new DentalLinkDbContext(options);
//    }

//    [Fact]
//    public async Task Handle_WhenSlotIsFree_ShouldHoldSlotFor10Minutes()
//    {
//        // Arrange
//        var db = CreateDbContext(Guid.NewGuid().ToString());
//        var slot = new TimeSlot
//        {
//            Id = 101,
//            ScheduleId = 1,
//            DoctorId = 1,
//            CabinetId = 1,
//            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow),
//            StartTime = new TimeOnly(14, 0),
//            EndTime = new TimeOnly(14, 30),
//            StatusId = LookupIds.TimeSlotStatus.Free
//        };
//        db.TimeSlots.Add(slot);
//        await db.SaveChangesAsync();

//        var lockMock = new Mock<IDistributedLockService>();
//        lockMock.Setup(l => l.TryAcquireLockAsync(
//            It.IsAny<string>(),
//            It.IsAny<TimeSpan>(),
//            It.IsAny<TimeSpan>(),
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new Mock<IDisposable>().Object);

//        var handler = new HoldSlotCommandHandler(db, lockMock.Object);

//        // Act
//        var result = await handler.Handle(new HoldSlotCommand(101), CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.SlotId.Should().Be(101);
//        result.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(9));

//        var updatedSlot = await db.TimeSlots.FindAsync(101L);
//        updatedSlot!.StatusId.Should().Be(LookupIds.TimeSlotStatus.Reserved);
//        updatedSlot.ReservedUntil.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(10), TimeSpan.FromSeconds(5));
//    }

//    [Fact]
//    public async Task Handle_WhenSlotIsBooked_ShouldThrowConflictException()
//    {
//        // Arrange
//        var db = CreateDbContext(Guid.NewGuid().ToString());
//        var slot = new TimeSlot
//        {
//            Id = 102,
//            ScheduleId = 1,
//            DoctorId = 1,
//            CabinetId = 1,
//            SlotDate = DateOnly.FromDateTime(DateTime.UtcNow),
//            StartTime = new TimeOnly(15, 0),
//            EndTime = new TimeOnly(15, 30),
//            StatusId = LookupIds.TimeSlotStatus.Booked
//        };
//        db.TimeSlots.Add(slot);
//        await db.SaveChangesAsync();

//        var lockMock = new Mock<IDistributedLockService>();
//        lockMock.Setup(l => l.TryAcquireLockAsync(
//            It.IsAny<string>(),
//            It.IsAny<TimeSpan>(),
//            It.IsAny<TimeSpan>(),
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new Mock<IDisposable>().Object);

//        var handler = new HoldSlotCommandHandler(db, lockMock.Object);

//        // Act & Assert
//        var act = async () => await handler.Handle(new HoldSlotCommand(102), CancellationToken.None);
//        await act.Should().ThrowAsync<ConflictException>();
//    }

//    [Fact]
//    public async Task Handle_WhenLockCannotBeAcquired_ShouldThrowConflictException()
//    {
//        // Arrange
//        var db = CreateDbContext(Guid.NewGuid().ToString());
//        var lockMock = new Mock<IDistributedLockService>();
//        lockMock.Setup(l => l.TryAcquireLockAsync(
//            It.IsAny<string>(),
//            It.IsAny<TimeSpan>(),
//            It.IsAny<TimeSpan>(),
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync((IDisposable?)null); // Lock acquisition fails (concurrent hold)

//        var handler = new HoldSlotCommandHandler(db, lockMock.Object);

//        // Act & Assert
//        var act = async () => await handler.Handle(new HoldSlotCommand(999), CancellationToken.None);
//        await act.Should().ThrowAsync<ConflictException>()
//            .WithMessage("*being processed by another patient*");
//    }
//}
