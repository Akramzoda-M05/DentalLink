using DentalLink.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities.Lookup
{
    public sealed class UserRole : BaseLookupCode
    {
        public ICollection<User> Users { get; set; } = [];
    }
}
