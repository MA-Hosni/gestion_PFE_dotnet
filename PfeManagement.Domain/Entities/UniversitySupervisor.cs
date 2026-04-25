using System.Collections.Generic;

namespace PfeManagement.Domain.Entities
{
    public class UniversitySupervisor : User
    {
        public string BadgeIMG { get; set; } = string.Empty;
        
        // Navigation property
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
