namespace DentalLink.Domain.Entities.Base
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? CreatedById { get; set; }
        public Guid? UpdatedById { get; set; }
        public Contact? CreatedBy { get; set; }
        public Contact? UpdatedBy { get; set; }
    }
}
