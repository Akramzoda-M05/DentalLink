using DentalLink.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Lookup
{
    public class ServiceCategory : BaseLookup
    {
        public ICollection<DentalService> Services { get; set; } = [];
    }
}
