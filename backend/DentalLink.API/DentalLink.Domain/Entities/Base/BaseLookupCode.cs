using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Base
{
    public abstract class BaseLookupCode : BaseLookup
    {
        public string Code { get; set; } = string.Empty;
    }
}
