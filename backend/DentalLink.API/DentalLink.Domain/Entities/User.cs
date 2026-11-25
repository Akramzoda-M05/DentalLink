using DentalLink.Domain.Entities.Base;
using DentalLink.Domain.Entities.Lookup;

namespace DentalLink.Domain.Entities
{
    public class User : BaseEntity
    {
        #region Properties

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        #endregion Properties

        #region Foreign Keys
        
        public Guid? RoleId { get; set; }
        public Guid? ContactId { get; set; }

        #endregion Foreign Keys

        #region Navigation Properties

        public UserRole Role { get; set; } = null!;
        public Contact? Contact { get; set; } = null!;

        #endregion Navigation Properties

        #region Collections

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        
        #endregion Collections
    }

}
