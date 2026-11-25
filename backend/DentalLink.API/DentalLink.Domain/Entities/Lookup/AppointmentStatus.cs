using DentalLink.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Lookup
{
    public class AppointmentStatus : BaseLookupCode
    {
        public ICollection<Appointment> Appointments { get; set; } = [];
    }
}
