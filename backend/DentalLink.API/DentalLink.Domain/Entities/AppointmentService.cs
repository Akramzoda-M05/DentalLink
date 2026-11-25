using DentalLink.Domain.Entities.Base;
using DentalLink.Domain.Entities.Lookup;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities
{
    public class AppointmentService : BaseEntity
    {
        public Guid AppointmentId { get; set; }
        public Guid ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal PriceAtBooking { get; set; }

        public Appointment Appointment { get; set; } = null!;
        public DentalService Service { get; set; } = null!;
    }
}
