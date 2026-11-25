using DentalLink.Domain.Entities.Base;

namespace DentalLink.Domain.Entities
{
    public class Contact : BaseEntity
    {
        #region Properties

        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public DateOnly? BirthDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        public Guid? TelegramChatId { get; set; }

        #endregion Properties

        #region Foreign Keys



        #endregion Foreign Keys

        #region Navigation Properties

        public User? User { get; set; }
        public Doctor? DoctorProfile { get; set; }

        #endregion Navigation Properties

        #region Collections

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        #endregion Collections
    }
}
