using System.Collections.Generic;

namespace PfeManagement.Domain.Entities
{
    public class CompanySupervisor : User
    {
        public string CompanyName { get; set; } = string.Empty;
        public string BadgeIMG { get; set; } = string.Empty;
        
        // Navigation property for aggregation (a supervisor oversees many students)
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
