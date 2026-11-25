using DentalLink.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Lookup
{
    public class Cabinet : BaseLookup
    {
        public int ChairNumber { get; set; }
        public bool IsAvailable { get; set; } = true;

        public ICollection<DoctorSchedule> Schedules { get; set; } = [];
        public ICollection<TimeSlot> TimeSlots { get; set; } = [];
        public ICollection<Appointment> Appointments { get; set; } = [];
    }
}
