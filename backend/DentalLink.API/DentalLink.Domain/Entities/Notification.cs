using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities.Base;
using DentalLink.Domain.Entities.Lookup;
using System;
using System.Collections.Generic;
using System.Text;

namespace DentalLink.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid AppointmentId { get; set; }
        public Guid TelegramChatId { get; set; }
        public Guid TypeId { get; set; }
        public NotificationType Type { get; set; } = null!;
        public Guid StatusId { get; set; } = LookupIds.NotificationStatus.Pending;
        public NotificationStatus Status { get; set; } = null!;
        public DateTimeOffset ScheduledAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public string? ResponsePayload { get; set; }

        public Appointment Appointment { get; set; } = null!;
    }
}
