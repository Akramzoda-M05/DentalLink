using DentalLink.Domain.Entities.Base;
using DentalLink.Domain.Entities.Lookup;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace DentalLink.Domain.Entities
{
    public class Doctor : BaseEntity
    {
        #region Properties

        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Qualification { get; set; }
        public int ExperienceYears { get; set; }

        #endregion Properties

        #region Foreign Keys

        public Guid ContactId { get; set; }
        public Guid SpecializationId { get; set; }

        #endregion Foreign Keys

        #region Navigation Properties

        public Contact Contact { get; set; } = null!;
        public DoctorSpecialization Specialization { get; set; } = null!;

        #endregion Navigation Properties

        #region Collections
        public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();

        #endregion Collections
    }

}
