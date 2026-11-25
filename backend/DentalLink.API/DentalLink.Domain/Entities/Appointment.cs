using DentalLink.Domain.Entities.Base;
using DentalLink.Domain.Entities.Lookup;

namespace DentalLink.Domain.Entities
{
    public class Appointment : BaseEntity
    {
        #region Properties

        public string? PatientComplaint { get; set; }
        public string AppointmentNumber { get; set; } = null!;
        public string? DoctorNotes { get; set; }
        public bool IsConfirmedByContact { get; set; } = false;

        #endregion Properties

        #region Foreign Keys
        
        public Guid ContactId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid SlotId { get; set; }
        public Guid CabinetId { get; set; }
        public Guid? UserId { get; set; }
        public Guid StatusId { get; set; }

        #endregion Foreign Keys

        #region Navigation Properties
        
        public AppointmentStatus Status { get; set; } = null!;
        public Contact Contact { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public TimeSlot Slot { get; set; } = null!;
        public Cabinet Cabinet { get; set; } = null!;
        public User? User { get; set; }

        #endregion Navigation Properties

        #region Collections
        
        public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        #endregion Collections
    }
}
