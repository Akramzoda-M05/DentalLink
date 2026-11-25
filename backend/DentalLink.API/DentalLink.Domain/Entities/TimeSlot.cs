using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities.Base;
using DentalLink.Domain.Entities.Lookup;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities
{
    public class TimeSlot : BaseEntity
    {
        public Guid ScheduleId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid CabinetId { get; set; }
        public DateOnly SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Guid StatusId { get; set; } = LookupIds.TimeSlotStatus.Free;
        public TimeSlotStatus Status { get; set; } = null!;
        public DateTimeOffset? ReservedUntil { get; set; }

        public DoctorSchedule Schedule { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public Cabinet Cabinet { get; set; } = null!;
        public Appointment? Appointment { get; set; }
    }
}
