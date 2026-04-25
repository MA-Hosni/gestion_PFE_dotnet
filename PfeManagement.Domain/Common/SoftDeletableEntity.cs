using System;

namespace PfeManagement.Domain.Common
{
    public abstract class SoftDeletableEntity : BaseEntity
    {
        public DateTime? DeletedAt { get; set; }
        
        public bool IsDeleted => DeletedAt.HasValue;
        
        public void Delete()
        {
            DeletedAt = DateTime.UtcNow;
        }
        
        public void Restore()
        {
            DeletedAt = null;
        }
    }
}
