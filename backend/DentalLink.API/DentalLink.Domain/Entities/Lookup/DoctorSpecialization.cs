using DentalLink.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Lookup
{
    public sealed class DoctorSpecialization : BaseLookupCode
    {
        public ICollection<Doctor> Doctors { get; set; } = [];
    }
}
