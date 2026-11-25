using DentalLink.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Lookup
{
    public class TimeSlotStatus : BaseLookupCode
    {
        public ICollection<TimeSlot> TimeSlots { get; set; } = [];
    }
}
