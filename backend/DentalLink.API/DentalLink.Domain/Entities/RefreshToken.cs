using DentalLink.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        public User User { get; set; } = null!;
    }
}
