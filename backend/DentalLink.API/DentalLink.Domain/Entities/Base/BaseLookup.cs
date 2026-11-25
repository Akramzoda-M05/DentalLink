using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Base
{
    public abstract class BaseLookup : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
