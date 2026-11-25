using DentalLink.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Lookup
{
    public class DentalService : BaseLookup
    {
        public Guid CategoryId { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;

        public ServiceCategory Category { get; set; } = null!;
        public ICollection<AppointmentService> AppointmentServices { get; set; } = [];
    }
}
