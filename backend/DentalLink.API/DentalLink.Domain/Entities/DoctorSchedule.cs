using DentalLink.Domain.Entities.Base;
using DentalLink.Domain.Entities.Lookup;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities
{
    public class DoctorSchedule : BaseEntity
    {
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public Guid CabinetId { get; set; }
        public Cabinet Cabinet { get; set; } = null!;
        public DateOnly WorkDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public ICollection<TimeSlot> TimeSlots { get; set; } = [];
    }
}
